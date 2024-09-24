using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    public static GameManager current;

    [Header("=== Menu Behavior ===")]
    [SerializeField] private CanvasGroup m_startCanvas;
    [SerializeField] private CanvasGroup m_optionsCanvas;
    [SerializeField] private CanvasGroup m_creditsCanvas;
    [SerializeField] private float m_menuTransitionTime = 1f;
    private bool m_inTransition = false;
    public bool inTransition => m_inTransition;
    [SerializeField] private AudioMixer m_masterMixer;

    [Header("=== Option Menu Toggles ===")]
    [SerializeField] private Toggle m_fogToggle;
    [SerializeField] private Toggle m_dirtyWallsToggle;
    [SerializeField] private Toggle m_useRoomLightsToggle;
    [SerializeField] private Toggle m_useFlashlightToggle;
    [SerializeField] private Toggle m_spawnObstaclesToggle;
    [SerializeField] private Toggle m_showParticlesToggle;
    [SerializeField] private Toggle m_useHorrorToggle;

    [Header("=== Default Options ===")]
    [SerializeField] private bool m_useFog = true;
    public bool useFog => m_useFog;
    [SerializeField] private bool m_useDirtyWalls = true;
    public bool useDirtyWalls => m_useDirtyWalls;
    [SerializeField] private bool m_useRoomLights = true;
    public bool useRoomLights => m_useRoomLights;
    [SerializeField] private bool m_useFlashlight = true;
    public bool useFlashlight => m_useFlashlight;
    [SerializeField] private bool m_spawnObstacles = true;
    public bool spawnObstacles => m_spawnObstacles;
    [SerializeField] private bool m_showParticles = true;
    public bool showParticles => m_showParticles;
    [SerializeField] private bool m_useHorrorFactor = true;
    public bool useHorrorFactor => m_useHorrorFactor;

    [SerializeField] private float m_masterVolume = 0f;
    public float masterVolume => m_masterVolume;
    [SerializeField] private float m_ambienceVolume = 0f;
    public float ambienceVolume => m_ambienceVolume;
    [SerializeField] private float m_sfxVolume = 0f;
    public float sfxVolume => m_sfxVolume;

    private void Awake() {
        // Prevent any new ones from appearing
        if (current != null) {
            Destroy(gameObject);
            return;
        }
        
        // Set this component as the singleton class
        current = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("Game Manager Active");
    }

    private void Start() {
        // Manually set the settings currently stored;
        RenderSetter.current.SetFog(m_useFog);
        if (m_fogToggle != null) m_fogToggle.isOn = m_useFog;

        MapController.current.SetDirtyWalls(m_useDirtyWalls);
        if (m_dirtyWallsToggle != null) m_dirtyWallsToggle.isOn = m_useDirtyWalls;

        MapController.current.SetRoomLights(m_useRoomLights);
        if (m_useRoomLightsToggle != null) m_useRoomLightsToggle.isOn = m_useRoomLights;

        PlayerControl.current.SetFlashlight(m_useFlashlight);
        if (m_useFlashlightToggle != null) m_useFlashlightToggle.isOn = m_useFlashlight;

        MapController.current.SetObstacles(m_spawnObstacles);
        if (m_spawnObstaclesToggle != null) m_spawnObstaclesToggle.isOn = m_spawnObstacles;

        MapController.current.SetParticles(m_showParticles);
        if (m_showParticlesToggle != null) m_showParticlesToggle.isOn = m_showParticles;

        PlayerControl.current.SetHorrorFactor(m_useHorrorFactor);
        if (m_useHorrorToggle != null) m_useHorrorToggle.isOn = m_useHorrorFactor;
    }

    public void ShowOptionsMenu() {
        // cancel if we're already in transition
        if (m_inTransition) return;
        // Start the coroutine. We're transitioning from the start menu to the options menu
        StartCoroutine(ToggleMenu(m_startCanvas, m_optionsCanvas));
    }
    public void ShowStartMenuFromOptions() {
        // Cancel if we're already in transition
        if (m_inTransition) return;
        // Start the coroutine. We're transitioning from either the options or credits menu to the start menu
        StartCoroutine(ToggleMenu(m_optionsCanvas, m_startCanvas));
    }
    public IEnumerator ToggleMenu(CanvasGroup fromMenu, CanvasGroup toMenu) {
        float startTime = Time.time;
        float fadeInOutTime = m_menuTransitionTime/2f;
        m_inTransition = true;
        float timeDiff, canvasAlpha;

        // Transition out of the `fromMenu`
        while(Time.time-startTime <= fadeInOutTime) {
            timeDiff = Time.time - startTime;
            fromMenu.alpha = 1f - Mathf.Clamp(timeDiff/fadeInOutTime,0f,1f);
            yield return null;
        }
        fromMenu.alpha = 0f;

        // Reset the start timer
        fromMenu.interactable = false;
        fromMenu.blocksRaycasts = false;
        toMenu.interactable = true;
        toMenu.blocksRaycasts = true;
        startTime = Time.time;
        
        // Transition into the `toMenu`
        while(Time.time-startTime <= fadeInOutTime) {
            timeDiff = Time.time - startTime;
            toMenu.alpha = Mathf.Clamp(timeDiff/fadeInOutTime,0f,1f);
            yield return null;
        }
        toMenu.alpha = 1f;

        // Now can transition out
        m_inTransition = false;
    }
    public void ShowCreditsMenu() {
        // Cancel if we're already in transition
        if (m_inTransition) return;
        // Start the coroutine. We're transitioning from the start menu to the credits menu
        StartCoroutine(ToggleMenu(m_startCanvas, m_creditsCanvas));
    }
    public void ShowStartMenuFromCredits() {
        // Cancel if we're already in transition
        if (m_inTransition) return;
        // Start the coroutine. We're transitioning from either the options or credits menu to the start menu
        StartCoroutine(ToggleMenu(m_creditsCanvas, m_startCanvas));
    }


    public void StartGame() {
        // prevent double-clicks 
        if (m_inTransition) return;
        m_inTransition = true;
        // Initialize transition
        StartCoroutine(InitializeStartGame());
    }
    private IEnumerator InitializeStartGame() {
        CameraFader camFader = PlayerControl.current.cameraFader;
        camFader.fadeColor = Color.black;
        camFader.transitionSpeed = 0.25f;
        camFader.FadeOut();
        // We first wait for the transition time to end,based on the camera fader's transition time
        float waitTimeStart = Time.time;
        float waitDuration = camFader.transitionDuration;
        while(Time.time - waitTimeStart <= waitDuration) yield return null;
        m_inTransition = false;
        // Upon waiting, we THEN deactivate start menu and load the main scene
        m_startCanvas.alpha = 0;
        m_startCanvas.interactable = false;
        m_startCanvas.blocksRaycasts = false;
        SceneManager.LoadScene("Main");
    }

    public void ShowSuccessScreen(float fadeTime, float screenTime) {
        // Double-check that we're not in transition
        if (m_inTransition) return;
        m_inTransition = true;
                
        // Then, we call `SuccessTransition()` coroutine
        StartCoroutine(SuccessTransition(fadeTime, screenTime));
    }
    private IEnumerator SuccessTransition(float fadeTime, float screenTime) {
        // Firstly, we want to change the properties of the player's camera fader
        CameraFader camFader = PlayerControl.current.cameraFader;
        camFader.fadeColor = Color.white;
        camFader.transitionSpeed = 1f/fadeTime;
        camFader.FadeOut();

        // We must wait for the cam fader to end
        float startTime = Time.time;
        while(Time.time - startTime <= fadeTime) yield return null;

        // end the transition
        m_startCanvas.alpha = 1;
        m_startCanvas.interactable = true;
        m_startCanvas.blocksRaycasts = true;
        m_inTransition = false;
        SceneManager.LoadScene("Start");
        ShowStartMenuFromCredits();
    }

    public void SetFog(bool setTo) {
        RenderSetter.current.SetFog(setTo);
        m_useFog = setTo;
    }
    public void SetDirtyWalls(bool setTo) {
        MapController.current.SetDirtyWalls(setTo);
        m_useDirtyWalls = setTo;
    }
    public void SetRoomLights(bool setTo) {
        MapController.current.SetRoomLights(setTo);
        m_useRoomLights = setTo;
    }
    public void SetFlashlight(bool setTo) {
        PlayerControl.current.SetFlashlight(setTo);
        m_useFlashlight = setTo;
    }
    public void SetHorrorFactor(bool setTo) {
        PlayerControl.current.SetHorrorFactor(setTo);
        m_useHorrorFactor = setTo;

    }
    public void SetObstacles(bool setTo) {
        MapController.current.SetObstacles(setTo);
        m_spawnObstacles = setTo;
    }
    public void SetParticles(bool setTo) {
        MapController.current.SetParticles(setTo);
        m_showParticles = setTo;
    }

    public void SetMasterLvl(float setTo) {
        m_masterMixer.SetFloat("MasterVol", setTo);
        m_masterVolume = setTo;
    }
    public void SetAmbienceLvl(float setTo) {
        m_masterMixer.SetFloat("AmbienceVol", setTo);
        m_ambienceVolume = setTo;
    }
    public void SetSFXLvl(float setTo) {
        m_masterMixer.SetFloat("SFXVol", setTo);
        m_sfxVolume = setTo;
    }
}
