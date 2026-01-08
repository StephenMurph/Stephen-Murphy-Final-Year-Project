using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    List<MeshNode> _currentPath;
    Dictionary<(string, string), List<MeshNode>> _allTownPaths = new();
    
    public GameObject playerPrefab;
    public MeshGenerator meshGenerator;

    public float moveSpeed = 2f;
    public float hopHeight = 0.3f;

    [Header("Trade")]
    public TradeMenuUI tradeMenu;
    public ItemData goldItem;
    public int startingGold = 50;

    public TownData townASettlement;
    public TownData townBSettlement;
    public TownData townCSettlement;

    GameObject _player;
    PlayerInventory _inv;
    
    class Town
    {
        public string name;
        public MeshNode node;
        public TownData settlement;
    }

    Dictionary<string, Town> _towns = new();
    string _currentTownName;

    void Start()
    {
        meshGenerator.GenerateMesh();
        CreateRandomTowns();
        PrecomputeTownPaths();
        SpawnPlayerAt("Town A");
        OpenTownMenu();
    }

    void CreateRandomTowns()
    {
        _towns.Clear();

        var nodes = meshGenerator.Nodes;
        int xSize = meshGenerator.xSize;
        int zSize = meshGenerator.zSize;
        
        MeshNode PickNode(HashSet<(int,int)> used)
        {
            while (true)
            {
                int x = Random.Range(0, xSize + 1);
                int z = Random.Range(0, zSize + 1);
                if (used.Add((x, z))) return nodes[x, z];
            }
        }

        var usedCoords = new HashSet<(int,int)>();

        _towns["Town A"] = new Town { name = "Town A", node = PickNode(usedCoords), settlement = townASettlement };
        _towns["Town B"] = new Town { name = "Town B", node = PickNode(usedCoords), settlement = townBSettlement };
        _towns["Town C"] = new Town { name = "Town C", node = PickNode(usedCoords), settlement = townCSettlement };
    }

    void SpawnPlayerAt(string townName)
    {
        _currentTownName = townName;

        var town = _towns[townName];
        Vector3 spawnPos = town.node.position + Vector3.up * 0.08f;

        _player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        
        _inv = _player.GetComponent<PlayerInventory>();
        if (_inv == null) _inv = _player.AddComponent<PlayerInventory>();
        _inv.goldItem = goldItem;
        _inv.AddGold(startingGold);
        
        var mainCam = Camera.main;
        if (mainCam != null)
        {
            var cam = mainCam.GetComponent<CameraManager>();
            if (cam != null) cam.SetTarget(_player.transform);
        }
    }

    void OpenTownMenu()
    {
        if (tradeMenu == null) return;

        var town = _towns[_currentTownName];
        
        var dests = new List<string>();
        foreach (var kv in _towns)
            if (kv.Key != _currentTownName)
                dests.Add(kv.Key);

        tradeMenu.Open(
            town.settlement,
            _inv,
            town.name,
            dests,
            OnTravelRequested
        );
    }

    void OnTravelRequested(string destinationTownName)
    {
        Debug.Log("Travel requested to: " + destinationTownName);
        StartCoroutine(TravelTo(destinationTownName));
    }
    
    void PrecomputeTownPaths()
    {
        _allTownPaths.Clear();

        foreach (var a in _towns)
        {
            foreach (var b in _towns)
            {
                if (a.Key == b.Key) continue;
                
                foreach (var n in meshGenerator.Nodes)
                {
                    if (n == null) continue;
                    n.GCost = float.PositiveInfinity;
                    n.HCost = 0f;
                    n.Parent = null;
                }
                a.Value.node.GCost = 0f;

                var path = Pathfinding.FindPath(a.Value.node, b.Value.node);
                if (path != null && path.Count > 0)
                {
                    _allTownPaths[(a.Key, b.Key)] = path;
                }
            }
        }
    }

    IEnumerator TravelTo(string destinationTownName)
    {
        Debug.Log($"TravelTo started: {_currentTownName} -> {destinationTownName}");
        var startTown = _towns[_currentTownName];
        var endTown = _towns[destinationTownName];
        
        foreach (var n in meshGenerator.Nodes)
        {
            if (n == null) continue;
            n.GCost = float.PositiveInfinity;
            n.HCost = 0f;
            n.Parent = null;
        }
        startTown.node.GCost = 0f;

        _currentPath = Pathfinding.FindPath(startTown.node, endTown.node);
        var path = _currentPath;
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning($"No path found from {_currentTownName} to {destinationTownName}");
            yield break;
        }
        
        foreach (MeshNode node in path)
        {
            Vector3 startPos = _player.transform.position;
            Vector3 endPos = node.position + Vector3.up * 0.08f;
            float distance = Vector3.Distance(startPos, endPos);
            float duration = distance / moveSpeed;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                float height = Mathf.Sin(t * Mathf.PI) * hopHeight;
                _player.transform.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * height;

                Vector3 dir = (endPos - startPos).normalized;
                if (dir != Vector3.zero)
                    _player.transform.rotation = Quaternion.Slerp(_player.transform.rotation, Quaternion.LookRotation(dir), 0.2f);

                yield return null;
            }

            _player.transform.position = endPos;
        }
        
        _currentTownName = destinationTownName;
        OpenTownMenu();
    }
    void OnDrawGizmos()
    {
        if (_towns == null || _towns.Count == 0) return;
        
        DrawTown("Town A", Color.green);
        DrawTown("Town B", Color.red);
        DrawTown("Town C", Color.cyan);
        
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.6f); 
        foreach (var kv in _allTownPaths)
        {
            var path = kv.Value;
            if (path == null || path.Count < 2) continue;
            
            if (_currentPath == path) continue;

            DrawPath(path);
        }
        
        if (_currentPath != null && _currentPath.Count > 1)
        {
            Gizmos.color = Color.yellow;
            DrawPath(_currentPath);
        }
    }

    void DrawTown(string name, Color color)
    {
        if (!_towns.TryGetValue(name, out var town)) return;
        if (town.node == null) return;

        Gizmos.color = color;
        Gizmos.DrawSphere(town.node.position, 0.3f);
    }

    void DrawPath(List<MeshNode> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(path[i].position, path[i + 1].position);
        }
    }
}
