using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AMAIManager : MonoBehaviour {

	public FMODUnity.StudioEventEmitter emitter;
	public Visualizer visualizerPrefab;
	public Visualizer visualizerInstance;
	public Text[] texts;
	public Canvas canvas;
	public Button continueButton;
	public Image oasisImage;
	public GameObject idObj;
	public InputField idInput;
	public Dropdown groupDropdown;
	//public PlayerEmotions emotionAnalyzer;

	private int stage = 0;
	private bool shouldStartMedia = false;
	private bool hasStartedStage2 = false;
	private bool hasStartedStage4 = false;
	private bool hasStartedStage6 = false;
	private float musicMaxDur = 10.0f;
	private string id;
	private int group;

	// Use this for initialization
	void Start () {
		//visualizer.gameObject.SetActive(false);
		//canvas.enabled = false;
		continueButton.gameObject.SetActive(true);

		oasisImage.gameObject.SetActive(false);
		DisableAllTexts ();
	}
	
	// Update is called once per frame
	void Update () {
		DisableAllTexts ();

		// Stage
		switch (stage) {
		case 1:
			id = idInput.text;
			group = groupDropdown.value + 1;
			idObj.SetActive (false);
			//emotionAnalyzer.SetID (id);

			texts [0].enabled = true;
			break;
		case 2:
			if (!hasStartedStage2) {
				StartMedia ();
				hasStartedStage2 = true;
			}
			break;
		case 3:
			texts [2].enabled = true;
			break;
		case 4:
			if (!hasStartedStage4) {
				StartMedia ();
				hasStartedStage4 = true;
			}
			break;
		case 5:
			texts [4].enabled = true;
			break;
		case 6:
			if (!hasStartedStage6) {
				StartMedia ();
				hasStartedStage6 = true;
			}
			break;
		case 7:
			texts [6].enabled = true;
			//continueButton.gameObject.SetActive(false);
			break;
		}

		// Media
//		if (shouldStartMedia) {
//			StartMedia ();
//			shouldStartMedia = false;
//		}
	}

	void DisableAllTexts() {
		for (int i=0; i<texts.Length; i++) {
			Text text = texts [i];
			text.enabled = false;
		}
	}

	void StartMedia() {
		// Disable continue button
		continueButton.gameObject.SetActive(false);

		switch (stage) {
		case 2:
			// Show OASIS images
			oasisImage.gameObject.SetActive(true);
			break;
		case 4:
			// Listen to music
			StartMusicEmitter();
			break;
		case 6:
			// Listen to music with visualizations
			StartMusicEmitter ();
			StartVisualizer ();
			break;
		}
	}

	void StartMusicEmitter() {
		emitter.SetParameter("sectionNum", 1f); // Reset
		emitter.Play ();

		canvas.enabled = false;

		Invoke ("StopMusicEmitter", musicMaxDur);
	}

	void StopMusicEmitter() {
		emitter.Stop ();
		canvas.enabled = true;
		continueButton.gameObject.SetActive(true);
		HandleContinueButtonClick ();
	}

	void StartVisualizer() {
		//visualizer.Begin ();
		visualizerInstance = Instantiate(visualizerPrefab);
	}

	public void HandleContinueButtonClick() {
		Debug.Log ("button clicked");
		if (stage < texts.Length) {
			stage++;
		}

		if (emitter.IsPlaying()) {
			emitter.Stop ();
		}
	}

	public void HandleOASISImageClick() {
		// TODO: Transition to further images

		// Enable continue button
		oasisImage.gameObject.SetActive(false);
		continueButton.gameObject.SetActive(true);
		HandleContinueButtonClick ();
	}

	public string GetID() {
		return id;
	}

	public int GetStage() {
		return stage;
	}

	public int GetGroup() {
		return group;
	}
}
