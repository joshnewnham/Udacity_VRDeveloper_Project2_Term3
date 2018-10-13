using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;
using UnityEngine.EventSystems;

public class VideoControlUI : Singleton<VideoControlUI> {

	public Texture2D playButtonTexture; 

	public Texture2D pauseButtonTexture; 

	private bool _isInFocus = false; 
	private float _isInFocusDeltaTime = 0.0f; 

	public bool isInFocus{
		get{ return _isInFocus; } 
		set{
			if (value != this._isInFocus) {
				_isInFocusDeltaTime = 0.0f; 
			}
			this._isInFocus = value; 
		}
	}

	private GvrPointerPhysicsRaycaster raycaster;

	private VideoControlButton[] videoControlButtons; 

	// Use this for initialization
	void Start () {
		raycaster = GameObject.FindObjectOfType<GvrPointerPhysicsRaycaster> (); 

		videoControlButtons = GameObject.FindObjectsOfType<VideoControlButton> (); 

		VideoControl.Instance.OnSceneUpdated += OnSceneUpdated;
		VideoControlButton.OnVideoControlButtonEvent += OnVideoControlButtonEvent;

		UpdatePlayButtonTexture (); 
	}		
	
	// Update is called once per frame
	void Update () {		
		this.isInFocus = (Camera.main.transform.eulerAngles.x >= 45.0f && Camera.main.transform.eulerAngles.x <= 88.0f);

		if (!this.isInFocus) {
			if (!Mathf.Approximately (this.transform.localScale.x, 0.0f)) {
				var scale = this.transform.localScale; 
				scale.x = Mathf.Lerp (scale.x, 0.0f, _isInFocusDeltaTime); 
				scale.y = scale.z = scale.x;
				this.transform.localScale = scale; 
			}
		} else {
			if (!Mathf.Approximately(this.transform.localScale.x, 1.0f)) {
				var scale = this.transform.localScale; 
				scale.x = Mathf.Lerp (scale.x, 1.0f, _isInFocusDeltaTime);
				scale.y = scale.z = scale.x;
				this.transform.localScale = scale; 
			}
		}

		_isInFocusDeltaTime += Time.deltaTime * 2.0f; 
	}

	void UpdatePlayButtonTexture(){
		var playPauseButton = videoControlButtons.Where ((but) => but.name == "PlayPause").First (); 
		if (VideoControl.Instance.CurrentState == VideoControl.State.Paused) {
			playPauseButton.GetComponent<MeshRenderer> ().enabled = true;
			playPauseButton.GetComponent<MeshRenderer> ().material.mainTexture = playButtonTexture; 
		} else if (VideoControl.Instance.CurrentState == VideoControl.State.Playing) {
			playPauseButton.GetComponent<MeshRenderer> ().enabled = true;
			playPauseButton.GetComponent<MeshRenderer> ().material.mainTexture = pauseButtonTexture; 
		} else {
			playPauseButton.GetComponent<MeshRenderer> ().enabled = false; 
		}
	}

	#region VideoControl events 

	void OnSceneUpdated (VideoControl videoControl, VideoControl.VideoScene scene)
	{
		UpdatePlayButtonTexture (); 
	}

	#endregion 

	#region VideoControlButton events 

	void OnVideoControlButtonEvent (VideoControlButton button, VideoControlButton.Event evt)
	{
		Debug.LogFormat ("VideoControlButton_OnVideoControlButtonEvent {0} {1}", button.name, evt);

		if (evt != VideoControlButton.Event.Clicked) {
			return; 
		}

		if (button.name == "PlayPause") {
			if (VideoControl.Instance.CurrentState == VideoControl.State.Paused) {
				VideoControl.Instance.Play (); 
			} else if (VideoControl.Instance.CurrentState == VideoControl.State.Playing) {
				VideoControl.Instance.Pause ();  
			} 

			UpdatePlayButtonTexture (); 
		} else if (button.name == "FastForward") {
			VideoControl.Instance.FastForward (); 
		} else if (button.name == "Rewind") {
			VideoControl.Instance.Rewind ();  
		}
	}

	#endregion 
}	
