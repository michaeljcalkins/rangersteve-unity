/**
 * Animation download. Adding points to the word loading
 */

using UnityEngine;
using UnityEngine.UI;

public class LoadingAnimation : MonoBehaviour
{
	// Dot animation during the boot
	float time = 0.5f;
	int amountOfPoints;

	// a line to which we add the dots
	string lineWithoutPoints;

	private Text connectingText;

	void Start ()
	{
		connectingText = GameObject.Find ("ConnectingText").GetComponent<Text> ();
		lineWithoutPoints = connectingText.text;
	}

	void Update ()
	{
		connectingText.text += ".";
		time += 0.01f;

		if (time >= 1f) {
			connectingText.text += ".";

			if (amountOfPoints == 3) {
				connectingText.text = lineWithoutPoints;
				amountOfPoints = -1;
			}

			time = 0;
			amountOfPoints++;
		}
	}
}
