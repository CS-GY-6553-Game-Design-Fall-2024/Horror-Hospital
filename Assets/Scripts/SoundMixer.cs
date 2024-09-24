using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundMixer : MonoBehaviour
{
    public AudioSource sound1; // Assign AudioSource for Sound1 in the Inspector
    public AudioSource sound2; // Assign AudioSource for Sound2 in the Inspector
    public AudioSource sound3; // Assign AudioSource for Sound3 in the Inspector
    public AudioSource sound4; // Assign AudioSource for Sound4 in the Inspector
    public AudioSource sound5; // Assign AudioSource for Sound5 in the Inspector

    void Start()
    {
        // Start checking if Sound4 is playing or not
        StartCoroutine(CheckAndPlayRandomSounds());
    }

    IEnumerator CheckAndPlayRandomSounds()
    {
        while (true)
        {
            // Check if sound4 is not playing
            if (!sound4.isPlaying)
            {
                sound1.loop = false;
                sound2.loop = false;
                sound3.loop = false;
                sound5.loop = false;

                while (sound1.isPlaying) {
                    yield return null;
                    while (sound2.isPlaying) {
                        yield return null;
                        while (sound3.isPlaying) {
                            yield return null;
                            while (sound5.isPlaying)
                            {
                                yield return null;
                            }
                        }
                    }
                }

                sound1.Stop();
                sound2.Stop();
                sound3.Stop();
                sound5.Stop();
                // Play sound4
                sound4.Play();
                // Generate and play a new random combination
                PlayRandomSoundCombination();
            }
            // Wait for a short interval before checking again
            yield return new WaitForSeconds(0.1f);  // Check every 0.1 seconds
        }
    }

    void PlayRandomSoundCombination()
    {
        // Randomly decide which sounds to play
        bool playSound1 = Random.value > 0.5f;
        bool playSound2 = Random.value > 0.5f;
        bool playSound3 = Random.value > 0.5f;
        bool playSound5 = Random.value > 0.5f;

        // Play the selected sounds
        if (playSound1)
        {
            sound1.Play();
            sound1.loop = true;
        }
        if (playSound2)
        {
            sound2.Play();
            sound2.loop = true;
        }
        if (playSound3)
        {
            sound3.Play();
            sound3.loop = true;
        }
        if(playSound5 && PlayerControl.current.horrorValue >= 5f)
        {
            sound5.Play();
            sound5.loop = true;
        }

        // Ensure at least one sound is played (optional)
        if (!playSound1 && !playSound2 && !playSound3)
        {
            // If no sound was selected, try again
            PlayRandomSoundCombination();
        }
    }
}
