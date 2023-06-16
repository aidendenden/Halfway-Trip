#if VISTA
using UnityEngine;
using Pinwheel.Vista.Graph;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System;

namespace Pinwheel.VistaEditor
{
    //[CreateAssetMenu(menuName = "Vista/Internal/Editor Settings")]
    public class EditorSettings : ScriptableObject
    {
        [System.Serializable]
        public class GraphEditorSettings
        {
            public enum TextureDisplayMode
            {
                Height,
                Mask
            }

            public const int MIN_VIS_QUALITY = 1;
            public const int MAX_VIS_QUALITY = 10;

            [SerializeField]
            private int m_terrainVisualizationQuality;
            public int terrainVisualizationQuality
            {
                get
                {
                    return m_terrainVisualizationQuality;
                }
                set
                {
                    m_terrainVisualizationQuality = Mathf.Clamp(value, MIN_VIS_QUALITY, MAX_VIS_QUALITY);
                }
            }

            [SerializeField]
            private bool m_showWaterLevel;
            public bool showWaterLevel
            {
                get
                {
                    return m_showWaterLevel;
                }
                set
                {
                    m_showWaterLevel = value;
                }
            }

            [SerializeField]
            private float m_waterLevel;
            public float waterLevel
            {
                get
                {
                    return m_waterLevel;
                }
                set
                {
                    m_waterLevel = value;
                }
            }

            [SerializeField]
            private bool m_showGridline;
            public bool showGrid
            {
                get
                {
                    return m_showGridline;
                }
                set
                {
                    m_showGridline = value;
                }
            }

            [SerializeField]
            private TextureDisplayMode m_defaultTextureDisplayMode;
            public TextureDisplayMode defaultTextureDisplayMode
            {
                get
                {
                    return m_defaultTextureDisplayMode;
                }
                set
                {
                    m_defaultTextureDisplayMode = value;
                }
            }

            public enum ViewportGradientOptions
            {
                BlackRed, WhiteRed, BlackWhite, BlackRedYellowGreen, Rainbow, Custom
            }

            [SerializeField]
            private ViewportGradientOptions m_viewportGradient;
            public ViewportGradientOptions viewportGradient
            {
                get
                {
                    return m_viewportGradient;
                }
                set
                {
                    m_viewportGradient = value;
                }
            }

            [SerializeField]
            private Texture2D m_blackRedGradient;
            [SerializeField]
            private Texture2D m_whiteRedGradient;
            [SerializeField]
            private Texture2D m_blackWhiteGradient;
            [SerializeField]
            private Texture2D m_blackRedYellowGreenGradient;
            [SerializeField]
            private Texture2D m_rainbowGradient;

            [SerializeField]
            private Texture2D m_customViewportGradient;
            public Texture2D customViewportGradient
            {
                get
                {
                    return m_customViewportGradient;
                }
                set
                {
                    m_customViewportGradient = value;
                }
            }

            public Texture2D GetViewportGradient()
            {
                if (viewportGradient == ViewportGradientOptions.BlackRed)
                    return m_blackRedGradient;
                else if (viewportGradient == ViewportGradientOptions.WhiteRed)
                    return m_whiteRedGradient;
                else if (viewportGradient == ViewportGradientOptions.BlackWhite)
                    return m_blackWhiteGradient;
                else if (viewportGradient == ViewportGradientOptions.BlackRedYellowGreen)
                    return m_blackRedYellowGreenGradient;
                else if (viewportGradient == ViewportGradientOptions.Rainbow)
                    return m_rainbowGradient;
                else if (viewportGradient == ViewportGradientOptions.Custom)
                    return m_customViewportGradient;
                return null;
            }



            public GraphEditorSettings()
            {
                m_terrainVisualizationQuality = 5;
                m_showWaterLevel = false;
                m_waterLevel = 0;
                m_showGridline = true;
                m_defaultTextureDisplayMode = TextureDisplayMode.Height;
            }
        }

        [System.Serializable]
        public class GeneralSettings
        {
            [SerializeField]
            private bool m_enableAffLinks;
            public bool enableAffLinks
            {
                get
                {
                    return m_enableAffLinks;
                }
                set
                {
                    m_enableAffLinks = value;
                }
            }

            public GeneralSettings()
            {
                m_enableAffLinks = true;
            }
        }

        [System.Serializable]
        public class TroubleshootingSettings
        {
            [SerializeField]
            private bool m_dontExecuteGraphOnSelection;
            public bool dontExecuteGraphOnSelection
            {
                get
                {
                    return m_dontExecuteGraphOnSelection;
                }
                set
                {
                    m_dontExecuteGraphOnSelection = value;
                }
            }

            [SerializeField]
            private bool m_enableTroubleshootingMode;
            public bool enableTroubleshootingMode
            {
                get
                {
                    return m_enableTroubleshootingMode;
                }
                set
                {
                    m_enableTroubleshootingMode = value;
                }
            }

            private const string GRAPH_EXEC_LOG_FILE_NAME = "GraphExecLog";

            public TroubleshootingSettings()
            {
                m_enableTroubleshootingMode = false;
                m_dontExecuteGraphOnSelection = false;
            }

            internal static string GetExecLogFilePath(TerrainGraph graph)
            {
                string tmpFolder = GetTempFolderPath();
                string logFilePath = Path.Combine(tmpFolder, $".{GRAPH_EXEC_LOG_FILE_NAME}_{graph.name}.txt");
                return logFilePath;
            }

            internal static string[] GetGraphExecLog(TerrainGraph graph)
            {
                string logFilePath = GetExecLogFilePath(graph);
                if (File.Exists(logFilePath))
                {
                    return File.ReadAllLines(logFilePath);
                }
                else
                {
                    return new string[0];
                }
            }

            public static bool IsTroubleshootingModeEnabled()
            {
                EditorSettings editorSettings = EditorSettings.Get();
                return editorSettings.troubleshootingSettings.enableTroubleshootingMode;
            }
        }

        private static EditorSettings s_instance;

        [SerializeField]
        private GeneralSettings m_generalSettings;
        public GeneralSettings generalSettings
        {
            get
            {
                return m_generalSettings;
            }
            set
            {
                m_generalSettings = value;
            }
        }

        [SerializeField]
        private GraphEditorSettings m_graphEditorSettings;
        public GraphEditorSettings graphEditorSettings
        {
            get
            {
                return m_graphEditorSettings;
            }
            set
            {
                m_graphEditorSettings = value;
            }
        }

        [SerializeField]
        private TroubleshootingSettings m_troubleshootingSettings;
        public TroubleshootingSettings troubleshootingSettings
        {
            get
            {
                return m_troubleshootingSettings;
            }
            set
            {
                m_troubleshootingSettings = value;
            }
        }

        public void Reset()
        {
            m_generalSettings = new GeneralSettings();
            m_graphEditorSettings = new GraphEditorSettings();
            m_troubleshootingSettings = new TroubleshootingSettings();
        }

        public static EditorSettings Get()
        {
            if (s_instance == null)
            {
                s_instance = Resources.Load<EditorSettings>("Vista/EditorSettings");
            }
            if (s_instance == null)
            {
                s_instance = ScriptableObject.CreateInstance<EditorSettings>();
                Debug.LogWarning("VISTA: Editor Settings asset does not exist. Please re-import the package.");
            }
            return s_instance;
        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {
        }

        public static string GetTempFolderPath()
        {
            EditorSettings editorSettingsAsset = EditorSettings.Get();
            string editorSettingsPath = AssetDatabase.GetAssetPath(editorSettingsAsset);
            string tempDirectory = Path.Combine(Path.GetDirectoryName(editorSettingsPath), "VistaTemp_CanIgnoreInVersionControl");
            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }
            return tempDirectory;
        }



    }
}
#endif
