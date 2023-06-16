#if POMINI
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Pinwheel.PolarisMini
{
    public static class Common
    {
        public const string PINWHEEL_STUDIO = "Pinwheel Studio";
        public const string SUPPORT_EMAIL = "support@pinwheel.studio";
        public const string BUSINESS_EMAIL = "hello@pinwheel.studio";
        public const string YOUTUBE_CHANNEL = "https://www.youtube.com/channel/UCebwuk5CfIe5kolBI9nuBTg";
        public const string ONLINE_MANUAL = "https://docs.google.com/document/d/1Mw51zhOBgosXf3I13qzbtIqjLNlvl2yo3B4bJ8bd6zU/edit?usp=sharing";
        public const string FACEBOOK_PAGE = "https://www.facebook.com/polaris.terrain";
        public const string FORUM = "https://forum.unity.com/threads/pre-release-polaris-hybrid-procedural-low-poly-terrain-engine.541792/#post-3572618";
        public const string DISCORD = "https://discord.gg/j9p5PMWPhk";

        public const int SUB_DIV_MAP_RESOLUTION = 512;
        public const string SUB_DIV_MAP_SHADER = "Hidden/Griffin/SubDivisionMap";
        public const float SUB_DIV_EPSILON = 0.005f;
        public const float SUB_DIV_PIXEL_OFFSET = 2;
        public const float SUB_DIV_STEP = 0.1f;

        public const string CHUNK_ROOT_NAME_OBSOLETED = "_Geometry";
        public const string CHUNK_ROOT_NAME = "~Geometry";
        public const int MAX_LOD_COUNT = 4;
        public const int MAX_MESH_BASE_RESOLUTION = 10;
        public const int MAX_MESH_RESOLUTION = 13;

        public const string CHUNK_MESH_NAME_PREFIX = "~Chunk";
        public const string GRASS_MESH_NAME_PREFIX = "~GrassPatch";

        public const string BRUSH_MASK_RESOURCES_PATH = "PolarisBrushes";

        public const int PREVIEW_TEXTURE_SIZE = 512;
        public const int TEXTURE_SIZE_MIN = 1;
        public const int TEXTURE_SIZE_MAX = 8192;

        public const float MAX_TREE_DISTANCE = 500;
        public const float MAX_GRASS_DISTANCE = 500;
        public const float MAX_COLLIDER_BUDGET = 1000;

        public const string FIRST_HISTORY_ENTRY_NAME = "Begin";

        private static int mainThreadId;
        public static int MainThreadId
        {
            get
            {
                return mainThreadId;
            }
        }

        static Common()
        {
            Init();
        }
        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
#if UNITY_EDITOR
            //EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }
        public static long GetTimeTick()
        {
            DateTime time = DateTime.Now;
            return time.Ticks;
        }
        public static string Reverse(string s)
        {
            char[] chars = s.ToCharArray();
            System.Array.Reverse(chars);
            return new string(chars);
        }
        public static string GetUniqueID()
        {
            string s = GetTimeTick().ToString();
            return Reverse(s);
        }
        public static Texture2D CreateTexture(int resolution, Color fill, TextureFormat format = TextureFormat.RGBA32, bool linear = true)
        {
            Texture2D t = new Texture2D(resolution, resolution, format, false, linear);
            Color[] colors = new Color[resolution * resolution];
            Utilities.Fill(colors, fill);
            t.SetPixels(colors);
            t.Apply();
            return t;
        }

        public static Texture2D CreateTexture(int width, int height, Color fill, TextureFormat format = TextureFormat.RGBA32, bool linear = true)
        {
            Texture2D t = new Texture2D(width, height, format, false, linear);
            Color[] colors = new Color[width * height];
            Utilities.Fill(colors, fill);
            t.SetPixels(colors);
            t.Apply();
            return t;
        }
        public static Texture2D CreateTextureFromCurve(AnimationCurve curve, int width, int height)
        {
            Texture2D t = Common.CreateTexture(width, height, Color.black);
            t.wrapMode = TextureWrapMode.Clamp;
            Color[] colors = new Color[width * height];
            for (int x = 0; x < width; ++x)
            {
                float f = Mathf.InverseLerp(0, width - 1, x);
                float value = curve.Evaluate(f);
                Color c = new Color(value, value, value, value);
                for (int y = 0; y < height; ++y)
                {
                    colors[Utilities.To1DIndex(x, y, width)] = c;
                }
            }
            t.filterMode = FilterMode.Bilinear;
            t.SetPixels(colors);
            t.Apply();
            return t;
        }
        public static void SetMaterialKeywordActive(Material mat, string keyword, bool active)
        {
            if (active)
            {
                mat.EnableKeyword(keyword);
            }
            else
            {
                mat.DisableKeyword(keyword);
            }
        }
        public static float DecodeTerrainHeight(Vector2 enc)
        {
            Vector2 kDecodeDot = new Vector2(1.0f, 1f / 255.0f);
            return Vector2.Dot(enc, kDecodeDot);
        }
        public static Rect UnitRect
        {
            get
            {
                return new Rect(0, 0, 1, 1);
            }
        }
        public static void RegisterBeginRender(Camera.CameraCallback callback)
        {
            Camera.onPreCull += callback;
        }

        public static void UnregisterBeginRender(Camera.CameraCallback callback)
        {
            Camera.onPreCull -= callback;
        }

        public static void RegisterEndRender(Camera.CameraCallback callback)
        {
            Camera.onPostRender += callback;
        }

        public static void UnregisterEndRender(Camera.CameraCallback callback)
        {
            Camera.onPostRender -= callback;
        }

        public static void CopyToRT(Texture t, RenderTexture rt)
        {
            RenderTexture.active = rt;
            Graphics.Blit(t, rt);
            RenderTexture.active = null;
        }
        public static void DrawQuad(RenderTexture rt, Vector2[] quadCorners, Material mat, int pass)
        {
            RenderTexture.active = rt;
            GL.PushMatrix();
            mat.SetPass(pass);
            GL.LoadOrtho();
            GL.Begin(GL.QUADS);
            GL.TexCoord(new Vector3(0, 0, 0));
            GL.Vertex3(quadCorners[0].x, quadCorners[0].y, 0);
            GL.TexCoord(new Vector3(0, 1, 0));
            GL.Vertex3(quadCorners[1].x, quadCorners[1].y, 0);
            GL.TexCoord(new Vector3(1, 1, 0));
            GL.Vertex3(quadCorners[2].x, quadCorners[2].y, 0);
            GL.TexCoord(new Vector3(1, 0, 0));
            GL.Vertex3(quadCorners[3].x, quadCorners[3].y, 0);
            GL.End();
            GL.PopMatrix();
            RenderTexture.active = null;
        }
        private static Vector2[] fullRectUvPoints;
        public static Vector2[] FullRectUvPoints
        {
            get
            {
                if (fullRectUvPoints == null)
                {
                    fullRectUvPoints = new Vector2[]
                    {
                        Vector2.zero,
                        Vector2.up,
                        Vector2.one,
                        Vector2.right
                    };
                }
                return fullRectUvPoints;
            }
        }
        public static void CopyFromRT(Texture2D t, RenderTexture rt)
        {
            RenderTexture.active = rt;
            t.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            t.Apply();
            RenderTexture.active = null;
        }

        public static void CopyTexture(Texture2D src, Texture2D des)
        {
            RenderTexture rt = new RenderTexture(des.width, des.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            CopyToRT(src, rt);
            CopyFromRT(des, rt);
            rt.Release();
            Utilities.DestroyObject(rt);
        }

        public static Texture2D CloneTexture(Texture2D t)
        {
            RenderTexture rt = new RenderTexture(t.width, t.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            CopyToRT(t, rt);
            Texture2D result = new Texture2D(t.width, t.height, TextureFormat.ARGB32, false, true);
            result.filterMode = t.filterMode;
            result.wrapMode = t.wrapMode;
            CopyFromRT(result, rt);
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);
            return result;
        }

        public static void FillTexture(Texture2D t, Color c)
        {
            Color[] colors = new Color[t.width * t.height];
            Utilities.Fill(colors, c);
            t.SetPixels(colors);
            t.Apply();
        }

        public static void FillTexture(RenderTexture rt, Color c)
        {
            Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            tex.SetPixel(0, 0, c);
            tex.Apply();
            CopyToRT(tex, rt);
            Utilities.DestroyObject(tex);
        }
        public static void TryAddObjectToAsset(Object objectToAdd, Object asset)
        {
#if UNITY_EDITOR
            //if (!IsMainThread)
            //   return;
            if (!Application.isPlaying)
            {
                if (!AssetDatabase.Contains(objectToAdd) && EditorUtility.IsPersistent(asset))
                {
                    AssetDatabase.AddObjectToAsset(objectToAdd, asset);
                }
            }
#endif
        }
        public static void RegisterBeginRenderSRP(System.Action<ScriptableRenderContext, Camera> callback)
        {
            RenderPipelineManager.beginCameraRendering += callback;
        }

        public static void UnregisterBeginRenderSRP(System.Action<ScriptableRenderContext, Camera> callback)
        {
            RenderPipelineManager.beginCameraRendering -= callback;
        }

    }
}


#endif