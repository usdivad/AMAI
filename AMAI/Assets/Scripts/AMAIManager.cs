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
	public string id = "0";
	public int group = 1; // 1-3

	private int stage = 0;
	private bool shouldStartMedia = false;
	private bool hasStartedStage2 = false;
	private bool hasStartedStage4 = false;
	private bool hasStartedStage6 = false;
	private float musicMaxDur = (3 * 60f) + 30f;

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
			//id = idInput.text;
			//group = groupDropdown.value + 1;
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
			if (!emitter.IsPlaying()) {
				emitter.Play ();
				//float sectionNum = (group == 2) ? 5f : 1f;
				//emitter.SetParameter("section_num", sectionNum);
			}
			else {
				CheckAndStopEmitterFromTimelinePosition();
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
			if (!emitter.IsPlaying()) {
				emitter.Play ();
				//float sectionNum = (group == 2) ? 5f : 1f;
				//emitter.SetParameter("section_num", sectionNum);
			}
			else {
				CheckAndStopEmitterFromTimelinePosition();
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
		//continueButton.gameObject.SetActive(false);

		switch (stage) {
		case 2:
			// Show OASIS images
			//oasisImage.gameObject.SetActive(true);
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
//		float sectionNum = 0;
//		if (group == 1) {
//			sectionNum = 1;
//		}
//		else if (group == 2) {
//			sectionNum = 5;
//		}
//		else if (group == 3) {
//			sectionNum = 1;
//		}

		//float sectionNum = (group == 2) ? 5f : 1f;
		//emitter.SetParameter("section_num", sectionNum); // Reset
		//Debug.Log("set section_num to " + sectionNum);
		//emitter.SetParameterValueByIndex (emitter.GetParameterIndex ("section_num"), sectionNum);
		emitter.Play ();
		//emitter.SetParameter("section_num", sectionNum); // Reset

		//if (stage == 6) {
			canvas.enabled = false;
		//}

		Invoke ("StopMusicEmitter", musicMaxDur);
	}

	void StopMusicEmitter() {
		CancelInvoke ("StopMusicEmitter");
		emitter.Stop ();
		canvas.enabled = true;
		continueButton.gameObject.SetActive(true);
		HandleContinueButtonClick ();
	}

	void CheckAndStopEmitterFromTimelinePosition() {
		bool shouldStop = false;
		int position = 0;
		emitter.GetTimelinePosition (out position);
		if (position >= (4*60000) + (21*1000) + 500) {
			shouldStop = true;
		}

		if (shouldStop) {
			StopMusicEmitter();
		}
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

	public void HandleToVizButtonClick() {
		Debug.Log("to viz");
		stage = 5;
		//id = idInput.text;
		//group = groupDropdown.value + 1;
		idObj.SetActive (false);
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
