#if POMINI
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.PolarisMini
{
    /// <summary>
    /// Responsible for extracting neccessary data from a terrain, that will be fed to the conversion pipeline later
    /// </summary>
    public static class TerrainDataExtractor
    {
        /// <summary>
        /// Extract the terrain height data to a RFloat read-write enabled texture
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Texture2D ExtractRFloatHeightMap(TerrainData data)
        {
            int heightmapResolution = data.heightmapResolution;
            float[,] terrainHeights = data.GetHeights(0, 0, heightmapResolution, heightmapResolution);
            Texture2D heightMap = new Texture2D(heightmapResolution, heightmapResolution, TextureFormat.RFloat, false, true);
            heightMap.wrapMode = TextureWrapMode.Clamp;

            Color[] colorArrays = new Color[heightmapResolution * heightmapResolution];
            for (int y = 0; y < heightmapResolution; y++)
            {
                for (int x = 0; x < heightmapResolution; x++)
                {
                    Color c = new Color(terrainHeights[y, x], 0, 0, 0);
                    colorArrays[Utilities.To1DIndex(x, y, heightmapResolution)] = c;
                }
            }
            heightMap.SetPixels(colorArrays);
            heightMap.Apply();

            return heightMap;
        }

        public static Texture2D ExtractRFloatHoleMap(TerrainData data)
        {
            int resolution = data.holesResolution;
            bool[,] holes = data.GetHoles(0, 0, resolution, resolution);
            Texture2D heightMap = new Texture2D(resolution, resolution, TextureFormat.RFloat, true, true);
            heightMap.wrapMode = TextureWrapMode.Clamp;

            Color[] colorArrays = new Color[resolution * resolution];
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    Color c = new Color(holes[y, x] ? 0 : 1, 0, 0, 0);
                    colorArrays[Utilities.To1DIndex(x, y, resolution)] = c;
                }
            }
            heightMap.SetPixels(colorArrays);
            heightMap.Apply();

            return heightMap;
        }

        public static Texture2D[] ExtractAlphaMaps(TerrainData data, int resolution)
        {
            Texture2D[] srcAlphaMaps = data.alphamapTextures;
            Texture2D[] alphaMaps = new Texture2D[srcAlphaMaps.Length];
            for (int i = 0; i < srcAlphaMaps.Length; ++i)
            {
                Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false, true);
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;
                tex.name = $"Splat Alpha {i}";

                CopyRGBA32(srcAlphaMaps[i], tex);
                alphaMaps[i] = tex;
            }

            return alphaMaps;
        }

        public static Texture2D[] ExtractAlbedoMetallicMap(TerrainData data, int resolution)
        {
            int alphaMapResolution = data.alphamapResolution;
            float[,,] weights = data.GetAlphamaps(0, 0, alphaMapResolution, alphaMapResolution);

            TerrainLayer[] layers = data.terrainLayers;
            Color[] layerColors = new Color[layers.Length];
            Color[] layerMS = new Color[layers.Length];
            for (int i = 0; i < layers.Length; ++i)
            {
                layerColors[i] = ReadLayerColors(layers[i]);
                layerMS[i] = new Color(layers[i].metallic, 0, 0, layers[i].smoothness);
            }

            Texture2D albedoMap = new Texture2D(alphaMapResolution, alphaMapResolution, TextureFormat.RGBA32, true, false);
            Texture2D metallicMap = new Texture2D(alphaMapResolution, alphaMapResolution, TextureFormat.RGBA32, true, false);

            for (int y = 0; y < alphaMapResolution; ++y)
            {
                for (int x = 0; x < alphaMapResolution; ++x)
                {
                    Color c = Color.black;
                    Color ms = Color.black;
                    for (int l = 0; l < layers.Length; ++l)
                    {
                        float w = weights[y, x, l];
                        c += w * layerColors[l];
                        ms += w * layerMS[l];
                    }
                    albedoMap.SetPixel(x, y, c);
                    metallicMap.SetPixel(x, y, ms);
                }
            }

            albedoMap.Apply();
            metallicMap.Apply();

            if (resolution == alphaMapResolution)
            {
                albedoMap.name = "Albedo Map";
                metallicMap.name = "Metallic Map";
                return new Texture2D[] { albedoMap, metallicMap };
            }
            else
            {
                Texture2D scaledAlbedoMap = new Texture2D(resolution, resolution, TextureFormat.RGBA32, true, false);
                Texture2D scaledMetallicMap = new Texture2D(resolution, resolution, TextureFormat.RGBA32, true, false);
                CopyRGBA32(albedoMap, scaledAlbedoMap);
                CopyRGBA32(metallicMap, scaledAlbedoMap);
                Object.DestroyImmediate(albedoMap);
                Object.DestroyImmediate(metallicMap);

                scaledAlbedoMap.name = "Albedo Map";
                scaledMetallicMap.name = "Metallic Map";
                return new Texture2D[] { scaledAlbedoMap, scaledMetallicMap };
            }
        }

        private static Color ReadLayerColors(TerrainLayer layers)
        {
            Texture2D clonedDiffuseMap = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
            if (layers.diffuseTexture != null)
            {
                CopyRGBA32(layers.diffuseTexture, clonedDiffuseMap);
            }
            else
            {
                CopyRGBA32(Texture2D.blackTexture, clonedDiffuseMap);
            }
            Color c = clonedDiffuseMap.GetPixelBilinear(0.5f, 0.5f);
            Object.DestroyImmediate(clonedDiffuseMap);
            return c;
        }

        private static void CopyRGBA32(Texture2D src, Texture2D dest)
        {
            RenderTexture tmpRT = RenderTexture.GetTemporary(dest.width, dest.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(src, tmpRT);
            RenderTexture.active = tmpRT;
            dest.ReadPixels(new Rect(0, 0, dest.width, dest.height), 0, 0);
            dest.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(tmpRT);
        }
    }
}

#endif