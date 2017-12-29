using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    public class EnemyHasBombArrowController : MonoBehaviour
    {
        private Transform target;

        private Vector3 v_diff;

        private float atan2;

        void Start()
        {
            InvokeRepeating("Test", 1f, 1f);
        }

        void Update()
        {
            // Checks if the enemy exists so you dont get any errors
            if (!target)
            {
                transform.localScale = new Vector3(0, 0, 0);
                return;
            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
            }

            //Rotate towards the enemy
            v_diff = (target.position - transform.position);

            atan2 = Mathf.Atan2(v_diff.y, v_diff.x);

            transform.rotation = Quaternion.Euler(0f, 0f, atan2 * Mathf.Rad2Deg - 90);

            // Move Towards the target
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, 1000);

            // Clamp to the screen view
            Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);

            // Hide the indicator if the player is in view
            if (pos.x <= 1 && pos.x >= 0 && pos.y <= 1 && pos.y >= 0)
            {
                transform.localScale = new Vector3(0, 0, 0);
            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
            }

            pos.x = Mathf.Clamp01(pos.x);

            pos.y = Mathf.Clamp01(pos.y);

            transform.position = Camera.main.ViewportToWorldPoint(pos);
        }

        void Test()
        {
            GameObject.Find("EnemyHasBombArrow").GetComponent<EnemyHasBombArrowController>().HandleSetTarget();
        }

        public void HandleSetTarget()
        {
            bool playerHasBomb = false;
            GameObject[] networkedPlayers = GameObject.FindGameObjectsWithTag("Networked Player");
            foreach (GameObject player in networkedPlayers)
            {
                if (player.GetComponent<PlayerManager>().hasBomb)
                {
                    playerHasBomb = true;
                    target = player.transform;
                }
            }

            if (!playerHasBomb)
            {
                target = null;
            }
        }
    }
}