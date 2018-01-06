using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Analytics;

namespace Com.LavaEagle.RangerSteve
{
    public class MenuController : MonoBehaviour
    {
        public void Start()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        public void HandlePlayNowClick()
        {
            Analytics.CustomEvent("Clicked Play");
            SceneManager.LoadScene("Launcher");
        }

        public void HandleDiscordClick()
        {
            Analytics.CustomEvent("Clicked Discord");
            Application.OpenURL("https://discord.gg/GqKgsmy");
        }

        public void HandleNewsletterClick()
        {
            Analytics.CustomEvent("Clicked Newsletter");
            Application.OpenURL("http://eepurl.com/deinnz");
        }

        public void HandleDownloadThisGameClick()
        {
            Analytics.CustomEvent("Clicked Download This Game");
            Application.OpenURL("https://github.com/michaeljcalkins/rangersteve-early-access/releases");
        }

        public void HandleExitClick()
        {
            Analytics.CustomEvent("Clicked Exit");
            Application.Quit();
        }
    }
}