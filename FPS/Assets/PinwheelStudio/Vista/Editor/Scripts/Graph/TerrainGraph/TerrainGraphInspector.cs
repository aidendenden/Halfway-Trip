#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [CustomEditor(typeof(TerrainGraph))]
    public class TerrainGraphInspector : Editor
    {
        private TerrainGraph instance;

        private void OnEnable()
        {
            instance = target as TerrainGraph;
        }

        private class BaseGUI
        {
            public static readonly GUIContent OPEN_EDITOR = new GUIContent("Open Editor");
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button(BaseGUI.OPEN_EDITOR))
            {
                TerrainGraphEditor editor = TerrainGraphEditor.OpenGraph<TerrainGraphEditor>(instance);
            }
        }
    }
}
#endif
