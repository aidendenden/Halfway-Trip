#if VISTA
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Compilation;

namespace Pinwheel.VistaEditor.Graph
{
    public class PropertiesView : VisualElement
    {
        private GraphEditorBase m_editor;

        private VisualElement m_tabContainer;
        private VisualElement m_bodyContainer;
        private Button m_nodePropertiesTab;
        private Button m_graphPropertiesTab;
        private Button m_exploreTab;

        private IMGUIContainer m_nodePropertiesView;
        private IMGUIContainer m_graphPropertiesView;
        private IMGUIContainer m_exploreView;

        public PropertiesView(GraphEditorBase editor) : base()
        {
            this.m_editor = editor;

            StyleSheet uss = Resources.Load<StyleSheet>("Vista/USS/Graph/PropertiesView");
            styleSheets.Add(uss);
            AddToClassList("properties-view");
            AddToClassList("panel");

            m_tabContainer = new VisualElement() { name = "tab-container" };
            this.Add(m_tabContainer);

            m_nodePropertiesTab = new Button() { text = "Node" };
            m_nodePropertiesTab.AddToClassList("tab");
            m_nodePropertiesTab.AddToClassList("active");
            m_nodePropertiesTab.clicked += OnNodePropertiesTabClicked;
            m_tabContainer.Add(m_nodePropertiesTab);

            m_graphPropertiesTab = new Button() { text = "Graph & Editor" };
            m_graphPropertiesTab.AddToClassList("tab");
            m_graphPropertiesTab.AddToClassList("active");
            m_graphPropertiesTab.clicked += OnGraphPropertiesTabClicked;
            m_tabContainer.Add(m_graphPropertiesTab);

            m_exploreTab = new Button() { text = "Explore" };
            m_exploreTab.AddToClassList("tab");
            m_exploreTab.AddToClassList("active");
            m_exploreTab.clicked += OnExploreTabClicked;
            m_tabContainer.Add(m_exploreTab);

            m_bodyContainer = new VisualElement() { name = "body" };
            this.Add(m_bodyContainer);

            ScrollView scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            scrollView.name = "scroll-view";
            m_bodyContainer.Add(scrollView);

            m_nodePropertiesView = new IMGUIContainer(OnNodePropertiesGUI);
            scrollView.Add(m_nodePropertiesView);

            m_graphPropertiesView = new IMGUIContainer(OnGraphPropertiesGUI);
            scrollView.Add(m_graphPropertiesView);

            m_exploreView = new IMGUIContainer(OnExploreGUI);
            scrollView.Add(m_exploreView);

            OnNodePropertiesTabClicked();
        }

        public void OnEnable()
        {

        }

        public void OnDisable()
        {

        }

        private void OnNodePropertiesTabClicked()
        {
            m_nodePropertiesTab.EnableInClassList("active", true);
            m_nodePropertiesView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);

            m_graphPropertiesTab.EnableInClassList("active", false);
            m_graphPropertiesView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            m_exploreTab.EnableInClassList("active", false);
            m_exploreView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        }

        private void OnGraphPropertiesTabClicked()
        {
            m_nodePropertiesTab.EnableInClassList("active", false);
            m_nodePropertiesView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            m_graphPropertiesTab.EnableInClassList("active", true);
            m_graphPropertiesView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);

            m_exploreTab.EnableInClassList("active", false);
            m_exploreView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        }

        private void OnExploreTabClicked()
        {
            m_nodePropertiesTab.EnableInClassList("active", false);
            m_nodePropertiesView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            m_graphPropertiesTab.EnableInClassList("active", false);
            m_graphPropertiesView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            m_exploreTab.EnableInClassList("active", true);
            m_exploreView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        }

        private void OnNodePropertiesGUI()
        {
            m_editor.OnDrawNodeProperties();
        }

        private void OnGraphPropertiesGUI()
        {
            m_editor.OnDrawGraphProperties();
        }

        private class ExploreGUI
        {

            public class AssetEntry
            {
                private readonly Texture IMAGE;
                private readonly string NAME;
                private readonly string DESCRIPTION;
                private readonly string LINK;

                private readonly bool INSTALLED;

                private readonly Color HIGHLIGHT_COLOR = new Color(1, 1, 1, 0.1F);
                private readonly Color FADE_COLOR = new Color(0, 0, 0, 0.6F);

                private static readonly Texture CHECKMARK = Resources.Load<Texture>("Vista/Textures/Checkmark");

                public AssetEntry(string imagePath, string name, string description, string link, bool installed = false)
                {
                    IMAGE = Resources.Load<Texture>(imagePath);
                    NAME = name + " →";
                    DESCRIPTION = description;
                    LINK = link;
                    INSTALLED = installed;
                }

                public void Draw()
                {
                    GUI.enabled = !INSTALLED;
                    Rect entryRect = EditorGUILayout.BeginVertical();
                    EditorGUIUtility.AddCursorRect(entryRect, MouseCursor.Link);
                    if (entryRect.Contains(Event.current.mousePosition))
                    {
                        EditorGUI.DrawRect(entryRect, HIGHLIGHT_COLOR);
                    }

                    EditorGUILayout.BeginHorizontal();
                    Rect imageRect = EditorGUILayout.GetControlRect(GUILayout.Width(64), GUILayout.Height(64));
                    imageRect = new RectOffset(4, 4, 4, 4).Remove(imageRect);
                    GUI.DrawTexture(imageRect, IMAGE ?? Texture2D.whiteTexture);
                    if (INSTALLED)
                    {
                        EditorGUI.DrawRect(imageRect, FADE_COLOR);
                    }

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField(NAME, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(DESCRIPTION, EditorCommon.Styles.grayMiniLabel);
                    if (INSTALLED)
                    {
                        EditorGUILayout.GetControlRect(GUILayout.Height(2));
                        Rect checkMarkRect = EditorGUILayout.GetControlRect(GUILayout.Height(12), GUILayout.Width(12));
                        GUI.DrawTexture(checkMarkRect, CHECKMARK);
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    GUI.enabled = true;

                    if (Event.current.type == EventType.MouseDown && entryRect.Contains(Event.current.mousePosition))
                    {
                        Application.OpenURL(LINK);
                    }
                }
            }

            public static readonly GUIContent COMPLETE_YOUR_COLLECTION = new GUIContent("COMPLETE YOUR VISTA COLLECTION");
            public static readonly AssetEntry CORE_MODULE = new AssetEntry(
                "Vista/Textures/Marketing/CoreIcon",
                "Vista",
                "Remember to leave a 5-stars review",
                Links.STORE_PAGE,
                true);

            public static readonly AssetEntry BIG_WORLD_MODULE = new AssetEntry(
                "Vista/Textures/Marketing/BigWorldIcon",
                "Big World Module",
                "Generate 4K maps, multi-biomes, advanced nodes to work with bigger world.",
                Links.BIG_WORLD,
                true);

            public static readonly AssetEntry PROBOOST_MODULE = new AssetEntry(
                "Vista/Textures/Marketing/ProboostIcon",
                "Productivity Boost Module",
                "Utilities to work faster, smarter and stay organized.",
                Links.PROBOOST,
                EditorCommon.HasAssembly("Pinwheel.Vista.Proboost.Runtime"));

            public static readonly AssetEntry MICRO_SPLAT_MODULE = new AssetEntry(
                "Vista/Textures/Marketing/MicroSplatIntegrationIcon",
                "MicroSplat Integration Module",
                "Setting up MS texture array and output terrain FX maps.",
                Links.MICROSPLAT_INTEGRATION,
                EditorCommon.HasAssembly("Pinwheel.Vista.MicroSplatIntegration.Runtime"));

            public static readonly AssetEntry HAND_PAINTING_MODULE = new AssetEntry(
                "Vista/Textures/Marketing/HandPaintingIcon",
                "Hand Painting Module",
                "Manual adjustment with brushes.",
                Links.HANDPAINTING,
                EditorCommon.HasAssembly("Pinwheel.Vista.HandPainting.Runtime"));

            public static readonly AssetEntry SPLINES_MODULE = new AssetEntry(
                "Vista/Textures/Marketing/SplinesIcon",
                "Splines Module",
                "Make ramps, mountain passes, paint paths, etc. with splines.",
                Links.SPLINES,
                EditorCommon.HasAssembly("Pinwheel.Vista.Splines.Runtime"));

            public static readonly GUIContent OTHER_PRODUCTS_FROM_PINWHEEL = new GUIContent("OTHER PRODUCTS FROM PINWHEEL STUDIO");
            public static readonly AssetEntry POLARIS = new AssetEntry(
                "Vista/Textures/Marketing/PolarisIcon",
                "Polaris",
                "Complete toolset for low poly terrain creation.",
                Links.POLARIS,
                EditorCommon.HasAssembly("Pinwheel.Polaris.Runtime"));

            public static readonly AssetEntry POSEIDON = new AssetEntry(
                 "Vista/Textures/Marketing/PoseidonIcon",
                 "Poseidon",
                 "Low poly water shader with high fidelity at high performance.",
                 Links.POSEIDON,
                 CheckPoseidonInstalled());

            private static bool CheckPoseidonInstalled()
            {
#if POSEIDON
                return true;
#else
                return false;
#endif
            }

            public static readonly AssetEntry JUPITER = new AssetEntry(
                 "Vista/Textures/Marketing/JupiterIcon",
                 "Jupiter",
                 "Animated skybox shader with day night cycle.",
                 Links.JUPITER,
                 CheckJupiterInstalled());

            private static bool CheckJupiterInstalled()
            {
#if JUPITER
                return true;
#else
                return false;
#endif
            }

            public static readonly AssetEntry TEXTURE_GRAPH = new AssetEntry(
                 "Vista/Textures/Marketing/TextureGraphIcon",
                 "Texture Graph",
                 "Graph based texture authoring tool.",
                 Links.TEXTURE_GRAPH,
                 CheckTextureGraphInstalled());

            private static bool CheckTextureGraphInstalled()
            {
#if TEXTURE_GRAPH
                return true;
#else
                return false;
#endif
            }

            public static readonly AssetEntry PINWHEEL_LIBRARY = new AssetEntry(
                 "Vista/Textures/Marketing/PinwheelIcon",
                 "Visit us on the Asset Store",
                 "Many other high quality assets to see.",
                 Links.TEXTURE_GRAPH,
                 false);
        }

        private void OnExploreGUI()
        {
            EditorCommon.Header(ExploreGUI.COMPLETE_YOUR_COLLECTION);
            ExploreGUI.CORE_MODULE.Draw();
            ExploreGUI.BIG_WORLD_MODULE.Draw();
            ExploreGUI.PROBOOST_MODULE.Draw();
            ExploreGUI.MICRO_SPLAT_MODULE.Draw();
            ExploreGUI.HAND_PAINTING_MODULE.Draw();
            ExploreGUI.SPLINES_MODULE.Draw();

            EditorCommon.Header(ExploreGUI.OTHER_PRODUCTS_FROM_PINWHEEL);
            ExploreGUI.POLARIS.Draw();
            ExploreGUI.POSEIDON.Draw();
            ExploreGUI.JUPITER.Draw();
            ExploreGUI.TEXTURE_GRAPH.Draw();
            ExploreGUI.PINWHEEL_LIBRARY.Draw();
        }
    }
}
#endif
