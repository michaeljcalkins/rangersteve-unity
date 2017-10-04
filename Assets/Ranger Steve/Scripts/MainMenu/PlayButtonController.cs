using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButtonController : MonoBehaviour
{
	public void HandlePlayNowClick ()
	{
		SceneManager.LoadScene ("Loading");
	}
}
