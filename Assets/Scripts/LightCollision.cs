using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
    
public class LightCollision : MonoBehaviour{
    public MapCell mapCell;
    public GameObject parentObject;
    void OnTriggerEnter(Collider other) {
        // Force the trigger to be disabled if parent collider is inactive
        if (!parentObject.activeSelf) return;
        if (other.gameObject.tag != "Player") return;
        if (mapCell == null) {
            Debug.Log("mapCell not referenced.");
            return;
        }
        other.gameObject.GetComponent<PlayerControl>().SetInLight(true);
    }
    void OnTriggerExit(Collider other) {
        // FOrce the trigger to be disabled if parent collider is inactive
        if (!parentObject.activeSelf) return;
        if (other.gameObject.tag != "Player") return;
        if (mapCell == null) {
            Debug.Log("mapCell not referenced.");
            return;
        }
        other.gameObject.GetComponent<PlayerControl>().SetInLight(false);
    }
}