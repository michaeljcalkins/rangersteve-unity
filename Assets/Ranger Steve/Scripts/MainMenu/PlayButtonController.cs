using UnityEngine;
using UnityEngine.SceneManagement;
using mixpanel;

namespace Com.LavaEagle.RangerSteve
{
    public class PlayButtonController : MonoBehaviour
    {
        public void Start()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        public void HandlePlayNowClick()
        {
            Mixpanel.Track("Clicked Play");
            SceneManager.LoadScene("Launcher");
        }

        public void HandleDiscordClick()
        {
            Mixpanel.Track("Clicked Discord");
            Application.OpenURL("https://discord.gg/GqKgsmy");
        }

        public void HandleNewsletterClick()
        {
            Mixpanel.Track("Clicked Newsletter");
            Application.OpenURL("http://eepurl.com/deinnz");
        }

        public void HandleExitClick()
        {
            Mixpanel.Track("Clicked Exit");
            Application.Quit();
        }
    }
}