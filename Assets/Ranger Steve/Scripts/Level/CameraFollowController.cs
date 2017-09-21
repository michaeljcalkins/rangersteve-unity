using UnityEngine;

public class CameraFollowController : MonoBehaviour
{
	public Transform playerTransform;
	public int depth;

	void Update ()
	{
		if (playerTransform != null) {
			transform.position = playerTransform.position + new Vector3 (0, 0, depth);
		}
	}

	public void SetTarget (Transform target)
	{
		playerTransform = target;
	}
}