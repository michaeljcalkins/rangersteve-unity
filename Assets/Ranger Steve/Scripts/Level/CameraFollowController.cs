using UnityEngine;

public class CameraFollowController : MonoBehaviour
{
	public int depth;
	private Transform playerTransform;

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