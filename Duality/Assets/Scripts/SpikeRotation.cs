using UnityEngine;

public class SpikeRotation : MonoBehaviour {
	Transform mTransform;
	//Transform childTransform;
	
	public float anglesPerSec;
	
	GameManager gm;
	Core core;
	
	void Start () {
		mTransform = transform;
		
		gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
		core = mTransform.parent.GetComponent<Core>();
	}
	
	void Update () {
		if (gm.on && core.on) {
			mTransform.Rotate(0, anglesPerSec * Time.deltaTime, 0);
		}
	}
}
