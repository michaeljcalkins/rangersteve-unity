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

        public void HandlePreorderClick()
        {
            Analytics.CustomEvent("Clicked Pre-order");
            Application.OpenURL("https://www.humblebundle.com/g/rangersteve");
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

        public void HandleFacebookClick()
        {
            Analytics.CustomEvent("Clicked Facebook");
            Application.OpenURL("https://www.facebook.com/rangersteveio");
        }

        public void HandleExitClick()
        {
            Analytics.CustomEvent("Clicked Exit");
            Application.Quit();
        }
    }
}