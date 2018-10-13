using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoControlButton : MonoBehaviour {

	public enum Event{
		OffFocus,
		OnFocus, 
		Clicked 

	}

	public delegate void VideoControlButtonEvent(VideoControlButton button, Event evt);
	public static event VideoControlButtonEvent OnVideoControlButtonEvent = delegate { };

	public GameObject panel; 

	public Color targetColor = new Color (249f / 255.0f, 180f / 255.0f, 77f / 255.0f, 0.5f); 

	private Color srcColor; 

	private Material panelMaterial; 

	public bool isFocused{ get; private set; } 

	private float lerpTime = 0.0f; 

	// Use this for initialization
	void Start () {
		if (panel != null) {
			panelMaterial = panel.GetComponent<MeshRenderer> ().material;
			srcColor = panelMaterial.color;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (this.panelMaterial != null) {
			if (this.isFocused) {
				this.panelMaterial.color = Color.Lerp (this.panelMaterial.color, targetColor, lerpTime); 
			} else {
				this.panelMaterial.color = Color.Lerp (this.panelMaterial.color, srcColor, lerpTime); 
			}

			lerpTime += Time.deltaTime * 0.1f; 
		}
	}

	public void onFocus(){
		Debug.Log ("onFocus"); 
		this.isFocused = true; 
		lerpTime += 0.0f; 
	}

	public void offFocus(){
		Debug.Log ("offFocus"); 
		this.isFocused = false; 
		lerpTime += 0.0f; 
	}

	public void onPointerEnter(){
		this.onFocus (); 

		OnVideoControlButtonEvent (this, Event.OnFocus); 
	}

	public void onPointerClicked(){
		OnVideoControlButtonEvent (this, Event.Clicked); 
	}

	public void onPointerExit(){
		this.offFocus (); 

		OnVideoControlButtonEvent (this, Event.OffFocus); 
	}
}
