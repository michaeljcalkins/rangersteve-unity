using UnityEngine;
using UnityEngine.UI;
/* Description

Animation download. Adding points to the word loading

Description */
public class LoadingAnimation : MonoBehaviour
{
    //// Dot animation during the boot
    float time_ =0.5f;
    int amount_of_points;
    string line_without_points; // a line to which we add the dots

    void Start()
    {
        line_without_points = GetComponent<Text>().text;
    }

	void Update ()
    {
        time_ += 0.01f;

        if (time_ >= 1f)
        {
            GetComponent<Text>().text += ".";
            if (amount_of_points == 3)
            {
                GetComponent<Text>().text = line_without_points;
                amount_of_points = -1;
            }
            time_ = 0;
            amount_of_points++;
        }
    }
}
