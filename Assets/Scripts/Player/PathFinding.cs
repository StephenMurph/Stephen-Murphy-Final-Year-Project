using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    public static List<MeshNode> FindPath(MeshNode startNode, MeshNode targetNode)
    {
        List<MeshNode> openSet = new List<MeshNode>();
        HashSet<MeshNode> closedSet = new HashSet<MeshNode>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            MeshNode currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentNode.FCost ||
                    openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (MeshNode neighbor in currentNode.Connections)
            {
                if (closedSet.Contains(neighbor)) continue;

                float newGCost = currentNode.GCost + Vector3.Distance(currentNode.position, neighbor.position);
                if (newGCost < neighbor.GCost || !openSet.Contains(neighbor))
                {
                    neighbor.GCost = newGCost;
                    neighbor.HCost = Vector3.Distance(neighbor.position, targetNode.position);
                    neighbor.Parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null; 
    }

    static List<MeshNode> RetracePath(MeshNode startNode, MeshNode endNode)
    {
        List<MeshNode> path = new List<MeshNode>();
        MeshNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return path;
    }
}