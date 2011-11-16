using UnityEngine;
using System.Collections;

public class Shot : MonoBehaviour {
	[System.NonSerialized]
	public bool isYin;
	
	[System.NonSerialized]
	public Vector3 dir;
	
	[System.NonSerialized]
	public GameObject instantiator;
	
	Material outlineMaterial;
	
	GameManager gm;
	LevelManager lm;

	void Start () {
		gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
		lm = GameObject.FindWithTag("LevelManager").GetComponent<LevelManager>();
	
		lm.shotList.Add(this);
		
		transform.parent = lm.shotHolder;
	
		renderer.material.color = isYin ? gm.color.yin : gm.color.yang;
		
		outlineMaterial = GetComponentsInChildren<Renderer>()[1].material; // material of children
		
		rigidbody.velocity = dir * gm.shoot.velocity;
		
		StartCoroutine(Blink());
		Destroy(gameObject, gm.shoot.lifeTime);
	}
	
	IEnumerator Blink () {
		while (true) {
			yield return new WaitForSeconds(gm.shoot.blinkInterval);
			
			outlineMaterial.mainTextureOffset += new Vector2(.5f, 0);
		}
	}
	
	void OnDestroy () {
		// the problem would be if we a reference remained somewhere but being null as this shot had been destroyed
			// and something would try to access!! it
		// thus the checking and the removing and this whole OnDestroy is perfectly fine here
			// (where garbage collection is done for us - though one better reads about garbage collection a bit)
		
		// causes NullReferenceExceptions to be thrown when there are bullets alive when quitting
			// LevelManager can't be found that it has already been destroyed and with it a List of references to
			// shots like this one
		// there's nothing to worry about, no one's referencing us so we can destroy ourselves
		
		if (lm != null) // needed? ifnot, why? *********************************************************************
			lm.shotList.Remove(this);
	}
	
	void OnTriggerEnter (Collider c) {
		bool shotButFromThisShip = c.gameObject.GetComponent<Shot>() != null
			&& c.gameObject.GetComponent<Shot>().instantiator == instantiator;
			
		bool motherShip = c.gameObject == instantiator;
		
		if (!shotButFromThisShip && !motherShip) {
			Destroy(gameObject);
			
			if (c.gameObject.GetComponent<Shot>() != null) {
				gm.explosion.Explode(gameObject, c.gameObject);
			}
		}
	}
}
