using UnityEngine;
using UnityEditor;

namespace MapGen {
    [CustomEditor(typeof(MapManager))]
    public class MapManagerEditor : Editor
    {
        private MapManager current;

        void OnEnable() {
            current = (MapManager)target;
        }
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            if (GUILayout.Button("Test Shake")) {
                Debug.Log("Hello");
            }
        }
    }
}
