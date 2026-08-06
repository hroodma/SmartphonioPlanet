using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BillboardGrassSpawner : MonoBehaviour
{
    [Header("Настройки")]
    public float sphereRadius = 5f;
    public int grassCount = 15000; // Количество травинок
    public float grassHeight = 0.4f;
    public float grassWidth = 0.15f;
    public Material grassMaterial;

    void Start()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Color> colors = new List<Color>();

        // Базовые вершины одного биллборда (плоский квадратик)
        // Координаты: X - ширина, Y - высота
        Vector3[] baseVerts = new Vector3[4];
        baseVerts[0] = new Vector3(-0.5f, 0, 0); // Низ лево
        baseVerts[1] = new Vector3(0.5f, 0, 0);  // Низ право
        baseVerts[2] = new Vector3(-0.5f, 1, 0); // Верх лево
        baseVerts[3] = new Vector3(0.5f, 1, 0);  // Верх право

        int[] baseTris = new int[6] { 0, 1, 2, 1, 3, 2 };
        Vector2[] baseUVs = new Vector2[4] {
            new Vector2(0, 0), new Vector2(1, 0),
            new Vector2(0, 1), new Vector2(1, 1)
        };

        for (int i = 0; i < grassCount; i++)
        {
            // 1. Случайная точка на сфере
            Vector3 normal = Random.onUnitSphere.normalized;

            Vector3 position = normal * sphereRadius;

            // 2. Поворот: сначала выравниваем по нормали, потом случайный поворот вокруг оси
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, normal);
            rot *= Quaternion.Euler(0, Random.Range(0, 360), 0);

            Vector3 scale = new Vector3(grassWidth, grassHeight, 1);

            int startVertex = vertices.Count;

            for (int v = 0; v < 4; v++)
            {
                // Трансформируем локальную вершину биллборда в мировую
                Vector3 localPos = Vector3.Scale(baseVerts[v], scale);
                Vector3 worldPos = position + rot * localPos;

                vertices.Add(worldPos);
                uvs.Add(baseUVs[v]);

                // ВАЖНО: Кодируем данные в цвет вершины для шейдера
                // R, G, B = Нормаль сферы (направление роста)
                // A = Высота вершины (0 для низа, 1 для верха)
                float heightFactor = baseVerts[v].y;
                Color col = new Color(normal.x, normal.y, normal.z, heightFactor);
                colors.Add(col);
            }

            for (int t = 0; t < 6; t++)
            {
                triangles.Add(baseTris[t] + startVertex);
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.SetColors(colors);
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = grassMaterial;

        Debug.Log($"Сгенерировано {grassCount} биллбордов.");
    }
}