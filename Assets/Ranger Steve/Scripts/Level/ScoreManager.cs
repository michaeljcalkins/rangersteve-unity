using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    public class ScoreManager : MonoBehaviour
    {
        public int redScore = 0;
        public int blueScore = 0;
        public Time endOfRoundTimestamp;
        public GameObject redScoreText;
        public GameObject blueScoreText;
        public GameObject timeRemainingText;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void HandleAddRedScore(int amount)
        {
            redScore += amount;
        }

        public void HandleAddBlueScore(int amount)
        {
            blueScore += amount;
        }

        public void HandleResetScore()
        {
            redScore = 0;
            blueScore = 0;
        }
    }
}