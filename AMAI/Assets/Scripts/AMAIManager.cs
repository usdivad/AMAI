using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AMAIManager : MonoBehaviour {

	public FMODUnity.StudioEventEmitter emitter;
	public Visualizer visualizer;
	public Text[] texts;
	public Canvas canvas;
	public Button continueButton;

	private int stage = 1;

	// Use this for initialization
	void Start () {
		visualizer.enabled = false;
		canvas.enabled = false;
		DisableAllTexts ();
	}
	
	// Update is called once per frame
	void Update () {
		DisableAllTexts ();

		switch (stage) {
		case 1:
			texts [0].enabled = true;
			break;
		case 2:
			// Show OASIS images
			break;
		case 3:
			texts [2].enabled = true;
			break;
		case 4:
			// Listen to music
			break;
		case 5:
			texts [4].enabled = true;
			break;
		case 6:
			// Listen to music with visualizations
			break;
		case 7:
			texts [6].enabled = true;
			//continueButton.enabled = false;
			break;
		}
	}

	void DisableAllTexts() {
		for (int i=0; i<texts.Length; i++) {
			Text text = texts [i];
			text.enabled = false;
		}
	}

	public void HandleContinueButtonClick() {
		Debug.Log ("button clicked");
		if (stage < texts.Length) {
			stage++;
		}
	}
}
