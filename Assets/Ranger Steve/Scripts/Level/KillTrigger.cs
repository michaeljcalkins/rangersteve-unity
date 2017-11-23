﻿using UnityEngine;
using UnityEngine.UI;

/* Network Description

If this player dies or if there is one alive player in the room,
or if everyone dies (the last two players killed each other simultaneously),
then all players again create prefab of the player player (Remover script). Nobody leaves the room

!!!! We sometimes go into the collider twice(OnCollisionEnter2D,OnTriggerEnter2D) - bag Physics Unity !!!! This is a very important point in PUN
Because of this we can send network data several times instead of once.
Extra traffic costs. And in the PUN delay I attack quickly.
Each extra byte of the transmitted traffic is already a problem.

Network Description */
namespace Com.LavaEagle.RangerSteve
{
    public class KillTrigger : Photon.MonoBehaviour
    {
        // animation River splash
        public GameObject splash;

        private Text remainingAmmoText;

        private Image activeWeaponImage;

        private Image hurtBorderImage;

        private Slider healthSlider;

        void Start()
        {
            healthSlider = GameObject.Find("HealthSlider").GetComponent<Slider>();
            hurtBorderImage = GameObject.Find("HurtBorderImage").GetComponent<Image>();
            remainingAmmoText = GameObject.Find("RemainingAmmoText").GetComponent<Text>();
            activeWeaponImage = GameObject.Find("ActiveWeaponImage").GetComponent<Image>();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.GetComponent<PhotonView>().isMine)
                return;

            // We sometimes go into the collider twice(OnCollisionEnter2D,OnTriggerEnter2D) - bag Physics Unity. It is necessary once
            // ... instantiate the splash where the player falls in.
            PhotonNetwork.Instantiate(splash.name, other.transform.position, transform.rotation, 0);

            // ... destroy the player or bomb;
            // In the PUN you can not get here twice. Red bug pan - “Ev Destroy Failed”
            Destroy(other.gameObject);

            if (other.tag == "Local Player")
            {
                remainingAmmoText.text = "";
                hurtBorderImage.GetComponent<CanvasRenderer>().SetAlpha(1f);
                healthSlider.value = 0;

                activeWeaponImage.enabled = false;
                activeWeaponImage.overrideSprite = null;

                Invoke("Respawn", 4f);
            }
        }

        void Respawn()
        {
            CreatePlayer createPlayer = FindObjectOfType<CreatePlayer>();
            if (createPlayer.player != null)
                PhotonNetwork.Destroy(createPlayer.player);

            createPlayer.HandleCreatePlayerObject();
        }
    }
}