using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGen {
    [System.Serializable]
    public class MapReader
    {
        [SerializeField] private Vector2Int _dimensions;
        [SerializeField] private float _cell_size;
        [SerializeField] private int[] _map_data;
        [SerializeField] private bool _map_loaded = false;
        public Vector2Int dimensions => _dimensions;
        public float cell_size => _cell_size;
        public int[] map_data => _map_data;
        public bool map_loaded => _map_loaded;

        // Returns TRUE or FALSE depending if the map file can be successfully read.
        public bool ReadFile(TextAsset map_file) {

            if (map_file == null) {
                Debug.LogError("Map file not passed");
                _map_loaded = false;
                return false;
            }
            
            // We expect this file to be a simple file with the following:
            // First Row: x, y, and cell_size
            // Subsequent: the layout of the grid, given x and y.

            // Read the data as lines
            string[] data = map_file.text.Split('\n');

            // Read the first line, which contains the dimensions and size of our map.
            // If the mfirst row doesn't match the required data, then we return an error.
            string[] map_details = data[0].Split(",");
            if (map_details.Length != 3) {
                // return early error if the number of header items cannot give us enough info of the map
                Debug.LogError("Map data has less or more than 3 values in the first row. We cannot build a map from this.");
                _map_loaded = false;
                return false;
            }

            // Extract the dimensions and cell size from the first row
            // We also create a 1D flattened array to contain layout data.
            Vector2Int m_dimensions = new Vector2Int(int.Parse(map_details[0]), int.Parse(map_details[1]));
            float m_cell_size = int.Parse(map_details[2]);
            int[] m_map_data = new int[m_dimensions.x * m_dimensions.y];

            // The number of rows in this file should match the `y` dimension + 1, so let's check.
            if (data.Length != m_dimensions.y + 1) {
                Debug.LogError("Provided map dimensions in map file don't match with drawn grid dimensions.");
                _map_loaded = false;
                return false;
            }

            // We expect to read the 1D flattened map data with the idea that each row is X amount of data values, from y=0 to Y=Max
            int index = 0;
            for(int y = data.Length-1; y >= 1; y--) {
                // Each line/row represents the x-axis, we need to double-check
                string[] line_data = data[y].Split(",");
                if (line_data.Length != m_dimensions.x) {
                    Debug.LogError("Provided map dimensions in map file don't match with drawn grid dimensions.");
                    return false;
                }
                for(int x = 0; x < line_data.Length; x++) {
                    m_map_data[index] = int.Parse(line_data[x]);
                    index+=1;
                }
            }

            // Set the output data, return successful
            _dimensions = m_dimensions;
            _cell_size = m_cell_size;
            _map_data = m_map_data;
            _map_loaded = true;
            Debug.Log($"Grid Size:({_dimensions.x},{_dimensions.y}) with cell size {_cell_size}");
            return true;
        }

        public int GetIndexFromXY(int x, int y) {
            return x + _dimensions.x*y;
        }
        public int GetIndexFromXY(Vector2Int xy) {
            return xy.x + _dimensions.x*xy.y;
        }
        
        public Vector2Int GetXYFromIndex(int index) {
            return new Vector2Int(Mathf.FloorToInt(index % _dimensions.x), Mathf.FloorToInt(index/_dimensions.y));
        }

        public Vector3 GetCornerPosFromXY(Vector2Int xy, float y = 0f) {
            return new Vector3(xy.x*_cell_size, y, xy.y*_cell_size);
        }
        public Vector3 GetCornerPosFromIndex(int index, float y = 0f) {
            return GetCornerPosFromXY(GetXYFromIndex(index), y);
        }

        public Vector3 GetCentroidPosFromXY(Vector2Int xy, float y = 0f) {
            return GetCornerPosFromXY(xy,y) + Vector3.one * _cell_size * 0.5f;
        }
        public Vector3 GetCentroidPosFromIndex(int index, float y=  0f) {
            return GetCornerPosFromIndex(index,y) + Vector3.one * _cell_size * 0.5f;
        }

        public int GetIndexFromPosition(Vector3 pos) {
            int x = Mathf.FloorToInt(pos.x / _cell_size);
            int y = Mathf.FloorToInt(pos.z / _cell_size);
            return GetIndexFromXY(x,y);
        }
        public Vector2Int GetXYFromPosition(Vector3 pos) {
            return new Vector2Int(
                Mathf.FloorToInt(pos.x / _cell_size),
                Mathf.FloorToInt(pos.z / _cell_size)
            );
        }
        
    }
}
