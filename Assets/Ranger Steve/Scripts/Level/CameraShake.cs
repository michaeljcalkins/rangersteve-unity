using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    public class CameraShake : MonoBehaviour
    {

        // Transform of the camera to shake. Grabs the gameObject's transform
        // if null.
        public Transform camTransform;

        // How long the object should shake for.
        public float shakeDuration = 0f;

        // Amplitude of the shake. A larger value shakes the camera harder.
        public float shakeAmount = 0.7f;
        public float decreaseFactor = 1.0f;

        public Vector3 originalPos;
        public GameObject localPlayer;

        void Awake()
        {
            if (camTransform == null)
            {
                camTransform = GetComponent(typeof(Transform)) as Transform;
            }
        }

        void OnEnable()
        {
            originalPos = camTransform.localPosition;
        }

        void Update()
        {
            if (shakeDuration > 0)
            {
                Vector3 currentPosition = new Vector3(localPlayer.gameObject.transform.localPosition.x, localPlayer.gameObject.transform.localPosition.y, -20);
                camTransform.localPosition = currentPosition + Random.insideUnitSphere * shakeAmount;

                shakeDuration -= Time.deltaTime * decreaseFactor;
            }
            else
            {
                shakeDuration = 0f;
                camTransform.localPosition = originalPos;
            }
        }
    }
}