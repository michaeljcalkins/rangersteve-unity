using UnityEngine;
/* Network Description

to see on the playing field where the enemies are and where my player

Network Description */
public class Hide_poiner : Photon.MonoBehaviour
{
	// Use this for initialization
	void Start ()
    {
        GetComponent<SpriteRenderer>().enabled = true;
        Invoke("hide_pointer", 2f);
	}
	
	// Update is called once per frame
	void hide_pointer()
    {
        gameObject.SetActive(false);
	}
}
