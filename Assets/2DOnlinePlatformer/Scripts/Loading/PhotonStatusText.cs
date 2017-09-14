using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class PhotonStatusText : MonoBehaviour
{
    private Text connectingText;

    // Use this for initialization
    void Start ()
    {
        connectingText = GameObject.Find ("ConnectingText").GetComponent<Text> ();
    }
	
    // Update is called once per frame
    void Update ()
    {
        connectingText.text = SplitCamelCase (PhotonNetwork.connectionStateDetailed.ToString ());
    }

    public static string SplitCamelCase (string input)
    {
        return System.Text.RegularExpressions.Regex.Replace (input, "(?<=[a-z])([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim ();
    }
}
