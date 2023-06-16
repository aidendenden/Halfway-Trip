#if POMINI
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.PolarisMini
{
    public class ConversionResources : System.IDisposable
    {
        public Texture2D HeightMap { get; set; }
        public Texture2D HoleMap { get; set; }

        private int heightMapResolution;
        public int HeightMapResolution
        {
            get
            {
                return heightMapResolution;
            }
            set
            {
                heightMapResolution = value;
            }
        }

        internal Texture2D subDivisionMap;
        public Texture2D Internal_SubDivisionMap
        {
            get
            {
                if (subDivisionMap == null)
                {
                    Internal_CreateNewSubDivisionMap();
                }
                return subDivisionMap;
            }
        }

        internal void Internal_CreateNewSubDivisionMap()
        {
            if (subDivisionMap != null)
            {
                if (subDivisionMap.width != Common.SUB_DIV_MAP_RESOLUTION ||
                    subDivisionMap.height != Common.SUB_DIV_MAP_RESOLUTION)
                    Object.DestroyImmediate(subDivisionMap);
            }

            if (subDivisionMap == null)
            {
                subDivisionMap = new Texture2D(Common.SUB_DIV_MAP_RESOLUTION, Common.SUB_DIV_MAP_RESOLUTION, TextureFormat.RGBA32, false);
            }

            int resolution = Common.SUB_DIV_MAP_RESOLUTION;
            RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RFloat);
            Material mat = new Material(Shader.Find("Hidden/PolarisMini/SubDivisionMap"));
            mat.SetTexture("_HeightMap", HeightMap);
            mat.SetTexture("_HoleMap", HoleMap);

            Common.DrawQuad(rt, Common.FullRectUvPoints, mat, 0);
            Common.CopyFromRT(subDivisionMap, rt);
            rt.Release();
            Object.DestroyImmediate(rt);
        }

        public void Dispose()
        {
            Object.DestroyImmediate(HeightMap);
            Object.DestroyImmediate(Internal_SubDivisionMap);
            Object.DestroyImmediate(HoleMap);
        }
    }

}

#endif