using UnityEngine;
using System.Collections.Generic;

public class ShipEnemy : ShipBase {
	Core core;
	
	/* AI === */
	AIManager AI;
	
	[System.NonSerialized]
	public List<AIManager.Node> path;
	
	public AIManager.Node nextNode;
	
	public Vector2 ultimateGoal;
	/* === AI */
	
	public void RemoveFromAIManager () {
		AI.aiEnemies.Remove(this);
	}
	
	protected override void Start () {
		base.Start();
		
		health = gm.health.enemyMax;
		
		core = lm.enemyCore.GetComponent<Core>();
		
		AI = GameObject.FindWithTag("AIManager").GetComponent<AIManager>();
		
		AI.aiEnemies.Add(this);
		
		path = null;
		nextNode = null;
		ultimateGoal = -Vector2.one;
	}
	
	protected override void InputHandling () {
		if (mInput == null) {
			mInput = new BaseInput();
		}
	}
	
	public override void InflictDamage (int damage) {
		health -= damage;
		
		if (health <= 0) {
			health = 0;
			on = false;
			RemoveFromAIManager();
			
			if (AI.aiEnemies.Count == 0) {
				core.on = false;
			}
		}
	}
}
