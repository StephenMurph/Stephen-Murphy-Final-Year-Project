using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public MeshGenerator meshGenerator;
    
    public float moveSpeed = 2f; 
    public float hopHeight = 0.3f; 
    
    
    private MeshNode _startNode;
    private MeshNode _endNode;
    private List<MeshNode> _path;
    
    void Start()
    {
        meshGenerator.GenerateMesh();
        SpawnPlayer();
    }
    
    void SpawnPlayer()
    {
        MeshNode[,] nodes = meshGenerator.Nodes;
        int xSize = meshGenerator.xSize;
        int zSize = meshGenerator.zSize;

        int startX = Random.Range(0, xSize + 1);
        int startZ = Random.Range(0, zSize + 1);
        int endX = Random.Range(0, xSize + 1);
        int endZ = Random.Range(0, zSize + 1);

        _startNode = nodes[startX, startZ];
        _endNode = nodes[endX, endZ];

        Vector3 spawnPos = _startNode.position + Vector3.up * 0.08f;
        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        
        var mainCam = Camera.main;
        if (mainCam != null)
        {
            var cam = mainCam.GetComponent<CameraManager>();
            if (cam != null) cam.SetTarget(player.transform);
        }
        else
        {
            Debug.LogWarning("No Camera tagged MainCamera found.");
        }

        _path = Pathfinding.FindPath(_startNode, _endNode);

        if (_path != null)
            StartCoroutine(MovePlayer(player, _path));
    }
    
    IEnumerator MovePlayer(GameObject player, List<MeshNode> path)
    {
        foreach (MeshNode node in path)
        {
            Vector3 startPos = player.transform.position;
            Vector3 endPos = node.position + Vector3.up * 0.08f;
            float distance = Vector3.Distance(startPos, endPos);
            float duration = distance / moveSpeed;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Hop arc
                float height = Mathf.Sin(t * Mathf.PI) * hopHeight;
                player.transform.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * height;

                // Face movement direction
                Vector3 dir = (endPos - startPos).normalized;
                if (dir != Vector3.zero)
                    player.transform.rotation = Quaternion.Slerp(player.transform.rotation, Quaternion.LookRotation(dir), 0.2f);

                yield return null;
            }

            player.transform.position = endPos; // ensure exact final position
        }
    }
    
    void OnDrawGizmos()
    {
        if (_startNode != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_startNode.position, 0.2f); // start node
        }

        if (_endNode != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_endNode.position, 0.2f); // end node
        }

        if (_path != null)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < _path.Count - 1; i++)
            {
                Gizmos.DrawLine(_path[i].position, _path[i + 1].position);
            }
        }
    }
}
