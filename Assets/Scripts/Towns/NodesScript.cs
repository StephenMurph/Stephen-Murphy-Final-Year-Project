using UnityEngine;
using System.Collections.Generic;

public class Node : MonoBehaviour
{
    public List<Node> connections = new List<Node>();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.1f);
        foreach (var c in connections)
        {
            if (c != null)
                Gizmos.DrawLine(transform.position, c.transform.position);
        }
    }
}