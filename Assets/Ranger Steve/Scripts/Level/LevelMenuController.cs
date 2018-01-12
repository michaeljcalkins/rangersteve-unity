using UnityEngine;
using UnityEngine.Analytics;

namespace Com.LavaEagle.RangerSteve
{
    public class LevelMenuController : MonoBehaviour
    {
        public void HandlePreorderClick()
        {
            Analytics.CustomEvent("Clicked Pre-order In Game");
            Application.OpenURL("https://www.humblebundle.com/g/rangersteve");
        }
    }
}