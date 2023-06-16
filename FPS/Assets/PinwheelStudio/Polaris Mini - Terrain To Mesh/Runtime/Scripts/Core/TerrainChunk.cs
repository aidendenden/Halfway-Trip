#if POMINI

using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;

namespace Pinwheel.PolarisMini
{
    public class TerrainChunk
    {
        private ConversionOptions conversionOptions;
        public ConversionOptions ConversionOptions
        {
            get
            {
                return conversionOptions;
            }
            set
            {
                conversionOptions = value;
            }
        }

        private Vector2 index;
        public Vector2 Index
        {
            get
            {
                return index;
            }
            internal set
            {
                index = value;
            }
        }

        private TerrainChunk[] neighborChunks;
        internal TerrainChunk[] Internal_NeighborChunks
        {
            get
            {
                if (neighborChunks == null || neighborChunks.Length != 4)
                {
                    neighborChunks = new TerrainChunk[4];
                }
                return neighborChunks;
            }
            set
            {
                neighborChunks = value;
            }
        }

        private NativeArray<SubdivNode> subdivNodeNativeArray;
        private NativeArray<byte> subdivNodeCreationState;

        private NativeArray<Vector3> vertexNativeArray;
        private Vector3[] vertexArray;

        private NativeArray<Vector2> uvsNativeArray;
        private Vector2[] uvsArray;

        private NativeArray<int> trianglesNativeArray;
        private int[] trianglesArray;

        private NativeArray<Vector3> normalsNativeArray;
        private Vector3[] normalsArray;

        private NativeArray<bool> vertexMarkerNativeArray;

        private NativeArray<int> generationMetadata;

        private Mesh[] nonSerializedMeshes;
        private Mesh[] NonSerializedMeshes
        {
            get
            {
                if (nonSerializedMeshes == null)
                {
                    nonSerializedMeshes = new Mesh[Common.MAX_LOD_COUNT];
                }
                if (nonSerializedMeshes.Length != Common.MAX_LOD_COUNT)
                {
                    CleanUpNonSerializedMeshes();
                    nonSerializedMeshes = new Mesh[Common.MAX_LOD_COUNT];
                }
                return nonSerializedMeshes;
            }
        }

        public Rect GetUvRange()
        {
            int gridSize = ConversionOptions.ChunkGridSize;
            Vector2 position = index / gridSize;
            Vector2 size = Vector2.one / gridSize;
            return new Rect(position, size);
        }

        private void RecalculateTangentIfNeeded(Mesh m)
        {
            if (m == null)
                return;
            m.RecalculateTangents();
        }

        public Mesh GetMesh(int lod)
        {
            int lodCount = ConversionOptions.LODCount;

            if (lod < 0 || lod >= lodCount)
            {
                throw new System.ArgumentOutOfRangeException("lod");
            }

            string key = GetChunkMeshName(Index, lod);
            Mesh m = NonSerializedMeshes[lod];
            if (m == null)
            {
                m = new Mesh();
                m.name = string.Format("{0}", key);
                m.MarkDynamic();
                NonSerializedMeshes[lod] = m;
            }
            //m.hideFlags = HideFlags.DontSave;

            return m;

        }

        public static string GetChunkMeshName(Vector2 index, int lod)
        {
            return string.Format("{0}_{1}_{2}_{3}", Common.CHUNK_MESH_NAME_PREFIX, (int)index.x, (int)index.y, lod);
        }



        internal int GetSubdivTreeNodeCount(int meshResolution)
        {
            int count = 0;
            for (int i = 0; i <= meshResolution; ++i)
            {
                count += GetSubdivTreeNodeCountForLevel(i);
            }
            return count;
        }

        internal int GetSubdivTreeNodeCountForLevel(int level)
        {
            return 2 * Mathf.FloorToInt(Mathf.Pow(2, level));
        }

        internal void InitMeshArrays()
        {
            int vertexCount = generationMetadata[0] * 3;
            vertexNativeArray = new NativeArray<Vector3>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            Utilities.EnsureArrayLength(ref vertexArray, vertexCount);

            uvsNativeArray = new NativeArray<Vector2>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            Utilities.EnsureArrayLength(ref uvsArray, vertexCount);

            trianglesNativeArray = new NativeArray<int>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            Utilities.EnsureArrayLength(ref trianglesArray, vertexCount);

            normalsNativeArray = new NativeArray<Vector3>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            Utilities.EnsureArrayLength(ref normalsArray, vertexCount);
        }

        internal void CleanUpMeshArrays()
        {
            NativeArrayUtilities.Dispose(vertexNativeArray);
            NativeArrayUtilities.Dispose(uvsNativeArray);
            NativeArrayUtilities.Dispose(normalsNativeArray);
            NativeArrayUtilities.Dispose(trianglesNativeArray);
        }

        internal void InitSubdivArrays(int meshResolution)
        {
            int treeNodeCount = GetSubdivTreeNodeCount(meshResolution);
            subdivNodeNativeArray = new NativeArray<SubdivNode>(treeNodeCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            subdivNodeCreationState = new NativeArray<byte>(treeNodeCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            generationMetadata = new NativeArray<int>(GeometryJobUtilities.METADATA_LENGTH, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            int dimension = GeometryJobUtilities.VERTEX_MARKER_DIMENSION;
            vertexMarkerNativeArray = new NativeArray<bool>(dimension * dimension, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        }

        internal void CleanUpSubdivArrays()
        {
            NativeArrayUtilities.Dispose(subdivNodeNativeArray);
            NativeArrayUtilities.Dispose(subdivNodeCreationState);
            NativeArrayUtilities.Dispose(generationMetadata);
            NativeArrayUtilities.Dispose(vertexMarkerNativeArray);
        }

        internal void InitMarkers()
        {

            int dimension = GeometryJobUtilities.VERTEX_MARKER_DIMENSION;
            vertexMarkerNativeArray = new NativeArray<bool>(dimension * dimension, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        }

        internal void CleanupMarkers()
        {

            NativeArrayUtilities.Dispose(vertexMarkerNativeArray);
        }

        internal void CleanUpNonSerializedMeshes()
        {
            if (nonSerializedMeshes != null)
            {
                for (int i = 0; i < nonSerializedMeshes.Length; ++i)
                {
                    Mesh m = nonSerializedMeshes[i];
                    if (m != null)
                    {
                        Utilities.DestroyObject(m);
                    }
                }
            }
        }

        internal CreateBaseTreeJob GetCreateBaseSubdivTreeJob(
            int meshBaseResolution,
            int meshResolution,
            int lod)
        {
            CreateBaseTreeJob job = new CreateBaseTreeJob()
            {
                nodes = subdivNodeNativeArray,
                creationState = subdivNodeCreationState,
                vertexMarker = vertexMarkerNativeArray,
                metadata = generationMetadata,
                baseResolution = meshBaseResolution,
                resolution = meshResolution,
                lod = lod
            };

            return job;
        }

        internal SplitBaseTreeForDynamicPolygonJob GetSplitBaseTreeForDynamicPolygonJob(
            int meshBaseResolution, int meshResolution, int lod,
            TextureNativeDataDescriptor<Color32> subdivMap)
        {
            Rect uvRect = GetUvRange();
            SplitBaseTreeForDynamicPolygonJob job = new SplitBaseTreeForDynamicPolygonJob()
            {
                baseTree = subdivNodeNativeArray,
                creationState = subdivNodeCreationState,
                vertexMarker = vertexMarkerNativeArray,
                subdivMap = subdivMap,
                baseResolution = meshBaseResolution,
                resolution = meshResolution,
                lod = lod,
                uvRect = uvRect
            };

            return job;
        }

        internal StitchSeamJob GetStitchSeamJob(
            int meshBaseResolution,
            int meshResolution,
            bool hasLeftMarkers, NativeArray<bool> leftMarkers,
            bool hasTopMarkers, NativeArray<bool> topMarkers,
            bool hasRightMarkers, NativeArray<bool> rightMarkers,
            bool hasBottomMarkers, NativeArray<bool> bottomMarkers)
        {
            StitchSeamJob job = new StitchSeamJob()
            {
                nodes = subdivNodeNativeArray,
                creationState = subdivNodeCreationState,
                vertexMarker = vertexMarkerNativeArray,
                metadata = generationMetadata,
                meshBaseResolution = meshBaseResolution,
                meshResolution = meshResolution,

                hasLeftMarker = hasLeftMarkers,
                vertexMarkerLeft = leftMarkers,

                hasTopMarker = hasTopMarkers,
                vertexMarkerTop = topMarkers,

                hasRightMarker = hasRightMarkers,
                vertexMarkerRight = rightMarkers,

                hasBottomMarker = hasBottomMarkers,
                vertexMarkerBottom = bottomMarkers
            };

            return job;
        }

        internal StitchSeamLODJob GetStitchSeamLODJob(
            int meshBaseResolution,
            int meshResolution,
            int lod,
            NativeArray<bool> markerLOD0)
        {
            StitchSeamLODJob job = new StitchSeamLODJob()
            {
                nodes = subdivNodeNativeArray,
                creationState = subdivNodeCreationState,
                vertexMarker = vertexMarkerNativeArray,
                metadata = generationMetadata,
                meshBaseResolution = meshBaseResolution,
                meshResolution = meshResolution,
                lod = lod,
                vertexMarker_LOD0 = markerLOD0
            };

            return job;
        }

        internal void CopyVertexMarker(NativeArray<bool> markers)
        {
            if (vertexMarkerNativeArray.IsCreated)
            {
                markers.CopyTo(vertexMarkerNativeArray);
            }
        }

        internal CountLeafNodeJob GetCountLeafNodeJob(
            int meshBaseResolution,
            int meshResolution,
            int lod)
        {
            CountLeafNodeJob job = new CountLeafNodeJob()
            {
                creationState = subdivNodeCreationState,
                metadata = generationMetadata,
                baseResolution = meshBaseResolution,
                resolution = meshResolution,
                lod = lod
            };

            return job;
        }

        internal CreateVertexJob GetCreateVertexJob(
            int meshBaseResolution,
            int meshResolution,
            int lod,
            int displacementSeed,
            float displacementStrength,
            bool smoothNormal,
            bool mergeUv,
            TextureNativeDataDescriptor<float> heightMap,
            TextureNativeDataDescriptor<float> holeMap,
            ConversionResources resources)
        {
            InitMeshArrays();

            Vector3 terrainSize = ConversionOptions.Size;
            Rect uvRect = GetUvRange();

            float texelSize = 1.0f / resources.HeightMapResolution;
            CreateVertexJob job = new CreateVertexJob()
            {
                nodes = subdivNodeNativeArray,
                creationState = subdivNodeCreationState,

                heightMap = heightMap,
                holeMap = holeMap,

                vertices = vertexNativeArray,
                uvs = uvsNativeArray,
                normals = normalsNativeArray,
                triangles = trianglesNativeArray,
                metadata = generationMetadata,

                meshBaseResolution = meshBaseResolution,
                meshResolution = meshResolution,
                lod = lod,
                displacementSeed = displacementSeed,
                displacementStrength = displacementStrength,
                smoothNormal = smoothNormal,
                mergeUV = mergeUv,

                terrainSize = terrainSize,
                chunkUvRect = uvRect,
                texelSize = texelSize
            };
            return job;
        }

        internal void UpdateMesh(int lod, ConversionOptions options, ConversionResult conversionResult)
        {
            Mesh m = GetMesh(lod);
            m.Clear();

            int leafCount = generationMetadata[GeometryJobUtilities.METADATA_LEAF_COUNT];
            int removedLeafCount = generationMetadata[GeometryJobUtilities.METADATA_LEAF_REMOVED];

            if (leafCount != removedLeafCount)
            {
                vertexNativeArray.CopyTo(vertexArray);
                uvsNativeArray.CopyTo(uvsArray);
                normalsNativeArray.CopyTo(normalsArray);
                trianglesNativeArray.CopyTo(trianglesArray);

                if (options.SmoothNormal && !options.MergeUv)
                {
                    MergeVertices(m);
                }
                else
                {
                    m.vertices = vertexArray;
                    m.uv = uvsArray;
                    m.triangles = trianglesArray;
                    m.normals = normalsArray;
                }

                m.RecalculateBounds();
                RecalculateTangentIfNeeded(m);
            }

            conversionResult.AddMesh(m, (int)Index.x, (int)Index.y, lod);
            CleanUpMeshArrays();
        }

        private void MergeVertices(Mesh m)
        {
            List<Vector3> vertexMerged = new List<Vector3>();
            List<Vector3> normalMerged = new List<Vector3>();
            List<Vector2> uvMerged = new List<Vector2>();
            List<int> indexMerged = new List<int>();

            for (int i = 0; i < vertexArray.Length; ++i)
            {
                int indexOf = vertexMerged.IndexOf(vertexArray[i]);
                if (indexOf < 0)
                {
                    vertexMerged.Add(vertexArray[i]);
                    normalMerged.Add(normalsArray[i]);
                    uvMerged.Add(uvsArray[i]);
                    indexMerged.Add(vertexMerged.Count - 1);
                }
                else
                {
                    indexMerged.Add(indexOf);
                }
            }

            m.SetVertices(vertexMerged);
            m.SetNormals(normalMerged);
            m.SetUVs(0, uvMerged);
            m.SetTriangles(indexMerged, 0);
        }

        internal NativeArray<bool> GetVertexMarker()
        {
            NativeArray<bool> markers = new NativeArray<bool>(vertexMarkerNativeArray, Allocator.TempJob);
            return markers;
        }

        internal bool[] cachedMarker0;
        internal void CacheMarker0()
        {
            cachedMarker0 = vertexMarkerNativeArray.ToArray();
        }

        internal NativeArray<bool> GetMarker0()
        {
            return new NativeArray<bool>(cachedMarker0, Allocator.TempJob);
        }

        internal int GetGenerationMetadata(int channel)
        {
            return generationMetadata[channel];
        }
    }
}

#endif