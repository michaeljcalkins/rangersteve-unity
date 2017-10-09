using UnityEngine;
using UnityEngine.SceneManagement;

namespace Com.LavaEagle.RangerSteve
{
	public class PlayButtonController : MonoBehaviour
	{
		public void HandlePlayNowClick ()
		{
			SceneManager.LoadScene ("Launcher");
		}
	}
}