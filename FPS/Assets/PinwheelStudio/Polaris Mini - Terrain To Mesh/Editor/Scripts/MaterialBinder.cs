#if POMINI
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.PolarisMiniEditor
{
    [System.Serializable]
    public class MaterialBinder
    {
        public string splatControlName = "_Control";
        public string splatDiffuseName = "_Splat";
        public string splatTilingName = "_SplatTiling";
        public string splatOffsetName = "_SplatOffset";
        public string splatMetallicName = "_Metallic";
        public string splatSmoothnessName = "_Smoothness";

        public string albedoName = "_MainTex";
        public string metallicName = "_MetallicGlossMap";

        public static MaterialBinder Get(RenderPipelineType rpType)
        {
            MaterialBinder mbi = new MaterialBinder();
            if (rpType == RenderPipelineType.Builtin)
            {

            }
            else if (rpType == RenderPipelineType.Universal)
            {
                mbi.albedoName = "_BaseMap";
            }
            return mbi;
        }

        public void Bind(Material mat, Texture2D[] alphaMaps, TerrainLayer[] layers, Vector3 terrainSize)
        {
            string prop = null;
            if (alphaMaps != null)
            {
                for (int i = 0; i < alphaMaps.Length; ++i)
                {
                    prop = $"{splatControlName}{i}";
                    if (mat.HasProperty(prop))
                    {
                        mat.SetTexture(prop, alphaMaps[i] ?? Texture2D.blackTexture);
                    }
                }
            }

            if (layers != null)
            {
                for (int i = 0; i < layers.Length; ++i)
                {
                    TerrainLayer l = layers[i];
                    if (l == null)
                        continue;

                    prop = $"{splatDiffuseName}{i}";
                    if (mat.HasProperty(prop))
                    {
                        mat.SetTexture(prop, l.diffuseTexture ?? Texture2D.blackTexture);

                        Vector2 textureScale = new Vector2(
                            l.tileSize.x != 0 ? terrainSize.x / l.tileSize.x : 0,
                            l.tileSize.y != 0 ? terrainSize.y / l.tileSize.y : 0);
                        Vector2 textureOffset = new Vector2(
                            l.tileOffset.x != 0 ? terrainSize.x / l.tileOffset.x : 0,
                            l.tileOffset.y != 0 ? terrainSize.y / l.tileOffset.y : 0);

                        prop = $"{splatTilingName}{i}";
                        if (mat.HasProperty(prop))
                        {
                            mat.SetVector(prop, textureScale);
                        }

                        prop = $"{splatOffsetName}{i}";
                        if (mat.HasProperty(prop))
                        {
                            mat.SetVector(prop, textureOffset);
                        }
                    }

                    prop = $"{splatMetallicName}{i}";
                    if (mat.HasProperty(prop))
                    {
                        mat.SetFloat(prop, l.metallic);
                    }

                    prop = $"{splatSmoothnessName}{i}";
                    if (mat.HasProperty(prop))
                    {
                        mat.SetFloat(prop, l.smoothness);
                    }
                }
            }
        }

        public void Bind(Material mat, Texture2D albedoMap, Texture2D metallicMap)
        {
            if (mat.HasProperty(albedoName))
            {
                mat.SetTexture(albedoName, albedoMap ?? Texture2D.blackTexture);
            }
            if (mat.HasProperty(metallicName))
            {
                mat.SetTexture(metallicName, metallicMap ?? Texture2D.blackTexture);
            }
        }
    }
}
#endif