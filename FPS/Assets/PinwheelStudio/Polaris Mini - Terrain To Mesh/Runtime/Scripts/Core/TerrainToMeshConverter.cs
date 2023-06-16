#if POMINI
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using StopWatch = System.Diagnostics.Stopwatch;

namespace Pinwheel.PolarisMini
{
    /// <summary>
    /// The main point of all API calls
    /// </summary>
    public static class TerrainToMeshConverter
    {
        /// <summary>
        /// Convert an existing terrain to mesh
        /// </summary>
        /// <param name="terrain">The terrain to be converted</param>
        /// <param name="options">Options for the conversion</param>
        /// <returns>The conversion result, including meshes, textures, materials, etc.</returns>
        public static ConversionResult Convert(Terrain terrain, ConversionOptions options)
        {
            StopWatch sw = new StopWatch();
            sw.Start();

            ConversionResult conversionResult = new ConversionResult(options);
            ConversionResources resources = new ConversionResources();

            resources.HeightMap = TerrainDataExtractor.ExtractRFloatHeightMap(terrain.terrainData);
            resources.HeightMapResolution = terrain.terrainData.heightmapResolution;
            resources.HoleMap = TerrainDataExtractor.ExtractRFloatHoleMap(terrain.terrainData);

            TerrainChunk[] chunks = CreateChunks(options);
            int lodCount = options.LODCount;

            for (int lod = 0; lod < lodCount; ++lod)
            {
                GenerateChunks(chunks, lod, options, conversionResult, resources);
            }

            if (options.ExportAlphaMaps)
            {
                Texture2D[] alphaMaps = TerrainDataExtractor.ExtractAlphaMaps(terrain.terrainData, terrain.terrainData.alphamapResolution);
                conversionResult.SetAlphaMaps(alphaMaps);
            }

            if (options.ExportAlbedoMetallicMap)
            {
                Texture2D[] albedoMetallic = TerrainDataExtractor.ExtractAlbedoMetallicMap(terrain.terrainData, terrain.terrainData.alphamapResolution);
                conversionResult.SetAlbedoMap(albedoMetallic[0]);
                conversionResult.SetMetallicMap(albedoMetallic[1]);
            }

            resources.Dispose();

            sw.Stop();
            conversionResult.ProcessingTimeMiliSec = sw.ElapsedMilliseconds;
            return conversionResult;
        }

        public static TerrainChunk[] CreateChunks(ConversionOptions options)
        {
            int gridSize = options.ChunkGridSize;

            //create a list of new chunks
            List<TerrainChunk> chunks = new List<TerrainChunk>();

            for (int z = 0; z < gridSize; ++z)
            {
                for (int x = 0; x < gridSize; ++x)
                {
                    TerrainChunk chunk = new TerrainChunk();
                    chunk.Index = new Vector2(x, z);
                    chunk.ConversionOptions = options;
                    chunks.Add(chunk);
                }
            }

            for (int i = 0; i < chunks.Count; ++i)
            {
                TerrainChunk currentChunk = chunks[i];

                Utilities.Fill(currentChunk.Internal_NeighborChunks, null);
                for (int j = 0; j < chunks.Count; ++j)
                {
                    TerrainChunk otherChunk = chunks[j];
                    if (otherChunk.Index == currentChunk.Index + Vector2.left)
                    {
                        currentChunk.Internal_NeighborChunks[0] = otherChunk;
                    }
                    if (otherChunk.Index == currentChunk.Index + Vector2.up)
                    {
                        currentChunk.Internal_NeighborChunks[1] = otherChunk;
                    }
                    if (otherChunk.Index == currentChunk.Index + Vector2.right)
                    {
                        currentChunk.Internal_NeighborChunks[2] = otherChunk;
                    }
                    if (otherChunk.Index == currentChunk.Index + Vector2.down)
                    {
                        currentChunk.Internal_NeighborChunks[3] = otherChunk;
                    }
                }
            }

            return chunks.ToArray();
        }

        public static void GenerateChunks(TerrainChunk[] chunks, int lod, ConversionOptions options, ConversionResult conversionResult, ConversionResources resources)
        {
            InitSubdivArrays(chunks, options);
            CreateBaseSubdivTree(chunks, options, lod);
            SplitBaseTreeForDynamicPolygon(chunks, options, resources, lod);
            if (lod == 0)
            {
                if (options.MeshBaseResolution != options.MeshResolution)
                {
                    StitchSeam(chunks, options);
                }
            }
            else
            {
                StitchSeamLOD(chunks, options, lod);
            }
            CountLeafNode(chunks, options, lod);
            CreateVertex(chunks, options, resources, lod);
            UpdateMesh(chunks, options, resources, conversionResult, lod);
            foreach (TerrainChunk c in chunks)
            {
                c.CleanUpSubdivArrays();
            }
        }

        private static void InitSubdivArrays(TerrainChunk[] chunks, ConversionOptions options)
        {
            foreach (TerrainChunk c in chunks)
            {
                c.InitSubdivArrays(options.MeshResolution);
            }
        }

        private static void CreateBaseSubdivTree(
            TerrainChunk[] chunks,
            ConversionOptions options,
            int lod)
        {
            JobHandle[] jobHandles = new JobHandle[chunks.Length];
            for (int i = 0; i < chunks.Length; ++i)
            {
                CreateBaseTreeJob j = chunks[i].GetCreateBaseSubdivTreeJob(
                    options.MeshBaseResolution,
                    options.MeshResolution,
                    lod);
                jobHandles[i] = j.Schedule();
            }
            JobUtilities.CompleteAll(jobHandles);
        }

        private static void SplitBaseTreeForDynamicPolygon(
            TerrainChunk[] chunks,
            ConversionOptions options,
            ConversionResources resources,
            int lod)
        {
            JobHandle[] jobHandles = new JobHandle[chunks.Length];
            resources.Internal_CreateNewSubDivisionMap();
            TextureNativeDataDescriptor<Color32> subdivMap = new TextureNativeDataDescriptor<Color32>(resources.Internal_SubDivisionMap);
            for (int i = 0; i < chunks.Length; ++i)
            {
                SplitBaseTreeForDynamicPolygonJob j = chunks[i].GetSplitBaseTreeForDynamicPolygonJob(
                    options.MeshBaseResolution,
                    options.MeshResolution,
                    lod,
                    subdivMap);
                jobHandles[i] = j.Schedule();
            }
            JobUtilities.CompleteAll(jobHandles);
        }

        private static void StitchSeam(
            TerrainChunk[] chunks,
            ConversionOptions options)
        {
            JobHandle[] jobHandles = new JobHandle[chunks.Length];
            int stitchSeamIteration = 0;
            int stitchSeamMaxIteration = 10;
            bool newVertexCreated = true;
            List<NativeArray<bool>> markers = new List<NativeArray<bool>>();

            while (newVertexCreated && stitchSeamIteration <= stitchSeamMaxIteration)
            {
                StitchSeamJob[] stitchJobs = new StitchSeamJob[chunks.Length];
                for (int i = 0; i < chunks.Length; ++i)
                {
                    TerrainChunk c = chunks[i];

                    TerrainChunk leftChunk = GetLeftNeighborChunk(c, options.ChunkGridSize);
                    bool hasLeftMarkers = leftChunk != null;
                    NativeArray<bool> leftMarkers = hasLeftMarkers ? leftChunk.GetVertexMarker() : new NativeArray<bool>(1, Allocator.TempJob);
                    markers.Add(leftMarkers);

                    TerrainChunk topChunk = GetTopNeighborChunk(c, options.ChunkGridSize);
                    bool hasTopMarkers = topChunk != null;
                    NativeArray<bool> topMarkers = hasTopMarkers ? topChunk.GetVertexMarker() : new NativeArray<bool>(1, Allocator.TempJob);
                    markers.Add(topMarkers);

                    TerrainChunk rightChunk = GetRightNeighborChunk(c, options.ChunkGridSize);
                    bool hasRightMarkers = rightChunk != null;
                    NativeArray<bool> rightMarkers = hasRightMarkers ? rightChunk.GetVertexMarker() : new NativeArray<bool>(1, Allocator.TempJob);
                    markers.Add(rightMarkers);

                    TerrainChunk bottomChunk = GetBottomNeighborChunk(c, options.ChunkGridSize);
                    bool hasBottomMarkers = bottomChunk != null;
                    NativeArray<bool> bottomMarkers = hasBottomMarkers ? bottomChunk.GetVertexMarker() : new NativeArray<bool>(1, Allocator.TempJob);
                    markers.Add(bottomMarkers);

                    StitchSeamJob j = c.GetStitchSeamJob(
                        options.MeshBaseResolution,
                        options.MeshResolution,
                        hasLeftMarkers, leftMarkers,
                        hasTopMarkers, topMarkers,
                        hasRightMarkers, rightMarkers,
                        hasBottomMarkers, bottomMarkers
                        );
                    stitchJobs[i] = j;
                }
                for (int i = 0; i < stitchJobs.Length; ++i)
                {
                    jobHandles[i] = stitchJobs[i].Schedule();
                }

                JobUtilities.CompleteAll(jobHandles);

                stitchSeamIteration += 1;
                int tmp = 0;
                for (int i = 0; i < chunks.Length; ++i)
                {
                    tmp += chunks[i].GetGenerationMetadata(GeometryJobUtilities.METADATA_NEW_VERTEX_CREATED);
                }
                newVertexCreated = tmp > 0;
            }

            for (int i = 0; i < markers.Count; ++i)
            {
                NativeArrayUtilities.Dispose(markers[i]);
            }

            foreach (TerrainChunk c in chunks)
            {
                c.CacheMarker0();
            }
        }

        private static TerrainChunk GetLeftNeighborChunk(TerrainChunk c, int chunkGridSize)
        {
            int maxIndex = chunkGridSize - 1;
            Vector2 index = c.Index;
            if (index.x > 0)
            {
                return c.Internal_NeighborChunks[0];
            }
            else
            {
                return null;
            }
        }

        private static TerrainChunk GetTopNeighborChunk(TerrainChunk c, int chunkGridSize)
        {
            int maxIndex = chunkGridSize - 1;
            Vector2 index = c.Index;
            if (index.y < maxIndex)
            {
                return c.Internal_NeighborChunks[1];
            }
            else
            {
                return null;
            }
        }

        private static TerrainChunk GetRightNeighborChunk(TerrainChunk c, int chunkGridSize)
        {
            int maxIndex = chunkGridSize - 1;
            Vector2 index = c.Index;
            if (index.x < maxIndex)
            {
                return c.Internal_NeighborChunks[2];
            }
            else
            {
                return null;
            }
        }

        private static TerrainChunk GetBottomNeighborChunk(TerrainChunk c, int chunkGridSize)
        {
            int maxIndex = chunkGridSize - 1;
            Vector2 index = c.Index;
            if (index.y > 0)
            {
                return c.Internal_NeighborChunks[3];
            }
            else
            {
                return null;
            }
        }

        private static void StitchSeamLOD(
            TerrainChunk[] chunks,
            ConversionOptions options,
            int lod)
        {
            JobHandle[] jobHandles = new JobHandle[chunks.Length];
            List<NativeArray<bool>> markers = new List<NativeArray<bool>>();
            int stitchSeamIteration = 0;
            int stitchSeamMaxIteration = 10;
            bool newVertexCreated = true;

            for (int i = 0; i < chunks.Length; ++i)
            {
                TerrainChunk c = chunks[i];
                NativeArray<bool> markerLOD0 = c.GetMarker0();
                markers.Add(markerLOD0);
            }

            while (newVertexCreated && stitchSeamIteration <= stitchSeamMaxIteration)
            {
                for (int i = 0; i < chunks.Length; ++i)
                {
                    TerrainChunk c = chunks[i];
                    StitchSeamLODJob j = c.GetStitchSeamLODJob(
                        options.MeshBaseResolution,
                        options.MeshResolution,
                        lod,
                        markers[i]);
                    jobHandles[i] = j.Schedule();
                }
                JobUtilities.CompleteAll(jobHandles);

                stitchSeamIteration += 1;
                int tmp = 0;
                for (int i = 0; i < chunks.Length; ++i)
                {
                    tmp += chunks[i].GetGenerationMetadata(GeometryJobUtilities.METADATA_NEW_VERTEX_CREATED);
                }
                newVertexCreated = tmp > 0;
            }

            for (int i = 0; i < markers.Count; ++i)
            {
                NativeArrayUtilities.Dispose(markers[i]);
            }
        }

        private static void CountLeafNode(
            TerrainChunk[] chunks,
            ConversionOptions options,
            int lod)
        {
            JobHandle[] jobHandles = new JobHandle[chunks.Length];
            for (int i = 0; i < chunks.Length; ++i)
            {
                CountLeafNodeJob j = chunks[i].GetCountLeafNodeJob(options.MeshBaseResolution, options.MeshResolution, lod);
                jobHandles[i] = j.Schedule();
            }
            JobUtilities.CompleteAll(jobHandles);
        }

        private static void CreateVertex(
            TerrainChunk[] chunks,
            ConversionOptions options,
            ConversionResources resources,
            int lod)
        {
            JobHandle[] jobHandles = new JobHandle[chunks.Length];
            TextureNativeDataDescriptor<float> heightMap = new TextureNativeDataDescriptor<float>(resources.HeightMap);
            TextureNativeDataDescriptor<float> holeMap = new TextureNativeDataDescriptor<float>(resources.HoleMap);
            for (int i = 0; i < chunks.Length; ++i)
            {
                CreateVertexJob j = chunks[i].GetCreateVertexJob(
                    options.MeshBaseResolution,
                    options.MeshResolution,
                    lod,
                    options.DisplacementSeed,
                    options.DisplacementStrength,
                    options.SmoothNormal,
                    options.MergeUv,
                    heightMap,
                    holeMap,
                    resources);

                jobHandles[i] = j.Schedule();
            }
            JobUtilities.CompleteAll(jobHandles);
        }

        private static void UpdateMesh(
            TerrainChunk[] chunks,
            ConversionOptions options,
            ConversionResources resources,
            ConversionResult conversionResult,
            int lod)
        {
            for (int i = 0; i < chunks.Length; ++i)
            {
                chunks[i].UpdateMesh(lod, options, conversionResult);
            }
        }

        private static void CleanupSubdivArrays(TerrainChunk[] chunks)
        {
            foreach (TerrainChunk c in chunks)
            {
                c.CleanupMarkers();
            }
        }
    }

}

#endif