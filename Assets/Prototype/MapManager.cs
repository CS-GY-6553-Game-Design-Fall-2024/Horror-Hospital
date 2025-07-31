using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

namespace MapGen {
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance;

        [Header("=== Reading Map Data ===")]
        public TextAsset map_file;
        public MapReader map_reader;

        [Header("=== Map Settings ===")]
        public Cell cell_prefab;
        public Transform cell_parent;
        public NavMeshSurface nav_controller;
        public AnimationCurve shake_curve;

        public Cell[] generated_cells;

        [Header("=== Debugging ===")]
        public Vector2Int query_xy;
        public int outcome_index;
        public Vector2Int outcome_xy;
        public Vector3 outcome_corner_pos;
        public Vector3 outcome_centroid_pos;

        private void Awake () {
            Instance = this;
            Initialize();
        }

        // Should be called if you want to set the layout of the map.
        public void Initialize(bool force_reset = true) {
            // If needed, reset the map entirely.
            if (force_reset) ResetMap();
            
            // If failure to read map, exit early
            if (!map_reader.ReadFile(map_file)) {
                Debug.LogError("Cannot read map! Map file missing!");
                return;
            }
            
            // Iterate through map reader's map data, instantiating new cells at each position depending on the value
            generated_cells = new Cell[map_reader.map_data.Length];
            if (cell_parent == null) cell_parent = this.transform;
            for(int i = 0; i < map_reader.map_data.Length; i++) {
                // get neighbor indices
                Vector2Int xy = map_reader.GetXYFromIndex(i);
                // Use switch to detemrine the cell type
                switch(map_reader.map_data[i]) {    
                    // Base cell type
                    case 1:
                        Vector3 pos = map_reader.GetCentroidPosFromIndex(i);
                        Cell new_cell = Instantiate(cell_prefab, pos, Quaternion.identity) as Cell;
                        new_cell.transform.localScale = Vector3.one * map_reader.cell_size;
                        new_cell.coords = xy;
                        new_cell.index = i;
                        new_cell.neighbor_indices = new int[4];
                        new_cell.neighbors = new List<Cell>();
                        new_cell.transform.SetParent(cell_parent);
                        generated_cells[i] = new_cell;
                        break;
                    // Do not place a cell under default conditions.
                    default: break;
                }
            }

            // Iterate through generated cells to instantiate walls
            for(int i = 0; i < map_reader.map_data.Length; i++) {
                Cell cell = generated_cells[i];
                if (cell != null) cell.Initialize();
            }

            // Create a NavMesh to create a pathing scheme throughout this map.
            nav_controller.BuildNavMesh();
        }

        // Basically resets the map, if needed.
        public void ResetMap() {
            for(int i = 0; i < generated_cells.Length; i++) Destroy(generated_cells[i].gameObject);
        }

        public void ShakeAtPosition(Vector3 pos) {
            int shake_index = map_reader.GetIndexFromPosition(pos);
        }

        void OnValidate() {
            if (map_file != null) {
                if (map_reader.ReadFile(map_file)) {
                    query_xy = new Vector2Int(Mathf.Clamp(query_xy.x,0,map_reader.dimensions.x-1),Mathf.Clamp(query_xy.y,0,map_reader.dimensions.y-1));
                    outcome_index = map_reader.GetIndexFromXY(query_xy);
                    outcome_xy = map_reader.GetXYFromIndex(outcome_index);
                    outcome_corner_pos = map_reader.GetCornerPosFromIndex(outcome_index);
                    outcome_centroid_pos = map_reader.GetCentroidPosFromIndex(outcome_index);
                }
            }
        }
    }   
}
