#if VISTA
using UnityEngine;

namespace Pinwheel.Vista
{
    public static class BiomeExtensions
    {
        public static void MarkChanged(this IBiome b)
        {
            b.updateCounter = System.DateTime.Now.Ticks;
        }

        public static void GenerateBiomesInGroup(this IBiome b)
        {
            if (b is MonoBehaviour mb)
            {
                VistaManager manager = mb.GetComponentInParent<VistaManager>();
                if (manager != null)
                {
                    manager.GenerateAll();
                }
            }
        }

        public static VistaManager GetVistaManagerInstance(this IBiome b)
        {
            VistaManager manager = null;
            if (b is MonoBehaviour mb)
            {
                manager = mb.GetComponentInParent<VistaManager>();
            }
            return manager;
        }
    }
}
#endif
