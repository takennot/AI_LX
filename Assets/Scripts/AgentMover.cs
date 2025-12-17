using System.Collections.Generic;
using UnityEngine;

public class AgentMover : MonoBehaviour
{
    public PathFinder pathfinder;
    public GridManager gridManager;
    public float moveSpeed = 3f;

    private List<Node> currentPath = null;
    private int currentIndex = 0;

    public KeyCode triggerKey = KeyCode.F;

    private void Update()
    {
        // Trigger movement when key is pressed
        if (Input.GetKeyDown(triggerKey))
        {
            BeginPathFollow();
        }

        if (currentPath == null || currentPath.Count == 0)
        {
            return;
        }

        MoveAlongPath();
    }

    private void BeginPathFollow()
    {
        var vector3 = transform.position;
        vector3.x = pathfinder.startNode.tile.transform.position.x;
        vector3.y = pathfinder.startNode.tile.transform.position.y + 1;
        vector3.z = pathfinder.startNode.tile.transform.position.z;
        transform.position = vector3;
        Node start = gridManager.GetNodeFromWorldPosition(transform.position);
        Node goal = pathfinder.goalNode;

        currentPath = pathfinder.FindPath(start, goal);
        currentIndex = 0;

        if (currentPath == null || currentPath.Count == 0)
        {
            currentPath = null;
        }
    }

    private void MoveAlongPath()
    {
        if (currentIndex >= currentPath.Count)
        {
            currentPath = null;
            return;
        }

        Node targetNode = currentPath[currentIndex];
        Vector3 targetPos = NodeToWorldPosition(targetNode);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        float distance = Vector3.Distance(transform.position, targetPos);

        if (distance < 0.05f)
        {
            currentIndex++;

            if (currentIndex >= currentPath.Count)
            {
                currentPath = null;
            }
        }
    }

    private Vector3 NodeToWorldPosition(Node node)
    {
        float cell = gridManager.CellSize;
        return new Vector3(node.x * cell, transform.position.y, node.y * cell);
    }
}