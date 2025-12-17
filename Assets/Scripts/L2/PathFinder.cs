using System.Collections.Generic;
using UnityEngine;

namespace L2
{
    public class PathFinder : MonoBehaviour
    {
        // Step 6 
        // 1. To be honest, I don't know how exactly does NavMesh work under the hood, so I can't say much about it
        // But I do know that both this code & NavMesh use A* so that's something I guess?
    
        // 2. It depends on the game:
        // If I want a free movement, I will probably pick NavMesh (also to avoid reinventing the wheel)
        // If I want a step-based game and I don't want to use (I can only presume) heavy NavMesh, I might pick grid-based A*
    
        // 3. A* in itself is pretty optimized at this point, I think?
        // So my main issue would be with misconfigured nodes or very dense grid tiles idk
        // But also, even though A* is optimized, it's still a pretty heavy algorithm to run.
        // So I would reduce amount of tiles, and somehow find a way to not run A* as often. 
        // but if we are talking about RTS map... maybe have separate navigation grid for the units that is lower density?
        // Or... maybe make use of multithreading to run A* computations in another thread?
    
        public GridManager gridManager;
        public Material guardMaterial;
        public Node startNode;
        public Node goalNode;
        public float Heuristic(Node current, Node goal)
        {
            // octile distance
            var dx = Mathf.Abs(current.x - goal.x);
            var dy = Mathf.Abs(current.y - goal.y);
            return 1 * (dx + dy) + (1.4f - 2 * 1) * Mathf.Min(dx, dy);
            //return Mathf.Abs(current.x - goal.x) + Mathf.Abs(current.y - goal.y);
        }

        public List<Node> FindPath(Node start, Node goal)
        {
            // 1. Reset costs
            for (int x = 0; x < gridManager.Width; x++)
            {
                for (int y = 0; y < gridManager.Height; y++)
                {
                    Node n = gridManager.Nodes[x, y];
                    n.gCost = float.PositiveInfinity;
                    n.hCost = 0f;
                    n.parent = null;
                }
            }

            // 2. Initialize sets
            var openSet = new List<Node>();
            var closedSet = new HashSet<Node>();

            start.gCost = 0f;
            start.hCost = Heuristic(start, goal);
            openSet.Add(start);

            // 3. loop zoop
            while (openSet.Count > 0)
            {
                // Pick lowest fCost
                Node current = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < current.fCost ||
                        (Mathf.Approximately(openSet[i].fCost, current.fCost) &&
                         openSet[i].hCost < current.hCost))
                    {
                        current = openSet[i];
                    }
                }

                // Goal reached
                if (current == goal)
                {
                    // again, debug stuff, I know its bad
                    // foreach (var n in openSet)
                    // {
                    //     n.tile.GetComponent<MeshRenderer>().material.color = Color.khaki;
                    // }
                    // foreach (var n in closedSet)
                    // {
                    //     n.tile.GetComponent<MeshRenderer>().material.color = Color.darkSalmon;
                    // }
                    return ReconstructPath(goal);
                }

                openSet.Remove(current);
                closedSet.Add(current);

                // Explore neighbours
                foreach (var neighbour in gridManager.GetNeighbours(current, true))
                {
                    if (neighbour == null || !neighbour.isWalkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    float stepCost = (current.x != neighbour.x && current.y != neighbour.y) ? 1.4f : 1f; // sets step cost depending on diagonal or not
                    float tentativeG = current.gCost + stepCost;


                    if (tentativeG < neighbour.gCost)
                    {
                        neighbour.parent = current;
                        neighbour.gCost = tentativeG;
                        neighbour.hCost = Heuristic(neighbour, goal);

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                    }
                }
            }

            return null;
        }

        private List<Node> ReconstructPath(Node goal)
        {
            var path = new List<Node>();
            Node current = goal;

            while (current != null)
            {
                path.Add(current);
                current = current.parent;
            }

            path.Reverse();
            return path;
        }

        public void ShowPath(Node start, Node goal)
        {
            List<Node> path = FindPath(start, goal);

            if (path != null)
            {
                if (path.Count > 0)
                {
                    foreach (var node in path)
                    {
                        if (node.tile.GetComponent<MeshRenderer>().sharedMaterial != gridManager.StartMaterial && node.tile.GetComponent<MeshRenderer>().sharedMaterial != gridManager.GoalMaterial)
                        {
                            node.tile.GetComponent<MeshRenderer>().material = gridManager.PathMaterial;
                        }
                    }
                }
            }
        }
    
        private void Update()
        {
            // this is all just for debug, I know its horrible
            if (Input.GetKeyUp(KeyCode.Keypad0))
            {
                MeshRenderer[] meshes = FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                foreach (var mesh in meshes)
                {
                    if (mesh.sharedMaterial == gridManager.StartMaterial)
                    {
                        startNode = gridManager.GetNodeFromWorldPosition(mesh.gameObject.transform.position);
                    }

                    if (mesh.sharedMaterial == gridManager.GoalMaterial)
                    {
                        goalNode = gridManager.GetNodeFromWorldPosition(mesh.gameObject.transform.position);
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                ShowPath(startNode, goalNode);
            }

            if (Input.GetKeyUp(KeyCode.R))
            {
                gridManager.ResetGrid();
            }

            if (Input.GetKeyUp(KeyCode.G))
            {
                gridManager.GenerateRandomGrid();
            }
        }
    }
}
