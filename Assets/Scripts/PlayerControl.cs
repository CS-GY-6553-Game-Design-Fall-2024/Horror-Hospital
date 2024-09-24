using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{

    public static PlayerControl current;

    [Header("=== References ===")]
    [SerializeField] private GameObject m_flashlight;
    [SerializeField] private Transform m_camera;
    public CameraFader cameraFader => m_camera.GetComponent<CameraFader>();
    [SerializeField] private CharacterController m_controller;
    public CharacterController controller => m_controller;

    [Header("=== Sound Sources ===")]
    [SerializeField] private AudioSource m_footstepSource;

    [Header("=== Player Settings ===")]
    [SerializeField] private bool m_cursorVisible = false;
    [SerializeField] private bool m_movementEnabled = true;
    [SerializeField] private float m_playerSpeed = 2f;
    [SerializeField] private float m_cameraRotationSpeed = 5f;
    [SerializeField] private bool m_inLight = false;
    private float m_velocityY = 0f;

    [Header("=== Horror Controls ===")]
    [SerializeField] private bool m_horrorEnabled = true;
    [SerializeField] private float m_horrorValue = 0f;
    public float horrorValue => m_horrorValue;
    [SerializeField] private float m_maxHorror = 10f;
    [SerializeField] private float m_horrorIncrement = 1f;
    [SerializeField] private float m_horrorDecrement = 0.1f;
    [SerializeField] private float m_minHeartPitch = 1.0f;
    [SerializeField] private float m_maxHeartPitch = 2.0f;
    [SerializeField] private float m_minBreathPitch = 1.0f;
    [SerializeField] private float m_maxBreathPitch = 1.3f;
    [SerializeField] private float m_minBreathVolume = 0.02f;
    [SerializeField] private float m_maxBreathVolume = 0.05f;
    [SerializeField] private float m_flashlightFlickerChance = 0.8f;
    private float m_flickerInterval;
    private float m_timer = 0f;
    [SerializeField] private AudioSource m_heartAudio;
    private float yaw = 0f;
    private float pitch = 0f;
    [SerializeField] private AudioSource m_breathAudio;
    
    private void Awake() {
        m_timer = 0f;
        current = this;
        m_controller = gameObject.GetComponent<CharacterController>();
    }

    void Start() {
        Cursor.visible = m_cursorVisible;
        SetFlashlight(GameManager.current.useFlashlight);
        SetHorrorFactor(GameManager.current.useHorrorFactor);
    }


    void Update() {
        // Only update if the controller is even active
        if (!m_controller.enabled) return;
        Rotation();
        Movement();
        HorrorManagement();
    }

    // Manage player rotation
    void Rotation(){
        // Prevent rotation if our movement is disabled
        if (!m_movementEnabled) return;
        
        // We rotate the camera vertically depending on the input of the mouse's y displacement.
        // We use `+=` to maintain camera's previous rotation. `Input.GetAxis` only checks the displacement during the frame.
        m_camera.localEulerAngles += m_cameraRotationSpeed * new Vector3(-Input.GetAxis("Mouse Y"), 0f, 0f);
        // We rotate the player itself when it comes to horizontal rotation. 
        // We use the `up` vector as the axis to rotate around. We rotate based on the mouse x input
        transform.Rotate(transform.up, Input.GetAxis("Mouse X")*m_cameraRotationSpeed);
    }

    // Manage player Movement
    void Movement(){
        // If movement is enabled, then we need to read the player inputs too
        Vector3 move = Vector3.zero;
        if (m_movementEnabled) {
            move = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            // We start the footstep audio if the movement value is bigger than some threshold
            if (move.magnitude >= 0.1f) m_footstepSource.mute = false;
            else m_footstepSource.mute = true;
            move = Vector3.Normalize(transform.TransformDirection(move)) * m_playerSpeed;
        } else {
            m_footstepSource.mute = true;
        }

        // Apply gravity if appropriate
        if (!m_controller.isGrounded) {
            m_velocityY += 9.81f*Time.deltaTime;
            move += new Vector3(0f, -m_velocityY, 0f);
        } else m_velocityY = 0f;

        // Commit movement
        m_controller.Move(move * Time.deltaTime);
    }

    void HorrorManagement(){
        // Exist if not enabled
        if (!m_horrorEnabled) return;
        // Incrementing/Decrementing horror value
        if (m_inLight && m_horrorValue >= 0){
            m_horrorValue -= m_horrorDecrement * Time.deltaTime;
        } else if (m_horrorValue < m_maxHorror) m_horrorValue += m_horrorIncrement * Time.deltaTime;
        // Adjusting breath audio
        m_breathAudio.pitch = m_minBreathPitch + (m_horrorValue / m_maxHorror) * (m_maxBreathPitch - m_minBreathPitch);
        m_breathAudio.volume = m_minBreathVolume + (m_horrorValue / m_maxHorror) * (m_maxBreathVolume - m_minBreathVolume);
        // Adjusting heart audio
        m_heartAudio.pitch = m_minHeartPitch + (m_horrorValue / m_maxHorror) * (m_maxHeartPitch - m_minHeartPitch);

        FlashlightFlicker();
    }
    void FlashlightFlicker(){
        // Don't proceed if flashlight is disabled
        if (!GameManager.current.useFlashlight) return;
        // Get current flicker chance
        float chanceMultplier = m_horrorValue / m_maxHorror;
        if (m_flashlight.activeSelf && Random.Range(0f, 1f) < chanceMultplier * m_flashlightFlickerChance){
            m_flickerInterval = Random.Range(0.1f, 0.5f);       //Randomized flicker interval
            SetFlashlight(false);
        } else {
            m_timer += Time.deltaTime;
            if (m_timer < m_flickerInterval) return;
            SetFlashlight(true);
            m_timer = 0f;
        }
    }
    public void SetInLight(bool isInLight){
        m_inLight = isInLight;
    }
    public void SetFlashlight(bool shouldUseFlashlight) {
        m_flashlight.SetActive(shouldUseFlashlight);
    }
    public void SetHorrorFactor(bool shouldUseHorror) {
        m_horrorEnabled = shouldUseHorror;
    }
    public void ToggleMovement(bool canMove) {
        m_movementEnabled = canMove;
    }
}
