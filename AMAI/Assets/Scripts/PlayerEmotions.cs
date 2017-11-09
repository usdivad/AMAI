// Adapted from https://knowledge.affectiva.com/docs/analyze-the-camera-stream-4

using Affdex;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerEmotions : ImageResultsListener
{
	public FMODUnity.StudioEventEmitter emitter;

	public float currentSmile;
	public float currentInterocularDistance;
	public float currentContempt;
	public float currentValence;
	public float currentAnger;
	public float currentFear;
	public float currentSadness;
	public FeaturePoint[] featurePointsList;

	private int numFramesElapsed = 1;
	private float overallValence = 0f;
	private int sectionNum = 1;

	void Start() {
		emitter.SetParameter ("section_num", 1f);
	}

	void UpdateMusicEmitter() {
		float currentValenceScaled = (currentValence + 100f) / 200f; 
		overallValence = (overallValence * 0.7f) + (currentValenceScaled * 0.3f);
		Debug.Log ("overall valence: " + overallValence);

		emitter.SetParameter ("valence_overall", overallValence);
		emitter.SetParameter ("arousal_overall", overallValence * 0.4f); // just testing

		emitter.SetParameter ("section_num", (float) sectionNum);

		if (overallValence > 0.5f && sectionNum < 2) {
			sectionNum = 2;
		}
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
			face.Emotions.TryGetValue(Emotions.Contempt, out currentContempt);
			face.Emotions.TryGetValue(Emotions.Valence, out currentValence);
			face.Emotions.TryGetValue(Emotions.Anger, out currentAnger);
			face.Emotions.TryGetValue(Emotions.Fear, out currentFear);
			face.Emotions.TryGetValue(Emotions.Fear, out currentSadness);

			//Retrieve the Smile Score
			face.Expressions.TryGetValue(Expressions.Smile, out currentSmile);


			//Retrieve the Interocular distance, the distance between two outer eye corners.
			currentInterocularDistance = face.Measurements.interOcularDistance;


			//Retrieve the coordinates of the facial landmarks (face feature points)
			featurePointsList = face.FeaturePoints;

		}

		UpdateMusicEmitter ();
	}
}