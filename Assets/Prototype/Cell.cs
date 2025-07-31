using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGen {
    public class Cell : MonoBehaviour
    {
        [Header("=== Core ===")]
        public Vector2Int coords;
        public int index;

        [Header("=== Neighbors ===")]
        // All neighbor indices are sorted in order of north, south, east, west.
        // Nonexistent neighbors are valued -1. We should expect 4 values always
        public int[] neighbor_indices;
        // Cells is similar to neighbors, except that it's a condensed list that only contains existing neighbors.
        // Its length can be at max 4 and at least 1.
        public List<Cell> neighbors;

        [Header("=== Meshes and Walls (NSEW) ===")]
        public Transform visible_parent;
        public GameObject[] walls;

        // Initializing the cell means 1. detecting neighbor cells, and 2. setting walls based on neighbors
        public void Initialize() {

            // Make sure that the Map Manager exists
            if (MapManager.Instance == null) return;
            MapReader map_reader = MapManager.Instance.map_reader;

            // Instantiate indices array+list and variables
            neighbor_indices = new int[4];
            neighbors = new List<Cell>();
            
            // Check each cardinal direction for potential neighbors
            CheckPotentialNeighbor(new Vector2Int(coords.x, coords.y+1), 0);    // North
            CheckPotentialNeighbor(new Vector2Int(coords.x, coords.y-1), 1);    // South
            CheckPotentialNeighbor(new Vector2Int(coords.x+1, coords.y), 2);    // East
            CheckPotentialNeighbor(new Vector2Int(coords.x-1, coords.y), 3);    // West
        }

        private void CheckPotentialNeighbor(Vector2Int nxy, int cardinal_index) {
            int ni = MapManager.Instance.map_reader.GetIndexFromXY(nxy);
            if (
                nxy.x >= 0 
                && nxy.x <= MapManager.Instance.map_reader.dimensions.x-1
                && nxy.y >= 0
                && nxy.y <= MapManager.Instance.map_reader.dimensions.y-1 
                && MapManager.Instance.map_reader.map_data[ni] != 0
            ) {
                neighbor_indices[cardinal_index] = ni;
                neighbors.Add(MapManager.Instance.generated_cells[ni]);
                walls[cardinal_index].SetActive(false);
            } else {
                neighbor_indices[cardinal_index] = -1;
            }
        }

        public void StartShake(AnimationCurve shake_curve, float strength=0.1f, float duration=1f) {
            StartCoroutine(Shake(shake_curve, strength,duration));
        }
        private IEnumerator Shake(AnimationCurve shake_curve, float strength, float duration) {
            float startTime = Time.time;
            while(Time.time - startTime <= duration) {
                float diff = Time.time - startTime;
                float durationRatio = diff/duration;
                Vector3 randomPos = Random.insideUnitSphere * shake_curve.Evaluate(durationRatio) * strength;
                visible_parent.localPosition = randomPos;
                yield return null;
            }
            visible_parent.localPosition = Vector3.zero;
        }


    }
}
