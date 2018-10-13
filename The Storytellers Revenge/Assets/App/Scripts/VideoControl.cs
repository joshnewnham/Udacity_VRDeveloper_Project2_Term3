using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VideoControl : Singleton<VideoControl> {

	public const string SCENE_INTRO = "scene_intro"; 
	public const string SCENE_TOP_MIDDLE = "scene_top_and_middle"; 
	public const string SCENE_UNDER = "scene_under_waterfall"; 
	public const string SCENE_OUTTRO = "scene_outtro"; 

	public static string[] SCENES = { SCENE_INTRO, SCENE_TOP_MIDDLE, SCENE_UNDER, SCENE_OUTTRO };

	public enum State { Initilising, Playing, Paused, Interactive, Transitioning, Stopped };

	public delegate void SceneUpdated(VideoControl videoControl, VideoScene scene);
	public event SceneUpdated OnSceneUpdated = delegate { };

	public delegate void SceneStateChanged(VideoControl videoControl, VideoScene scene, VideoControl.State videoControlState);
	public event SceneStateChanged OnSceneStateChanged = delegate { };

	public delegate void SceneKeyFrame(VideoControl videoControl, VideoScene scene, VideoSceneKeyframe keyframe);
	public event SceneKeyFrame OnSceneKeyFrame = delegate { };

	public float rewindTime = 5.0f; 
	public float fastForwardTime = 5.0f; 

	private UnityEngine.Video.VideoPlayer videoPlayer;

	private int nextKeyframeIndex = 0; 

	[SerializeField]
	private AudioSource audioSource;

	public struct VideoScene{
		public const string START = "start"; 
		public const string INTERACTIVCE = "interactive"; 
		public const string END = "end"; 

		public string label; 

		public VideoSceneKeyframe[] keyframes; 

		public VideoSceneKeyframe? getKeyFrame(string label){
			foreach (var keyframe in keyframes) {
				if (keyframe.label == label) {
					return keyframe; 
				}
			}

			return null; 
		}

		public VideoSceneKeyframe? getStartKeyframe(){
			return getKeyFrame (VideoScene.START);
		}

		public VideoSceneKeyframe? getInteractiveKeyFrame(){
			return getKeyFrame (VideoScene.INTERACTIVCE);
		}

		public VideoSceneKeyframe? getEndKeyFrame(){
			return getKeyFrame (VideoScene.END);
		}
	}

	public struct VideoSceneKeyframe{
		public string label; 
		public float time; 
	}

	public List<VideoScene> keyScenes = new List<VideoScene> (); 

	private VideoScene? _currentScene; 

	public VideoScene? currentScene{
		get{			
			return this._currentScene; 
		}
		set{
			this._currentScene = value; 

			OnSceneUpdated (this, this._currentScene.Value); 
		}
	}

	private State _currentState = State.Initilising; 

	public State CurrentState {
		get {
			return _currentState; 
		}
		set {
			_currentState = value;

            Debug.LogFormat("VideoControl.CurrentState updated {0}", _currentState); 

			OnSceneStateChanged (this, this._currentScene.Value, this._currentState); 
		}
	}

	void Start () {		
		this.OnSceneUpdated += VideoControl_OnSceneUpdated;
        this.OnSceneStateChanged += VideoControl_OnSceneStateChanged;
		this.OnSceneKeyFrame += VideoControl_OnSceneKeyFrame;

		initKeyScenes (); 
		initVideoPlayer (); 
	}		

	void initVideoPlayer(){
		videoPlayer = GetComponent<UnityEngine.Video.VideoPlayer> ();
		videoPlayer.prepareCompleted += VideoPlayer_prepareCompleted;
		videoPlayer.seekCompleted += VideoPlayer_seekCompleted;
		videoPlayer.started += VideoPlayer_started;

		if (videoPlayer.clip != null) 
		{
			videoPlayer.EnableAudioTrack (0, true);
			videoPlayer.SetTargetAudioSource(0, audioSource);
		}
	}				

	//Check if input keys have been pressed and call methods accordingly.
	void Update(){
		
		if (Input.GetKeyDown ("space")) 
		{
			this.GotoScene (VideoControl.SCENE_UNDER); 
		}

		// search for interactive scene 
		if (this.CurrentState == State.Playing && currentScene.HasValue) {

			var targetKeyframe = this.currentScene.Value.keyframes [this.nextKeyframeIndex];
			if (this.videoPlayer.time >= targetKeyframe.time) {
				setNextKeyframeIndex ();  // update keyframe index 
				// broadcast keyframe change  
				OnSceneKeyFrame(this, this.currentScene.Value, targetKeyframe); 
			}				
		}			
	}

	#region Video player control 

	public bool Play(){
		if (this.CurrentState != State.Paused) {
			return false; 
		}
			
		return this._play(); 
	}

	private bool _play(){
		videoPlayer.Play (); 

		if (audioSource != null) {
			audioSource.Play ();
		}

		this.CurrentState = State.Playing; 

		return true; 
	}

	public bool Pause(){
		if (this.CurrentState != State.Playing) {
			return false; 
		}

		videoPlayer.Pause ();

		if (audioSource != null) {
			audioSource.Pause (); 
		}

		this.CurrentState = State.Paused; 

		return true; 
	}

	public bool FastForward(){
		if (this.CurrentState != State.Playing && this.CurrentState != State.Paused) {
			return false; 
		}

		Skip (this.fastForwardTime);
		return true; 
	}

	public bool Rewind(){
		if (this.CurrentState != State.Playing && this.CurrentState != State.Paused) {
			return false; 
		}

		Skip (this.rewindTime);
		return true; 
	}

	void Skip(float t){
		float targetTime = (float)videoPlayer.time + t; 

		// Don't skip over time beyond the bounds of the keyframes 
		var startKeyframe = this.currentScene.Value.getStartKeyframe().Value;
		var targetKeyframe = this.currentScene.Value.keyframes [this.nextKeyframeIndex];

		if(t < 0){
			targetTime = Mathf.Max (targetTime, startKeyframe.time); 
		} else{
			targetTime = Mathf.Min (targetTime, targetKeyframe.time); 
		}

		videoPlayer.time = targetTime; 
	}

	#endregion

	#region Scene management 

	public bool GotoScene(string scene){
		Debug.LogFormat ("GotoScene {0}", scene); 
		var filteredScenes = keyScenes.Where (s => s.label == scene).ToList (); 

		if (filteredScenes.Count == 0) {
			Debug.LogWarningFormat ("GotoScene failed; could not find {0}", scene);
			return false; 
		}

		var targetScene = filteredScenes [0]; 
		StartCoroutine(TransitionScenes(targetScene));

		return true; 
	}

	IEnumerator TransitionScenes(VideoScene targetScene)
	{
		this._play ();
		var targetTime = this.currentScene.Value.getEndKeyFrame ().Value.time; 
		while (this.videoPlayer.time < targetTime)
		{
			yield return null;
		}
			
		this.currentScene = targetScene; 
	}

	void initKeyScenes (){

		var introScene = new VideoScene {
			label = VideoControl.SCENE_INTRO, 
			keyframes = new VideoSceneKeyframe[]{
				new VideoSceneKeyframe{ label=VideoScene.START, time=0.0f }, 
				new VideoSceneKeyframe{ label=VideoScene.END, time=4.28f }
			}
		};

		var topMiddleScene = new VideoScene {
			label = VideoControl.SCENE_TOP_MIDDLE, 
			keyframes = new VideoSceneKeyframe[]{
				new VideoSceneKeyframe{ label=VideoScene.START, time=5.0f }, 
				new VideoSceneKeyframe{ label="middle", time=17.20f },
				new VideoSceneKeyframe{ label=VideoScene.INTERACTIVCE, time=26.13f }, 
				new VideoSceneKeyframe{ label=VideoScene.END, time=29.08f }
			}
		};

		var underWaterfallScene = new VideoScene {
			label = VideoControl.SCENE_UNDER, 
			keyframes = new VideoSceneKeyframe[]{
				new VideoSceneKeyframe{ label=VideoScene.START, time=29.29f }, 
				//new VideoSceneKeyframe{ label=VideoScene.INTERACTIVCE, time=51.29f }, 
				new VideoSceneKeyframe{ label="rain", time=42.15f }, 
				new VideoSceneKeyframe{ label=VideoScene.END, time=54.00f }
			}
		};

		var outtroScene = new VideoScene {
			label = VideoControl.SCENE_OUTTRO, 
			keyframes = new VideoSceneKeyframe[]{
				new VideoSceneKeyframe{ label=VideoScene.START, time=59.20f }, 
				new VideoSceneKeyframe{ label=VideoScene.END, time=64.10f }
			}
		};
				
		this.keyScenes.Add (introScene);
		this.keyScenes.Add (topMiddleScene);
		this.keyScenes.Add (underWaterfallScene);
		this.keyScenes.Add (outtroScene);
	}			

	void setNextKeyframeIndex(){
		this.nextKeyframeIndex = Mathf.Min (this.nextKeyframeIndex + 1, this.currentScene.Value.keyframes.Length - 1); 
	}

	#endregion 

	#region Callbacks 

	void VideoControl_OnSceneUpdated (VideoControl videoControl, VideoScene scene)
	{
		this.nextKeyframeIndex = 0; 
		this.videoPlayer.time = scene.getStartKeyframe ().Value.time;
	}

	void VideoControl_OnSceneKeyFrame (VideoControl videoControl, VideoScene scene, VideoSceneKeyframe keyframe)
	{
		if (keyframe.label == VideoScene.END) {
			if (scene.label == VideoControl.SCENE_INTRO) {
				GotoScene (VideoControl.SCENE_TOP_MIDDLE);
			} else if (scene.label == VideoControl.SCENE_OUTTRO) {
				this.videoPlayer.Stop (); 
				this.CurrentState = State.Stopped; 
			}
		} else if (keyframe.label == VideoScene.INTERACTIVCE) {
			this.videoPlayer.Pause (); 
			this.CurrentState = State.Interactive;
		}
	}			

    void VideoControl_OnSceneStateChanged(VideoControl videoControl, VideoScene scene, State videoControlState)
    {

    }

	#endregion 

	#region VideoPlayer handlers 

	void VideoPlayer_prepareCompleted (UnityEngine.Video.VideoPlayer source)
	{
		Debug.Log ("VideoPlayer_prepareCompleted"); 

		if (this.CurrentState == State.Initilising) {
			this.CurrentState = State.Playing; 
		}
	}

	void VideoPlayer_started (UnityEngine.Video.VideoPlayer source)
	{
		Debug.Log ("VideoPlayer_started");

		if (!_currentScene.HasValue) {
			_currentScene = keyScenes [0]; 
		}
	}

	void VideoPlayer_seekCompleted (UnityEngine.Video.VideoPlayer source)
	{
		Debug.Log ("VideoPlayer_seekCompleted");

	}

	#endregion 
}
