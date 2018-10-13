using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPost : MonoBehaviour {

	public delegate void AudioPostFinished(AudioPost audioPost, int status);

    public List<AudioClip> audioClips = new List<AudioClip>(); 

    // Use this for initialization
    void Start () {
		
	}

	public bool hasAudioClip(string audioClipKey){
		foreach (var audioClip in audioClips) {
			if (audioClip.name == audioClipKey) {
				return true; 
			}
		}

		return false; 
	}

	public bool Play(AudioPostFinished handler)
    {
		if (audioClips.Count == 0) {
			return false; 
		}

		return Play (audioClips [0].name, handler); 
    }

    public bool Play(string audioClipKey, AudioPostFinished handler)
    {
		AudioClip audioClip = audioClips.Where((ac) => {
			return ac.name == audioClipKey;
		}).First();

		if (audioClip == null) {
			return false; 
		}

		GetComponent<AudioSource> ().clip = audioClip; 
		GetComponent<AudioSource> ().Play (); 

		StartCoroutine (AudioFinishedListener (handler)); 

		return true; 
    }

	IEnumerator AudioFinishedListener(AudioPostFinished handler){

		while (GetComponent<AudioSource> ().isPlaying) {
			yield return new WaitForSeconds(0.5f); 
		}

		GetComponent<AudioSource> ().clip = null; 

		handler (this, 1); 
	}

    public void Stop(){
		GetComponent<AudioSource> ().Stop (); 
		GetComponent<AudioSource> ().clip = null; 
    }		
}
