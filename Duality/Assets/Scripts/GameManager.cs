using UnityEngine;
using System.Collections.Generic;

// GameStuff.cs UnityGUI - pause menu atleast

public class GameManager : MonoBehaviour {
	public bool on = true;
	
	[System.Serializable]
	public class ColorChange {
		[System.NonSerialized]
		public Color yin = new Color(255, 255, 255, 255);
		[System.NonSerialized]
		public Color yang = new Color(0, 0, 0, 255);
		public float interval;
	}
	public ColorChange color;
	
	public float moveTime;
	
	[System.Serializable]
	public class Shoot {
		public float velocity;
		public float lifeTime;
		public float blinkInterval;
		public float offset;
		public float interval;
		public int dmg;
	}
	public Shoot shoot;
	
	[System.Serializable]
	public class Cam {
		public float zoomSpd;
		public float minOrthoSize;
		public float maxOrthoSize;
	}
	public Cam cam;
	
	[System.Serializable]
	public class Health {
		public int playerMax;
		public int enemyMax;
		public int coreMax;
	}
	public Health health;
	
	[System.Serializable]
	public class Energy {
		public int costShoot;
		public int costColorChange;
		public int costMove;
		public int amountMax;
		public float recoveryRate;
		public float recoveryDelay;
	}
	public Energy energy;
	
	[System.Serializable]
	public class Heat {
		public float freezeDmgPerSec;
		public float shutdownInSecs;
		public float coolRate;
		public float coolDelay;
		public int move;
		public int colorChange;
		public int shoot;
		public int max;
		public int min;
		public int healthLossThreshold;
	}
	public Heat heat;
	
	[System.Serializable]
	public class Explosion {
		public Transform segmentPrefab;
		GameObject[] bullets;
		public int minForce;
		public int maxForce;
		public float duration;
		public int dmg;
		
		[System.NonSerialized]
		public LevelManager lm;
		
		public void Explode (GameObject g1, GameObject g2) {
			if (bullets != null) {
				for (int i = 0; i < 2; i++) {
					if (bullets[i] == g1 || bullets[i] == g2) {
						return;
					}
				}
			}
			
			bullets = new GameObject[] {g1, g2};
			
			Vector2 pos =
				new Vector2(
					Mathf.Round((g1.transform.position.x + g2.transform.position.x) / 2),
					Mathf.Round((g1.transform.position.y + g2.transform.position.y) / 2));
					
			GameObject explosionHolder = new GameObject("explosion");
			explosionHolder.transform.position = pos;
			Destroy(explosionHolder, duration);
			
			List<Vector2> explosionSegmentPositions = new List<Vector2>();
			
			List<Vector2> adjacentPositions = new List<Vector2>();
			
			// all 4 adjacent nodes not considering walls( and maybe else) only level bounds
			Vector2
			a = new Vector2(pos.x + 1, pos.y); // right adjacent
			if (lm.InLevelBounds(a)) // is the position reachable(inside level bounds)
				adjacentPositions.Add(a);
				
			a = new Vector2(pos.x - 1, pos.y); // left adjacent
			if (lm.InLevelBounds(a))
				adjacentPositions.Add(a);
				
			a = new Vector2(pos.x, pos.y + 1); // lower adjacent
			if (lm.InLevelBounds(a))
				adjacentPositions.Add(a);
				
			a = new Vector2(pos.x, pos.y - 1); // upper adjacent
			if (lm.InLevelBounds(a))
				adjacentPositions.Add(a);
				
			int force = Random.Range(minForce, maxForce + 1);
			
			if (force != 0) {
				explosionSegmentPositions.Add(pos);
			}
				
			foreach (Vector2 v in adjacentPositions) {
				for (int i = 1; i <= force; i++) {
					Vector2 nextPos = pos + (v - pos) * i;
					
					if (lm.blockList.Exists(l => HlpVect.EqualInt(l.position, nextPos))) {
						break;
					}
					else
						explosionSegmentPositions.Add(nextPos);
					
					foreach (ShipEnemy enemy in lm.enemies) {
						if (HlpVect.EqualInt(enemy.trgtPos, nextPos)) {
							// damage
							enemy.InflictDamage(dmg);
						}
					}
					
					if (HlpVect.EqualInt(lm.playerShip.trgtPos, nextPos)) {
						// damage
						lm.playerShip.InflictDamage(dmg);
					}
				}
			}
			
			foreach (Vector2 v in explosionSegmentPositions) {
				Transform t = Instantiate(segmentPrefab,
					v,
					segmentPrefab.rotation) as Transform;
				t.parent = explosionHolder.transform;
			}
		}
	}
	public Explosion explosion;
	
	void Awake () {
		explosion.lm = GameObject.FindWithTag("LevelManager").GetComponent<LevelManager>();
	}
}
