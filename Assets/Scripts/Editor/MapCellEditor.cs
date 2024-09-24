using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MapCell))]
public class MapCellEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        MapCell mapCell = (MapCell)target;
        DrawDefaultInspector();

        if(GUILayout.Button("Debug Shake")) {
            mapCell.StartShake(mapCell.debugShakeStrength);
        }
    }
}
