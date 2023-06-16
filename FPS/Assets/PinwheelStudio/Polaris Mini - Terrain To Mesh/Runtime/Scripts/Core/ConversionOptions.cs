#if POMINI
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.PolarisMini
{
    /// <summary>
    /// Contains all options for the conversion pipeline, including world size, mesh density, etc.
    /// </summary>
    [System.Serializable]
    public struct ConversionOptions
    {
        [SerializeField]
        internal float width;
        public float Width
        {
            get
            {
                return width;
            }
            set
            {
                width = Mathf.Max(1, value);
            }
        }

        [SerializeField]
        internal float height;
        public float Height
        {
            get
            {
                return height;
            }
            set
            {
                height = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        internal float length;
        public float Length
        {
            get
            {
                return length;
            }
            set
            {
                length = Mathf.Max(1, value);
            }
        }

        [SerializeField]
        private int meshBaseResolution;
        public int MeshBaseResolution
        {
            get
            {
                return meshBaseResolution;
            }
            set
            {
                meshBaseResolution = Mathf.Min(meshResolution, Mathf.Clamp(value, 0, Common.MAX_MESH_BASE_RESOLUTION));
            }
        }

        [SerializeField]
        private int meshResolution;
        public int MeshResolution
        {
            get
            {
                return meshResolution;
            }
            set
            {
                meshResolution = Mathf.Clamp(value, 0, Common.MAX_MESH_RESOLUTION);
            }
        }

        [SerializeField]
        private int chunkGridSize;
        public int ChunkGridSize
        {
            get
            {
                return chunkGridSize;
            }
            set
            {
                chunkGridSize = Mathf.Max(1, value);
            }
        }

        [SerializeField]
        private int lodCount;
        public int LODCount
        {
            get
            {
                return lodCount;
            }
            set
            {
                lodCount = Mathf.Clamp(value, 1, Common.MAX_LOD_COUNT);
            }
        }

        [SerializeField]
        private int displacementSeed;
        public int DisplacementSeed
        {
            get
            {
                return displacementSeed;
            }
            set
            {
                displacementSeed = value;
            }
        }

        [SerializeField]
        private float displacementStrength;
        public float DisplacementStrength
        {
            get
            {
                return displacementStrength;
            }
            set
            {
                displacementStrength = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private bool smoothNormal;
        public bool SmoothNormal
        {
            get
            {
                return smoothNormal;
            }
            set
            {
                smoothNormal = value;
            }
        }

        [SerializeField]
        private bool mergeUv;
        public bool MergeUv
        {
            get
            {
                return mergeUv;
            }
            set
            {
                mergeUv = value;
            }
        }

        public Vector3 Size
        {
            get
            {
                return new Vector3(Width, Height, Length);
            }
        }

        [SerializeField]
        private bool exportAlphaMaps;
        public bool ExportAlphaMaps
        {
            get
            {
                return exportAlphaMaps;
            }
            set
            {
                exportAlphaMaps = value;
            }
        }

        [SerializeField]
        private bool exportAlbedoMetallicMap;
        public bool ExportAlbedoMetallicMap
        {
            get
            {
                return exportAlbedoMetallicMap;
            }
            set
            {
                exportAlbedoMetallicMap = value;
            }
        }

        public static ConversionOptions Create()
        {
            ConversionOptions o = new ConversionOptions();
            o.Width = 1000;
            o.Height = 600;
            o.Length = 1000;
            o.MeshResolution = 12;
            o.MeshBaseResolution = 6;
            o.ChunkGridSize = 5;
            o.LODCount = 1;
            o.DisplacementSeed = 0;
            o.DisplacementStrength = 0;
            o.SmoothNormal = false;
            o.MergeUv = false;

            o.ExportAlphaMaps = true;
            o.ExportAlbedoMetallicMap = true;

            return o;
        }
    }
}

#endif