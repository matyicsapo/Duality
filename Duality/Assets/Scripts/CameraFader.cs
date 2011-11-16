using UnityEngine;

public class CameraFader : MonoBehaviour {
	public float fadeTime;
	
	void Start () {
		iTween.Init(gameObject);
		
		Transform cameraFader = iTween.CameraFadeAdd().transform;
		cameraFader.position = new Vector3(cameraFader.position.x, cameraFader.position.y, -10);
		
		iTween.CameraFadeFrom(1, fadeTime);
	}
	
	public void FadeOut (string oncomplete, GameObject oncompletetarget) {
		iTween.CameraFadeTo(iTween.Hash("amount", 1, "time", fadeTime,
			"oncomplete", oncomplete, "oncompletetarget", oncompletetarget));
	}
}
