using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {

	public float masterVolume = 1;
	public float musicVolume = .8f;
	public float sfxVolume = 1f;
	public AudioClip defaultMusic;

	private AudioSource[] musicSources;
	private AudioSource sfx2DSource;
	private int activeMusicSourceIndex;

	public static AudioManager instance;
	private SoundLibrary soundLibrary;


	void Start() {

		instance = this;
		soundLibrary = GetComponent<SoundLibrary> ();

		musicSources = new AudioSource[2];
		for (int i = 0; i < 2; i++) {
			GameObject newMusicSource = new GameObject ("Music source " + (i + 1));
			musicSources [i] = newMusicSource.AddComponent<AudioSource> ();
			musicSources [i].GetComponent<AudioSource> ().loop = true;
			newMusicSource.transform.parent = transform;
		}
		GameObject newSFXSource = new GameObject ("SFX2D source");
		sfx2DSource = newSFXSource.AddComponent<AudioSource> ();
		sfx2DSource.transform.parent = transform;


		if (defaultMusic != null) {
			StartCoroutine (PlayMusicOnDelay (.2f));
		}
	}

	public void PlayMusic(AudioClip clip, float fadeDuration = 1) {
		activeMusicSourceIndex = 1 - activeMusicSourceIndex;
		musicSources [activeMusicSourceIndex].clip = clip;
		musicSources[activeMusicSourceIndex].Play();

		StartCoroutine (AnimateMusicCrossfade (fadeDuration));
	}
		
	public void PlayCustomSound (string clip,Vector3 soundPos, string entityToFollow, float volume, bool useGroup) {
		// Get correct audio clip to play
		AudioClip clipToPlay = null;
		if (useGroup) {
			clipToPlay = soundLibrary.GetGroupClip (clip);
		} else {
			clipToPlay = soundLibrary.GetClip (clip);
		}

		if (clipToPlay == null) {
			return;
		}
			
		// Setup sound game object
		GameObject newSFXSource = new GameObject ("SFX2D source");
		AudioSource newAudioSource = newSFXSource.AddComponent<AudioSource> ();
		newSFXSource.transform.parent = transform;
		// Sound volume settings
		newAudioSource.maxDistance = 30;
		newAudioSource.spatialBlend = .96f;
		// Play audio and destroy it after X amount of time
		newAudioSource.PlayOneShot (clipToPlay, masterVolume * sfxVolume * volume * volume);
		StartCoroutine (DestroyCustomSFXSource (newSFXSource));
	}

	public void PlayCustomSound2D (string clip, float volume, bool useGroup) {

		// Get correct audio clip to play
		AudioClip clipToPlay = null;
		if (useGroup) {
			clipToPlay = soundLibrary.GetGroupClip (clip);
		} else {
			clipToPlay = soundLibrary.GetClip (clip);
		}

		if (clipToPlay == null) {
			return;
		}
			
		sfx2DSource.PlayOneShot (clipToPlay, masterVolume * sfxVolume * volume);
	}

	IEnumerator DestroyCustomSFXSource(GameObject obj) {
		yield return new WaitForSeconds (10);
		Destroy (obj);
	}

	IEnumerator AnimateMusicCrossfade (float duration) {
		float percent = 0;

		while (percent < 1) {
			percent += Time.deltaTime * 1 / duration;
			musicSources [activeMusicSourceIndex].volume = Mathf.Lerp (0, musicVolume * masterVolume, percent);
			musicSources [1-activeMusicSourceIndex].volume = Mathf.Lerp (musicVolume * masterVolume, 0, percent);
			yield return null;
		}
	}

	IEnumerator StopAudio(AudioSource source, float duration) {
		yield return new WaitForSeconds (duration);
		source.Stop ();
	}


	IEnumerator PlayMusicOnDelay(float delay) {
		yield return new WaitForSeconds (delay);
		PlayMusic (defaultMusic);
	}
}