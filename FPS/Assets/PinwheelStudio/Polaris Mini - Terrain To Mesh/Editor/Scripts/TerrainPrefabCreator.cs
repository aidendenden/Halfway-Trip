#if POMINI
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using Pinwheel.PolarisMini;

namespace Pinwheel.PolarisMiniEditor
{
    public static class MeshTerrainPrefabCreator
    {
        public static GameObject Create(ConversionResult convResult, string directory, string prefabName, Material materialTemplate, TerrainLayer[] terrainLayers, MaterialBinder matBinder)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            GameObject go = new GameObject(prefabName);
            Material material = Object.Instantiate(materialTemplate);
            material.name = "Terrain Material";

            List<Texture2D> clonedTextures = new List<Texture2D>();
            if (convResult.Options.ExportAlphaMaps)
            {
                Texture2D[] srcAlphaMaps = convResult.AlphaMaps;
                Texture2D[] clonedAlphaMaps = new Texture2D[srcAlphaMaps.Length];
                for (int i = 0; i < srcAlphaMaps.Length; ++i)
                {
                    Texture2D clonedTex = Object.Instantiate(srcAlphaMaps[i]);
                    clonedAlphaMaps[i] = clonedTex;
                    clonedTextures.Add(clonedTex);
                }

                matBinder.Bind(material, clonedAlphaMaps, terrainLayers, convResult.Options.Size);
            }

            if (convResult.Options.ExportAlbedoMetallicMap)
            {
                Texture2D clonedAlbedoMap = Object.Instantiate(convResult.AlbedoMap);
                Texture2D clonedMetallicMap = Object.Instantiate(convResult.MetallicMap);
                clonedTextures.Add(clonedAlbedoMap);
                clonedTextures.Add(clonedMetallicMap);

                matBinder.Bind(material, clonedAlbedoMap, clonedMetallicMap);
            }

            ConversionOptions convOptions = convResult.Options;
            float transitionStep = 1.0f / convOptions.LODCount;
            List<Mesh> clonedMeshes = new List<Mesh>();

            for (int x = 0; x < convOptions.ChunkGridSize; ++x)
            {
                for (int z = 0; z < convOptions.ChunkGridSize; ++z)
                {
                    GameObject chunkLOD0 = new GameObject();
                    chunkLOD0.name = $"C({x},{z})";
                    chunkLOD0.transform.parent = go.transform;

                    Mesh meshLOD0 = GameObject.Instantiate<Mesh>(convResult.GetMesh(x, z, 0));
                    clonedMeshes.Add(meshLOD0);

                    MeshFilter mf0 = chunkLOD0.AddComponent<MeshFilter>();
                    mf0.sharedMesh = meshLOD0;

                    MeshRenderer mr0 = chunkLOD0.AddComponent<MeshRenderer>();
                    mr0.sharedMaterial = material;

                    MeshCollider mc0 = chunkLOD0.AddComponent<MeshCollider>();
                    mc0.sharedMesh = meshLOD0;

                    LODGroup lodGroup = chunkLOD0.AddComponent<LODGroup>();
                    LOD[] lods = new LOD[convOptions.LODCount];

                    lods[0] = new LOD(
                    1 - transitionStep,
                    new Renderer[] { mr0 });

                    for (int level = 1; level < convOptions.LODCount; level++)
                    {
                        GameObject chunkLODi = new GameObject();

                        chunkLODi.name = $"C({x},{z})_LOD{level}";
                        chunkLODi.transform.parent = chunkLOD0.transform;

                        Mesh meshLODi = GameObject.Instantiate<Mesh>(convResult.GetMesh(x, z, level));
                        clonedMeshes.Add(meshLODi);

                        MeshFilter mfi = chunkLODi.AddComponent<MeshFilter>();
                        mfi.sharedMesh = meshLODi;

                        MeshRenderer mri = chunkLODi.AddComponent<MeshRenderer>();
                        mri.material = material;

                        lods[level] = new LOD(
                        1 - (level + 1) * transitionStep,
                        new Renderer[] { mri });
                    }

                    lodGroup.SetLODs(lods);
                    lodGroup.RecalculateBounds();
                }
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(go, Path.Combine(directory, $"{prefabName}.prefab"), InteractionMode.AutomatedAction);

            AssetDatabase.AddObjectToAsset(material, prefab);
            for (int i = 0; i < clonedTextures.Count; ++i)
            {
                Texture2D tex = clonedTextures[i];
                if (tex != null)
                {
                    tex.name = tex.name.Replace("(Clone)", "");
                    AssetDatabase.AddObjectToAsset(tex, prefab);
                }
            }

            for (int i = 0; i < clonedMeshes.Count; ++i)
            {
                Mesh m = clonedMeshes[i];
                if (m != null)
                {
                    m.name = m.name.Replace("(Clone)", "");
                    AssetDatabase.AddObjectToAsset(clonedMeshes[i], prefab);
                }
            }

            PrefabUtility.ApplyPrefabInstance(go, InteractionMode.AutomatedAction);
            Object.DestroyImmediate(go);

            return prefab;
        }
    }
} 
#endif