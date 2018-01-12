using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpInfoController : MonoBehaviour
{

    public bool isHelpInfoVisible = false;

    public GameObject helpInfoPanel;

    // Use this for initialization
    void Start()
    {
        helpInfoPanel = GameObject.Find("HelpInfo");
    }

    // Update is called once per frame
    void Update()
    {
        helpInfoPanel.gameObject.SetActive(isHelpInfoVisible);
    }

    public void HandleShowHelpInfo()
    {
        isHelpInfoVisible = true;
    }

    public void HandleHideHelpInfo()
    {
        isHelpInfoVisible = false;
    }
}
