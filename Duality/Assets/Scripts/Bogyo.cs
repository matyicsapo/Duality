using UnityEngine;
using System.Collections;

public class Bogyo : MonoBehaviour {
	[System.NonSerialized]
	public Vector3 dir;
	
	[System.NonSerialized]
	public float lifeTime;
	
	[System.NonSerialized]
	public float spd;
	
	[System.NonSerialized]
	public Color fullColor;
	
	[System.NonSerialized]
	public float fadeTime;
	
	Transform mTransform;
	Material mMaterial;
	
	void Start () {
		mTransform = transform;
		mMaterial = renderer.material;
		
		mMaterial.color = new Color(fullColor.r, fullColor.g, fullColor.b, 0);
		iTween.Init(gameObject);
		iTween.FadeTo(gameObject, fullColor.a, fadeTime);
		StartCoroutine(InvokeFadeOut());
		
		Destroy(gameObject, lifeTime);
	}
	
	void Update () {
		mTransform.position += Time.deltaTime * spd * dir;
	}
	
	IEnumerator InvokeFadeOut () {
		yield return new WaitForSeconds(lifeTime - fadeTime);
		
		iTween.FadeTo(gameObject, 0, fadeTime);
	}
}
