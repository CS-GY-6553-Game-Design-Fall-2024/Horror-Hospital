using UnityEngine;

public class RenderSetter : MonoBehaviour
{
    public static RenderSetter current;

    void Awake() {
        current = this;
    }

    void Start() {
        // Can't set anything if the game manager is nonexistent
        if (GameManager.current == null) {
            Debug.LogError("Cannot run render setter: Game Manager singleton not set");
            return;
        }
        Debug.Log("Render Setter Enabled");

        // Looking at the game manager, we set all the render settings necessary.
        RenderSettings.fog = GameManager.current.useFog;
    }

    public void SetFog(bool setTo) {
        RenderSettings.fog = setTo;
    }
}
