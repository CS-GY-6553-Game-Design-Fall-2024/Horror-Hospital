using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapController : MonoBehaviour
{
    [System.Serializable]
    public class GridCell {
        public int gridIndex;
        public Vector2Int gridIndices;
        public bool isActive;
        public MapCell mapCell;
        public int[] neighbors; // 0 = left, 1 = top, 2 = bottom, 3 = right
        public GridCell(int gridIndex, Vector2Int gridIndices, MapCell cell) {
            this.gridIndex = gridIndex;
            this.gridIndices = gridIndices;
            this.isActive = true;
            this.mapCell = cell;
            this.neighbors = new int[4]{-1,-1,-1,-1};
        }
        public GridCell(int gridIndex, Vector2Int gridIndices) {
            this.gridIndex = gridIndex;
            this.gridIndices = gridIndices;
            this.isActive = false;
            this.mapCell = null;
            this.neighbors = new int[4]{-1,-1,-1,-1};
        }
        public void AddNeighbor(int neighborIndex, int newNeighbor) {
            this.neighbors[neighborIndex] = newNeighbor;
        }
        public void SetWalls() {
            if (mapCell == null) return;
            for(int i = 0; i < neighbors.Length; i++) {
                if (neighbors[i] != -1) mapCell.SetWall(i, false);
                else mapCell.SetWall(i, true);
            }
        }
    }

    public static MapController current;

    [Header("=== References ===")]
    [SerializeField] private NavMeshSurface m_surface;
    [SerializeField] private MapCell m_cellPrefab;
    [SerializeField] private Transform m_player;

    [Header("=== Map Files & Generation ===")]
    [SerializeField] private bool m_initializeOnStart = true;
    [SerializeField] private TextAsset[] m_mapFiles;
    [SerializeField] private bool m_randomizeMapFile = true;
    private float m_cellSize = 1f;
    [SerializeField] private GridCell[] m_cells;
    private Vector2Int m_gridDimensions = new Vector2Int(5,5);
    private bool[] m_activeCells;

    [Header("=== Player, Destination Placement ===")]
    [SerializeField] private bool m_randomizePlayerSpawn = true;
    [SerializeField] private Vector2 m_pathCostWeights = new Vector2(0.25f, 0.75f);
    [SerializeField] private float playerHeight = 1f;
    
    
    [Header("=== Debug Settings ===")]
    [SerializeField] private bool m_showGizmos = false;
    [SerializeField] private Transform m_debugPosition;
    [SerializeField] private int m_cellToHighlight = -1;

    #if UNITY_EDITOR
    private void OnDrawGizmos() {
        if (!m_showGizmos) return;
        float width = m_cellSize * m_gridDimensions.x;
        float height = m_cellSize * m_gridDimensions.y;
        Vector3 center = new Vector3(width/2f, 0f, height/2f);

        if (m_gridDimensions.x > 0 && m_gridDimensions.y > 0) {
            Vector3 cellExtents = new Vector3(m_cellSize, 0f, m_cellSize);
            for(int x = 0; x < m_gridDimensions.x; x++) {
                for(int y = 0; y < m_gridDimensions.y; y++) {
                    // Get center position of grid cell
                    Vector3 cellCenter = new Vector3(x*m_cellSize + m_cellSize/2f, 0f, y*m_cellSize + m_cellSize/2f);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(cellCenter, cellExtents);
                    if (Application.isPlaying) {
                        GridCell cell = m_cells[x*m_gridDimensions.y+y];
                        if (!cell.isActive) continue;
                        Gizmos.color = (cell.gridIndex == m_cellToHighlight) ? Color.yellow : Color.blue;
                        Gizmos.DrawSphere(cellCenter, 0.1f);
                        foreach(int neighborIndex in cell.neighbors) {
                            if (neighborIndex != -1) Gizmos.DrawRay(cell.mapCell.transform.position, (m_cells[neighborIndex].mapCell.transform.position - cell.mapCell.transform.position).normalized);
                        }
                    }
                }
            }
        }

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(center, new Vector3(width, 0f, height));
    }
    #endif

    private void Awake() {
        current = this;
    }

    private void Start() {
        // We can have the map controller already ahve cells associated with it... or we ask it to initialize via script
        // We control this behavior via this boolean check. If so, it'll re-set any cells that are active in the scene.
        if (m_initializeOnStart) InitializeGrid();

        // Depending on settings initialized by the game manager... we have to set the proper settings
        SetAllSettings();
    }

    public void InitializeGrid() {
        if (m_mapFiles.Length == 0) {
            Debug.LogError("Cannot initialize grid without at least one map file to refer to...");
            return;
        }
        if (m_cellPrefab == null) {
            Debug.LogError("Cannot initialize grid with missing cell prefab.");
            return;
        }

        // Read the map data, but exit early if any errors occur
        TextAsset mapFile = m_mapFiles[0];
        if (m_randomizeMapFile) mapFile = m_mapFiles[UnityEngine.Random.Range(0,m_mapFiles.Length)];
        if (!ReadMapFile(mapFile)) return;

        // Initialize cells. Take a look at our grid dimensions and, based on what we know about the active grids
        if (m_cells != null && m_cells.Length > 0) {
            foreach(GridCell cell in m_cells) Destroy(cell.mapCell);
        } 
        m_cells = new GridCell[m_gridDimensions.x * m_gridDimensions.y];
        for(int x = 0; x < m_gridDimensions.x; x++) {
            for(int y = 0; y < m_gridDimensions.y; y++) {
                // What's the index of this grid cell?
                int index = x*m_gridDimensions.y + y;
                Vector2Int indices = new Vector2Int(x,y);
                // Check if the cell is active
                bool activeCell = m_activeCells[index];
                if (activeCell) {
                    // Initialize an active grid cell
                    Vector3 cellCenter = new Vector3(x*m_cellSize + m_cellSize/2f, 0f, y*m_cellSize + m_cellSize/2f);
                    MapCell cell = Instantiate(m_cellPrefab, cellCenter, Quaternion.identity) as MapCell;
                    m_cells[index] = new GridCell(index, indices, cell);
                } else {
                    // Initialize an inactive grid cell
                    m_cells[index] = new GridCell(index, indices);
                }
            }
        }

        // Now, we iterate through each cell and have them determine if there's a neighbor.
        int otherIndex;
        foreach(GridCell cell in m_cells) {
            Vector2Int indices = cell.gridIndices;
            // first cell - left of us
            if (indices.x > 0) {
                otherIndex = (indices.x-1)*m_gridDimensions.y + indices.y;
                if (m_cells[otherIndex].isActive) cell.AddNeighbor(0, otherIndex);
            }
            // Second cell - top of us
            if (indices.y < m_gridDimensions.y-1) {
                otherIndex = indices.x*m_gridDimensions.y + (indices.y+1);
                if (m_cells[otherIndex].isActive) cell.AddNeighbor(1, otherIndex);
            }
            // Third cell - below us
            if (indices.y > 0) {
                otherIndex = indices.x*m_gridDimensions.y + (indices.y-1);
                if (m_cells[otherIndex].isActive) cell.AddNeighbor(2, otherIndex);
            }
            // Fourth cell - right of us
            if (indices.x < m_gridDimensions.x-1) {
                otherIndex = (indices.x+1)*m_gridDimensions.y + indices.y;
                if (m_cells[otherIndex].isActive) cell.AddNeighbor(3, otherIndex);
            }
        }

        // Knowing which cells have which neighbors, set the walls and obstacles
        foreach(GridCell cell in m_cells) {
            cell.SetWalls();
            if (cell.mapCell != null) {
                cell.mapCell.CreateObstacle();
                cell.mapCell.CreateLight();
            }
        }

        // Build the nav mesh
        m_surface.BuildNavMesh();

        // Now, determine a starting point for the player. This is truly random among all active cells. Then place the player in it.
        GridCell spawnCell = null;
        if (m_randomizePlayerSpawn) {
            do {
                int spawnCellIndex = UnityEngine.Random.Range(0, m_cells.Length);
                if (m_activeCells[spawnCellIndex]) spawnCell = m_cells[spawnCellIndex];
            } while(spawnCell == null);
        } else {
            int startCellIndex = GetGridIndexFromPosition(m_player.position);
            spawnCell = m_cells[startCellIndex];
        }
        m_player.position = spawnCell.mapCell.transform.position;
        m_player.transform.Translate(Vector3.up * playerHeight);
        m_player.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f,359f), 0f);
        m_player.gameObject.SetActive(true);

        // Now, we need to determine an optimal destination point.
        // don't want this to be a simple "longest path from player" path algo. Rather, I want to determine the destination...
        //  ... to be one that is spawned from a path that is the most "complicated". This is a factor of distance AND number of turns.
        // So what we need to do is, given a start and end, what cell produces the navmesh path that is the most complicated?
        NavMeshPath navPath = new NavMeshPath();
        GridCell destinationCell = spawnCell;
        float worstPathCost = 0f;
        for(int i = 0; i < m_cells.Length; i++) {
            if (i == spawnCell.gridIndex) continue;
            if (!m_activeCells[i]) continue;
            GridCell otherCell = m_cells[i];
            NavMesh.CalculatePath(spawnCell.mapCell.transform.position, otherCell.mapCell.transform.position, NavMesh.AllAreas, navPath);
            float distance = 0f;
            for(int j = 0; j < navPath.corners.Length-1; j++) {
                distance += Vector3.Distance(navPath.corners[j], navPath.corners[j+1]) / m_cellSize;
            }
            float pathCost = distance * m_pathCostWeights.x + (float)navPath.corners.Length * m_pathCostWeights.y;
            if (pathCost > worstPathCost) {
                worstPathCost = pathCost;
                destinationCell = otherCell;
            }
        }
        destinationCell.mapCell.SetAsDestination();
        destinationCell.mapCell.DeleteObstacle();
        destinationCell.mapCell.DeleteLight();
    }

    private bool ReadMapFile(TextAsset mapFile) {
        // Read the data as lines
        string[] data = mapFile.text.Split('\n');
        
        // Read the first line, which contains the dimensions and size of our map. MUST have at least 3 elements for x, y, and cell size
        string[] mapDetails = data[0].Split(",");
        if (mapDetails.Length < 3) {
            // return early error if the number of header items cannot give us enough info of the map
            Debug.LogError("Map data has less than 3 values in the first row. We cannot build a map from this.");
            return false;
        }
        m_gridDimensions = new Vector2Int(int.Parse(mapDetails[0]), int.Parse(mapDetails[1]));
        m_cellSize = int.Parse(mapDetails[2]);
        Debug.Log($"Grid Size:({mapDetails[0]},{mapDetails[1]}) with cell size {mapDetails[2]}");
        
        bool[] activeCells = new bool[m_gridDimensions.x * m_gridDimensions.y];
        // All subsequent lines should match up... so let's double check.
        if (data.Length-1 != m_gridDimensions.y) {
            Debug.LogError("Provided map dimensions in map file don't match with drawn grid dimensions.");
            return false;
        }
        // Loop through lines, which represent the y-axis. We start from the last line and move upwards
        for(int y = data.Length-1; y >= 1; y--) {
            // Each line/row represents the x-axis, we need to double-check
            string[] lineData = data[y].Split(",");
            if (lineData.Length != m_gridDimensions.x) {
                Debug.LogError("Provided map dimensions in map file don't match with drawn grid dimensions.");
                return false;
            }
            for(int x = 0; x < lineData.Length; x++) {
                // Get the combined grid index
                int index = x*m_gridDimensions.y+(m_gridDimensions.y - y);
                activeCells[index] = int.Parse(lineData[x]) == 1;
            }
        }

        // Set the list of active cells, then return true;
        m_activeCells = activeCells;
        return true;
    }

    private void Update() {
        if (m_debugPosition != null) {
            m_cellToHighlight = GetGridIndexFromPosition(m_debugPosition.position);
        }
    }

    public int GetGridIndexFromPosition(Vector3 position) {
        int x = Mathf.FloorToInt(position.x/m_cellSize);
        int y = Mathf.FloorToInt(position.z/m_cellSize);
        if (x < 0 || x >= m_gridDimensions.x || y < 0 || y >= m_gridDimensions.y) return -1;
        return x*m_gridDimensions.y+y;
    }

    public void StartShake() {
        if (m_debugPosition == null) {
            Debug.LogError("Cannot shake the map if no debug position is used as a representation of the monster's current position");
            return;
        }

        int monsterCellIndex = GetGridIndexFromPosition(m_debugPosition.position);

    }

    public void SetAllSettings() {
        foreach(GridCell cell in m_cells) {
            if (cell.mapCell != null) {
                cell.mapCell.SetDirtyWalls(GameManager.current.useDirtyWalls);  // Dirty walls?
                cell.mapCell.SetObstacle(GameManager.current.spawnObstacles);   // Obstacles?
                cell.mapCell.SetLight(GameManager.current.useRoomLights);       // Lights?
                cell.mapCell.SetParticles(GameManager.current.showParticles);   // Particles?
            }
        }
    }

    public void SetDirtyWalls(bool shouldBeDirty) {
        foreach(GridCell cell in m_cells) if (cell.mapCell != null) cell.mapCell.SetDirtyWalls(shouldBeDirty);
    }
    public void SetObstacles(bool obstaclesAreActive) {
        foreach(GridCell cell in m_cells) if (cell.mapCell != null) cell.mapCell.SetObstacle(obstaclesAreActive);
    }
    public void SetRoomLights(bool shouldBeOn) {
        foreach(GridCell cell in m_cells) if (cell.mapCell != null) cell.mapCell.SetLight(shouldBeOn);
    }
    public void SetParticles(bool showParticles) {
        foreach(GridCell cell in m_cells) if (cell.mapCell != null) cell.mapCell.SetParticles(showParticles);
    }
}
