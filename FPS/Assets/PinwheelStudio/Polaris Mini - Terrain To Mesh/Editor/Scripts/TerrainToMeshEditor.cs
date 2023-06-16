#if POMINI
using UnityEngine;
using UnityEditor;
using Pinwheel.PolarisMini;
using System.Collections.Generic;


namespace Pinwheel.PolarisMiniEditor
{
    public class TerrainToMeshEditor : EditorWindow
    {
        [System.Serializable]
        public class EditorOptions
        {
            [SerializeField]
            public ConversionOptions conversionOptions;
            [SerializeField]
            public MaterialTemplateOptions materialTemplate;
            [SerializeField]
            public MaterialBinder customMatBinder;
            [SerializeField]
            public string directory;

            public EditorOptions()
            {
                conversionOptions = ConversionOptions.Create();
                materialTemplate = MaterialTemplateOptions.Splat4NoNormal;
                customMatBinder = new MaterialBinder();
                directory = "Assets/";
            }
        }

        private EditorOptions options;
        private Terrain terrain;
        private Material customMaterial;
        private long lastProcessingTime;

        private const string KEY_OPTIONS = "com.pinwheel.pomini.options";

        private void OnEnable()
        {
            options = new EditorOptions();
            if (EditorPrefs.HasKey(KEY_OPTIONS))
            {
                string json = EditorPrefs.GetString(KEY_OPTIONS);
                EditorJsonUtility.FromJsonOverwrite(json, options);
            }
        }

        private void OnDisable()
        {
            string json = EditorJsonUtility.ToJson(options);
            EditorPrefs.SetString(KEY_OPTIONS, json);
        }

        void OnGUI()
        {
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200;
            EditorGUI.indentLevel += 1;

            ConversionOptions conversionOptions = options.conversionOptions;

            EditorCommon.Header("TERRAIN");
            EditorGUI.BeginChangeCheck();
            terrain = EditorGUILayout.ObjectField(terrain, typeof(Terrain), true, GUILayout.Height(32)) as Terrain;
            if (EditorGUI.EndChangeCheck() && terrain != null && terrain.terrainData != null)
            {
                conversionOptions.Width = terrain.terrainData.size.x;
                conversionOptions.Height = terrain.terrainData.size.y;
                conversionOptions.Length = terrain.terrainData.size.z;
            }

            if (terrain != null && terrain.terrainData != null)
            {
                EditorCommon.Header("MESH CREATION");
                conversionOptions.Width = EditorGUILayout.DelayedFloatField("Width", conversionOptions.Width);
                conversionOptions.Height = EditorGUILayout.DelayedFloatField("Height", conversionOptions.Height);
                conversionOptions.Length = EditorGUILayout.DelayedFloatField("Length", conversionOptions.Length);
                conversionOptions.MeshBaseResolution = EditorGUILayout.DelayedIntField("Mesh Base Resolution", conversionOptions.MeshBaseResolution);
                conversionOptions.MeshResolution = EditorGUILayout.DelayedIntField("Mesh Resolution", conversionOptions.MeshResolution);
                conversionOptions.ChunkGridSize = EditorGUILayout.DelayedIntField("Chunk Grid Size", conversionOptions.ChunkGridSize);
                conversionOptions.LODCount = EditorGUILayout.DelayedIntField("LOD Count", conversionOptions.LODCount);
                conversionOptions.DisplacementSeed = EditorGUILayout.DelayedIntField("Displacement Seed", conversionOptions.DisplacementSeed);
                conversionOptions.DisplacementStrength = EditorGUILayout.DelayedFloatField("Displacement Strength", conversionOptions.DisplacementStrength);
                conversionOptions.SmoothNormal = EditorGUILayout.Toggle("Smooth Normal", conversionOptions.SmoothNormal);
                conversionOptions.MergeUv = EditorGUILayout.Toggle("Merge UV", conversionOptions.MergeUv);

                EditorCommon.Header("MATERIAL & TEXTURES");
                RenderPipelineType rpType = EditorCommon.GetCurrentRP();
                EditorGUILayout.LabelField("Render Pipeline", rpType.ToString());
                if (rpType == RenderPipelineType.Other)
                {
                    options.materialTemplate = MaterialTemplateOptions.Custom;
                }

                options.materialTemplate = (MaterialTemplateOptions)EditorGUILayout.EnumPopup("Material Template", options.materialTemplate);
                if (options.materialTemplate == MaterialTemplateOptions.Custom)
                {
                    customMaterial = EditorGUILayout.ObjectField("Material", customMaterial, typeof(Material), false) as Material;

                    conversionOptions.ExportAlphaMaps = EditorGUILayout.Toggle("Export Alpha Maps", conversionOptions.ExportAlphaMaps);
                    conversionOptions.ExportAlbedoMetallicMap = EditorGUILayout.Toggle("Export Albedo Metallic Map", conversionOptions.ExportAlbedoMetallicMap);

                    MaterialBinder binder = options.customMatBinder;
                    if (conversionOptions.ExportAlphaMaps)
                    {
                        binder.splatControlName = EditorGUILayout.TextField("Splat Control Name", binder.splatControlName);
                        binder.splatDiffuseName = EditorGUILayout.TextField("Splat Diffuse Name", binder.splatDiffuseName);
                        binder.splatMetallicName = EditorGUILayout.TextField("Splat Metallic Name", binder.splatMetallicName);
                        binder.splatSmoothnessName = EditorGUILayout.TextField("Splat Smoothness Name", binder.splatSmoothnessName);
                    }
                    if (conversionOptions.ExportAlbedoMetallicMap)
                    {
                        binder.albedoName = EditorGUILayout.TextField("Albedo Name", binder.albedoName);
                        binder.metallicName = EditorGUILayout.TextField("Metallic Name", binder.metallicName);
                    }
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(" ");
                    if (GUILayout.Button("Use Default Property Name"))
                    {
                        options.customMatBinder = MaterialBinder.Get(rpType);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else if (options.materialTemplate == MaterialTemplateOptions.Splat4NoNormal)
                {
                    conversionOptions.ExportAlphaMaps = true;
                    conversionOptions.ExportAlbedoMetallicMap = false;
                }
                else if (options.materialTemplate == MaterialTemplateOptions.ColorMap)
                {
                    conversionOptions.ExportAlphaMaps = false;
                    conversionOptions.ExportAlbedoMetallicMap = true;
                }
                options.conversionOptions = conversionOptions;

                EditorCommon.Header("FILES");
                EditorGUILayout.BeginHorizontal();
                options.directory = EditorGUILayout.TextField("Directory", options.directory);
                if (GUILayout.Button("Browse", GUILayout.Width(100)))
                {
                    string dir = FileUtil.GetProjectRelativePath(EditorUtility.OpenFolderPanel("Select Folder", "Assets/", ""));
                    if (!string.IsNullOrEmpty(dir))
                    {
                        options.directory = dir;
                    }
                    else
                    {
                        options.directory = "Assets/";
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorCommon.SpacePixel(2);
                GUI.enabled = terrain != null;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.GetControlRect(GUILayout.Width(11));
                if (GUILayout.Button("Convert", GUILayout.Height(25)))
                {
                    ConversionResult result = TerrainToMeshConverter.Convert(terrain, conversionOptions);
                    lastProcessingTime = result.ProcessingTimeMiliSec;

                    Material tempMat = GetTemplateMaterial(options.materialTemplate);
                    MaterialBinder binder;
                    if (options.materialTemplate == MaterialTemplateOptions.Custom)
                    {
                        binder = options.customMatBinder;
                    }
                    else
                    {
                        binder = MaterialBinder.Get(rpType);
                    }
                    GameObject prefab = MeshTerrainPrefabCreator.Create(result, options.directory, terrain.name + "_" + System.DateTime.Now.Ticks, tempMat, terrain.terrainData.terrainLayers, binder);

                    result.Dispose();
                    Selection.activeObject = prefab;
                    EditorGUIUtility.PingObject(prefab);
                }
                EditorGUILayout.EndHorizontal();
            }


            EditorGUILayout.BeginHorizontal();
            if (lastProcessingTime > 0)
            {
                EditorGUILayout.LabelField($"Last processing time: {lastProcessingTime} ms", EditorCommon.ItalicLabel);
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Read the Manual", EditorCommon.LinkLabel))
            {
                EditorMenus.OpenUserGuide();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel -= 1;
            EditorGUIUtility.labelWidth = labelWidth;
        }

        private Material GetTemplateMaterial(MaterialTemplateOptions template)
        {
            RenderPipelineType pipeline = EditorCommon.GetCurrentRP();
            if (pipeline == RenderPipelineType.Builtin)
            {
                if (template == MaterialTemplateOptions.Splat4NoNormal)
                {
                    Material mat = new Material(Shader.Find("PolarisMini/BuiltinRP/Terrain/PBR_4Splats"));
                    return mat;
                }
                else if (template == MaterialTemplateOptions.ColorMap)
                {
                    Material mat = new Material(Shader.Find("Standard"));
                    return mat;
                }
                else
                {
                    return customMaterial;
                }
            }
            else if (pipeline == RenderPipelineType.Universal)
            {
                if (template == MaterialTemplateOptions.Splat4NoNormal)
                {
                    Material mat = new Material(Shader.Find("PolarisMini/URP/Terrain/URP_4Splats_SG"));
                    return mat;
                }
                else if (template == MaterialTemplateOptions.ColorMap)
                {
                    Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    return mat;
                }
                else
                {
                    return customMaterial;
                }
            }
            else
            {
                return customMaterial;
            }
        }
    }
}
#endif