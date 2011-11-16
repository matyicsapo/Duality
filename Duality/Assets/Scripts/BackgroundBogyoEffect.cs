using UnityEngine;
using System.Collections.Generic;

public class BackgroundBogyoEffect : MonoBehaviour {
	public Transform bogyo;
	
	public int minStartBogyoCnt;
	public int maxBogyoCnt; // this one's only for start too (atleast for now)
	
	// seemed a good idea while i knew what it actually was
	//public float minInstantiateDelay;
	//public float maxInstantiateDelay;
	
	public float minLifeTime;
	public float maxLifeTime;
	
	public float minSpd;
	public float maxSpd;
	
	public float minScale;
	public float maxScale;
	
	public int minColorValue;
	public int maxColorValue;
	
	public int minColorAlpha;
	public int maxColorAlpha;
	
	public float avgFadeTimePercentage;
	
	LevelManager lm;
	GameManager gm;
	
	Transform mTransform;
	
	int childCnt;
	
	void Start () {
		mTransform = transform;
		
		lm = mTransform.parent.Find("LevelManager").GetComponent<LevelManager>();
		
		childCnt = 0;
		
		int startBogyoCnt = Random.Range(minStartBogyoCnt, maxBogyoCnt);
		for (int i = 0; i < startBogyoCnt; i++) {
			CreateBogyo();
		}
	}
	
	void Update () {
		int loss = childCnt - mTransform.GetChildCount();
		childCnt -= loss;
		
		for (int i = 0; i < loss; i++) {
			CreateBogyo();
		}
	}
	
	/// <summary>
	/// Creates a "Bogyo" at a random position within the level bounds
	/// </summary>
	void CreateBogyo () {
		Vector3 pos = new Vector3(Random.Range(.0f, (float)lm.size.x), Random.Range(.0f, (float)lm.size.y));
				
		Transform newBogyoTransform = Instantiate(bogyo, pos, bogyo.rotation) as Transform;
		
		newBogyoTransform.parent = mTransform;
		
		float scale = Random.Range(minScale, maxScale);
		newBogyoTransform.localScale = new Vector3(scale, 1, scale);
		
		Bogyo newBogyo = newBogyoTransform.GetComponent<Bogyo>();
		
		int colorValue = Random.Range(minColorValue, maxColorValue);
		newBogyo.fullColor = new Color(colorValue, colorValue, colorValue,
			Random.Range(minColorAlpha, maxColorAlpha)) / 255;
		
		newBogyo.lifeTime = Random.Range(minLifeTime, maxLifeTime);
		newBogyo.spd = Random.Range(minSpd, maxSpd);
		
		newBogyo.fadeTime = newBogyo.lifeTime * avgFadeTimePercentage;
		
		//Vector2 center = new Vector2(lm.size.x / 2, lm.size.y / 2);
		//float dst = Vector3.Distance(center, pos);
		Vector3 dir = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
		newBogyo.dir = dir;
		
		childCnt++;
	}
}
