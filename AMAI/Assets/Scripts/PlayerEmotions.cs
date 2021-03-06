﻿// Adapted from https://knowledge.affectiva.com/docs/analyze-the-camera-stream-4

using Affdex;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mono.Csv;

public class PlayerEmotions : ImageResultsListener
{
	public FMODUnity.StudioEventEmitter emitter;
	public string basePath;
	public AMAIManager amaiManager;

	public float currentSmile;
	public float currentInterocularDistance;

	public float currentJoy;
	public float currentFear;
	public float currentDisgust;
	public float currentSadness;
	public float currentAnger;
	public float currentSurprise;
	public float currentContempt;
	public float currentValence;
	public float currentEngagement;

	public FeaturePoint[] featurePointsList;

	private int numFramesElapsed = 1;
	private int numImageResults = 0;
	private float overallValence = 0f;
	private float initialValence = 0f;
	private int numDataPointsForInitialValence = 60;
	private int sectionNum = 0;
	private bool hasSection4Occurred = false;
	private bool shouldGoToNextSection = false;
	private bool isSectionTransitionForced = false;
	private float forceSectionTransitionTime = 5.0f;

	//private string id;

	void Start() {
		//emitter.SetParameter ("section_num", 1f); // Gets set by AMAIManager now
	}

	void UpdateMusicEmitter() {
		// Calculate overall valence (scaled 0-1)
		float currentValenceScaled = (currentValence + 100f) / 200f; 
		overallValence = (overallValence * 0.7f) + (currentValenceScaled * 0.3f);

		// Initial valence, if necessary
		if (numImageResults <= numDataPointsForInitialValence) {
			initialValence = overallValence;
		}

		// Difference between overall and initial valence
		float overallInitialValenceDiff = overallValence - initialValence;

		// Kenny Loggins
		//Debug.Log ("overall valence: " + overallValence);
		//Debug.Log ("difference between overall and initial: " + overallInitialValenceDiff);

		// Set section
		//bool shouldGoToNextSection = false;
		int position = 0;
		emitter.GetTimelinePosition (out position);
		//Debug.Log ("position: " + position);

		if (!(shouldGoToNextSection || isSectionTransitionForced)) {
			Debug.Log ("a");

			if (position < (0*60000) + (3*1000) + 333) {
				//Debug.Log ("invok");
				//Invoke ("ForceSectionTransition", forceSectionTransitionTime);
				ForceSectionTransition(); // immediately
			}

			shouldGoToNextSection = overallValence > 0.5f || overallInitialValenceDiff > 0.1f; // Actually, this *is* basically looking for peaks
			if (shouldGoToNextSection) {
				Debug.Log ("Peak detected! overall=" + overallValence + ", diff=" + overallInitialValenceDiff);
				WriteRowToCSV (true);
				
				CancelInvoke ("ForceSectionTransition");
				SetNextSection();
			}

			// Set based on group
			int group = amaiManager.GetGroup ();
			if (group == 2 && (sectionNum == 1 || sectionNum == 2)) {
				sectionNum = 5;
			}


			if (shouldGoToNextSection || isSectionTransitionForced) {
				CancelInvoke ("ForceSectionTransition");
				Invoke ("ForceSectionTransition", forceSectionTransitionTime);

				//SetNextSection();
			}

			// REset
			//shouldGoToNextSection = false;
			//isSectionTransitionForced = false;
		}
		else {
			Debug.Log("b");

			int trueSectionNum = 0;
			if (position >= (4*60000) + (12*1000) + 982) {
				trueSectionNum = 7;
			}
			else if (position >= (3*60000) + (44*1000) + 561) {
				trueSectionNum = 6;
			}
			else if (position >= (3*60000) + (6*1000) + 667) {
				trueSectionNum = 5;
			}
			else if (position >= (2*60000) + (0*1000) + 0) {
				trueSectionNum = 4;
			}
			else if (position >= (0*60000) + (30*1000) + 0) {
				trueSectionNum = 2;
			}
			else if (position >= (0*60000) + (3*1000) + 333) {
				trueSectionNum = 1;
			}

			Debug.Log("pos=" + position + ", n=" + sectionNum + ", true=" + trueSectionNum);

			if (sectionNum == trueSectionNum) {
				// Clear/reset
				shouldGoToNextSection = false;
				isSectionTransitionForced = false;
			}
		}

		// Update music emitter
		emitter.SetParameter ("valence_overall", overallValence);
		emitter.SetParameter ("arousal_overall", overallValence * 0.4f); // just testing
		emitter.SetParameter ("section_num", (float) sectionNum);
	}

	public override void onFaceFound(float timestamp, int faceId)
	{
		Debug.Log("Found the face");
	}

	public override void onFaceLost(float timestamp, int faceId)
	{
		Debug.Log("Lost the face");
	}

	public override void onImageResults(Dictionary<int, Face> faces)
	{
		Debug.Log("Got face results");

		foreach (KeyValuePair<int, Face> pair in faces)
		{
			int FaceId = pair.Key;  // The Face Unique Id.
			Face face = pair.Value;    // Instance of the face class containing emotions, and facial expression values.

			//Retrieve the Emotions Scores
			face.Emotions.TryGetValue(Emotions.Joy, out currentJoy);
			face.Emotions.TryGetValue(Emotions.Fear, out currentFear);
			face.Emotions.TryGetValue(Emotions.Disgust, out currentDisgust);
			face.Emotions.TryGetValue(Emotions.Sadness, out currentSadness);
			face.Emotions.TryGetValue(Emotions.Anger, out currentAnger);
			face.Emotions.TryGetValue(Emotions.Surprise, out currentSurprise);
			face.Emotions.TryGetValue(Emotions.Contempt, out currentContempt);
			face.Emotions.TryGetValue(Emotions.Valence, out currentValence);
			face.Emotions.TryGetValue(Emotions.Engagement, out currentEngagement);


			//Retrieve the Smile Score
			face.Expressions.TryGetValue(Expressions.Smile, out currentSmile);
			//face.Expressions.TryGetValue(Expressions.Smile, out currentSmile);


			//Retrieve the Interocular distance, the distance between two outer eye corners.
			currentInterocularDistance = face.Measurements.interOcularDistance;


			//Retrieve the coordinates of the facial landmarks (face feature points)
			featurePointsList = face.FeaturePoints;

			// Write to CSV
			WriteRowToCSV();
		}

		int stage = amaiManager.GetStage();
		if (stage == 4 || stage == 6) {
			UpdateMusicEmitter ();
			numImageResults++;
		}
		else {
			// Reset params
			numImageResults = 0;
			initialValence = 0;
			overallValence = 0;
		}

	}

//	public void SetID(string newId) {
//		id = newId;
//	}

	public void ForceSectionTransition() {
		Debug.Log ("forcing section transition");
		isSectionTransitionForced = true;
		//shouldGoToNextSection = true;
		SetNextSection();
	}

	void SetNextSection() {
		int group = amaiManager.GetGroup ();
		int prevSectionNum = sectionNum;

		if (group == 1) { // Discharge: 1-2-4-1-2-8
			if (sectionNum == 0) {
				sectionNum = 1;
			}
			else if (sectionNum == 1) {
				sectionNum = 2;
			} else if (sectionNum == 2) {
				if (hasSection4Occurred) {
					sectionNum = 8;
				} else {
					sectionNum = 4;
				}
			} else if (sectionNum == 4) {
				sectionNum = 1;
				hasSection4Occurred = true;
			}
			//				} else if (sectionNum == 5) {
			//					sectionNum = 6;
			//				} else {
			//					sectionNum = 7;
			//				}
		} else if (group == 2) { // Diversion: 5-6-4-5-6-7
			if (sectionNum == 0) {
				sectionNum = 5;
			}
			else if (sectionNum == 5) {
				sectionNum = 6;
			} else if (sectionNum == 6) {
				if (hasSection4Occurred) {
					sectionNum = 7;
				} else {
					sectionNum = 4;
				}
			} else if (sectionNum == 4) {
				sectionNum = 5;
				hasSection4Occurred = true;
			}
		} else if (group == 3) { // Combination: 1-2-4-5-6-7
			if (sectionNum == 0) {
				sectionNum = 1;
			}
			else if (sectionNum == 1) {
				sectionNum = 2;
			} else if (sectionNum == 2) {
				sectionNum = 4;
			} else if (sectionNum == 4) {
				sectionNum = 5;
				hasSection4Occurred = true;
			} else if (sectionNum == 5) {
				sectionNum = 6;
			} else {
				sectionNum = 7;
			}
		}
		Debug.Log ("going from section " + prevSectionNum + " to " + sectionNum);
	}

	void WriteRowToCSV(bool sectionChange=false) {
		string id = amaiManager.GetID();
		int group = amaiManager.GetGroup ();
		int stage = amaiManager.GetStage();
		string now = System.DateTime.Now.ToString ("MM/dd/yyyy hh:mm:ss");
		if (stage < 1) {
			return;
		}

		string csvPath = basePath + "/" + id + ".csv";
		List<List<string>> dataGrid = new List<List<string>>();
		if (System.IO.File.Exists (csvPath)) {
			dataGrid = CsvFileReader.ReadAll (csvPath, Encoding.GetEncoding("gbk"));
		}
		else {
			List<string> header = new List<string>() {
				"id",
				"group",
				"stage",
				"datetime",
				"emotion_joy",
				"emotion_fear",
				"emotion_disgust",
				"emotion_sadness",
				"emotion_anger",
				"emotion_surprise",
				"emotion_contempt",
				"emotion_valence",
				"emotion_engagement",
				"expression_smile",
				"music_valence_initial",
				"music_valence_overall",
				"music_section_change"
			};
			dataGrid.Add (header);
		}

		CsvFileWriter writer = new CsvFileWriter(csvPath);
		List<string> row = new List<string> () {
			id,
			group.ToString(),
			stage.ToString(),
			now,
			currentJoy.ToString(),
			currentFear.ToString(),
			currentDisgust.ToString(),
			currentSadness.ToString(),
			currentAnger.ToString(),
			currentSurprise.ToString(),
			currentContempt.ToString(),
			currentValence.ToString(),
			currentEngagement.ToString(),
			currentSmile.ToString(),
			initialValence.ToString(),
			overallValence.ToString(),
			sectionChange ? "1" : "0"
		};

		//writer.WriteRow (row);
		//writer.Dispose ();

		dataGrid.Add(row);
		//CsvFileWriter.WriteAll (dataGrid, csvPath, Encoding.GetEncoding("utf-32"));
		foreach(List<string> r in dataGrid) {
			writer.WriteRow(r);
		}
		writer.Dispose ();
	}
}