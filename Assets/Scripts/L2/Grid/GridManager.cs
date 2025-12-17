using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace L2
{
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")] 
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 10;
        [SerializeField] private float cellSize = 1f;
    
        [Header("Prefabs & Materials")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Material walkableMaterial;
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material goalMaterial;
        [SerializeField] private Material pathMaterial;
        [SerializeField] private Material startMaterial;
        private Node[,] nodes;
        private Dictionary<GameObject, Node> tileToNode = new();
    
        //Input action for click
        private InputAction clickAction;
    
        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;
        public Node[,] Nodes => nodes;
        public Material StartMaterial => startMaterial;
        public Material GoalMaterial => goalMaterial;
        public Material PathMaterial => pathMaterial;
    
        public PathFinder pathFinder;
        private void Awake()
        {
            GenerateGrid();
        }

        private void OnEnable()
        {
            clickAction = new InputAction(
                name: "Click",
                type: InputActionType.Button,
                binding: "<Mouse>/leftButton");
            clickAction.performed += OnClickPerformed;
            clickAction.Enable();
        }

        private void OnDisable()
        {
            if (clickAction != null)
            {
                clickAction.performed -= OnClickPerformed;
                clickAction.Disable();
            }
        }

        private void GenerateGrid()
        {
            nodes = new Node[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 worldPos = new Vector3(x * cellSize, 0f, y * cellSize);
                    GameObject tileGO = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
                    tileGO.name = $"Tile_{x}_{y}";
                
                    Node node = new Node(x, y, true, tileGO);
                    nodes[x, y] = node;
                
                    tileToNode[tileGO] = node;
                
                    SetTileMaterial(node, walkableMaterial);
                }
            }
        }

        private void OnClickPerformed(InputAction.CallbackContext context)
        {
            HandleMouseClick();
        }

        private void HandleMouseClick()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                return;
            }
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clicked = hit.collider.gameObject;
                if (tileToNode.TryGetValue(clicked, out Node node))
                {
                    switch (node.tile.GetComponent<MeshRenderer>().sharedMaterial)
                    {
                        case var mat when mat == walkableMaterial:
                            SetTileMaterial(node, wallMaterial);
                            SetWalkable(node, false);
                            break;
                        case var mat when mat == wallMaterial:
                            SetTileMaterial(node, goalMaterial);
                            SetWalkable(node, true);
                            break;
                        case var mat when mat == goalMaterial:
                            SetTileMaterial(node, pathMaterial);
                            SetWalkable(node, true);
                            break;
                        case var mat when mat == pathMaterial:
                            SetTileMaterial(node, startMaterial);
                            SetWalkable(node, true);
                            break;
                        case var mat when mat == startMaterial:
                            SetTileMaterial(node, walkableMaterial);
                            SetWalkable(node, true);
                            break;
                    }
                }
            }
        }

        public Node GetNode(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return null;
            }
            return nodes[x, y];
        }

        public Node GetNodeFromWorldPosition(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt(worldPos.x / cellSize);
            int y = Mathf.FloorToInt(worldPos.z / cellSize);
            return nodes[x, y];
        }

        public IEnumerable<Node> GetNeighbours(Node node, bool allowDiagonals = false)
        {
            int x = node.x;
            int y = node.y;
        
            //4-neighbour
            yield return GetNode(x+1, y);
            yield return GetNode(x-1, y);
            yield return GetNode(x, y+1);
            yield return GetNode(x, y-1);

            if (allowDiagonals)
            {
                yield return GetNode(x+1, y+1);
                yield return GetNode(x-1, y+1);
                yield return GetNode(x+1, y-1);
                yield return GetNode(x-1, y-1);
            }
        }

        public void SetWalkable(Node node, bool walkable)
        {
            node.isWalkable = walkable;
        }

        private void SetTileMaterial(Node node, Material material)
        {
            var renderer = node.tile.GetComponent<MeshRenderer>();
            if (renderer != null && material != null)
            {
                renderer.material = material;
            }
        }

        public void ResetGrid()
        {
            // foreach (Node node in nodes)
            // {
            //     SetWalkable(node, true);
            //     node.tile.GetComponent<MeshRenderer>().material = walkableMaterial;
            // }
        
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Node node = nodes[x, y];
                    node.isWalkable = true;
                    node.parent = null;
                    node.gCost = float.PositiveInfinity;
                    node.hCost = 0f;
                    //SetTileMaterial(node, walkableMaterial);
                    node.tile.GetComponent<MeshRenderer>().material = walkableMaterial;
                }
            }
        }

        public void GenerateRandomGrid()
        {
            while (true)
            {
                // Clear previous state
                ResetGrid();

                // Choose random start and goal
                Node start = nodes[Random.Range(0, width), Random.Range(0, height)];
                Node goal = start;

                while (goal == start)
                {
                    goal = nodes[Random.Range(0, width), Random.Range(0, height)];
                }

                SetTileMaterial(start, startMaterial);
                SetTileMaterial(goal, goalMaterial);
                start.isWalkable = true;
                goal.isWalkable = true;

                // Fill the rest with random walls and walkables
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Node node = nodes[x, y];

                        if (node == start || node == goal)
                        {
                            continue;
                        }

                        bool makeWall = Random.value < 0.2f; // adjust density
                        node.isWalkable = !makeWall;

                        SetTileMaterial(node, makeWall ? wallMaterial : walkableMaterial);
                    }
                }

                pathFinder.startNode = start;
                pathFinder.goalNode = goal;

                // this should ensure that grid has a possible path + that path is long enough
                if (pathFinder.FindPath(start, goal) == null || pathFinder.FindPath(goal, start).Count < 5)
                {
                    continue;
                }

                break;
            }
        }
    }
}
