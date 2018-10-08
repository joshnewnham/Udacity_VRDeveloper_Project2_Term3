using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VideoControl : Singleton<VideoControl> {

	public enum State { Initilising, Playing, Paused, Interactive, Transitioning, Stopped };

	public delegate void SceneUpdated(VideoControl videoControl, VideoScene scene);
	public event SceneUpdated OnSceneUpdated = delegate { };

	public delegate void SceneStateChanged(VideoControl videoControl, VideoScene scene, VideoControl.State videoControlState);
	public event SceneStateChanged OnSceneStateChanged = delegate { };

	private UnityEngine.Video.VideoPlayer videoPlayer;

	[SerializeField]
	private AudioSource audioSource;

	public struct VideoScene{
		public string label; 
		public float startTime; 
		public float softEndTime; 
		public float hardEndTime; 
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

			OnSceneStateChanged (this, this._currentScene.Value, this._currentState); 
		}
	}

	void Start () {		
		this.OnSceneUpdated += VideoControl_OnSceneUpdated;

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
			this.GotoScene ("outtro"); 
		}

//		if (this.IsPlaying && currentScene.HasValue) {
//			
//		}
	}

	#region Video player control 

	public bool Play(){
		if (!videoPlayer.isPrepared) {
			return false; 
		}
			
		videoPlayer.Play (); 
		if (audioSource != null) {
			audioSource.Play ();
		}

		return true; 
	}

	public bool Pause(){
		if (!videoPlayer.isPrepared) {
			return false; 
		}			

		videoPlayer.Pause (); 
		if (audioSource != null) {
			audioSource.Pause (); 
		}

		return true; 
	}

	#endregion

	#region Scene management 

	public bool GotoScene(string scene){
		var filteredScenes = keyScenes.Where (s => s.label == scene).ToList (); 

		if (filteredScenes.Count == 0) {
			return false; 
		}

		var targetScene = filteredScenes [0]; 
		StartCoroutine(TransitionScenes(targetScene));

		return true; 
	}

	IEnumerator TransitionScenes(VideoScene targetScene)
	{
		this.Play (); 

		while (this.videoPlayer.time < this.currentScene.Value.hardEndTime)
		{
			yield return null;
		}

		this.videoPlayer.time = targetScene.startTime;
		this.currentScene = targetScene; 
	}

	void initKeyScenes (){
		keyScenes.Add (new VideoScene{ label = "intro", startTime = 0.0f, softEndTime = 4.28f, hardEndTime=4.28f }); 
		keyScenes.Add (new VideoScene{ label = "scene1", startTime = 5.0f, softEndTime = 26.13f, hardEndTime=29.08f }); 
		keyScenes.Add (new VideoScene{ label = "scene2", startTime = 29.29f, softEndTime = 51.29f, hardEndTime=54.00f }); 
		keyScenes.Add (new VideoScene{ label = "outtro", startTime = 59.20f, softEndTime = 64.20f, hardEndTime=64.20f }); 
	}			

	#endregion 

	#region Callbacks 

	void VideoControl_OnSceneUpdated (VideoControl videoControl, VideoScene scene)
	{
		this.videoPlayer.time = scene.startTime; 
	}

	#endregion 

	#region VideoPlayer handlers 

	void VideoPlayer_prepareCompleted (UnityEngine.Video.VideoPlayer source)
	{
		Debug.Log ("VideoPlayer_prepareCompleted"); 
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
