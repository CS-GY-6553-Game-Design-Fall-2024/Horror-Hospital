using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationTrigger : MonoBehaviour
{

    // This is a trigger detector that purely focuses on creating the end game screen.
    [SerializeField] private float m_fadeOutTime = 2f;
    [SerializeField] private float m_screenTransitionTime = 1f;

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player" && !GameManager.current.inTransition) {
            // Disable movement
            PlayerControl.current.ToggleMovement(false);
            // Start transition
            GameManager.current.ShowSuccessScreen(
                m_fadeOutTime, 
                m_screenTransitionTime
            );
        }
    }
}
