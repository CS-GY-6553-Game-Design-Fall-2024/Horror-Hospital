using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanSoundEffectTrigger : MonoBehaviour
{

    [System.Serializable]
    public struct HumanAudioClip {
        public AudioClip clip;
        public bool playOnAwake;
        public bool loop;
    }

    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private HumanAudioClip[] m_clips;
    private int m_audioIndex = -1;
    private bool m_hasPlayed = false;

    void OnEnable() {
        // If this is enabled, then we want to randomize which clip to play, and then set the appropriate settings
        if (m_clips.Length == null || m_clips.Length == 0) return;
        m_audioIndex = UnityEngine.Random.Range(0, m_clips.Length);
        m_audioSource.clip = m_clips[m_audioIndex].clip;
        m_audioSource.loop = m_clips[m_audioIndex].loop;
        if (m_clips[m_audioIndex].playOnAwake) m_audioSource.Play();
    }

    void OnTriggerEnter(Collider other) {
        // The collider is ONLY relevant if we haven't set a `playOnAwake` enabled
        // Thus, this collider logic is ONLY needed for sound bytes we want to play once only, and never again until a cooldown is reached
        if (
            other.gameObject.tag == "Player" 
            && m_audioIndex != -1 
            && !m_clips[m_audioIndex].playOnAwake
            && !m_hasPlayed
        ) {
            StartCoroutine(PlayOnce());
        }
    }

    private IEnumerator PlayOnce() {
        // Initialize the `hasPlayed` toggle
        m_hasPlayed = true;

        // Grab a random wait time, then wait
        float waitTime = UnityEngine.Random.Range(0f, 0.5f);
        yield return new WaitForSeconds(waitTime);

        // Play the audio clip
        m_audioSource.Play();

        // Wait for a cooldown. In this case, 10 seconds
        yield return new WaitForSeconds(10f);

        // Disable the hasPlayed to allow the sound efect to re-run;
        m_hasPlayed = false;
    }
}
