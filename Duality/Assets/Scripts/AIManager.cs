/*energy and heat not public between "teams"
 * 	=> no need for fancy graphics for enemy stats
 * 	health should be displayed directly on enemies
 * 		outer circle diminishing? little texture work but easy, alpha values, health, blah, blah and blah*/

using UnityEngine;
using System.Collections.Generic;
using System.Linq; // List.OrderBy() 'n .First() are ==> extension methods <==

public class AIManager : MonoBehaviour {
	public class Node {
		public Node parent = null;
		
		public Vector2 pos = Vector2.zero;
		
		public int H = 0;
		public int G = 0;
		public int F = 0;
	}
	
	LevelManager lm;
	GameManager gm;
	
	[System.NonSerialized]
	public List<ShipEnemy> aiEnemies;
	
	Vector2 lastPlayerPos;
	
	bool recalc;
	
	void Start () {
		lm = GameObject.FindWithTag("LevelManager").GetComponent<LevelManager>();
		gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
		
		aiEnemies = new List<ShipEnemy>();
		
		lastPlayerPos = -Vector2.one;
	}
	
	void AIRecalc () {
		DestroyVisualisedPaths();
		
		foreach (ShipEnemy enemy in aiEnemies) {
			if (!AlreadyInEyeSight(enemy.trgtPos, lm.playerShip.trgtPos)) {
				List<Vector2> tabooList = new List<Vector2>();
				foreach (ShipEnemy otherEnemy in aiEnemies) {
					if (otherEnemy != enemy && ! HlpVect.EqualInt(otherEnemy.ultimateGoal, -Vector2.one)) {
						tabooList.Add(otherEnemy.ultimateGoal);
					}
				}
				
				enemy.path = Path_HeuristicClosestInEyeSightWith(enemy.trgtPos, lm.playerShip.trgtPos, tabooList);
				
				if (enemy.path != null) {
					enemy.nextNode = enemy.path[0];
					
					enemy.ultimateGoal = enemy.path.Last().pos;
					
					VisualisePath(enemy.path);
				}
				else {
					enemy.nextNode = null;
					
					enemy.ultimateGoal = -Vector2.one;
				}
			}
		}
	}
	
	void AIUpdate () {
		foreach (ShipEnemy enemy in aiEnemies) {
			enemy.mInput = new ShipBase.BaseInput();
			
			if (!enemy.shipRestarting) {
				bool inEyeSight = AlreadyInEyeSight(enemy.trgtPos, lm.playerShip.trgtPos);
				
				if (enemy.nextNode != null
				    	&& iTween.Count(enemy.gameObject, "MoveBy") == 0
				    	&& enemy.energy >= gm.energy.costMove
				    	&& !inEyeSight) {
					
					// don't worry about overheating if
					//		shot is coming but can get away to the side
					//		would get to goal def pos as tank for core
					//if (enemy.heat + gm.heat.move < gm.heat.max) {}
					
					enemy.mInput.moveAxes = enemy.nextNode.pos - enemy.trgtPos;
					
					if (!lm.CanTraverse(enemy.trgtPos + enemy.mInput.moveAxes)) {
						enemy.mInput.moveAxes = Vector2.zero;
					}
					else {
						enemy.nextNode = enemy.nextNode.parent;
					}
				}
				
				if (inEyeSight) {
					int energyReq = gm.energy.costShoot;
					
					int heatAmount = gm.heat.shoot;
					
					if (enemy.isYin == lm.playerShip.isYin) {
						energyReq += gm.energy.costColorChange;
						heatAmount += gm.heat.colorChange;
					}
					
					if (enemy.energy >= energyReq) {
					    
					    // don't worry about overheating if
					    //		will hit(shot spd, dst) &&  it will kill player(playerhealth)
					    //if (enemy.heat + heatAmount <= gm.heat.max) {}
						
						if (enemy.isYin == lm.playerShip.isYin) {
							enemy.mInput.colorChange = true;
						}
						
						enemy.mInput.shootDir = lm.playerShip.trgtPos - enemy.trgtPos;
					}
				}
			}
		}
	}
	
	void Update () {
		if (gm.on) {
			recalc = false;
			
			if (lastPlayerPos != lm.playerShip.trgtPos)
				recalc = true;
			
			if (recalc)
				AIRecalc();
			
			AIUpdate();
			
			lastPlayerPos = lm.playerShip.trgtPos;
		}
	}
	
	#region path visualisation
	/// <summary>
	/// For path visualisation.
	/// </summary>
	struct visualPath {
		public List<Transform> nodeBlocks;
		public GameObject holder;
	}
	List<visualPath> visualPaths;
	public Transform pathBlock;
	
	void DestroyVisualisedPaths () {
		if (visualPaths != null) {
			foreach (visualPath vp in visualPaths) {
				if (vp.nodeBlocks != null) {
					foreach (Transform t in vp.nodeBlocks) {
						Destroy(t.gameObject);
					}
				}
				Destroy(vp.holder);
			}
		}
		
		visualPaths = new List<visualPath>();
	}
	
	/// <summary>
	/// 
	/// </summary>
	/// <param name="path">
	/// A <see cref="List<Node>"/>
	/// </param>
	void VisualisePath (List<Node> path) {
		visualPath vp = new visualPath();
		vp.nodeBlocks = new List<Transform>();
		vp.holder = new GameObject("pathHolder" + Random.value);
		
		Color color = new Color(Random.value, Random.value, Random.value);
		
		foreach (Node n in path) {
			Transform t = Instantiate(pathBlock,
				new Vector3(n.pos.x, n.pos.y, -1 * (visualPaths.Count+1)), Quaternion.identity) as Transform;
			t.renderer.material.color = color;
			t.parent = vp.holder.transform;
			vp.nodeBlocks.Add(t);
		}
		
		visualPaths.Add(vp);
	}
	#endregion path visualisation
	
	/// <summary>
	/// Finds a path from 'from' to 'to' in a range defined by level dimensions and blocked nodes being those
	/// 	reported by LevelManager.Occupied .
	/// </summary>
	/// <param name="from">
	/// A <see cref="Vector2"/>
	/// </param>
	/// <param name="to">
	/// A <see cref="Vector2"/>
	/// </param>
	/// <returns>
	/// A <see cref="List<Node>"/>
	/// The list of Node-s - the path - not including the 'from' position (only the rest and the 'to' pos
	/// </returns>
	List<Node> Path (Vector2 from, Vector2 to) {
		// currNode is the node with the lowest F
			// which, at start, is the goal position since the algorithm works backwards
		Node currNode = new Node();
		currNode.pos = to;
		
		// the list holding the nodes connecting the 2 ends(notincluding the from node)
		List<Node> closedList = new List<Node>();
		closedList.Add(currNode);
		
		// creating the open list - to hold potential nodes for closed list
		List<Node> openList = new List<Node>();
		
		// whether the algorithm has to search on or is it finished
		bool unresolved = true;
		
		do {
			List<Vector2> adjacentPositions = new List<Vector2>();
			
			// all 4 adjacent nodes not considering walls( and maybe else) only level bounds
			Vector2
			a = new Vector2(currNode.pos.x + 1, currNode.pos.y); // right adjacent
			if (lm.InLevelBounds(a)) // is the position reachable(inside level bounds)
				adjacentPositions.Add(a);
				
			a = new Vector2(currNode.pos.x - 1, currNode.pos.y); // left adjacent
			if (lm.InLevelBounds(a))
				adjacentPositions.Add(a);
				
			a = new Vector2(currNode.pos.x, currNode.pos.y + 1); // lower adjacent
			if (lm.InLevelBounds(a))
				adjacentPositions.Add(a);
				
			a = new Vector2(currNode.pos.x, currNode.pos.y - 1); // upper adjacent
			if (lm.InLevelBounds(a))
				adjacentPositions.Add(a);
			
			foreach (Vector2 aPos in adjacentPositions) {
				if (HlpVect.EqualInt(aPos, from)) {
					List<Node> path = new List<Node>();
					
					path.Add(currNode);
					do {
						if (currNode.parent != null) {
							currNode = currNode.parent;
							path.Add(currNode);
						}
						else
							unresolved = false;
					} while (unresolved);
					
					return path;
				}
			}
			
			// check for the next best node
			if (unresolved) {
				foreach (Vector2 v in adjacentPositions) {
					bool ok = !lm.Occupied(v) && !closedList.Exists(l => HlpVect.EqualInt(l.pos, v));
					
					if (ok) {
						// searches for node on this (adjacent) position in the openList
						Node an = openList.Find(l => HlpVect.EqualInt(l.pos, v));
						
						if (an != null) {
							// it's already on the openList
							
							int newG = currNode.G + 1;
							if (newG < an.G) {
								// this is a better path
								an.G = newG;
								an.F = newG + an.H;
								an.parent = currNode;
							}
						}
						else {
							Node newOpenNode = new Node();
							newOpenNode.parent = currNode;
							newOpenNode.pos = v;
							newOpenNode.G = currNode.G + 1;
							newOpenNode.H = (int)(Mathf.Abs(from.x - v.x)
								+ Mathf.Abs(from.y - v.y));
							newOpenNode.F = newOpenNode.G + newOpenNode.H;
							openList.Add(newOpenNode);
						}
					}
				}
				
				if (openList.Count == 0) {
					// there are no more nodes in the openList - no path
					unresolved = false;
				}
				else {
					// after refreshing the openList we sort it ascending by the nodes' F value
					openList = openList.OrderBy(l => l.F).ToList();
					
					// currNode is the one with the lowest F (the first in an ascending list)
					currNode = openList.First();
					
					// moving the node to the closedList from the openList
					closedList.Add(currNode);
					openList.Remove(currNode);
				}
			}
		} while (unresolved);
		
		return null;
	}
	
	/// <summary>
	/// Determines whether there is a clear line of sight from 'pos' to 'trgt'.
	/// </summary>
	/// <param name="pos">
	/// A <see cref="Vector2"/>
	/// </param>
	/// <param name="trgt">
	/// A <see cref="Vector2"/>
	/// </param>
	/// <returns>
	/// A <see cref="System.Boolean"/>
	/// </returns>
	bool AlreadyInEyeSight (Vector2 pos, Vector2 trgt) {
		Vector2 vToPos = pos - trgt;
		
		if ((int)vToPos.x == 0 || (int)vToPos.y == 0) {
			int forHighValue;
			Vector2 dir;
			
			if (vToPos.x == 0) {
				forHighValue = (int)Mathf.Abs(vToPos.y);
				dir = Mathf.Sign(-vToPos.y) * Vector2.up;
			}
			else {
				forHighValue = (int)Mathf.Abs(vToPos.x);
				dir = Mathf.Sign(-vToPos.x) * Vector2.right;
			}
			
			for (int i = 1; i < forHighValue; i++) {
				Vector2 m = pos + i * dir;
				
				if (lm.Occupied(m)) {
					return false;
				}
			}
			
			return true;
		}
		
		return false;
	}
	
	List<Node> Path_HeuristicClosestInEyeSightWith (Vector2 from, Vector2 target, List<Vector2> tabooList) {
		int joker = 999;
	
		Vector2 vToPos = target - from;
		
		Vector2[] dummies = new Vector2[4];
		
		if (Mathf.Abs(vToPos.x) < Mathf.Abs(vToPos.y)) {
			dummies[0] = new Vector2(target.x,						-Mathf.Sign(vToPos.y) * joker);
			dummies[1] = new Vector2(-Mathf.Sign(vToPos.x) * joker,	target.y);
			dummies[2] = new Vector2(Mathf.Sign(vToPos.x) * joker,	target.y);
			dummies[3] = new Vector2(target.x,						Mathf.Sign(vToPos.y) * joker);
		}
		else {
			dummies[0] = new Vector2(-Mathf.Sign(vToPos.x) * joker,	target.y);
			dummies[1] = new Vector2(target.x,						-Mathf.Sign(vToPos.y) * joker);
			dummies[2] = new Vector2(target.x,						Mathf.Sign(vToPos.y) * joker);
			dummies[3] = new Vector2(Mathf.Sign(vToPos.x) * joker,	target.y);
		}
		
		foreach (Vector2 dummy in dummies) {
			Vector2 anInEyeSightPos = -Vector2.one;
			
			Vector2 eyeSightBase;
			Vector2 axis; // the axis along which the search is done
			int dir; // the direction on the axis in which the search is done
			int forHighValue;
			
			if (Mathf.Abs(dummy.x) == joker) {
				axis = Vector2.right;
				dir = (int)Mathf.Sign(dummy.x);
				
				eyeSightBase = new Vector2(0, target.y);
				
				forHighValue = -(int)vToPos.x * dir + 1;
			}
			else { // if (Mathf.Abs(dummy.y) == joker) {
				axis = Vector2.up;
				dir = (int)Mathf.Sign(dummy.y);
				
				eyeSightBase = new Vector2(target.x, 0);
				
				forHighValue = -(int)vToPos.y * dir + 1;
			}
			
			if (forHighValue <= 0) {
				Vector2 m = eyeSightBase + Vector2.Scale(target, axis) + axis * dir;
				
				if (lm.CanTraverse(m)) {
					anInEyeSightPos = m;
				}
			}
			else {
				for (int i = 1; i < forHighValue; i++) {
					Vector2 m = eyeSightBase + Vector2.Scale(target, axis) + i * axis * dir;
					
					if (lm.Occupied(m)) {
						break;
					}
					
					anInEyeSightPos = m;
				}
			}
			
			if (anInEyeSightPos != -Vector2.one) {
				if (!tabooList.Exists(l => HlpVect.EqualInt(l, anInEyeSightPos))) {
					List<Node> path = Path(from, anInEyeSightPos);
					
					if (path != null) {
						return path;
					}
				}
			}
		}
		
		return null;
	}
	
	//List<Vector2> FindClosestInEyeSightPositionsOf (Vector2 target) {}
}
