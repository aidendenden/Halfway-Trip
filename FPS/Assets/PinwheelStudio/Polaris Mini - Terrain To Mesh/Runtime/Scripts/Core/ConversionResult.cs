#if POMINI
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Pinwheel.PolarisMini
{
    /// <summary>
    /// Contains generated data of the conversion pipeline, including terrain meshes, textures, material, etc.
    /// </summary>
    public class ConversionResult : System.IDisposable
    {
        public ConversionOptions Options { get; internal set; }

        private Texture2D[] alphaMaps;
        public Texture2D[] AlphaMaps
        {
            get
            {
                if (alphaMaps == null)
                {
                    return new Texture2D[0];
                }
                else
                {
                    Texture2D[] textures = new Texture2D[alphaMaps.Length];
                    alphaMaps.CopyTo(textures, 0);
                    return textures;
                }
            }
        }

        private Texture2D albedoMap;
        public Texture2D AlbedoMap
        {
            get
            {
                return albedoMap;
            }
        }

        private Texture2D metallicMap;
        public Texture2D MetallicMap
        {
            get
            {
                return metallicMap;
            }
        }

        private Dictionary<Vector3Int, Mesh> meshes;

        public long ProcessingTimeMiliSec { get; internal set; }

        internal ConversionResult(ConversionOptions options)
        {
            Options = options;
            meshes = new Dictionary<Vector3Int, Mesh>();
        }

        internal void AddMesh(Mesh mesh, int indexX, int indexY, int lod)
        {
            meshes.Add(new Vector3Int(indexX, indexY, lod), mesh);
        }

        public Mesh GetMesh(int indexX, int indexY, int lod)
        {
            Mesh res;
            if (meshes.TryGetValue(new Vector3Int(indexX, indexY, lod), out res))
            {
                return res;
            }
            else
            {
                return null;
            }
        }

        public List<Mesh> GetMeshes()
        {
            List<Mesh> result = new List<Mesh>(meshes.Count);

            foreach (Mesh mesh in meshes.Values)
            {
                result.Add(mesh);
            }

            return result;
        }

        internal void SetAlphaMaps(Texture2D[] src)
        {
            alphaMaps = src;
        }

        internal void SetAlbedoMap(Texture2D albedo)
        {
            albedoMap = albedo;
        }

        internal void SetMetallicMap(Texture2D metallic)
        {
            metallicMap = metallic;
        }

        public void Dispose()
        {
            if (meshes != null)
            {
                foreach (Mesh mesh in meshes.Values)
                {
                    Object.DestroyImmediate(mesh, true);
                }
            }

            if (AlphaMaps != null)
            {
                foreach (Texture2D t in AlphaMaps)
                {

                }
            }
        }
    }
}

#endif