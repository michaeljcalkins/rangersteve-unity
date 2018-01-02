using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    public class TowerExplosion : Photon.MonoBehaviour
    {
        void Start()
        {
            Invoke("DestroyTowerExplosionAnimation", 4f);
        }

        void DestroyTowerExplosionAnimation()
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}