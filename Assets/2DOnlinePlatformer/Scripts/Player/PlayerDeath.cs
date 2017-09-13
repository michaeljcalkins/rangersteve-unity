using UnityEngine;
/* Network description

disable the firing of weapons and control in the state of death (photonView.isMine == true) - owner prefab "hero"
all the players change it sortingLayerName so that he was on top of all in the background when the river falls

Network description */
public class PlayerDeath : Photon.MonoBehaviour {
    public AudioClip[] ouchClips; // Array of clips to play when the player is damaged.
    private Animator anim; // Reference to the Animator on the player

    void Awake () {
        anim = GetComponent<Animator> ();
    }

    [PunRPC]
    void Death (int number_clip) {
        if (photonView.isMine && GetComponent<PlayerControl> ().enabled) // The second condition // Correction of double death animation of the player
        {
            // ... disable user Player Control script
            GetComponent<PlayerControl> ().enabled = false;

            // ... disable the Weapon
            transform.GetChild (0).gameObject.SetActive (false);

            // ... Trigger the 'Die' animation state
            anim.SetTrigger ("Die");
        }

        // Find all of the colliders on the gameobject and set them all to be triggers.
        Collider2D[] cols = GetComponents<Collider2D> ();

        if (!cols[0].isTrigger)
            AudioSource.PlayClipAtPoint (ouchClips[number_clip], transform.position); // Correction of double death sound of the player

        foreach (Collider2D c in cols) {
            c.isTrigger = true;
        }

        // Move all sprite parts of the player to the front
        SpriteRenderer[] spr = GetComponentsInChildren<SpriteRenderer> ();
        foreach (SpriteRenderer s in spr) {
            s.sortingLayerName = "UI";
        }
    }
}