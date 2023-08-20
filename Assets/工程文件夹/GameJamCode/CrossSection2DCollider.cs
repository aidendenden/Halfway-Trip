using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class CrossSection2DCollider : MonoBehaviour
{
    public Vector3 planeNormal = Vector3.up; // 默认裁剪平面的法线
    public Vector3 planePoint = Vector3.zero; // 默认裁剪平面上的一点

    private void Start()
    {
        Create2DCollider();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Create2DCollider();
        }
    }

    private void Create2DCollider()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        List<Vector3> intersectionPoints = GetIntersectionPoints(mesh);

        List<Vector2> projectedPoints = intersectionPoints.Select(p => new Vector2(p.x, p.z)).ToList();
        projectedPoints = ComputeConvexHull(projectedPoints);

        if (projectedPoints.Count == 0)
        {
            Debug.LogWarning("No intersection points found with the clipping plane.");
            return;
        }

        // 计算交点的中心
        Vector2 center = new Vector2(projectedPoints.Average(p => p.x), projectedPoints.Average(p => p.y));

        // 创建新的2D碰撞体
        GameObject colliderObject = new GameObject("2D Collider");
        colliderObject.transform.position = new Vector3(center.x, 0, center.y);
        PolygonCollider2D collider = colliderObject.AddComponent<PolygonCollider2D>();
        collider.points = projectedPoints.Select(p => p - center).ToArray();

        // 创建新的Mesh来显示2D横切面
        GameObject sectionObject = new GameObject("2D Section");
        sectionObject.transform.position = new Vector3(center.x, 0, center.y);

        Mesh sectionMesh = new Mesh();
        Vector3[] meshVertices = projectedPoints.Select(p => new Vector3(p.x - center.x, 0, p.y - center.y)).ToArray();
        int[] triangles = Triangulate(projectedPoints);

        sectionMesh.vertices = meshVertices;
        sectionMesh.triangles = triangles;
        sectionMesh.RecalculateNormals();

        MeshFilter meshFilter = sectionObject.AddComponent<MeshFilter>();
        meshFilter.mesh = sectionMesh;

        MeshRenderer meshRenderer = sectionObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard"));
        meshRenderer.material.color = Color.green; // 设置颜色
    }




    
    
    private int[] Triangulate(List<Vector2> points)
    {
        List<int> triangles = new List<int>();
        for (int i = 1; i < points.Count - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }
        return triangles.ToArray();
    }



    private List<Vector3> GetIntersectionPoints(Mesh mesh)
    {
        List<Vector3> intersectionPoints = new List<Vector3>();
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            for (int j = 0; j < 3; j++)
            {
                Vector3 startVertex = transform.TransformPoint(vertices[triangles[i + j]]);
                Vector3 endVertex = transform.TransformPoint(vertices[triangles[i + (j + 1) % 3]]);
                Vector3 intersectionPoint;

                if (LinePlaneIntersection(startVertex, endVertex, planeNormal, planePoint, out intersectionPoint))
                {
                    intersectionPoints.Add(intersectionPoint);
                }
            }
        }

        return intersectionPoints;
    }

    private bool LinePlaneIntersection(Vector3 lineStart, Vector3 lineEnd, Vector3 planeNormal, Vector3 planePoint, out Vector3 intersection)
    {
        intersection = Vector3.zero;
        Vector3 lineDir = lineEnd - lineStart;
        float dotNumerator = Vector3.Dot(planePoint - lineStart, planeNormal);
        float dotDenominator = Vector3.Dot(lineDir, planeNormal);

        if (dotDenominator == 0.0f)
            return false;

        float t = dotNumerator / dotDenominator;
        if (t < 0.0f || t > 1.0f)
            return false;

        intersection = lineStart + t * lineDir;
        return true;
    }

    private List<Vector2> ComputeConvexHull(List<Vector2> points)
    {
        if (points.Count <= 3)
            return points;

        // 1. 找到基点
        Vector2 pivot = points[0];
        int pivotIndex = 0;
        for (int i = 1; i < points.Count; i++)
        {
            if (points[i].y < pivot.y || (points[i].y == pivot.y && points[i].x < pivot.x))
            {
                pivot = points[i];
                pivotIndex = i;
            }
        }
        points.RemoveAt(pivotIndex);

        // 2. 根据极角排序
        points.Sort((a, b) =>
        {
            float angleA = Mathf.Atan2(a.y - pivot.y, a.x - pivot.x);
            float angleB = Mathf.Atan2(b.y - pivot.y, b.x - pivot.x);
            if (angleA < angleB) return -1;
            if (angleA > angleB) return 1;
            return (a - pivot).sqrMagnitude.CompareTo((b - pivot).sqrMagnitude);
        });

        // 3. 构建凸包
        List<Vector2> hull = new List<Vector2> { pivot, points[0], points[1] };
        for (int i = 2; i < points.Count; i++)
        {
            while (hull.Count >= 2 && CrossProduct(hull[hull.Count - 2], hull[hull.Count - 1], points[i]) <= 0)
            {
                hull.RemoveAt(hull.Count - 1);
            }
            hull.Add(points[i]);
        }

        return hull;
    }

    private float CrossProduct(Vector2 o, Vector2 a, Vector2 b)
    {
        return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
    }

    void OnDrawGizmos()
    {
        // 绘制planePoint位置的小球
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(planePoint, 0.1f);

        // 绘制表示planeNormal的线
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(planePoint, planePoint + planeNormal);
    }
}
