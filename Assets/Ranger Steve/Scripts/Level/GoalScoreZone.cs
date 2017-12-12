using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    public class GoalScoreZone : Photon.MonoBehaviour
    {
        #region Public Variables

        public string team;

        #endregion


        #region MonoBehaviour CallBacks

        private void Start()
        {
            //Physics2D.IgnoreLayerCollision(14, 9);

            // Bullets
            Physics2D.IgnoreLayerCollision(14, 15);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Local Player" && other.GetComponent<PhotonView>().isMine && other.gameObject.GetComponent<PlayerManager>().hasBomb)
            {
                // emit bomb explosion on the tower
                PhotonNetwork.Instantiate("TowerExplosion", other.gameObject.transform.position, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)), 0);

                if (team == "red")
                {
                    GameObject.Find("ScoreManager").GetComponent<ScoreManager>().EmitAddRedScore();
                    // disable user controls
                    // pause timer
                }
                else
                {
                    GameObject.Find("ScoreManager").GetComponent<ScoreManager>().EmitAddBlueScore();
                    // disable user controls
                    // pause timer
                }
            }
        }

        #endregion

    }
}