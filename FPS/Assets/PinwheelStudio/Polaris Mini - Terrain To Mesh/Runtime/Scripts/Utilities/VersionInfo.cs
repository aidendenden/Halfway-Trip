#if POMINI

namespace Pinwheel.PolarisMini
{
    /// <summary>
    /// Utility class contains product info
    /// </summary>
    public static class VersionInfo
    {
        public static float Number
        {
            get
            {
                return 2022.1f;
            }
        }

        public static string Code
        {
            get
            {
                return "2022.1.1";
            }
        }

        public static string ProductName
        {
            get
            {
                return "Polaris Mini - Terrain To Mesh";
            }
        }

        public static string ProductNameAndVersion
        {
            get
            {
                return string.Format("{0} {1}", ProductName, Code);
            }
        }

        public static string ProductNameShort
        {
            get
            {
                return "Polaris Mini";
            }
        }

        public static string ProductNameAndVersionShort
        {
            get
            {
                return string.Format("{0} {1}", ProductNameShort, Code);
            }
        }
    }
}

#endif