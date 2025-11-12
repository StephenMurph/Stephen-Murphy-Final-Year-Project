using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MeshNode
{
    public Vector3 position;
    public List<MeshNode> connections = new List<MeshNode>();
    
    public MeshNode parent;
    public float gCost;
    public float hCost; 
    public float fCost => gCost + hCost;
}

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    
    Vector3[] vertices;
    int[] triangles;
    public MeshNode[,] nodes;

    public int xSize = 20;
    public int zSize = 20;
    public void GenerateMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        CreateShape();
        UpdateMesh();
        CreateNodes();
        ConnectNodes();
    }
    
    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }
        
        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSize; z++)
        {
            for(int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    void CreateNodes()
    {
        nodes = new MeshNode[xSize + 1, zSize + 1];

        int i = 0;
        for (int z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                MeshNode node = new MeshNode();
                node.position = vertices[i];
                nodes[x, z] = node;
                i++;
            }
        }
    }
    
    void ConnectNodes()
    {
        for (int z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                MeshNode node = nodes[x, z];
                
                for (int nz = z - 1; nz <= z + 1; nz++)
                {
                    for (int nx = x - 1; nx <= x + 1; nx++)
                    {
                        if (nx == x && nz == z) continue;
                        if (nx < 0 || nx > xSize || nz < 0 || nz > zSize) continue;

                        node.connections.Add(nodes[nx, nz]);
                    }
                }
            }
        }
    }
    
    public Vector3 GetNodeNormal(int x, int z)
    {
        if (x < 0 || x > xSize || z < 0 || z > zSize) return Vector3.up;
    
        mesh.RecalculateNormals();
        int i = z * (xSize + 1) + x;
        return mesh.normals[i];
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        
        mesh.RecalculateNormals();
    }
    
    /*void OnDrawGizmos()
    {
        if (nodes == null) return;
        Gizmos.color = Color.red;

        foreach (MeshNode n in nodes)
        {
            if (n != null)
                Gizmos.DrawSphere(n.position, 0.1f);
        }
    }*/
}
