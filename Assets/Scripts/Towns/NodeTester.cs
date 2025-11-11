using UnityEngine;

public class NodeTester : MonoBehaviour
{
    public Node startNode;
    public Node endNode;
    public int divisions = 10;
    public GameObject piece;

    void Start()
    {
        for (int i = 0; i <= divisions; i++)
        {
            float t = (float)i / divisions;
            Vector3 point = Vector3.Lerp(startNode.transform.position, endNode.transform.position, t);
            Debug.DrawLine(point, point + Vector3.up * 0.2f, Color.cyan, 10f);
        }
    }
}