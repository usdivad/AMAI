// Adapted from https://knowledge.affectiva.com/docs/analyze-the-camera-stream-4

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
	private int numDataPointsForInitialValence = 120;
	private int sectionNum = 1;

	//private string id;

	void Start() {
		emitter.SetParameter ("section_num", 1f);
	}

	void UpdateMusicEmitter() {
		// Calculate overall valence (scaled 0-1)
		float currentValenceScaled = (currentValence + 100f) / 200f; 
		overallValence = (overallValence * 0.7f) + (currentValenceScaled * 0.3f);
		Debug.Log ("overall valence: " + overallValence);

		// Initial valence, if necessary
		if (numImageResults <= numDataPointsForInitialValence) {
			initialValence = overallValence;
		}

		// Difference between overall and initial valence
		float overallInitialValenceDiff = overallValence - initialValence;
		Debug.Log ("difference between overall and initial: " + overallInitialValenceDiff);

		// Set section
		bool shouldGoToNextSection = false;
		shouldGoToNextSection = overallValence > 0.5f || overallInitialValenceDiff > 0.1f; // Actually, this *is* basically looking for peaks
		// TODO: Another one based on current timeline position

		if (shouldGoToNextSection) {
			if (sectionNum == 1) {
				sectionNum = 2;
			}
			else if (sectionNum == 2) {
				sectionNum = 4;
			}
			else if (sectionNum == 4) {
				sectionNum = 5;
			}
			else if (sectionNum == 5) {
				sectionNum = 6;
			}
			else {
				sectionNum = 7;
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
			string id = amaiManager.GetID();
			int stage = amaiManager.GetStage();
			if (stage < 1) {
				break;
			}

			string csvPath = basePath + "/" + id + ".csv";
			List<List<string>> dataGrid = new List<List<string>>();
			if (System.IO.File.Exists (csvPath)) {
				dataGrid = CsvFileReader.ReadAll (csvPath, Encoding.GetEncoding("gbk"));
			}
			else {
				List<string> header = new List<string>() {
					"id",
					"stage",
					"emotion_joy",
					"emotion_fear",
					"emotion_disgust",
					"emotion_sadness",
					"emotion_anger",
					"emotion_surprise",
					"emotion_contempt",
					"emotion_valence",
					"emotion_engagement",
				};
				dataGrid.Add (header);
			}

			CsvFileWriter writer = new CsvFileWriter(csvPath);
			List<string> row = new List<string> () {
				id,
				stage.ToString(),
				currentJoy.ToString(),
				currentFear.ToString(),
				currentDisgust.ToString(),
				currentSadness.ToString(),
				currentAnger.ToString(),
				currentSurprise.ToString(),
				currentContempt.ToString(),
				currentValence.ToString(),
				currentEngagement.ToString()
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

		UpdateMusicEmitter ();
		numImageResults++;
	}

//	public void SetID(string newId) {
//		id = newId;
//	}
}