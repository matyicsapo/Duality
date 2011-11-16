using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour {
	/*magic number replacaments ===*/
	float gridBaseSize = 10; // built-in plane mesh size
	float gridDepthOffset = -.2f;
	float cameraDepthOffset = 10;
	/*=== magic number replacaments*/
	
	[System.Serializable]
	public class LevelElement {
		public Transform transf;
		
		public enum EType {Block, Player, EnemyCore, Enemy};
		public EType eType;
	}
	public List<LevelElement> elems;
	
	[System.NonSerialized]
	public Transform shotHolder;
	[System.NonSerialized]
	public List<Shot> shotList;
	
	[System.NonSerialized]
	public Transform holderBlock;
	[System.NonSerialized]
	public Transform holderEnemy;
	
	/* save === */
	public string levelName;
	public Vector2 size;
	
	public string desc;
	
	[System.NonSerialized]
	public List<Transform> blockList;
	
	[System.NonSerialized]
	public Transform enemyCore;
	[System.NonSerialized]
	public List<ShipEnemy> enemies;
	
	[System.NonSerialized]
	public ShipTrianglePlayerMouse playerShip;
	/* === save */
	
	public struct LevelInfo {
		public string levelName;
		public string desc;
		public Vector2 size;
	}
	
	void Awake () {
		shotList = new List<Shot>();
		
		shotHolder = transform.parent.Find("Shots");
		
		blockList = new List<Transform>();
		
		enemies = new List<ShipEnemy>();
		
		enemyCore = null;
		
		holderBlock = transform.parent.Find("Blocks");
		holderEnemy = transform.parent.Find("Circles");
		
		// needed only if camera is not following anything and want it to be centered on level
		GameObject.FindWithTag("MainCamera").transform.position =
			new Vector3(size.x - 1, size.y - 1, cameraDepthOffset * 2) / 2;
	}
	
	public void SetGrid () {
		Transform grid = transform.parent.Find("Grid");
		grid.renderer.material.mainTextureScale = new Vector2(size.x - 1, size.y - 1);
		grid.renderer.material.mainTextureOffset = new Vector2(.5f, .5f);
		grid.localScale = new Vector3(size.x - 1, gridBaseSize, size.y - 1) / gridBaseSize;
		grid.position = new Vector3(size.x - 1, size.y - 1, gridDepthOffset * 2) / 2;
	}
	
	public bool CanTraverse (Vector2 pos) {
		return (InLevelBounds(pos) && !Occupied(pos));
	}

	public bool InLevelBounds (Vector2 pos) {
		if (0 <= (int)pos.x && (int)pos.x <= size.x - 1
				&& 0 <= (int)pos.y && (int)pos.y <= size.y - 1)
			return true;
		else
			return false;
	}
	
	public bool Occupied (Vector2 pos) {
		return blockList.Exists(l => HlpVect.EqualInt(l.position, pos))
			|| enemies.Exists(l => HlpVect.EqualInt(l.trgtPos, pos))
			|| (playerShip != null && HlpVect.EqualInt(playerShip.trgtPos, pos))
			|| (enemyCore != null && HlpVect.EqualInt(enemyCore.position, pos)); // evaluated from left to right
																					// => no prob if null
	}
	
	public LevelInfo LoadLevelInfo (string levelName) {
		Save.Level level = Save.DeSerialize(levelName);
		
		LevelInfo inf = new LevelInfo();
		inf.levelName = levelName;
		inf.size = new Vector2(level.size.x, level.size.y);
		inf.desc = level.desc;
		
		return inf;
	}
	
	public void LoadLevel (string levelName) {
		Save.Level level = Save.DeSerialize(levelName);
		
		this.levelName = level.levelName;
		size = new Vector2(level.size.x, level.size.y);
		
		desc = level.desc;
		
		Transform original = null;
		
		original = elems.Find(l => l.eType == LevelElement.EType.Block).transf;
		blockList = new List<Transform>();
		foreach (Save.Vect2Int pos in level.blocks) {
			Vector2 fPos = new Vector2(pos.x, pos.y);
			Transform t = Instantiate(original, fPos, original.rotation) as Transform;
			t.parent = holderBlock;
			blockList.Add(t);
		}
		
		original = elems.Find(l => l.eType == LevelElement.EType.EnemyCore).transf;
		{
			Vector2 fPos = new Vector2(level.enemyCore.x, level.enemyCore.y);
			Transform t = Instantiate(original, fPos, original.rotation) as Transform;
			t.parent = transform;
			enemyCore = t;
		}
		
		original = elems.Find(l => l.eType == LevelElement.EType.Enemy).transf;
		enemies = new List<ShipEnemy>();
		foreach (Save.Vect2Int pos in level.enemies) {
			Vector2 fPos = new Vector2(pos.x, pos.y);
			Transform t = Instantiate(original, fPos, original.rotation) as Transform;
			t.parent = holderEnemy;
			enemies.Add(t.GetComponent<ShipEnemy>());
		}
		
		original = elems.Find(l => l.eType == LevelElement.EType.Player).transf;
		{
			Vector2 fPos = new Vector2(level.playerPos.x, level.playerPos.y);
			Transform t = Instantiate(original, fPos, original.rotation) as Transform;
			t.parent = transform;
			playerShip = t.GetComponent<ShipTrianglePlayerMouse>();
			playerShip.faceDir = new Vector2(level.playerFaceDir.x, level.playerFaceDir.y);
			//playerShip.lifeCnt = level.playerLifeCnt
		}
	}
}
