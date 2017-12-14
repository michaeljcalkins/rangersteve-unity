using UnityEngine;
using UnityEngine.SceneManagement;

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
            SceneManager.LoadScene("Launcher");
        }

        public void HandleDiscordClick()
        {
            Application.OpenURL("https://discord.gg/GqKgsmy");
        }

        public void HandleNewsletterClick()
        {
            Application.OpenURL("http://eepurl.com/deinnz");
        }

        public void HandleExitClick()
        {
            Application.Quit();
        }
    }
}