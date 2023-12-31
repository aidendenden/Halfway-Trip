using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

/**
 * Ara is a trail renderer for Unity, meant to replace and extend the standard TrailRenderer.
 */

namespace Ara{

    [ExecuteInEditMode]
    public class AraTrail : MonoBehaviour {

        public const float epsilon = 0.00001f;

        public enum TrailAlignment {
            View,
            Velocity,
            Local
        }

        public enum TrailSpace
        {
            World,
            Self,
            Custom
        }

        public enum TrailSorting {
            OlderOnTop,
            NewerOnTop
        }

        public enum Timescale {
            Normal,
            Unscaled
        }

        public enum TextureMode {
            Stretch,
            Tile,
            WorldTile
        }

        /**
         * Spatial frame, consisting of a point an three axis. This is used to implement the parallel transport method 
         * along the curve defined by the trail points. Using this instead of a Frenet-esque method avoids flipped frames
         * at points where the curvature changes.
         */
        public struct CurveFrame {

            public Vector3 position;
            public Vector3 normal;
            public Vector3 bitangent;
            public Vector3 tangent;

            public CurveFrame(Vector3 position, Vector3 normal, Vector3 bitangent, Vector3 tangent) {
                this.position = position;
                this.normal = normal;
                this.bitangent = bitangent;
                this.tangent = tangent;
            }

            public Vector3 Transport(Vector3 newTangent, Vector3 newPosition) {

                // double-reflection rotation-minimizing frame transport:
                Vector3 v1 = newPosition - position;
                float c1 = Vector3.Dot(v1, v1);

                Vector3 rL = normal - 2 / (c1 + epsilon) * Vector3.Dot(v1, normal) * v1;
                Vector3 tL = tangent - 2 / (c1 + epsilon) * Vector3.Dot(v1, tangent) * v1;

                Vector3 v2 = newTangent - tL;
                float c2 = Vector3.Dot(v2, v2);

                Vector3 r1 = rL - 2 / (c2 + epsilon) * Vector3.Dot(v2, rL) * v2;
                Vector3 s1 = Vector3.Cross(newTangent, r1);

                normal = r1;
                bitangent = s1;
                tangent = newTangent;
                position = newPosition;

                return normal;
            }
        }

        /**
         * Holds information for each point in a trail: position, velocity and remaining lifetime. Points
         * can be added or subtracted, and interpolated using Catmull-Rom spline interpolation.
         */
        public struct Point {

            public Vector3 position;
            public Vector3 velocity;
            public Vector3 tangent;
            public Vector3 normal;
            public Color color;
            public float thickness;
            public float life;
            public float texcoord;
            public bool discontinuous;

            public Point(Vector3 position, Vector3 velocity, Vector3 tangent, Vector3 normal, Color color, float thickness, float texcoord, float lifetime) {
                this.position = position;
                this.velocity = velocity;
                this.tangent = tangent;
                this.normal = normal;
                this.color = color;
                this.thickness = thickness;
                this.life = lifetime;
                this.texcoord = texcoord;
                this.discontinuous = false;
            }

            public static float CatmullRom(float p0, float p1, float p2, float p3, float t) {
                float t2 = t * t;
                return 0.5f * ((2 * p1) +
                              (-p0 + p2) * t +
                              (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 +
                              (-p0 + 3 * p1 - 3 * p2 + p3) * t2 * t);
            }

            public static Point operator +(Point p1, Point p2)
            {
                return new Point(p1.position + p2.position,
                                 p1.velocity + p2.velocity,
                                 p1.tangent + p2.tangent,
                                 p1.normal + p2.normal,
                                 p1.color + p2.color,
                                 p1.thickness + p2.thickness,
                                 p1.texcoord + p2.texcoord,
                                 p1.life + p2.life);
            }
            public static Point operator -(Point p1, Point p2)
            {
                return new Point(p1.position - p2.position,
                                 p1.velocity - p2.velocity,
                                 p1.tangent - p2.tangent,
                                 p1.normal - p2.normal,
                                 p1.color - p2.color,
                                 p1.thickness - p2.thickness,
                                 p1.texcoord - p2.texcoord,
                                 p1.life - p2.life);
            }
        }

        [Header("Overall")]
        [Tooltip("Trail cross-section asset, determines the shape of the emitted trail. If no asset is specified, the trail will be a simple strip.")]
        public TrailSection section = null;
        [Tooltip("Whether to use world or local space to generate and simulate the trail.")]
        public TrailSpace space = TrailSpace.World;
        [Tooltip("Custom space to use when generating and simulating the trail")]
        public Transform customSpace = null;
        [Tooltip("Whether to use regular time.")]
        public Timescale timescale = Timescale.Normal;
        [Tooltip("How to align the trail geometry: facing the camera (view) of using the transform's rotation (local).")]
        public TrailAlignment alignment = TrailAlignment.View;
        [Tooltip("Determines the order in which trail points will be rendered.")]
        public TrailSorting sorting = TrailSorting.OlderOnTop;
        [Tooltip("Thickness multiplier, in meters.")]
        public float thickness = 0.1f;
        [Tooltip("Amount of smoothing iterations applied to the trail shape.")]
        [Range(1, 8)]
        public int smoothness = 1;
        [Min(0)]
        public float smoothingDistance = 0.05f;
        [Tooltip("Calculate accurate thickness at sharp corners.")]
        public bool highQualityCorners = false;
        [Range(0, 12)]
        public int cornerRoundness = 5;

        [Header("Length")]

        [Tooltip("How should the thickness of the curve evolve over its lenght. The horizontal axis is normalized lenght (in the [0,1] range) and the vertical axis is a thickness multiplier.")]
        [FormerlySerializedAs("thicknessOverLenght")]
        public AnimationCurve thicknessOverLength = AnimationCurve.Linear(0, 1, 0, 1);    /**< maps trail length to thickness.*/
        [Tooltip("How should vertex color evolve over the trail's length.")]
        [FormerlySerializedAs("colorOverLenght")]
        public Gradient colorOverLength = new Gradient();

        [Header("Time")]

        [Tooltip("How should the thickness of the curve evolve with its lifetime. The horizontal axis is normalized lifetime (in the [0,1] range) and the vertical axis is a thickness multiplier.")]
        public AnimationCurve thicknessOverTime = AnimationCurve.Linear(0, 1, 0, 1);  /**< maps trail lifetime to thickness.*/
        [Tooltip("How should vertex color evolve over the trail's lifetime.")]
        public Gradient colorOverTime = new Gradient();

        [Header("Emission")]

        public bool emit = true;
        [Tooltip("Initial thickness of trail points when they are first spawned.")]
        public float initialThickness = 1; /**< initial speed of trail, in world space. */
        [Tooltip("Initial color of trail points when they are first spawned.")]
        public Color initialColor = Color.white; /**< initial color of trail, in world space. */
        [Tooltip("Initial velocity of trail points when they are first spawned.")]
        public Vector3 initialVelocity = Vector3.zero; /**< initial speed of trail, in world space. */
        [Tooltip("Minimum amount of time (in seconds) that must pass before spawning a new point.")]
        public float timeInterval = 0.025f;
        [Tooltip("Minimum distance (in meters) that must be left between consecutive points in the trail.")]
        public float minDistance = 0.025f;
        [Tooltip("Duration of the trail (in seconds).")]
        public float time = 2f;

        [Header("Physics")]

        [Tooltip("Toggles trail physics.")]
        public bool enablePhysics = false;
        [Tooltip("Amount of seconds pre-simulated before the trail appears. Useful when you want a trail to be already simulating when the game starts.")]
        public float warmup = 0;               /**< simulation warmup seconds.*/
        [Tooltip("Gravity affecting the trail.")]
        public Vector3 gravity = Vector3.zero;  /**< gravity applied to the trail, in world space. */
        [Tooltip("Amount of speed transferred from the transform to the trail. 0 means no velocity is transferred, 1 means 100% of the velocity is transferred.")]
        [Range(0, 1)]
        public float inertia = 0;               /**< amount of GameObject velocity transferred to the trail.*/
        [Tooltip("Amount of temporal smoothing applied to the velocity transferred from the transform to the trail.")]
        [Range(0, 1)]
        public float velocitySmoothing = 0.75f;     /**< velocity smoothing amount.*/
        [Tooltip("Amount of damping applied to the trail's velocity. Larger values will slow down the trail more as time passes.")]
        [Range(0, 1)]
        public float damping = 0.75f;               /**< velocity damping amount.*/

        [Header("Rendering")]

        public Material[] materials = new Material[] { null };
        public UnityEngine.Rendering.ShadowCastingMode castShadows = UnityEngine.Rendering.ShadowCastingMode.On;
        public bool receiveShadows = true;
        public bool useLightProbes = true;

        [Header("Texture")]

        [Tooltip("Quad mapping will send the shader an extra coordinate for each vertex, that can be used to correct UV distortion using tex2Dproj.")]
        public bool quadMapping = false;
        [Tooltip("How to apply the texture over the trail: stretch it all over its lenght, or tile it.")]
        public TextureMode textureMode = TextureMode.Stretch;
        [Tooltip("Defines how many times are U coords repeated across the length of the trail.")]
        public float uvFactor = 1;
        [Tooltip("Defines how many times are V coords repeated trough the width of the trail.")]
        public float uvWidthFactor = 1;
        [Tooltip("When the texture mode is set to 'Tile', defines where to begin tiling from: 0 means the start of the trail, 1 means the end.")]
        [Range(0, 1)]
        public float tileAnchor = 1;

        public event System.Action onUpdatePoints;

        [HideInInspector] public ElasticArray<Point> points = new ElasticArray<Point>();
        private ElasticArray<Point> renderablePoints = new ElasticArray<Point>();

        private List<int> discontinuities = new List<int>();

        private Mesh mesh_;
        private Vector3 velocity = Vector3.zero;
        private Vector3 prevPosition;
        private float accumTime = 0;

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector3> normals = new List<Vector3>();
        private List<Vector4> tangents = new List<Vector4>();
        private List<Vector4> uvs = new List<Vector4>();
        private List<Color> vertColors = new List<Color>();
        private List<int> tris = new List<int>();

        private Vector3 nextV = Vector3.zero, prevV = Vector3.zero, vertex = Vector3.zero, normal = Vector3.zero, bitangent = Vector3.zero;
        private Vector4 tangent = new Vector4(0, 0, 0, 1), texTangent = Vector4.zero, uv = Vector4.zero;
        private Color color;

        public Vector3 Velocity
        {
            get { return velocity; }
        }

        private float DeltaTime
        {
            get { return timescale == Timescale.Unscaled ? Time.unscaledDeltaTime : Time.deltaTime; }
        }

        private float FixedDeltaTime
        {
            get { return timescale == Timescale.Unscaled ? Time.fixedUnscaledDeltaTime : Time.fixedDeltaTime; }
        }

        public Mesh mesh
        {
            get { return mesh_; }
        }

        public Matrix4x4 worldToTrail
        {
            get
            {
                switch (space)
                {
                    case TrailSpace.World: return Matrix4x4.identity;
                    case TrailSpace.Self: return transform.worldToLocalMatrix;
                    case TrailSpace.Custom: return customSpace != null ? customSpace.worldToLocalMatrix : Matrix4x4.identity;
                    default: return Matrix4x4.identity;
                }
            }
        }

        public void OnValidate()
        {
            time = Mathf.Max(time, epsilon);
            warmup = Mathf.Max(0, warmup);
        }

        public void Awake()
        {
            Warmup();
        }

#if (UNITY_2019_1_OR_NEWER)
        System.Action<ScriptableRenderContext, Camera> renderCallback;
#endif
        void OnEnable()
        {

            // initialize previous position, for correct velocity estimation in the first frame:
            prevPosition = transform.position;
            velocity = Vector3.zero;

            // create a new mesh for the trail:
            mesh_ = new Mesh();
            mesh_.name = "ara_trail_mesh";
            mesh_.MarkDynamic();

            AttachToCameraRendering();

        }

        void OnDisable()
        {

            // destroy both the trail mesh and the hidden renderer object:
            DestroyImmediate(mesh_);

            DetachFromCameraRendering();

        }

        private void AttachToCameraRendering()
        {
#if (UNITY_2019_1_OR_NEWER)
            // subscribe to OnPreCull for all cameras.
            renderCallback = new System.Action<ScriptableRenderContext, Camera>((cntxt, cam) => { UpdateTrailMesh(cam); });
            RenderPipelineManager.beginCameraRendering += renderCallback;
#endif
            Camera.onPreCull += UpdateTrailMesh;
        }

        private void DetachFromCameraRendering()
        {
            // unsubscribe from OnPreCull.
#if (UNITY_2019_1_OR_NEWER)
            RenderPipelineManager.beginCameraRendering -= renderCallback;
#endif
            Camera.onPreCull -= UpdateTrailMesh;
        }

        /**
         * Removes all points in the trail, effectively removing any rendered trail segments.
         */
        public void Clear()
        {
            points.Clear();
        }

        private void UpdateVelocity()
        {

            if (DeltaTime > 0)
                velocity = Vector3.Lerp((transform.position - prevPosition) / DeltaTime, velocity, velocitySmoothing);

            prevPosition = transform.position;

        }

        /**
         * Updates point logic.
         */
        private void LateUpdate()
        {
            UpdateVelocity();

            EmissionStep(DeltaTime);

            SnapLastPointToTransform();

            UpdatePointsLifecycle();

            if (onUpdatePoints != null)
                onUpdatePoints();
        }

        private void EmissionStep(float time)
        {

            // Acumulate the amount of time passed:
            accumTime += time;

            // If enough time has passed since the last emission (>= timeInterval), consider emitting new points.
            if (accumTime >= timeInterval) {

                if (emit) {

                    var position = worldToTrail.MultiplyPoint3x4(transform.position);

                    // If there's less than 2 points, or if the last 2 points are too far apart, spawn a new one:
                    if (points.Count < 2 || (Vector3.Distance(position, points[points.Count - 2].position) >= minDistance))
                    {
                        EmitPoint(position);
                        accumTime = 0;
                    }
                }
            }

        }

        private void Warmup()
        {

            if (!Application.isPlaying || !enablePhysics)
                return;

            float simulatedTime = warmup;

            while (simulatedTime > FixedDeltaTime)
            {

                PhysicsStep(FixedDeltaTime);

                EmissionStep(FixedDeltaTime);

                SnapLastPointToTransform();

                UpdatePointsLifecycle();

                if (onUpdatePoints != null)
                    onUpdatePoints();

                simulatedTime -= FixedDeltaTime;
            }
        }

        private void PhysicsStep(float timestep)
        {

            float velocity_scale = Mathf.Pow(1 - Mathf.Clamp01(damping), timestep);

            for (int i = 0; i < points.Count; ++i)
            {

                Point point = points[i];

                // apply gravity and external forces:
                point.velocity += gravity * timestep;
                point.velocity *= velocity_scale;

                // integrate velocity:
                point.position += point.velocity * timestep;

                points[i] = point;
            }
        }

        /**
         * Updates point physics.
         */
        private void FixedUpdate()
        {

            if (!enablePhysics)
                return;

            PhysicsStep(FixedDeltaTime);

        }

        /**
         * Spawns a new point in the trail.
         */
        public void EmitPoint(Vector3 position, bool adjustEnd = true)
        {
            // Adjust the current end of the trail, if any:
            /*if (adjustEnd && points.Count > 1)
            {
                Point lastPoint = points[points.Count - 1];
                lastPoint.position = (position + points[points.Count - 2].position) * 0.5f;
                points[points.Count - 1] = lastPoint;
            }*/

            float texcoord = 0;

            // if there's a previous point in the trail, use its texcoord to calculate ours.
            if (points.Count > 0)
                texcoord = points[points.Count - 1].texcoord + Vector3.Distance(position, points[points.Count - 1].position);

            var nrm = worldToTrail.MultiplyVector(transform.forward);
            var tgt = worldToTrail.MultiplyVector(transform.right);
            points.Add(new Point(position, initialVelocity + velocity * inertia, tgt, nrm, initialColor, initialThickness, texcoord, time));
        }

        /**
         * Makes sure the first point is always at the transform's center, and that its orientation matches it.
         */
        private void SnapLastPointToTransform()
        {
            // Last point always coincides with transform:
            if (points.Count > 0)
            {
                Point lastPoint = points[points.Count - 1];

                // if we are not emitting, the last point is a discontinuity.
                if (!emit)
                    lastPoint.discontinuous = true;

                // if the point is not discontinuous, move and orient it according to the transform.
                if (!lastPoint.discontinuous)
                {
                    lastPoint.position = worldToTrail.MultiplyPoint3x4(transform.position);
                    lastPoint.normal = worldToTrail.MultiplyVector(transform.forward);
                    lastPoint.tangent = worldToTrail.MultiplyVector(transform.right);

                    // if there's a previous point in the trail, use its texcoord to calculate ours.
                    if (points.Count > 1)
                        lastPoint.texcoord = points[points.Count - 2].texcoord + Vector3.Distance(lastPoint.position, points[points.Count - 2].position);
                }

                points[points.Count - 1] = lastPoint;
            }
        }

        /**
         * Updated trail lifetime and removes dead points.
         */
        private void UpdatePointsLifecycle()
        {

            for (int i = points.Count - 1; i >= 0; --i)
            {

                Point point = points[i];
                point.life -= DeltaTime;
                points[i] = point;

                if (point.life <= 0)
                {

                    // Unsmoothed trails delete points as soon as they die.
                    if (smoothness <= 1)
                    {
                        points.RemoveAt(i);
                    }
                    // Smoothed trails however, should wait until the next 2 points are dead too. This ensures spline continuity.
                    else
                    {
                        if (points[Mathf.Min(i + 1, points.Count - 1)].life <= 0 &&
                            points[Mathf.Min(i + 2, points.Count - 1)].life <= 0)
                            points.RemoveAt(i);
                    }

                }
            }
        }

        /**
         * Clears all mesh data: vertices, normals, tangents, etc. This is called at the beginning of UpdateTrailMesh().
         */
        private void ClearMeshData()
        {

            mesh_.Clear();
            vertices.Clear();
            normals.Clear();
            tangents.Clear();
            uvs.Clear();
            vertColors.Clear();
            tris.Clear();

        }

        /**
         * Applies vertex, normal, tangent, etc. data to the mesh. Called at the end of UpdateTrailMesh()
         */
        private void CommitMeshData()
        {

            mesh_.SetVertices(vertices);
            mesh_.SetNormals(normals);
            mesh_.SetTangents(tangents);
            mesh_.SetColors(vertColors);
            mesh_.SetUVs(0, uvs);
            mesh_.SetTriangles(tris, 0, true);

        }

        /** 
         * Asks Unity to render the trail mesh.
         */
        private void RenderMesh(Camera cam)
        {
            Matrix4x4 renderMatrix = worldToTrail.inverse;

            for (int i = 0; i < materials.Length; ++i)
            {
                Graphics.DrawMesh(mesh_, renderMatrix,
                                  materials[i], gameObject.layer, cam, 0, null, castShadows, receiveShadows, null, useLightProbes);
            }
        }

        private ElasticArray<Point> GetRenderablePoints(int start, int end)
        {

            renderablePoints.Clear();

            if (smoothness <= 1) {
                for (int i = start; i <= end; ++i)
                    renderablePoints.Add(points[i]);
                return renderablePoints;
            }

            var data = points.Data;

            // calculate sample size in normalized coordinates:
            float samplesize = 1.0f / smoothness;

            Point interpolated = new Point(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Color.white, 0, 0, 0);

            for (int i = start; i < end; ++i) {

                int i_1 = i == start ? start : i - 1;
                int i2 = i == end - 1 ? end : i + 2;
                int i1 = i + 1;

                float pax = data[i_1].position[0], pay = data[i_1].position[1], paz = data[i_1].position[2];
                float vax = data[i_1].velocity[0], vay = data[i_1].velocity[1], vaz = data[i_1].velocity[2];
                float tax = data[i_1].tangent[0], tay = data[i_1].tangent[1], taz = data[i_1].tangent[2];
                float nax = data[i_1].normal[0], nay = data[i_1].normal[1], naz = data[i_1].normal[2];
                float cax = data[i_1].color[0], cay = data[i_1].color[1], caz = data[i_1].color[2], caw = data[i_1].color[3];

                float pbx = data[i].position[0], pby = data[i].position[1], pbz = data[i].position[2];
                float vbx = data[i].velocity[0], vby = data[i].velocity[1], vbz = data[i].velocity[2];
                float tbx = data[i].tangent[0], tby = data[i].tangent[1], tbz = data[i].tangent[2];
                float nbx = data[i].normal[0], nby = data[i].normal[1], nbz = data[i].normal[2];
                float cbx = data[i].color[0], cby = data[i].color[1], cbz = data[i].color[2], cbw = data[i].color[3];

                float pcx = data[i1].position[0], pcy = data[i1].position[1], pcz = data[i1].position[2];
                float vcx = data[i1].velocity[0], vcy = data[i1].velocity[1], vcz = data[i1].velocity[2];
                float tcx = data[i1].tangent[0], tcy = data[i1].tangent[1], tcz = data[i1].tangent[2];
                float ncx = data[i1].normal[0], ncy = data[i1].normal[1], ncz = data[i1].normal[2];
                float ccx = data[i1].color[0], ccy = data[i1].color[1], ccz = data[i1].color[2], ccw = data[i1].color[3];

                float pdx = data[i2].position[0], pdy = data[i2].position[1], pdz = data[i2].position[2];
                float vdx = data[i2].velocity[0], vdy = data[i2].velocity[1], vdz = data[i2].velocity[2];
                float tdx = data[i2].tangent[0], tdy = data[i2].tangent[1], tdz = data[i2].tangent[2];
                float ndx = data[i2].normal[0], ndy = data[i2].normal[1], ndz = data[i2].normal[2];
                float cdx = data[i2].color[0], cdy = data[i2].color[1], cdz = data[i2].color[2], cdw = data[i2].color[3];

                for (int j = 0; j < smoothness; ++j)
                {
                    float t = j * samplesize;

                    if (float.IsInfinity(data[i_1].life) || float.IsInfinity(data[i].life) ||
                        float.IsInfinity(data[i1].life) || float.IsInfinity(data[i2].life))
                    interpolated.life = float.PositiveInfinity;
                    else
                    interpolated.life = Point.CatmullRom(data[i_1].life, data[i].life, data[i1].life, data[i2].life, t);

                    float dx = pcx - pbx;
                    float dy = pcy - pby;
                    float dz = pcz - pbz;
                    if (dx * dx + dy * dy + dz * dz < smoothingDistance * smoothingDistance)
                    {
                        renderablePoints.Add(data[i]);
                        break;
                    }

                    // only if the interpolated point is alive, we add it to the list of points to render.
                    if (interpolated.life > 0)
                    {

                        interpolated.position.x = Point.CatmullRom(pax, pbx, pcx, pdx, t);
                        interpolated.position.y = Point.CatmullRom(pay, pby, pcy, pdy, t);
                        interpolated.position.z = Point.CatmullRom(paz, pbz, pcz, pdz, t);

                        interpolated.velocity.x = Point.CatmullRom(vax, vbx, vcx, vdx, t);
                        interpolated.velocity.y = Point.CatmullRom(vay, vby, vcy, vdy, t);
                        interpolated.velocity.z = Point.CatmullRom(vaz, vbz, vcz, vdz, t);

                        interpolated.tangent.x = Point.CatmullRom(tax, tbx, tcx, tdx, t);
                        interpolated.tangent.y = Point.CatmullRom(tay, tby, tcy, tdy, t);
                        interpolated.tangent.z = Point.CatmullRom(taz, tbz, tcz, tdz, t);

                        interpolated.normal.x = Point.CatmullRom(nax, nbx, ncx, ndx, t);
                        interpolated.normal.y = Point.CatmullRom(nay, nby, ncy, ndy, t);
                        interpolated.normal.z = Point.CatmullRom(naz, nbz, ncz, ndz, t);

                        interpolated.color.r = Point.CatmullRom(cax, cbx, ccx, cdx, t);
                        interpolated.color.g = Point.CatmullRom(cay, cby, ccy, cdy, t);
                        interpolated.color.b = Point.CatmullRom(caz, cbz, ccz, cdz, t);
                        interpolated.color.a = Point.CatmullRom(caw, cbw, ccw, cdw, t);

                        interpolated.thickness = Point.CatmullRom(data[i_1].thickness, data[i].thickness, data[i1].thickness, data[i2].thickness, t);
                        interpolated.texcoord = Point.CatmullRom(data[i_1].texcoord, data[i].texcoord, data[i1].texcoord, data[i2].texcoord, t);

                        renderablePoints.Add(interpolated);
                    }
                }

            }

            if (points[end].life > 0)
                renderablePoints.Add(points[end]);

            return renderablePoints;
        }

        /**
         * Initializes the frame used to generate the locally aligned trail mesh.
         */
        private CurveFrame InitializeCurveFrame(Vector3 point, Vector3 nextPoint)
        {

            Vector3 tgnt = nextPoint - point;

            // Calculate tangent proximity to the normal vector of the frame (transform.forward).
            float tangentProximity = Mathf.Abs(Vector3.Dot(tgnt.normalized, transform.forward));

            // If both vectors are dangerously close, skew the tangent a bit so that a proper frame can be formed:
            if (Mathf.Approximately(tangentProximity, 1))
                tgnt += transform.right * 0.01f;

            // Generate and return the frame:
            return new CurveFrame(point, transform.forward, transform.up, tgnt);
        }

        /**
         * Updates the trail mesh to be seen from the camera passed to the function.
         */
        private void UpdateTrailMesh(Camera cam)
        {

            if ((cam.cullingMask & (1 << gameObject.layer)) == 0)
                return;

            ClearMeshData();

            // We need at least two points to create a trail mesh.
            if (points.Count > 1)
            {
                Vector3 localCamPosition = worldToTrail.MultiplyPoint3x4(cam.transform.position);

                // get discontinuous point indices:
                discontinuities.Clear();
                for (int i = 0; i < points.Count; ++i)
                    if (points[i].discontinuous || i == points.Count - 1) discontinuities.Add(i);

                // generate mesh for each trail segment:
                int start = 0;
                for (int i = 0; i < discontinuities.Count; ++i)
                {
                    UpdateSegmentMesh(start, discontinuities[i], localCamPosition);
                    start = discontinuities[i] + 1;
                }

                CommitMeshData();

                RenderMesh(cam);
            }
        }

        /**
         * Updates mesh for one trail segment:
         */
        private void UpdateSegmentMesh(int start, int end, Vector3 localCamPosition)
        {

            // Get a list of the actual points to render: either the original, unsmoothed points or the smoothed curve.
            ElasticArray<Point> trail = GetRenderablePoints(start, end);

            if (sorting == TrailSorting.NewerOnTop)
                trail.Reverse();

            var data = trail.Data;

            if (trail.Count > 1)
            {

                float totalLength = 0;
                for (int i = 0; i < trail.Count - 1; ++i)
                    totalLength += Vector3.Distance(data[i].position, data[i + 1].position);

                totalLength = Mathf.Max(totalLength, epsilon);
                float partialLength = 0;
                float vCoord = textureMode == TextureMode.Stretch ? 0 : -uvFactor * totalLength * tileAnchor;

                if (sorting == TrailSorting.NewerOnTop)
                    vCoord = 1 - vCoord;

                // Initialize curve frame using the first two points to calculate the first tangent vector:
                CurveFrame frame = InitializeCurveFrame(data[trail.Count - 1].position,
                                                        data[trail.Count - 2].position);

                int va = 1;
                int vb = 0;

                for (int i = trail.Count - 1; i >= 0; --i)
                {

                    // Calculate next and previous point indices:
                    int nextIndex = Mathf.Max(i - 1, 0);
                    int prevIndex = Mathf.Min(i + 1, trail.Count - 1);

                    // Calculate next and previous trail vectors:
                    nextV.x = data[nextIndex].position.x - data[i].position.x;
                    nextV.y = data[nextIndex].position.y - data[i].position.y;
                    nextV.z = data[nextIndex].position.z - data[i].position.z;

                    prevV.x = data[i].position.x - data[prevIndex].position.x;
                    prevV.y = data[i].position.y - data[prevIndex].position.y;
                    prevV.z = data[i].position.z - data[prevIndex].position.z;

                    float sectionLength = nextIndex == i ? prevV.magnitude : nextV.magnitude;

                    nextV.Normalize();
                    prevV.Normalize();

                    // Calculate tangent vector:
                    if (alignment == TrailAlignment.Local)
                        tangent = data[i].tangent.normalized;
                    else
                    {
                        tangent.x = (nextV.x + prevV.x) * 0.5f;
                        tangent.y = (nextV.y + prevV.y) * 0.5f;
                        tangent.z = (nextV.z + prevV.z) * 0.5f;
                    }

                    // Calculate normal vector:
                    normal = data[i].normal;
                    if (alignment != TrailAlignment.Local)
                        normal = alignment == TrailAlignment.View ? localCamPosition - data[i].position : frame.Transport(tangent, data[i].position);
                    normal.Normalize();

                    // Calculate bitangent vector:
                    if (alignment == TrailAlignment.Velocity)
                        bitangent = frame.bitangent;
                    else
                    {
                        // cross(tangent, normal):
                        bitangent.x = tangent.y * normal.z - tangent.z * normal.y;
                        bitangent.y = tangent.z * normal.x - tangent.x * normal.z;
                        bitangent.z = tangent.x * normal.y - tangent.y * normal.x;
                    }
                    bitangent.Normalize();

                    // Calculate this point's normalized (0,1) lenght and life.
                    float normalizedLength = sorting == TrailSorting.OlderOnTop ? partialLength / totalLength : (totalLength - partialLength) / totalLength;
                    float normalizedLife = float.IsInfinity(time) ? 1 : Mathf.Clamp01(1 - data[i].life / time);
                    partialLength += sectionLength;

                    // Calulate vertex color:
                    color = data[i].color * colorOverTime.Evaluate(normalizedLife) * colorOverLength.Evaluate(normalizedLength);

                    // Calulate final thickness:
                    float sectionThickness = thickness * data[i].thickness * thicknessOverTime.Evaluate(normalizedLife) * thicknessOverLength.Evaluate(normalizedLength);

                    // In world tile mode, override texture coordinate with the point's one:
                    if (textureMode == TextureMode.WorldTile)
                        vCoord = tileAnchor + data[i].texcoord * uvFactor;

                    if (section != null)
                        AppendSection(data, ref frame, i, trail.Count, sectionThickness, vCoord);
                    else
                        AppendFlatTrail(data, ref frame, i, trail.Count, sectionThickness, vCoord, ref va, ref vb);

                    // Update vcoord:
                    float uvDelta = (textureMode == TextureMode.Stretch ? sectionLength / totalLength : sectionLength);
                    vCoord += uvFactor * (sorting == TrailSorting.NewerOnTop ? -uvDelta : uvDelta);
                }
            }
        }

        private void AppendSection(Point[] data, ref CurveFrame frame, int i, int count, float sectionThickness, float vCoord)
        {
            // Loop around each segment:
            int sectionSegments = section.Segments;
            int verticesPerSection = sectionSegments + 1;

            int vc = vertices.Count; 

            for (int j = 0; j <= sectionSegments; ++j)
            {
                // calculate normal using section vertex, curve normal and binormal:
                normal.x = (section.vertices[j].x * bitangent.x + section.vertices[j].y * tangent.x) * sectionThickness;
                normal.y = (section.vertices[j].x * bitangent.y + section.vertices[j].y * tangent.y) * sectionThickness;
                normal.z = (section.vertices[j].x * bitangent.z + section.vertices[j].y * tangent.z) * sectionThickness;

                // offset curve position by normal:
                vertex.x = data[i].position.x + normal.x;
                vertex.y = data[i].position.y + normal.y;
                vertex.z = data[i].position.z + normal.z;

                // cross(normal, curve tangent)
                texTangent.x = -(normal.y * frame.tangent.z - normal.z * frame.tangent.y);
                texTangent.y = -(normal.z * frame.tangent.x - normal.x * frame.tangent.z);
                texTangent.z = -(normal.x * frame.tangent.y - normal.y * frame.tangent.x);
                texTangent.w = 1;

                uv.x = (j / (float)sectionSegments) * uvWidthFactor;
                uv.y = vCoord;
                uv.z = 0;
                uv.w = 1;

                vertices.Add(vertex);
                normals.Add(normal);
                tangents.Add(texTangent);
                uvs.Add(uv);
                vertColors.Add(color);

                if (j < sectionSegments && i < count - 1)
                {
                    tris.Add(vc + j);
                    tris.Add(vc + (j + 1));
                    tris.Add(vc - verticesPerSection + j);

                    tris.Add(vc + (j + 1));
                    tris.Add(vc - verticesPerSection + (j + 1));
                    tris.Add(vc - verticesPerSection + j);
                }
            }
        }

        private void AppendFlatTrail(Point[] data, ref CurveFrame frame, int i, int count, float sectionThickness, float vCoord, ref int va, ref int vb)
        {

            bool hqCorners = highQualityCorners && alignment != TrailAlignment.Local;

            Quaternion q = Quaternion.identity;
            Vector3 corner = Vector3.zero;
            float curvatureSign = 0;
            float correctedThickness = sectionThickness;
            Vector3 prevSectionBitangent = bitangent;

            // High-quality corners: 
            if (hqCorners)
            {

                Vector3 nextSectionBitangent = i == 0 ? bitangent : Vector3.Cross(nextV, Vector3.Cross(bitangent, tangent)).normalized;

                // If round corners are enabled:
                if (cornerRoundness > 0)
                {

                    prevSectionBitangent = i == count - 1 ? -bitangent : Vector3.Cross(prevV, Vector3.Cross(bitangent, tangent)).normalized;

                    // Calculate "elbow" angle:
                    curvatureSign = (i == 0 || i == count - 1) ? 1 : Mathf.Sign(Vector3.Dot(nextV, -prevSectionBitangent));
                    float angle = (i == 0 || i == count - 1) ? Mathf.PI : Mathf.Acos(Mathf.Clamp(Vector3.Dot(nextSectionBitangent, prevSectionBitangent), -1, 1));

                    // Prepare a quaternion for incremental rotation of the corner vector:
                    q = Quaternion.AngleAxis(Mathf.Rad2Deg * angle / cornerRoundness, normal * curvatureSign);
                    corner = prevSectionBitangent * sectionThickness * curvatureSign;
                }

                // Calculate correct thickness by projecting corner bitangent onto the next section bitangent. This prevents "squeezing"
                if (nextSectionBitangent.sqrMagnitude > 0.1f)
                    correctedThickness = sectionThickness / Mathf.Max(Vector3.Dot(bitangent, nextSectionBitangent), 0.15f);

            }


            // Append straight section mesh data:
            if (hqCorners && cornerRoundness > 0)
            {

                // bitangents are slightly asymmetrical in case of high-quality round or sharp corners:
                if (curvatureSign > 0)
                {
                    vertices.Add(data[i].position + prevSectionBitangent * sectionThickness);
                    vertices.Add(data[i].position - bitangent * correctedThickness);
                }
                else
                {
                    vertices.Add(data[i].position + bitangent * correctedThickness);
                    vertices.Add(data[i].position - prevSectionBitangent * sectionThickness);
                }

            }
            else
            {
                vertices.Add(data[i].position + bitangent * correctedThickness);
                vertices.Add(data[i].position - bitangent * correctedThickness);
            }

            normals.Add(normal);
            normals.Add(normal);

            tangents.Add(tangent);
            tangents.Add(tangent);

            vertColors.Add(color);
            vertColors.Add(color);

            if (quadMapping)
            {
                // passing perspective-correct coords requires the use of tex2Dproj in the shader, instead of tex2D.
                uv.Set(vCoord * sectionThickness, sorting == TrailSorting.NewerOnTop ? uvWidthFactor * sectionThickness : 0, 0, sectionThickness);
                uvs.Add(uv);
                uv.Set(vCoord * sectionThickness, sorting == TrailSorting.NewerOnTop ? 0 : uvWidthFactor * sectionThickness, 0, sectionThickness);
                uvs.Add(uv);
            }
            else
            {
                uv.Set(vCoord, sorting == TrailSorting.NewerOnTop ? uvWidthFactor : 0, 0, 1);
                uvs.Add(uv);
                uv.Set(vCoord, sorting == TrailSorting.NewerOnTop ? 0 : uvWidthFactor, 0, 1);
                uvs.Add(uv);
            }


            if (i < count - 1)
            {

                int vc = vertices.Count - 1;

                tris.Add(vc);
                tris.Add(va);
                tris.Add(vb);

                tris.Add(vb);
                tris.Add(vc - 1);
                tris.Add(vc);
            }

            va = vertices.Count - 1;
            vb = vertices.Count - 2;

            // Append smooth corner mesh data:
            if (hqCorners && cornerRoundness > 0)
            {

                for (int p = 0; p <= cornerRoundness; ++p)
                {

                    vertices.Add(data[i].position + corner);
                    normals.Add(normal);
                    tangents.Add(tangent);
                    vertColors.Add(color);
                    uv.Set(vCoord, curvatureSign > 0 ? 0 : 1, 0, 1);
                    uvs.Add(uv);

                    int vc = vertices.Count - 1;

                    tris.Add(vc);
                    tris.Add(va);
                    tris.Add(vb);

                    if (curvatureSign > 0)
                        vb = vc;
                    else va = vc;

                    // rotate corner point:
                    corner = q * corner;
                }

            }
        }
    }
}
