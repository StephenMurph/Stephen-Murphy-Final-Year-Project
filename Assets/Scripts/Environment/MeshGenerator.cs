using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MeshNode
{
    public Vector3 position;
    [System.NonSerialized] public List<MeshNode> Connections = new List<MeshNode>();
    
    [System.NonSerialized] public MeshNode Parent;
    [System.NonSerialized] public float GCost;
    [System.NonSerialized] public float HCost; 
    public float FCost => GCost + HCost;
}

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh _mesh;
    
    Vector3[] _vertices;
    int[] _triangles;
    public MeshNode[,] Nodes;

    public int xSize = 20;
    public int zSize = 20;
    public void GenerateMesh()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
        
        CreateShape();
        UpdateMesh();
        CreateNodes();
        ConnectNodes();
    }
    
    void CreateShape()
    {
        _vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;
                _vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }
        
        _triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSize; z++)
        {
            for(int x = 0; x < xSize; x++)
            {
                _triangles[tris + 0] = vert + 0;
                _triangles[tris + 1] = vert + xSize + 1;
                _triangles[tris + 2] = vert + 1;
                _triangles[tris + 3] = vert + 1;
                _triangles[tris + 4] = vert + xSize + 1;
                _triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    void CreateNodes()
    {
        Nodes = new MeshNode[xSize + 1, zSize + 1];

        int i = 0;
        for (int z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                MeshNode node = new MeshNode();
                node.position = _vertices[i];
                Nodes[x, z] = node;
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
                MeshNode node = Nodes[x, z];
                
                for (int nz = z - 1; nz <= z + 1; nz++)
                {
                    for (int nx = x - 1; nx <= x + 1; nx++)
                    {
                        if (nx == x && nz == z) continue;
                        if (nx < 0 || nx > xSize || nz < 0 || nz > zSize) continue;

                        node.Connections.Add(Nodes[nx, nz]);
                    }
                }
            }
        }
    }
    
    public Vector3 GetNodeNormal(int x, int z)
    {
        if (x < 0 || x > xSize || z < 0 || z > zSize) return Vector3.up;
    
        _mesh.RecalculateNormals();
        int i = z * (xSize + 1) + x;
        return _mesh.normals[i];
    }

    void UpdateMesh()
    {
        _mesh.Clear();

        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        
        _mesh.RecalculateNormals();
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
