using UnityEngine;

public class FollowCamera : MonoBehaviour {
	Transform mTransform;
	
	public Transform target;
	public Vector3 offset;
	
	public float zoomSpd;
	public float minOrthoSize;
	public float maxOrthoSize;

	void Start () {
		mTransform = transform;
	}
	
	void Update () {
		camera.orthographicSize =
			Mathf.Clamp(camera.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomSpd,
				minOrthoSize, maxOrthoSize);
	}
	
	void LateUpdate () {
		if (target != null) {
			mTransform.position = target.position + offset;
		}
	}
}
