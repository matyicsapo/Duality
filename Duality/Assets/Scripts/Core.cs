using UnityEngine;

public class Core : MonoBehaviour {
	public bool on;
	
	public bool isYin;
	
	int health;
	
	[System.NonSerialized]
	public Material mMaterial;
	
	GameManager gm;
	LevelManager lm;
	
	void Start () {
		gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
		lm = GameObject.FindWithTag("LevelManager").GetComponent<LevelManager>();
		
		on = true;
		
		mMaterial = renderer.material;
		mMaterial.color = isYin ? gm.color.yin : gm.color.yang;
		
		health = gm.health.coreMax;
	}
	
	void CheckDead () {
		if (health <= 0) {
			health = 0;
			on = false;
			
			foreach (ShipEnemy fellowEnemy in lm.enemies) {
				fellowEnemy.on = false;
				fellowEnemy.RemoveFromAIManager();
			}
		}
	}
	
	void OnTriggerEnter (Collider c) {
		if (c.gameObject.layer == 8) {
			// been caught in explosion
			health -= gm.explosion.dmg;
			
			CheckDead();
		}
		else {
			// if collided with something it is sure to be a shot due to how the collision action matrix is set up
				// (hopefully)
			Shot shot = c.GetComponent<Shot>(); // should always have one as there's nothing else to collide with
			
			if (shot.instantiator != gameObject) {
				if (shot.isYin != isYin) {
					// been shot
					
					health -= gm.shoot.dmg;
					
					CheckDead();
				}
			}
		}
	}
	
	//***************************************************************
	public Vector2 statusOffset;
	public Vector2 statusDimensions;
	
	void OnGUI () {
		GUILayout.BeginArea(new Rect(Screen.width / 2 + statusOffset.x, Screen.height - statusOffset.y,
				statusDimensions.x, statusDimensions.y));
			GUILayout.Label("Core: " + health.ToString());
		GUILayout.EndArea();
	}
}
