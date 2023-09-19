using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class PlaneGenerator : MonoBehaviour
{
    public int quadsForSide = 10;

    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<int> triangles = new List<int>();

    private void Start()
    {
        Mesh quadMesh = GetQuadMesh();
        Vector3 v0 = quadMesh.vertices[quadMesh.triangles[0]] + Vector3.one * 0.5f;
        Vector3 v1 = quadMesh.vertices[quadMesh.triangles[2]] + Vector3.one * 0.5f;
        Vector3 v2 = quadMesh.vertices[quadMesh.triangles[5]] + Vector3.one * 0.5f;
        Vector3 v3 = quadMesh.vertices[quadMesh.triangles[1]] + Vector3.one * 0.5f;

        float center = quadsForSide * 0.5f;
        float scale = 10f / quadsForSide;
        for (int i = 0; i < quadsForSide; i++)
        {
            float offsetX = i - center;
            for (int j = 0; j < quadsForSide; j++)
            {
                float offsetY = j - center;
                Vector3 nextV0 = new Vector3((v0.x + offsetX) * scale, 0, (v0.y + offsetY) * scale);
                Vector3 nextV1 = new Vector3((v1.x + offsetX) * scale, 0, (v1.y + offsetY) * scale);
                Vector3 nextV2 = new Vector3((v2.x + offsetX) * scale, 0, (v2.y + offsetY) * scale);
                Vector3 nextV3 = new Vector3((v3.x + offsetX) * scale, 0, (v3.y + offsetY) * scale);

                bool v0AlreadyInside = vertices.Contains(nextV0);
                bool v1AlreadyInside = vertices.Contains(nextV1);
                bool v2AlreadyInside = vertices.Contains(nextV2);
                bool v3AlreadyInside = vertices.Contains(nextV3);

                int v0Index, v1Index, v2Index, v3Index;

                if (!v0AlreadyInside) { vertices.Add(nextV0); uvs.Add(new Vector2((nextV0.x / 10f + 0.5f), (nextV0.z / 10f + 0.5f))); v0Index = vertices.Count - 1; } else v0Index = vertices.IndexOf(nextV0);
                if (!v1AlreadyInside) { vertices.Add(nextV1); uvs.Add(new Vector2((nextV1.x / 10f + 0.5f), (nextV1.z / 10f + 0.5f))); v1Index = vertices.Count - 1; } else v1Index = vertices.IndexOf(nextV1);
                if (!v2AlreadyInside) { vertices.Add(nextV2); uvs.Add(new Vector2((nextV2.x / 10f + 0.5f), (nextV2.z / 10f + 0.5f))); v2Index = vertices.Count - 1; } else v2Index = vertices.IndexOf(nextV2);
                if (!v3AlreadyInside) { vertices.Add(nextV3); uvs.Add(new Vector2((nextV3.x / 10f + 0.5f), (nextV3.z / 10f + 0.5f))); v3Index = vertices.Count - 1; } else v3Index = vertices.IndexOf(nextV3);

                triangles.Add(v0Index);
                triangles.Add(v3Index);
                triangles.Add(v1Index);
                triangles.Add(v3Index);
                triangles.Add(v0Index);
                triangles.Add(v2Index);
            }
        }

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    private Mesh GetQuadMesh()
    {
        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
        Destroy(gameObject);
        return mesh;
    }

    [ContextMenu("Save Current Mesh")]
    private void SaveMesh()
    {
        AssetDatabase.CreateAsset(gameObject.GetComponent<MeshFilter>().mesh, "Assets/GeneratedMesh.asset");
        AssetDatabase.SaveAssets();
    }
}
