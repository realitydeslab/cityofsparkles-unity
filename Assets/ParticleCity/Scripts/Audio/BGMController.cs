using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class BGMController : MonoBehaviour {

    public Transform SoundCenter;
    public AudioMixer AudioMixer;

    private AudioSource[] AudioTracks;

	void Start () {
	    AudioTracks = GetComponents<AudioSource>();
	}
	
	void Update () {
        // Keep tracks in sync
        /*
        for (int i = 1; i < AudioTracks.Length; i++) {
            AudioTracks[i].timeSamples = AudioTracks[0].timeSamples;
        }
        */

        // Adjust volume by distance
	    float distance = Mathf.Abs(SoundCenter.position.y - transform.position.y);

	    float fxVolume = -Mathf.Exp(distance / 100) + 1;
	    AudioMixer.SetFloat("FXVol", fxVolume);
	}
}
