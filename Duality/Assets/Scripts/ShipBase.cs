using UnityEngine;
using System.Collections;

public abstract class ShipBase : MonoBehaviour {
	/* === color changing */
	public bool isYin;
	protected float lastTimeColorChange;
	/* color changing === */
	
	[System.NonSerialized]
	public Material mMaterial;
	
	/* === movement */
	[System.NonSerialized]
	public Vector2 trgtPos;
	[System.NonSerialized]
	public Vector2 lastPos;
	/* movement === */
	
	/* === shoot */
	public Transform shotPrefab;
	[System.NonSerialized]
	public float lastTimeShoot;
	/* shoot === */
	
	/* energy === */
	[System.NonSerialized]
	public int energy;
	float energyRecovered;
	float lastTimeEnergyCost;
	/* === energy */
	
	/* heat === */
	[System.NonSerialized]
	public int heat;
	[System.NonSerialized]
	public bool shipRestarting;
	float heatLoss;
	float healthFreezeLoss;
	/* === heat */
	
	protected int health;
	
	public bool on;
	
	protected Transform mTransform;
	
	public class BaseInput {
		public bool colorChange;
		
		public Vector2 shootDir;
		
		public Vector2 moveAxes;
	}
	public BaseInput mInput;
	
	protected GameManager gm;
	protected LevelManager lm;
	
	protected virtual void Start () {
		gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
		lm = GameObject.FindWithTag("LevelManager").GetComponent<LevelManager>();
		
		mTransform = transform;
		
		iTween.Init(gameObject); // advised by pixelplacement
		// gun doesn't rotate using iTweens (doesn't really rotate at all so it doesn't need this (for now atleast***)
		lastPos = trgtPos = mTransform.position;
		
		mMaterial = renderer.material;
		mMaterial.color = isYin ? gm.color.yin : gm.color.yang;
		
		energy = gm.energy.amountMax;
		
		lastTimeColorChange = lastTimeShoot = -1;
		
		heat = gm.heat.max / 2;
		shipRestarting = false;
		
		on = true;
	}
	
	protected abstract void InputHandling ();
	
	void Update () {
		if (gm.on && on) {
			if (!shipRestarting) {
				InputHandling();
				
				if (mInput.colorChange) {
					ActionColorChange();
				}
				
				if ( ! HlpVect.EqualInt(mInput.moveAxes, Vector2.zero) ) {
					ActionMovement();
				}
				
				if ( ! HlpVect.EqualInt(mInput.shootDir, Vector2.zero) ) {
					ActionShoot();
				}
				
				EnergyRecovery();
			}
			
			HeatLoss();
		}
	}
	
	void ActionColorChange () {
		isYin = ! isYin;
		mMaterial.color = isYin ? gm.color.yin : gm.color.yang;
		
		lastTimeColorChange = Time.time;
		
		OnActionEnergy_n_Heat(gm.energy.costColorChange, gm.heat.colorChange);
	}
	
	protected virtual void ActionMovement () {
		lastPos = trgtPos;
		trgtPos += mInput.moveAxes;
		
		iTween.MoveBy(gameObject, iTween.Hash("amount", (Vector3)mInput.moveAxes, "time", gm.moveTime,
			"easetype", iTween.EaseType.linear, "space", Space.World));
		
		OnActionEnergy_n_Heat(gm.energy.costMove, gm.heat.move);
	}
	
	void ActionShoot () {
		Quaternion r = shotPrefab.rotation;
		//r.SetLookRotation(mTransform.forward, Vector3.forward);
		r.SetLookRotation(-mInput.shootDir, Vector3.forward);
		
		Vector3 dir = (Vector3)mInput.shootDir.normalized;
		
		Shot xShot = (Instantiate(shotPrefab,
			new Vector3(mTransform.position.x, mTransform.position.y, shotPrefab.position.z)
				+ dir * gm.shoot.offset,
			r) as Transform).GetComponent<Shot>();
		
		xShot.isYin = isYin;
		xShot.dir = dir;
		
		xShot.instantiator = gameObject;
		
		lastTimeShoot = Time.time;
		
		OnActionEnergy_n_Heat(gm.energy.costShoot, gm.heat.shoot);
	}
	
	void OnActionEnergy_n_Heat (int energyCost, int heatAmount) {
		energy -= energyCost;
		energyRecovered = 0;
		
		lastTimeEnergyCost = Time.time;
		
		Boil(heatAmount);
	}
	
	void EnergyRecovery () {
		if (energy < gm.energy.amountMax) {
			if (Time.time - lastTimeEnergyCost > gm.energy.recoveryDelay) {
				energyRecovered += Time.deltaTime * gm.energy.recoveryRate *
					(gm.energy.amountMax - energy) / gm.energy.amountMax;
				
				if (energyRecovered >= 1) {
					int intPart = (int)Mathf.Floor(energyRecovered);
					energy += intPart;
					energyRecovered -= intPart;
				}
			}
		}
	}
	
	void Boil (int heat) {
		this.heat += heat;
		
		if (this.heat >= gm.heat.max) {
			this.heat = gm.heat.max;
			StartCoroutine(ShipRestart());
		}
	}
	
	IEnumerator ShipRestart () {
		shipRestarting = true;
		
		yield return new WaitForSeconds(gm.heat.shutdownInSecs);
		
		shipRestarting = false;
	}
	
	void HeatLoss () {
		if (heat > gm.heat.min) {
			if (Time.time - lastTimeEnergyCost > gm.heat.coolDelay) {
				heatLoss += Time.deltaTime * gm.heat.coolRate *
					(gm.heat.max - gm.heat.min) / ((gm.heat.max - gm.heat.min) - heat);
				
				if (heatLoss >= 1) {
					int intPart = (int)Mathf.Floor(heatLoss);
					heat -= intPart;
					heatLoss -= intPart;
				}
			}
		}
		
		if (heat <= gm.heat.healthLossThreshold) {
			healthFreezeLoss += Time.deltaTime * gm.heat.freezeDmgPerSec;
			
			if (healthFreezeLoss >= 1) {
				int intPart = (int)Mathf.Floor(healthFreezeLoss);
				healthFreezeLoss -= intPart;
				
				InflictDamage(intPart);
			}
		}
	}
				    
	public virtual void InflictDamage (int damage) {
		health -= damage;
		
		if (health <= 0) {
			health = 0;
			on = false;
		}
	}
	
	/*
	void OnTriggerStay (Collider c) {
		if (c.gameObject.layer == 8) {
			print("stay");
			
			health -= gm.explosion.dmg;
		}
	}
	*/
	
	void OnTriggerEnter (Collider c) {
		//print(LayerMask.NameToLayer("explosion"));
		if (c.gameObject.layer == 8) {
			// been caught in explosion
			//health -= gm.explosion.dmg;
			
			//print(Random.value);
			
			//CheckDead();
		}
		else {
			// if collided with something it is sure to be a shot due to how the collision action matrix is set up
				// (hopefully)
			Shot shot = c.GetComponent<Shot>(); // should always have one as there's nothing else to collide with
			
			if (shot.instantiator != gameObject) {
				if (shot.isYin != isYin) {
					// been shot
					
					InflictDamage(gm.shoot.dmg);
				}
			}
		}
	}
	
	// *********************************************************************
	public float statusOffsetX;
	public Vector2 statusDimensions;
	
	void OnGUI () {
		GUILayout.BeginArea(new Rect(Screen.width / 2 + statusOffsetX, 0, statusDimensions.x, statusDimensions.y));
			GUILayout.Label("Health: " + health.ToString());
			GUILayout.Label("Energy: " + energy.ToString());
			GUILayout.Label("Heat: " + heat.ToString());
		GUILayout.EndArea();
	}
}
