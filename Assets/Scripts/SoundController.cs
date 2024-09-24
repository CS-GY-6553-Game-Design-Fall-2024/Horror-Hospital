using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public static SoundController current;

    [Header("=== Audio Sources ===")]
    [SerializeField] private AudioSource m_backgroundAudioSource;
    [SerializeField] private AudioSource m_sfxAudioSource;

    [Header("=== Audio Clips ===")]
    [SerializeField] private AudioClip m_background;


    private void Awake() {
        current = this;
    }

    public void Initialize() {

    }
}
