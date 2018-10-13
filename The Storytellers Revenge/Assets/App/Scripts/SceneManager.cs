using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;

/// <summary>
/// Manger controller for the scene; controls when the video should pause and when UI and interactive 
/// elements should be enabled and visible to the user 
/// </summary>
public class SceneManager : Singleton<SceneManager>
{
	private AudioPost[] audioPosts; 

	private NavigationButton[] navigationButtons; 

	public ParticleSystem rainParticleSystem; 

	public Dictionary<string, int> videoScenesVisitCount = new Dictionary<string, int> (); 

    // Use this for initialization
    void Start()
    {		    
		HideRainingEffect (); 

		audioPosts = GameObject.FindObjectsOfType<AudioPost> (); 

		navigationButtons = GameObject.FindObjectsOfType<NavigationButton> (); 
		foreach (var but in navigationButtons) {
			but.Hide (); 
		}

		NavigationButton.OnNavigationButtonEvent += OnNavigationButtonEvent;

		VideoControl.Instance.OnSceneUpdated += OnSceneUpdated;
		VideoControl.Instance.OnSceneStateChanged += OnSceneStateChanged;
		VideoControl.Instance.OnSceneKeyFrame += OnSceneKeyFrame;

		foreach (var scene in VideoControl.SCENES) {
			videoScenesVisitCount.Add (scene, 0); 
		}
    }
		    		 
    // Update is called once per frame
    void Update()
    {

    }

	void ShowRainingEffect(){
		if (!rainParticleSystem.isPlaying) {
			rainParticleSystem.Play (); 
		}
	}

	void HideRainingEffect(){
		if (rainParticleSystem.isPlaying) {
			rainParticleSystem.Stop (); 
		}
	}

	#region NavigationButton events 

	void OnNavigationButtonEvent (NavigationButton button, NavigationButton.Event evt)
	{
		Debug.LogFormat ("OnNavigationButtonEvent {0} {1}", button.name, evt); 
		if (evt != NavigationButton.Event.Clicked) {
			return; 
		}			
			
		if (VideoControl.Instance.CurrentState == VideoControl.State.Interactive &&
			VideoControl.Instance.currentScene.Value.label == VideoControl.SCENE_TOP_MIDDLE) {

			if (button.name == "Top") {
				VideoControl.Instance.GotoScene (VideoControl.SCENE_TOP_MIDDLE); 
			} else {
				VideoControl.Instance.GotoScene (VideoControl.SCENE_UNDER); 
			}

			// enable interactive controls 
			foreach (var but in navigationButtons) {
				but.Hide ();  
			}
		}			
	}

	#endregion 

    #region VideoControl handlers 

    void OnSceneUpdated(VideoControl videoControl, VideoControl.VideoScene scene)
    {
		Debug.LogFormat ("OnSceneUpdated :: scene:{0}", scene.label);

		videoScenesVisitCount [scene.label] += 1; 

		if (videoScenesVisitCount [scene.label] == 1) {
			// if first time visit then play the associated audio track 
			switch (scene.label) {
			case VideoControl.SCENE_INTRO:
				playAudioClips (new string[]{ "scene0_a" });
				break; 
			case VideoControl.SCENE_TOP_MIDDLE:
				playAudioClips (new string[]{ "scene1_a" });
				break; 
			case VideoControl.SCENE_UNDER:
				playAudioClips (new string[]{ "scene2_a" });
				break; 
			case VideoControl.SCENE_OUTTRO:
				playAudioClips (new string[]{ "scene3_a" });
				break; 
			}
		}
    }

	void OnSceneKeyFrame (VideoControl videoControl, VideoControl.VideoScene scene, VideoControl.VideoSceneKeyframe keyframe)
	{
		Debug.LogFormat ("OnSceneKeyFrame :: scene:{0}, keyframe:{1}", scene.label, keyframe.label);

		HideRainingEffect (); 

		if (keyframe.label == VideoControl.VideoScene.START) {
			// ignore 
		} else if (keyframe.label == VideoControl.VideoScene.END) {
			if (scene.label == VideoControl.SCENE_UNDER) {
				VideoControl.Instance.GotoScene (VideoControl.SCENE_OUTTRO); 
			} else if (scene.label == VideoControl.SCENE_OUTTRO) { // that's all folks 
				VideoControl.Instance.Pause (); 
			}
		} else if (keyframe.label == VideoControl.VideoScene.INTERACTIVCE) {
			if (scene.label == VideoControl.SCENE_TOP_MIDDLE) {
				playAudioClips (new string[]{ "scene1_c", "scene1_d", "scene1_e" });
			}

			// enable interactive controls 
			foreach (var but in navigationButtons) {
				if (but.AssociatedScene == scene.label) {
					but.Show (); 
				}
			}

		} else if (keyframe.label == "middle" && scene.label == VideoControl.SCENE_TOP_MIDDLE) {
			// play associated audio 
			playAudioClips (new string[]{ "scene1_b" });
		} else if (keyframe.label == "rain") {
			ShowRainingEffect (); 
		}
	}

    void OnSceneStateChanged(VideoControl videoControl, VideoControl.VideoScene scene, VideoControl.State videoControlState)
    {		
		Debug.LogFormat ("OnSceneStateChanged :: scene:{0}, state:{1}", scene.label, videoControlState.ToString()); 
    }

    #endregion 

	#region AudioPost methods 

	void playAudioClips(string[] audioClipKeys){		
		if (audioClipKeys == null || audioClipKeys.Length == 0) {
			onFinishedPlayingAudioClips (1); 
			return; 
		}

		Debug.LogFormat ("playAudioClips {0}", audioClipKeys);

		AudioPost audioPost = findAudioPostForAudioClip (audioClipKeys [0]); 
		if (audioPost == null) {
			onFinishedPlayingAudioClips (-1); 
			return;
		}

		audioPost.Play(audioClipKeys[0], (AudioPost soruceAudioPost, int status) => {
			this.playAudioClips(audioClipKeys.Skip(1).ToArray()); 
		}); 
	}

	AudioPost findAudioPostForAudioClip(string audioClipKey){

		var audioPostList = audioPosts.Where ((audioPost) => {
			return audioPost.hasAudioClip(audioClipKey);
		}).ToList();

		if (audioPostList.Count == 0) {
			return null; 
		}

		return audioPostList [0];
	}

	void onFinishedPlayingAudioClips(int status){

	}

	#endregion 

}
