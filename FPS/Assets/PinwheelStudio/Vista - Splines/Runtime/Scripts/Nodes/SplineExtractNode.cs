#if VISTA
using Pinwheel.Vista.Graph;
using Pinwheel.Vista.Graphics;
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista.Splines.Graph
{
    [NodeMetadata(
        title = "Spline Extract",
        path = "General/Spline Extract",
        icon = "",
        documentation = "",
        description = "Extract a spline information to be used in other operation. There are other nodes for common spline based actions such as Ramp, Path, Thin Out Along, etc. node")]
    public class SplineExtractNode : ImageNodeBase
    {
        public readonly MaskSlot maskOutputSlot = new MaskSlot("Mask", SlotDirection.Output, 100);
        public readonly MaskSlot maskBoolOutputSlot = new MaskSlot("Mask Bool", SlotDirection.Output, 101);
        public readonly MaskSlot regionOutputSlot = new MaskSlot("Region", SlotDirection.Output, 102);
        public readonly MaskSlot heightOutputSlot = new MaskSlot("Height", SlotDirection.Output, 103);

        public readonly BufferSlot anchorsOutputSlot = new BufferSlot("Anchors", SlotDirection.Output, 200);
        public readonly BufferSlot pointsOutputSlot = new BufferSlot("Points", SlotDirection.Output, 201);

        [SerializeField]
        private string m_splineId;
        public string splineId
        {
            get
            {
                return m_splineId;
            }
            set
            {
                m_splineId = value;
            }
        }

        private static readonly string TEMP_VERTICES_BUFFER_NAME = "~TempVerticesBuffer";
        private static readonly string TEMP_ALPHAS_BUFFER_NAME = "~TempAlphasBuffer";

        public SplineExtractNode() : base()
        {
            m_splineId = "PICK_A_SPLINE_ID";
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            ISplineEvaluator splineEvaluator = SplineModuleUtilities.GetFirstSplineWithId(m_splineId);
            if (splineEvaluator == null)
                return;

            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            int resolution = this.CalculateResolution(baseResolution, baseResolution);
            Vector4 worldBounds = context.GetArg(Args.WORLD_BOUNDS).vectorValue;

            SlotRef maskRef = new SlotRef(m_id, maskOutputSlot.id);
            SlotRef maskBoolRef = new SlotRef(m_id, maskBoolOutputSlot.id);
            SlotRef heightRef = new SlotRef(m_id, heightOutputSlot.id);
            if (context.GetReferenceCount(maskRef) > 0 ||
                context.GetReferenceCount(maskBoolRef) > 0 ||
                context.GetReferenceCount(heightRef) > 0)
            {
                Vector3[] worldVertices = null;
                float[] alphas = null;
                splineEvaluator.GetWorldTrianglesAndAlphas(out worldVertices, out alphas);

                if (worldVertices.Length > 0)
                {
                    DataPool.BufferDescriptor vertBufferDesc = DataPool.BufferDescriptor.Create(worldVertices.Length * 3);
                    ComputeBuffer verticesBuffer = context.CreateTemporaryBuffer(vertBufferDesc, TEMP_VERTICES_BUFFER_NAME);
                    verticesBuffer.SetData(worldVertices);

                    DataPool.BufferDescriptor alphaBufferDesc = DataPool.BufferDescriptor.Create(worldVertices.Length * 1);
                    ComputeBuffer alphasBuffer = context.CreateTemporaryBuffer(alphaBufferDesc, TEMP_ALPHAS_BUFFER_NAME);
                    alphasBuffer.SetData(alphas);

                    if (context.GetReferenceCount(maskRef) > 0 || context.IsTargetNode(m_id))
                    {
                        OutputMask(context, maskRef, resolution, worldBounds, verticesBuffer, alphasBuffer, worldVertices.Length);
                    }

                    if (context.GetReferenceCount(maskBoolRef) > 0)
                    {
                        OutputMaskBool(context, maskBoolRef, resolution, worldBounds, verticesBuffer, worldVertices.Length);
                    }

                    if (context.GetReferenceCount(heightRef) > 0)
                    {
                        OutputHeight(context, heightRef, resolution, worldBounds, verticesBuffer, alphasBuffer, worldVertices.Length);
                    }

                    context.ReleaseTemporary(TEMP_VERTICES_BUFFER_NAME);
                    context.ReleaseTemporary(TEMP_ALPHAS_BUFFER_NAME);
                }
            }

            SlotRef regionRef = new SlotRef(m_id, regionOutputSlot.id);
            if (context.GetReferenceCount(regionRef) > 0)
            {
                Vector3[] worldPoints = splineEvaluator.GetWorldPoints();
                OutputRegion(context, regionRef, resolution, worldBounds, worldPoints);
            }

            SlotRef anchorsRef = new SlotRef(m_id, anchorsOutputSlot.id);
            if (context.GetReferenceCount(anchorsRef) > 0)
            {
                OutputAnchors(context, anchorsRef, worldBounds, splineEvaluator);
            }

            SlotRef pointsRef = new SlotRef(m_id, pointsOutputSlot.id);
            if (context.GetReferenceCount(pointsRef) > 0)
            {
                OutputPoints(context, pointsRef, worldBounds, splineEvaluator);
            }
        }

        private PositionSample[] WorldPointsToNormalizedSample(Vector3[] worldPoints, Vector4 worldBounds)
        {
            PositionSample[] normalizedSamples = new PositionSample[worldPoints.Length];
            for (int i = 0; i < worldPoints.Length; ++i)
            {
                PositionSample sample = new PositionSample();
                Vector3 p = new Vector3(
                    Utilities.InverseLerpUnclamped(worldBounds.x, worldBounds.x + worldBounds.z, worldPoints[i].x),
                    0,
                    Utilities.InverseLerpUnclamped(worldBounds.y, worldBounds.y + worldBounds.w, worldPoints[i].z));
                sample.position = p;
                sample.isValid = 1;
                normalizedSamples[i] = sample;
            }
            return normalizedSamples;
        }

        private Vector2[] WorldPointsToNormalized(List<Vector3> worldPoints, Vector4 worldBounds)
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

        private void OutputMask(GraphContext context, SlotRef maskRef, int resolution, Vector4 worldBounds, ComputeBuffer verticesBuffer, ComputeBuffer alphasBuffer, int vertexCount)
        {
            DataPool.RtDescriptor targetRtDesc = DataPool.RtDescriptor.Create(resolution, resolution, RenderTextureFormat.RFloat);
            RenderTexture targetRt = context.CreateRenderTarget(targetRtDesc, maskRef);
            Drawing.Blit(Texture2D.blackTexture, targetRt);

            SplineExtractUtilities.RenderFalloffMask(targetRt, verticesBuffer, alphasBuffer, vertexCount, worldBounds);
        }

        private void OutputMaskBool(GraphContext context, SlotRef maskBoolRef, int resolution, Vector4 worldBounds, ComputeBuffer verticesBuffer, int vertexCount)
        {
            DataPool.RtDescriptor targetRtDesc = DataPool.RtDescriptor.Create(resolution, resolution, RenderTextureFormat.RFloat);
            RenderTexture targetRt = context.CreateRenderTarget(targetRtDesc, maskBoolRef);
            Drawing.Blit(Texture2D.blackTexture, targetRt);

            SplineExtractUtilities.RenderBoolMask(targetRt, verticesBuffer, vertexCount, worldBounds);

        }

        private void OutputRegion(GraphContext context, SlotRef regionRef, int resolution, Vector4 worldBounds, Vector3[] worldPoints)
        {
            DataPool.RtDescriptor regionDesc = DataPool.RtDescriptor.Create(resolution, resolution);
            RenderTexture targetRt = context.CreateRenderTarget(regionDesc, regionRef);

            SplineExtractUtilities.RenderRegionMask(targetRt, worldPoints, worldBounds);
        }

        private void OutputHeight(GraphContext context, SlotRef heightRef, int resolution, Vector4 worldBounds, ComputeBuffer verticesBuffer, ComputeBuffer alphasBuffer, int vertexCount)
        {
            DataPool.RtDescriptor targetRtDesc = DataPool.RtDescriptor.Create(resolution, resolution, RenderTextureFormat.RFloat);
            RenderTexture targetRt = context.CreateRenderTarget(targetRtDesc, heightRef);
            Drawing.Blit(Texture2D.blackTexture, targetRt);

            float maxHeight = context.GetArg(Args.TERRAIN_HEIGHT).floatValue;
            SplineExtractUtilities.RenderHeightMap(targetRt, verticesBuffer, alphasBuffer, vertexCount, worldBounds, maxHeight);
        }

        private void OutputAnchors(GraphContext context, SlotRef anchorsRef, Vector4 worldBounds, ISplineEvaluator splineEvaluator)
        {
            Vector3[] worldAnchors = splineEvaluator.GetWorldAnchors();
            if (worldAnchors?.Length > 0)
            {
                PositionSample[] normalizedSamples = WorldPointsToNormalizedSample(worldAnchors, worldBounds);

                DataPool.BufferDescriptor desc = DataPool.BufferDescriptor.Create(normalizedSamples.Length * PositionSample.SIZE);
                ComputeBuffer destBuffer = context.CreateBuffer(desc, anchorsRef);

                destBuffer.SetData(normalizedSamples);
            }
        }

        private void OutputPoints(GraphContext context, SlotRef pointsRef, Vector4 worldBounds, ISplineEvaluator splineEvaluator)
        {
            Vector3[] worldPoints = splineEvaluator.GetWorldPoints();
            if (worldPoints?.Length > 0)
            {
                PositionSample[] normalizedSamples = WorldPointsToNormalizedSample(worldPoints, worldBounds);

                DataPool.BufferDescriptor desc = DataPool.BufferDescriptor.Create(normalizedSamples.Length * PositionSample.SIZE);
                ComputeBuffer destBuffer = context.CreateBuffer(desc, pointsRef);
                destBuffer.SetData(normalizedSamples);
            }
        }
    }
}
#endif
