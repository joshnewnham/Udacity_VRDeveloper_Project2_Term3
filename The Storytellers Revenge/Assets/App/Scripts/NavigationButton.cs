using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class NavigationButton : MonoBehaviour {

	public enum Event{
		OffFocus,
		OnFocus, 
		Clicked 

	}		

	public delegate void NavigationButtonEvent(NavigationButton button, Event evt);
	public static event NavigationButtonEvent OnNavigationButtonEvent = delegate { };

	public string AssociatedScene; 

	public bool isFocused{ get; private set; } 

	private float lerpTime = 0.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		float scaleX = this.transform.localScale.x; 

		if (this.isFocused) {			
			scaleX = Mathf.Lerp (scaleX, 1.3f, lerpTime); 
		} else {
			scaleX = Mathf.Lerp (scaleX, 1.0f, lerpTime); 
		}

		var scale = this.transform.localScale; 
		scale.x = scale.y = scale.z = scaleX; 
		this.transform.localScale = scale; 

		lerpTime += Time.deltaTime * 0.1f; 
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

		OnNavigationButtonEvent (this, Event.OnFocus); 
	}

	public void onPointerClicked(){
		OnNavigationButtonEvent (this, Event.Clicked); 
	}

	public void onPointerExit(){
		this.offFocus (); 

		OnNavigationButtonEvent (this, Event.OffFocus); 
	}

	public void Hide(){
		GetComponent<Image> ().enabled = false; 
	}

	public void Show(){
		GetComponent<Image> ().enabled = true; 
	}
}
