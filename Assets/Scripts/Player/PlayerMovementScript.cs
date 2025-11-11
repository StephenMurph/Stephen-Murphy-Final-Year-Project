using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    public Node startNode;  
    public Node endNode;    

    private bool atStart = true;   
    public float moveDelay = 1f;  
    private float timer = 0f;

    void Start()
    {
        if (startNode == null || endNode == null)
        {
            Debug.LogError("Assign both nodes!");
            return;
        }

        // start at the first node
        transform.position = startNode.transform.position;
        atStart = true;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= moveDelay)
        {
            timer = 0f;
            TeleportToNextNode();
        }
    }

    void TeleportToNextNode()
    {
        if (atStart)
        {
            transform.position = endNode.transform.position;
            atStart = false;
        }
        else
        {
            transform.position = startNode.transform.position;
            atStart = true;
        }
    }
}