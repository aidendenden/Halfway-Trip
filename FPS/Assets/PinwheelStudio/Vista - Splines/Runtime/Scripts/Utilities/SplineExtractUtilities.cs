#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections.Generic;
using UnityEngine;
using UnityGraphics = UnityEngine.Graphics;

namespace Pinwheel.Vista.Splines
{
    public static class SplineExtractUtilities
    {
        private static string SHADER_NAME = "Hidden/Vista/SplineExtract";
        private static readonly int VERTICES = Shader.PropertyToID("_Vertices");
        private static readonly int ALPHAS = Shader.PropertyToID("_Alphas");
        private static readonly int WORLD_BOUNDS = Shader.PropertyToID("_WorldBounds");
        private static readonly int TEXTURE_SIZE = Shader.PropertyToID("_TextureSize");
        private static readonly int DEPTH_BUFFER = Shader.PropertyToID("_DepthBuffer");
        private static readonly int MAX_HEIGHT = Shader.PropertyToID("_MaxHeight");

        private static readonly int PASS_DEPTH = 0;
        private static readonly int PASS_MASK_FALLOFF = 1;
        private static readonly int PASS_MASK_BOOL = 2;
        private static readonly int PASS_HEIGHT_MASK = 3;
        private static readonly int PASS_HEIGHT = 4;

        public static void RenderFalloffMask(RenderTexture targetRt, ComputeBuffer worldTrianglesBuffer, ComputeBuffer alphasBuffer, int vertexCount, Vector4 worldBounds)
        {
            ComputeBuffer depthBuffer = new ComputeBuffer(targetRt.width * targetRt.height, sizeof(int));
            Drawing.Blit(Texture2D.blackTexture, targetRt);

            Material material = new Material(Shader.Find(SHADER_NAME));
            material.SetBuffer(VERTICES, worldTrianglesBuffer);
            material.SetBuffer(ALPHAS, alphasBuffer);
            material.SetVector(WORLD_BOUNDS, worldBounds);
            material.SetVector(TEXTURE_SIZE, new Vector2(targetRt.width, targetRt.height));
            material.SetBuffer(DEPTH_BUFFER, depthBuffer);

            UnityGraphics.SetRandomWriteTarget(1, depthBuffer);
            RenderTexture.active = targetRt;
            GL.PushMatrix();
            GL.LoadOrtho();
            material.SetPass(PASS_DEPTH);
            UnityGraphics.DrawProceduralNow(MeshTopology.Triangles, vertexCount);
            material.SetPass(PASS_MASK_FALLOFF);
            UnityGraphics.DrawProceduralNow(MeshTopology.Triangles, vertexCount);
            GL.PopMatrix();
            RenderTexture.active = null;
            UnityGraphics.ClearRandomWriteTargets();

            depthBuffer.Release();
            Object.DestroyImmediate(material);
        }

        public static void RenderBoolMask(RenderTexture targetRt, ComputeBuffer worldTrianglesBuffer, int vertexCount, Vector4 worldBounds)
        {
            Material material = new Material(Shader.Find(SHADER_NAME));
            material.SetBuffer(VERTICES, worldTrianglesBuffer);
            material.SetVector(WORLD_BOUNDS, worldBounds);
            material.SetVector(TEXTURE_SIZE, new Vector2(targetRt.width, targetRt.height));

            RenderTexture.active = targetRt;
            GL.PushMatrix();
            GL.LoadOrtho();
            material.SetPass(PASS_MASK_BOOL);
            UnityGraphics.DrawProceduralNow(MeshTopology.Triangles, vertexCount);
            GL.PopMatrix();
            RenderTexture.active = null;
            Object.DestroyImmediate(material);
        }

        private static Vector2[] WorldPointsToNormalized(List<Vector3> worldPoints, Vector4 worldBounds)
        {
            Vector2[] normalizedPoints = new Vector2[worldPoints.Count];
            for (int i = 0; i < worldPoints.Count; ++i)
            {
                Vector2 p = new Vector2(
                    Utilities.InverseLerpUnclamped(worldBounds.x, worldBounds.x + worldBounds.z, worldPoints[i].x),
                    Utilities.InverseLerpUnclamped(worldBounds.y, worldBounds.y + worldBounds.w, worldPoints[i].z));

                normalizedPoints[i] = p;
            }
            return normalizedPoints;
        }

        public static void RenderRegionMask(RenderTexture targetRt, Vector3[] worldPoints, Vector4 worldBounds)
        {
            List<Vector3> worldPointsList = new List<Vector3>(worldPoints);
            if (worldPointsList.Count < 3)
            {
                Drawing.Blit(Texture2D.blackTexture, targetRt);
                return;
            }
            if (worldPointsList[0] == worldPointsList[worldPointsList.Count - 1])
            {
                worldPointsList.RemoveAt(worldPointsList.Count - 1);
            }

            Vector2[] normalizedPoints = WorldPointsToNormalized(worldPointsList, worldBounds);
            PolygonMaskRenderer.Configs rendererConfigs = new PolygonMaskRenderer.Configs();
            rendererConfigs.vertices = normalizedPoints;
            rendererConfigs.falloffVertices = normalizedPoints;
            rendererConfigs.falloffTexture = Texture2D.blackTexture;

            PolygonMaskRenderer.Render(targetRt, rendererConfigs);
        }

        public static void RenderHeightMap(RenderTexture targetRt, ComputeBuffer worldTrianglesBuffer, ComputeBuffer alphasBuffer, int vertexCount, Vector4 worldBounds, float maxHeight)        
        {
            ComputeBuffer depthBuffer = new ComputeBuffer(targetRt.width * targetRt.height, sizeof(int));
            Drawing.Blit(Texture2D.blackTexture, targetRt);

            Material material = new Material(Shader.Find(SHADER_NAME));
            material.SetBuffer(VERTICES, worldTrianglesBuffer);
            material.SetBuffer(ALPHAS, alphasBuffer);
            material.SetVector(WORLD_BOUNDS, worldBounds);
            material.SetFloat(MAX_HEIGHT, maxHeight);
            material.SetVector(TEXTURE_SIZE, new Vector2(targetRt.width, targetRt.height));
            material.SetBuffer(DEPTH_BUFFER, depthBuffer);

            UnityGraphics.SetRandomWriteTarget(1, depthBuffer);
            RenderTexture.active = targetRt;
            GL.PushMatrix();
            GL.LoadOrtho();
            material.SetPass(PASS_HEIGHT_MASK);
            UnityGraphics.DrawProceduralNow(MeshTopology.Triangles, vertexCount);
            material.SetPass(PASS_HEIGHT);
            UnityGraphics.DrawProceduralNow(MeshTopology.Triangles, vertexCount);
            GL.PopMatrix();
            RenderTexture.active = null;
            UnityGraphics.ClearRandomWriteTargets();

            depthBuffer.Release();
            Object.DestroyImmediate(material);
        }
    }
}
#endif
