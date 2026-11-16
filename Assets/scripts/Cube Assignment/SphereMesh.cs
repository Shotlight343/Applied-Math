using System.Collections.Generic;
using UnityEngine;

public class SphereMesh : MonoBehaviour
{
    [SerializeField] private Material lineMat;
    [SerializeField] private Vector3 position = Vector3.zero;
    [SerializeField] private float scale = 1;
    [SerializeField] private float yRotation = 0;
    [SerializeField] private float xRotation = 0;

    [Range(3, 64)] [SerializeField] private int segments = 12;
    [Range(2, 64)] [SerializeField] private int rings = 8;

    [Range(1, 500)] [SerializeField] private float focalLength = 20f;

    private readonly List<Vector3> verts = new();
    private readonly List<int[]> faces = new();
    private readonly List<int> visibleEdges = new();
    private readonly List<Vector3> actualPosition = new();

    private Mesh mesh;

    private void Awake()
    {
        BuildSphere();

        mesh = new Mesh();
        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        gameObject.AddComponent<MeshRenderer>().material = lineMat;
    }

    private void LateUpdate()
    {
        actualPosition.Clear();
        foreach (var point in verts)
        {
            actualPosition.Add(Project(point));
        }

        mesh.vertices = actualPosition.ToArray();
        visibleEdges.Clear();

        foreach (var f in faces)
        {
            if (!FacesCamera(actualPosition[f[0]], actualPosition[f[1]], actualPosition[f[2]])) continue;
            for (int i = 0; i < f.Length; i++)
            {
                visibleEdges.Add(f[i]);
                visibleEdges.Add(f[(i + 1) % f.Length]);
            }
        }

        mesh.SetIndices(visibleEdges, MeshTopology.Lines, 0);
        mesh.RecalculateBounds();
    }

    private void BuildSphere()
    {
        verts.Clear();
        faces.Clear();

        verts.Add(new Vector3(0, 1, 0));

        for (int r = 1; r < rings; r++)
        {
            var theta = Mathf.PI * r / rings;
            for (int s = 0; s < segments; s++)
            {
                var phi = 2f * Mathf.PI * s / segments;
                verts.Add(new Vector3(
                    Mathf.Sin(theta) * Mathf.Cos(phi),
                    Mathf.Cos(theta),
                    Mathf.Sin(theta) * Mathf.Sin(phi)));
            }
        }

        verts.Add(new Vector3(0, -1, 0));

        var top = 0;
        var bottom = verts.Count - 1;

        for (int s = 0; s < segments; s++)
        {
            faces.Add(new[] { top, RingIndex(1, s), RingIndex(1, s + 1) });
        }

        for (int r = 1; r < rings - 1; r++)
        {
            for (int s = 0; s < segments; s++)
            {
                faces.Add(new[]
                {
                    RingIndex(r, s),
                    RingIndex(r + 1, s),
                    RingIndex(r + 1, s + 1),
                    RingIndex(r, s + 1)
                });
            }
        }

        for (int s = 0; s < segments; s++)
        {
            faces.Add(new[] { RingIndex(rings - 1, s), bottom, RingIndex(rings - 1, s + 1) });
        }
    }

    private int RingIndex(int ring, int segment)
    {
        return 1 + (ring - 1) * segments + segment % segments;
    }

    private Vector2 Project(Vector3 point)
    {
        point *= .5f;

        var xRot = point.x * Mathf.Cos(yRotation * Mathf.Deg2Rad) - point.z * Mathf.Sin(yRotation * Mathf.Deg2Rad);
        var zRot = point.x * Mathf.Sin(yRotation * Mathf.Deg2Rad) + point.z * Mathf.Cos(yRotation * Mathf.Deg2Rad);
        point = new Vector3(xRot, point.y, zRot);

        var yRotX = point.y * Mathf.Cos(xRotation * Mathf.Deg2Rad) - point.z * Mathf.Sin(xRotation * Mathf.Deg2Rad);
        var zRotX = point.y * Mathf.Sin(xRotation * Mathf.Deg2Rad) + point.z * Mathf.Cos(xRotation * Mathf.Deg2Rad);
        point = new Vector3(point.x, yRotX, zRotX);

        point = new Vector3(point.x + position.x, point.y + position.y, point.z + position.z);

        var p = focalLength / (focalLength + point.z);

        return new Vector2(point.x, point.y) * scale * p;
    }

    private bool FacesCamera(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b - a).x * (c - a).y - (b - a).y * (c - a).x > 0;
    }
}