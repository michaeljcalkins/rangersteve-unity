using UnityEngine;
using UnityEngine.UI;

namespace Com.LavaEagle.RangerSteve
{
    public class AlertMessageController : MonoBehaviour
    {
        Text alertMessageText;

        string value;

        void Start()
        {
            alertMessageText = GetComponent<Text>();
        }

        [PunRPC]
        public void HandleSetMessage(string textValue, string colorValue, float secondsUntilClear = 3f)
        {
            alertMessageText.text = textValue;

            switch (colorValue)
            {
                case "red":
                    alertMessageText.color = new Color(0.89f, 0.16f, 0.16f);
                    break;

                case "blue":
                    alertMessageText.color = new Color(0f, 0.43f, 0.72f);
                    break;

                case "yellow":
                    alertMessageText.color = new Color(0.98f, 1f, 0f);
                    break;
            }

            Invoke("HandleClearMessage", secondsUntilClear);
        }

        public void EmitSetMessage(string textValue, string colorValue, float secondsUntilClear = 3f)
        {
            GetComponent<PhotonView>().RPC("HandleSetMessage", PhotonTargets.All, textValue, colorValue, secondsUntilClear);
        }

        void HandleClearMessage()
        {
            alertMessageText.text = "";
        }
    }
}