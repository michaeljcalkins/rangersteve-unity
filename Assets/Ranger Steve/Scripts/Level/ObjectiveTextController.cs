using UnityEngine;
using UnityEngine.UI;

namespace Com.LavaEagle.RangerSteve
{
    public class ObjectiveTextController : Photon.MonoBehaviour
    {
        Text objectiveText;

        void Start()
        {
            objectiveText = GetComponent<Text>();
            Invoke("HandleClearMessage", 3f);
        }

        [PunRPC]
        public void HandleSetMessage(string textValue, float secondsUntilClear = 3f)
        {
            objectiveText.text = textValue;
            Invoke("HandleClearMessage", secondsUntilClear);
        }

        public void EmitSetMessage(string textValue = "Grab bomb from middle and push to enemy base", float secondsUntilClear = 3f)
        {
            GetComponent<PhotonView>().RPC("HandleSetMessage", PhotonTargets.All, textValue, secondsUntilClear);
        }

        void HandleClearMessage()
        {
            objectiveText.text = "";
        }
    }
}