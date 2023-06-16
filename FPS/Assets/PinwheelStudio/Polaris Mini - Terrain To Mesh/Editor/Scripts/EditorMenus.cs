#if POMINI
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Pinwheel.PolarisMini;

namespace Pinwheel.PolarisMiniEditor
{
    public static class EditorMenus
    {
        [MenuItem("Window/Polaris Mini/Terrain To Mesh")]
        public static void OpenConverterWindow()
        {
            TerrainToMeshEditor editor = EditorWindow.GetWindow<TerrainToMeshEditor>();
            editor.titleContent = new GUIContent(VersionInfo.ProductNameAndVersionShort);
            editor.Show();
        }

        [MenuItem("Window/Polaris Mini/User Guide")]
        public static void OpenUserGuide()
        {
            TextAsset manual = Resources.Load<TextAsset>("PolarisMiniManual");
            if (manual != null)
            {
                AssetDatabase.OpenAsset(manual);
            }
        }

        [MenuItem("Window/Polaris Mini/Support")]
        public static void OpenEmailEditor()
        {
            EditorCommon.OpenEmailEditor(
                Common.SUPPORT_EMAIL,
                "[Polaris Mini] SHORT_QUESTION_HERE",
                "YOUR_QUESTION_IN_DETAIL");
        }

        [MenuItem("Window/Polaris Mini/Version Info")]
        public static void ShowVersionInfo()
        {
            EditorUtility.DisplayDialog(
                "Version Info",
                VersionInfo.ProductNameAndVersion,
                "OK");
        }

        [MenuItem("Window/Polaris Mini/Write a Review")]
        public static void OpenStorePage()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/slug/227310");
        }

        [MenuItem("Window/Polaris Mini/Explore/Vista - Procedural Terrain Generator")]
        public static void ShowAsset_Vista()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/terrain/vista-advanced-terrain-graph-editor-210496?aid=1100l3QbW&pubref=pomini-editor");
        }

        [MenuItem("Window/Polaris Mini/Explore/Polaris - Low Poly & Mesh Terrain Editor")]
        public static void ShowAsset_Polaris()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/terrain/low-poly-terrain-polaris-2021-196648?aid=1100l3QbW&pubref=pomini-editor");
        }

        [MenuItem("Window/Polaris Mini/Explore/Poseidon - Low Poly Water System")]
        public static void ShowAsset_Poseidon()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/vfx/shaders/substances/poseidon-low-poly-water-system-builtin-lwrp-153826?aid=1100l3QbW&pubref=pomini-editor");
        }

        [MenuItem("Window/Polaris Mini/Explore/Jupiter - Procedural Sky")]
        public static void ShowAsset_Jupiter()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/2d/textures-materials/sky/procedural-sky-builtin-lwrp-urp-jupiter-159992?aid=1100l3QbW&pubref=pomini-editor");
        }

        [MenuItem("Window/Polaris Mini/Explore/Texture Graph - Node Based Texture Generator")]
        public static void ShowAsset_TextureGraph()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/slug/185542?aid=1100l3QbW&pubref=pomini-editor");
        }
    }
}

#endif