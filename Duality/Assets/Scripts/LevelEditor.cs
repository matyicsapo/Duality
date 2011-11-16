using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class LevelEditor : MonoBehaviour {
	/*public enum Aspect {a4_3, a5_4, a3_2, a16_10, a16_9};
	public Aspect aspectRatio;
	public float resMultiplier;*/
	public Vector2 trgtRes;
	
	public GUISkin mSkin;
	
	bool levelToBeRestarted;
	
	[System.Serializable]
	public class GUIEdit {
		public Vector2 leftBoxSize;
		public Vector2 labelSize;
		public float horizontalSpacing;
		public Vector2 textFieldSize;
		public Vector2 btnSize;
		public float verticalSpacing;
		public Vector2 smallLabelSize;
		public Vector2 smallTextFieldSize;
		public Vector2 toolBtnSize;
	}
	public GUIEdit guiEdit;
	
	[System.Serializable]
	public class GUILevDesc {
		public Vector2 boxSize;
	}
	public GUILevDesc guiLevDesc;
	
	[System.Serializable]
	public class GUISelected {
		public Vector2 boxSize;
		public Vector2 labelSize;
		public Vector2 toggleSize;
		public Vector2 btnSize;
		public float verticalSpacing;
		public float horizontalSpacing;
		public Vector2 lowerLabelSize;
	}
	public GUISelected guiSelected;
	
	[System.Serializable]
	public class GUIPause {
		public Vector2 btnSize;
		public float verticalSpacing;
	}
	public GUIPause guiPause;
	
	[System.Serializable]
	public class GUILoad {
		public Vector2 centerGroupOffset;
		public Vector2 scrollviewSize;
		public Vector2 itemBtnSize;
		public Vector2 btnSize;
		public float verticalSpacing;
	}
	public GUILoad guiLoad;
	
	[System.Serializable]
	public class GUILevInf {
		public Vector2 levelDescBoxSize;
		public Vector2 labelSize;
		public Vector2 descSize;
		public Vector2 btnSize;
		public float verticalSpacing;
	}
	public GUILevInf guiLevInf;
	
	[System.Serializable]
	public class GUINew {
		public Vector2 btnSize;
		public Vector2 labelSize;
		public Vector2 textFieldSize;
		public float verticalSpacing;
	}
	public GUINew guiNew;
	
	bool justStarted;
	
	Vector2 loadScrollPos;
	
	LevelManager.LevelInfo selLevInf;
	
	LevelManager lm;
	GameManager gm;
	
	public Texture[] tools;
	int selectedTool = 0;
	int lastSelectedTool = 0;
	
	public Transform[] placeables;
	
	enum PlaceableType {Block, Triangle, Core, Enemy}
	
	LevelManager.LevelElement selectedElement;
	
	bool mouseOverGUI;
	
	bool drag;
	
	bool editDesc;
	
	LevelManager.LevelElement duplicatee;
	
	public Transform defaultToolPrefab;
	public Material defaultToolMaterial;
	Transform toolPointer;
	
	Camera mainCamera;
	public float cameraSpd;
	
	enum State {Edit, Test, Pause, New, Load}
	State state;
	State lastState;
	
	public int mainMenuLevelIndex;
	
	void OnLevelWasLoaded () {
		enabled = true;
	}
	
	void Start () {
		levelToBeRestarted = false;
		
		selLevInf.levelName = "";
		
		editDesc = false;
		
		justStarted = true;
		
		lastState = state = State.Pause;
		
		lm = GameObject.FindWithTag("LevelManager").GetComponent<LevelManager>();
		gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
		
		gm.on = false;
		
		mainCamera = GameObject.FindWithTag("MainCamera").camera;
		
		selectedElement = null;
		
		drag = false;
		
		defaultToolPrefab.renderer.material = defaultToolMaterial;
		toolPointer = Instantiate(defaultToolPrefab, new Vector3(0, 0), defaultToolPrefab.rotation)
			as Transform;
		
		selectedTool = 0;
		lastSelectedTool = -1;
	}
	
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			if (state != State.Pause) {
				if (state == State.Test) {
					gm.on = false;
				}
				
				lastState = state;
				state = State.Pause;
			}
			else {
				state = lastState;
				
				if (state == State.Test) {
					gm.on = true;
				}
			}
		}
		
		if (state == State.Edit) {
			if (!mouseOverGUI) {
				Vector3 wMouse = GameObject.FindWithTag("MainCamera").camera.ScreenToWorldPoint(Input.mousePosition);
				wMouse = new Vector3(Mathf.Round(wMouse.x), Mathf.Round(wMouse.y), 0);
				
				toolPointer.position = new Vector3(Mathf.Clamp(wMouse.x, 0, lm.size.x - 1),
				                             		Mathf.Clamp(wMouse.y, 0, lm.size.y - 1), wMouse.z);
				
				if (Input.GetMouseButtonUp(0))
					drag = false;
				
				if (selectedTool <= 2) {
					switch (selectedTool) {
						case 0:
							if (Input.GetMouseButtonDown(0)) {
								SelectElement(toolPointer.position);
							}
							else if (Input.GetMouseButton(0)) {
								if (drag) {
									if (!lm.Occupied(toolPointer.position)) {
										selectedElement.transf.position = toolPointer.position;
									
										ShipBase ship = selectedElement.transf.GetComponent<ShipBase>();
										if (ship != null) {
											ship.lastPos = ship.trgtPos = selectedElement.transf.position;
										}
									}
								}
								else if (selectedElement != null) {
									LevelManager.LevelElement lastSelectedElement = selectedElement;
									SelectElement(toolPointer.position);
								
									if (selectedElement.transf.Equals(lastSelectedElement.transf)) {
										drag = true;
									}
								}
							}
							break;
						case 1:
							if (Input.GetMouseButtonDown(0)) {
								SelectElement(toolPointer.position);
								if (selectedElement != null) {
									switch (selectedElement.eType) {
										case LevelManager.LevelElement.EType.Block:
											lm.blockList.Remove(selectedElement.transf);
											break;
										case LevelManager.LevelElement.EType.Player:
											lm.playerShip = null;
											break;
										case LevelManager.LevelElement.EType.EnemyCore:
											lm.enemyCore = null;
											break;
										case LevelManager.LevelElement.EType.Enemy:
											lm.enemies.Remove(selectedElement.transf.GetComponent<ShipEnemy>());
											break;
									}
									Destroy(selectedElement.transf.gameObject);
									selectedElement = null;
								}
							}
							break;
						case 2:
							if (Input.GetMouseButtonDown(0)) {
								if (duplicatee != null) {
									if (lm.Occupied(toolPointer.position)) {
										duplicatee = null;
										selectedElement = null;
									}
									else {
										Transform t = Instantiate(toolPointer, toolPointer.position, toolPointer.rotation)
									        as Transform;
										t.parent = lm.holderEnemy;
										lm.enemies.Add(t.GetComponent<ShipEnemy>());
									}
								}
								else {
									SelectElement(toolPointer.position);
									if (selectedElement != null
								    		&& selectedElement.eType == LevelManager.LevelElement.EType.Enemy) {
									
										duplicatee = selectedElement;
									
										Destroy(toolPointer.gameObject);
										toolPointer = Instantiate(duplicatee.transf,
									    	duplicatee.transf.position, duplicatee.transf.rotation) as Transform;
									
										selectedElement = new LevelManager.LevelElement();
										selectedElement.transf = toolPointer;
										selectedElement.eType = LevelManager.LevelElement.EType.Enemy;
									
										editDesc = false;
									}
								}
							}
							break;
					}
				}
				else {
					if (Input.GetMouseButtonDown(0)) {
						if (!lm.Occupied(toolPointer.position)) {
							if (!(selectedTool == 4 && lm.playerShip != null) && !(selectedTool == 5 && lm.enemyCore != null)) {
								Transform t = Instantiate(toolPointer, toolPointer.position, toolPointer.rotation)
							        as Transform;
								
								editDesc = false;
								
								selectedElement = new LevelManager.LevelElement();
								selectedElement.transf = t;
								
								switch (selectedTool) {
									case 3:
										t.parent = lm.holderBlock;
										lm.blockList.Add(t);
										selectedElement.eType = LevelManager.LevelElement.EType.Block;
										break;
									case 4:
										t.parent = lm.transform;
										lm.playerShip = t.GetComponent<ShipTrianglePlayerMouse>();
										selectedElement.eType = LevelManager.LevelElement.EType.Player;
										break;
									case 5:
										t.parent = lm.transform;
										lm.enemyCore = t;
										selectedElement.eType = LevelManager.LevelElement.EType.EnemyCore;
										break;
									case 6:
										t.parent = lm.holderEnemy;
										lm.enemies.Add(t.GetComponent<ShipEnemy>());
										selectedElement.eType = LevelManager.LevelElement.EType.Enemy;
										break;
								}
							}
						}
						else {
							print("occupied");
						}
					}
				}
			}
		}
	}
	
	void LateUpdate () {
		if (state == State.Edit) {
			if (Input.GetKey(KeyCode.UpArrow)) {
				mainCamera.transform.position -= new Vector3(0, Time.deltaTime * cameraSpd);
			}
			else if (Input.GetKey(KeyCode.DownArrow)) {
				mainCamera.transform.position += new Vector3(0, Time.deltaTime * cameraSpd);
			}
			else if (Input.GetKey(KeyCode.LeftArrow)) {
				mainCamera.transform.position -= new Vector3(Time.deltaTime * cameraSpd, 0);
			}     
			else if (Input.GetKey(KeyCode.RightArrow)) {
				mainCamera.transform.position += new Vector3(Time.deltaTime * cameraSpd, 0);
			}
			
			mainCamera.orthographicSize =
				Mathf.Clamp(mainCamera.orthographicSize -
				    	Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * gm.cam.zoomSpd,
					gm.cam.minOrthoSize, gm.cam.maxOrthoSize);
		}
	}
	
	void SelectElement (Vector2 pos) {
		// block?
		Transform block = lm.blockList.Find(l => HlpVect.EqualInt(l.position, pos));
		if (block != null) {
			selectedElement = new LevelManager.LevelElement();
			selectedElement.transf = block;
			selectedElement.eType = LevelManager.LevelElement.EType.Block;
			
			editDesc = false;
			
			return;
		}
		
		// enemy?
		// using .transform.position as .trgtPos as that doesn't get updated and there's no need for it as it's set to
			// .transform.position on Start() in ShipBase.cs
		ShipEnemy shipEnemy = lm.enemies.Find(l => HlpVect.EqualInt(l.transform.position, pos));
		if (shipEnemy != null) {
			selectedElement = new LevelManager.LevelElement();
			selectedElement.transf = shipEnemy.transform;
			selectedElement.eType = LevelManager.LevelElement.EType.Enemy;
			
			editDesc = false;
			
			return;
		}
		
		// player?
		if (lm.playerShip != null && HlpVect.EqualInt(lm.playerShip.transform.position, pos)) {
			selectedElement = new LevelManager.LevelElement();
			selectedElement.transf = lm.playerShip.transform;
			selectedElement.eType = LevelManager.LevelElement.EType.Player;
			
			editDesc = false;
			
			return;
		}
		
		// enemy core?
		if (lm.enemyCore != null && HlpVect.EqualInt(lm.enemyCore.position, pos)) {
			selectedElement = new LevelManager.LevelElement();
			selectedElement.transf = lm.enemyCore;
			selectedElement.eType = LevelManager.LevelElement.EType.EnemyCore;
			
			editDesc = false;
			
			return;
		}
		
		selectedElement = null;
	}
	
	void OnGUI () {
		/*switch (aspectRatio) {
			case Aspect.a4_3:
				trgtRes = new Vector2(4, 3) * resMultiplier;
			break;
			case Aspect.a5_4:
				trgtRes = new Vector2(5, 4) * resMultiplier;
			break;
			case Aspect.a3_2:
				trgtRes = new Vector2(3, 2) * resMultiplier;
			break;
			case Aspect.a16_10:
				trgtRes = new Vector2(16, 10) * resMultiplier;
			break;
			case Aspect.a16_9:
				trgtRes = new Vector2(16, 9) * resMultiplier;
			break;
		}*/
		
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
			new Vector3(Screen.width / trgtRes.x, Screen.height / trgtRes.y, 1));
		
		GUI.skin = mSkin;
		
		switch (state) {
			case State.Edit:
				GUIStateEdit();
			break;
			case State.Pause:
				GUIStatePause();
			break;
			case State.Load:
				GUIStateLoad();
			break;
			case State.New:
				GUIStateNew();
			break;
		}
	}
	
	void OnButtonEdit () {
		state = State.Edit;
		if (toolPointer == null) {
			toolPointer = Instantiate(defaultToolPrefab, new Vector3(0, 0), defaultToolPrefab.rotation)
				as Transform;
		}
		
		if (levelToBeRestarted)
			LoadNewLevel(lm.levelName);
	}
	
	void OnButtonTest () {
		state = State.Test;
		gm.on = true;
		if (toolPointer != null) {
			Destroy(toolPointer.gameObject);
		}
		levelToBeRestarted = true;
	}
	
	void OnButtonSave () {
		SaveLevel();
	}
	
	void OnButtonLoad () {
		lastState = state;
		state = State.Load;
	}
	
	void OnButtonNew () {
		lastState = state;
		state = State.New;
		if (toolPointer == null) {
			toolPointer = Instantiate(defaultToolPrefab, new Vector3(0, 0), defaultToolPrefab.rotation)
				as Transform;
		}
	}
	
	void OnButtonQuit () {
		Destroy(gameObject);
		mainCamera.GetComponent<CameraFader>().FadeOut("LoadMainMenu", gameObject);
		LoadMainMenu(); //
	}
	
	void LoadMainMenu () {
		Application.LoadLevel(mainMenuLevelIndex);
	}
	
	void GUIStateEdit () {
		if (Event.current.type == EventType.Repaint) {
			mouseOverGUI = false;
		}
		
		Rect r = new Rect(0, trgtRes.y / 2 - guiEdit.leftBoxSize.y / 2,
			guiEdit.leftBoxSize.x, guiEdit.leftBoxSize.y);
		
		UpdateMouseOverGUI(r);
		
		GUI.BeginGroup(r, mSkin.GetStyle("box"));
			r = new Rect(0, 0, guiEdit.labelSize.x, guiEdit.labelSize.y);
			GUI.Label(r, "Levelname:");
			r = new Rect(guiEdit.labelSize.x + guiEdit.horizontalSpacing, 0, guiEdit.textFieldSize.x, guiEdit.textFieldSize.y);
			lm.levelName = GUITextField(r, lm.levelName);
		
			r = new Rect(0, guiEdit.labelSize.y + guiEdit.verticalSpacing, guiEdit.labelSize.x, guiEdit.labelSize.y);
			GUI.Label(r, "Levelsize:");
			r = new Rect(guiEdit.labelSize.x + guiEdit.horizontalSpacing, r.y,
				guiEdit.smallLabelSize.x, guiEdit.smallLabelSize.y);
			GUI.Label(r, "X:");
			r = new Rect(r.x + guiEdit.smallLabelSize.x + guiEdit.horizontalSpacing, r.y,
				guiEdit.smallTextFieldSize.x, guiEdit.smallTextFieldSize.y);
			lm.size.x = GUIFloatField(r, lm.size.x);
			r = new Rect(r.x + guiEdit.smallTextFieldSize.x + guiEdit.horizontalSpacing, r.y,
				guiEdit.smallLabelSize.x, guiEdit.smallLabelSize.y);
			GUI.Label(r, "Y:");
			r = new Rect(r.x + guiEdit.smallLabelSize.x + guiEdit.horizontalSpacing, r.y,
				guiEdit.smallTextFieldSize.x, guiEdit.smallTextFieldSize.y);
			lm.size.y = GUIFloatField(r, lm.size.y);
			if (GUI.changed) {
				lm.SetGrid();
			}
		
			r = new Rect(0, r.y + guiEdit.labelSize.y + guiEdit.verticalSpacing, guiEdit.btnSize.x, guiEdit.btnSize.y);
			if (GUI.Button(r, "Edit Description")) {
				editDesc = !editDesc;
			}
			if (editDesc)
				selectedElement = null;
		
			r = new Rect(0, r.y + guiEdit.btnSize.y + guiEdit.verticalSpacing,
				guiEdit.labelSize.x, guiEdit.labelSize.y);
			GUI.Label(r, "Tools:");
		
			r = new Rect(0, r.y + guiEdit.labelSize.y + guiEdit.verticalSpacing,
				guiEdit.toolBtnSize.x, guiEdit.toolBtnSize.y);
		
			r.y -= guiEdit.toolBtnSize.y;
			for (int i = 0; i < tools.Length; i++) {
				if ((i % 2) == 0) {
					r.x = 0;
					r.y += guiEdit.toolBtnSize.y;
				}
				else {
					r.x = guiEdit.toolBtnSize.x + guiEdit.horizontalSpacing;
				}
			
				bool t = (selectedTool == i);
				t = GUI.Toggle(r, t, tools[i], mSkin.GetStyle("button"));
				if (t != (selectedTool == i)) {
					selectedTool = i;
				}
			}
			SetTool();
		
		GUI.EndGroup();
		
		if (selectedElement != null) {
			GUIDisplaySelectedElementInfo();
		}
		else if (editDesc) {
			GUIEditLevelDesc();
		}
	}
	
	void SetTool () {
		if (selectedTool != lastSelectedTool) {
			Vector3 toolPointerPosition = Vector3.zero;
			Quaternion toolPointerRotation = Quaternion.identity;
	
			Transform t = null;
	
			if (selectedTool <= 2) {
				// change texture only
				if (lastSelectedTool <= 2) {
					toolPointer.renderer.material.mainTexture = tools[selectedTool];
					lastSelectedTool = selectedTool;
					return;
				}
				
				t = defaultToolPrefab;
				t.renderer.material.mainTexture = tools[selectedTool];
			}
			else {
				if (selectedTool == 3) {
					t = lm.elems.Find(l => l.eType == LevelManager.LevelElement.EType.Block).transf;
				}
				else if (selectedTool == 4) {
					t = lm.elems.Find(l => l.eType == LevelManager.LevelElement.EType.Player).transf;
				}
				else if (selectedTool == 5) {
					t = lm.elems.Find(l => l.eType == LevelManager.LevelElement.EType.EnemyCore).transf;
				}
				else {
					t = lm.elems.Find(l => l.eType == LevelManager.LevelElement.EType.Enemy).transf;
				}
			}
			
			toolPointerRotation = t.rotation;
	
			toolPointerPosition = toolPointer.position;
			Destroy(toolPointer.gameObject);
	
			toolPointer = Instantiate(t, toolPointerPosition, toolPointerRotation) as Transform;
	
		    lastSelectedTool = selectedTool;
		}
	}
	
	void GUIStatePause () {
		int btnCnt = justStarted ? 3 : 6;
	
		Vector2 centerGroupSize = new Vector2(guiPause.btnSize.x,
			guiPause.btnSize.y * btnCnt + guiPause.verticalSpacing * (btnCnt - 1));
		Rect r = new Rect(trgtRes.x / 2 - centerGroupSize.x / 2,
			trgtRes.y / 2 - centerGroupSize.y / 2, centerGroupSize.x, centerGroupSize.y);
	
		GUI.BeginGroup(r);
			//edit, test, save, load, new, quit
			r = new Rect(0, 0, guiPause.btnSize.x, guiPause.btnSize.y);
	
			if (!justStarted) {
				if (GUI.Button(r, "Edit")) {
					OnButtonEdit();
				}
		
				r.y += guiPause.btnSize.y + guiPause.verticalSpacing;
				if (GUI.Button(r, "Test")) {
					OnButtonTest();
				}
		
				r.y += guiPause.btnSize.y + guiPause.verticalSpacing;
				if (GUI.Button(r, "Save")) {
					OnButtonSave();
				}
		
				r.y += guiPause.btnSize.y + guiPause.verticalSpacing;
			}
			if (GUI.Button(r, "Load")) {
				if (System.IO.Directory.GetFiles("Levels/", "*.lev").Length != 0) {
					OnButtonLoad();
				}
			}
	
			r.y += guiPause.btnSize.y + guiPause.verticalSpacing;
			if (GUI.Button(r, "New")) {
				OnButtonNew();
			}
	
			r.y += guiPause.btnSize.y + guiPause.verticalSpacing;
			if (GUI.Button(r, "Quit")) {
				OnButtonQuit();
			}
		GUI.EndGroup();
	}
	
	void GUIStateLoad () {
		Vector2 centerGroupSize = new Vector2(guiLoad.scrollviewSize.x,
			guiLoad.scrollviewSize.y + guiLoad.verticalSpacing + guiLoad.btnSize.y);
		Rect r = new Rect(trgtRes.x / 2 - centerGroupSize.x / 2 + guiLoad.centerGroupOffset.x,
			trgtRes.y / 2 - centerGroupSize.y / 2 + guiLoad.centerGroupOffset.y, centerGroupSize.x, centerGroupSize.y);
	
		GUI.BeginGroup(r);
			r = new Rect(0, 0, guiLoad.scrollviewSize.x, guiLoad.scrollviewSize.y);
		
			string[] files = System.IO.Directory.GetFiles("Levels/", "*.lev");
		
			Rect viewRect = new Rect(0, 0, 0,
				files.Length * guiLoad.itemBtnSize.y + (files.Length - 1) * guiLoad.verticalSpacing);
		
			loadScrollPos = GUI.BeginScrollView(r, loadScrollPos, viewRect);
				r = new Rect(0, 0, guiLoad.itemBtnSize.x, guiLoad.itemBtnSize.y);
		
				for (int i = 0; i < files.Length; i++) {
					int start = files[i].LastIndexOf("/") + 1;
					int end = files[i].LastIndexOf(".");
					string levelName = files[i].Substring(start, end - start); 
			
					if (GUI.Button(r, levelName)) {
						selLevInf = lm.LoadLevelInfo(levelName);
					}
			
					if (i+1 != files.Length) {
						r.y += guiLoad.itemBtnSize.y + guiLoad.verticalSpacing;
					}
				}
			GUI.EndScrollView();
		
			r = new Rect(0, guiLoad.scrollviewSize.y + guiLoad.verticalSpacing, guiLoad.btnSize.x, guiLoad.btnSize.y);
			if (GUI.Button(r, "Back")) {
				lastState = state;
				state = State.Pause;
				selLevInf.levelName = "";
			}
		GUI.EndGroup();
		
		if (selLevInf.levelName != "") {
			GUIDisplayLevelInfo();
		}
	}

	void LoadNewLevel (string levelName) {
		DestroyLevel();
		lm.LoadLevel(levelName);
		
		levelToBeRestarted = false;
	}
	
	void GUIDisplaySelectedElementInfo () {
		Rect r = new Rect(trgtRes.x -  guiSelected.boxSize.x, trgtRes.y / 2 - guiSelected.boxSize.y / 2,
			guiSelected.boxSize.x, guiSelected.boxSize.y);
		
		UpdateMouseOverGUI(r);
		
		GUI.BeginGroup(r, mSkin.GetStyle("box"));
			r = new Rect(0, 0, guiSelected.labelSize.x, guiSelected.labelSize.y);
		
			GUI.Label(r, "position: X: " + selectedElement.transf.position.x + " Y: " + selectedElement.transf.position.y);
		
			switch (selectedElement.eType) {
				case LevelManager.LevelElement.EType.Player:
					// could just use lm.playerShip
					ShipTrianglePlayerMouse playerShip =
						selectedElement.transf.GetComponent<ShipTrianglePlayerMouse>();
			
					r = new Rect(0, guiSelected.labelSize.y + guiSelected.verticalSpacing,
						guiSelected.toggleSize.x, guiSelected.toggleSize.y);
					playerShip.isYin = GUI.Toggle(r, playerShip.isYin, "isYin");
					playerShip.mMaterial.color = playerShip.isYin ? gm.color.yin : gm.color.yang;
			
					r = new Rect(0, r.y + guiSelected.toggleSize.y + guiSelected.verticalSpacing,
						guiSelected.lowerLabelSize.x, guiSelected.lowerLabelSize.y);
					GUI.Label(r, "faceDir");
					string[] captions = new string[] {"Right", "Down", "Left", "Up"};
					Vector2[] dirs = new Vector2[] {Vector2.right, Vector2.up, -Vector2.right, -Vector2.up};
					int i;
					if (HlpVect.EqualInt(playerShip.faceDir, dirs[0])) {
						i = 0;
					}
					else if (HlpVect.EqualInt(playerShip.faceDir, dirs[1])) {
						i = 1;
					}
					else if (HlpVect.EqualInt(playerShip.faceDir, dirs[2])) {
						i = 2;
					}
					else {
						i = 3;
					}
					r = new Rect(guiSelected.labelSize.x + guiSelected.horizontalSpacing, r.y,
						guiSelected.btnSize.x, guiSelected.btnSize.y);
					if (GUI.Button(r, captions[i])) {
						i++;
						if (i == captions.Length) {
							i = 0;
						}
						playerShip.transform.Rotate(Vector3.up * 90);
						playerShip.faceDir = dirs[i];
					}
					break;
				case LevelManager.LevelElement.EType.Enemy:
					ShipEnemy xEnemyShip =
						selectedElement.transf.GetComponent<ShipEnemy>();
			
					r = new Rect(0, guiSelected.labelSize.y + guiSelected.verticalSpacing,
						guiSelected.toggleSize.x, guiSelected.toggleSize.y);
					xEnemyShip.isYin = GUI.Toggle(r, xEnemyShip.isYin, "isYin");
					xEnemyShip.mMaterial.color = xEnemyShip.isYin ? gm.color.yin : gm.color.yang;
					break;
			}
		GUI.EndGroup();
	}
	
	void GUIEditLevelDesc () {
		Rect r = new Rect(trgtRes.x - guiLevDesc.boxSize.x, trgtRes.y / 2 - guiLevDesc.boxSize.y / 2,
			guiLevDesc.boxSize.x, guiLevDesc.boxSize.y);
		
		UpdateMouseOverGUI(r);
		
		int maxLength = 100;
		
		//GUI.BeginGroup(r, mSkin.GetStyle("box"));
			//r = new Rect(0, 0, r.width, r.height);
			lm.desc = GUI.TextArea(r, lm.desc, maxLength);
		//GUI.EndGroup();
	}
	
	void GUIDisplayLevelInfo () {
		Rect r = new Rect(trgtRes.x - guiLevInf.levelDescBoxSize.x, trgtRes.y / 2 - guiLevInf.levelDescBoxSize.y / 2,
			guiLevInf.levelDescBoxSize.x, guiLevInf.levelDescBoxSize.y);
		
		GUI.BeginGroup(r, mSkin.GetStyle("box"));
			r = new Rect(0, 0, guiLevInf.labelSize.x, guiLevInf.labelSize.y);
			GUI.Label(r, "Name: " + selLevInf.levelName);
		
			r.y += guiLevInf.labelSize.y + guiLevInf.verticalSpacing;
			GUI.Label(r, "Size: X:" + selLevInf.size.x + " Y: " + selLevInf.size.y);
		
			r.y += guiLevInf.labelSize.y + guiLevInf.verticalSpacing;
			r.width = guiLevInf.descSize.x;
			r.height = guiLevInf.descSize.y;
			GUI.Label(r, selLevInf.desc);
		
			r.y += guiLevInf.descSize.y + guiLevInf.verticalSpacing;
			r.width = guiLevInf.btnSize.x;
			r.height = guiLevInf.btnSize.y;
			if (GUI.Button(r, "Load")) {
				justStarted = false;
				state = State.Pause;
				LoadNewLevel(selLevInf.levelName);
			}
		GUI.EndGroup();
	}
	
	void DestroyLevel () {
		selectedElement = null;
		duplicatee = null;
		
		foreach (Transform t in lm.blockList) {
			Destroy(t.gameObject);
		}
		lm.blockList.Clear();
		
		if (lm.enemyCore != null) {
			Destroy(lm.enemyCore.gameObject);
			lm.enemyCore = null;
		}
		
		foreach (ShipEnemy se in lm.enemies) {
			Destroy(se.gameObject);
		}
		lm.enemies.Clear();
		
		if (lm.playerShip != null) {
			Destroy(lm.playerShip.gameObject);
			lm.playerShip = null;
		}
	}
	
	void GUIStateNew () {
		Vector2 centerGroupSize =
			new Vector2(Mathf.Max(new float[] {guiNew.btnSize.x, guiNew.labelSize.x, guiNew.textFieldSize.x}),
				guiNew.btnSize.y * 2 + guiNew.labelSize.y + guiNew.textFieldSize.y + guiNew.verticalSpacing * 3);
		Rect r = new Rect(trgtRes.x / 2 - centerGroupSize.x / 2,
			trgtRes.y / 2 - centerGroupSize.y / 2, centerGroupSize.x, centerGroupSize.y);
		
		GUI.BeginGroup(r);
			r = new Rect(0, 0, guiNew.labelSize.x, guiNew.labelSize.y);
			GUI.Label(r, "New level's name");
		
			r.y += guiNew.labelSize.y + guiNew.verticalSpacing;
			r.width = guiNew.textFieldSize.x;
			r.height = guiNew.textFieldSize.y;
			lm.levelName = GUITextField(r, lm.levelName);
		
			r.y += guiNew.textFieldSize.y + guiNew.verticalSpacing;
			r.width = guiNew.btnSize.x;
			r.height = guiNew.btnSize.y;
			if (GUI.Button(r, "Apply")) {
				if (lm.levelName != "") {
					justStarted = false;
					state = State.Edit;
					DestroyLevel();
				}
			}
		
			r.y += guiNew.btnSize.y + guiNew.verticalSpacing;
			if (GUI.Button(r, "Back")) {
				state = State.Pause;
				lm.levelName = "";
			}
		GUI.EndGroup();
	}
	
	string GUITextField (Rect r, string inValue) {
		string regexPattern = "[a-zA-z0-9]+";
		int maxLength = 20;
		
		string result = "";
		MatchCollection mc =
			Regex.Matches(GUI.TextField(r, inValue, maxLength), regexPattern);
		foreach (Match m in mc) {
			result += m;
		}
		
		return result;
	}
	
	float GUIFloatField (Rect r, float inValue) {
		float minValue = 5;
		
		string regexPattern = "[0-9]+";
		int maxLength = 2;
		
		string result = "";
		MatchCollection mc =
			Regex.Matches(GUI.TextField(r, inValue.ToString(), maxLength), regexPattern);
		foreach (Match m in mc) {
			result += m;
		}
		
		float floatResult;
		if (!float.TryParse(result, out floatResult)) {
			floatResult = inValue;
		}
	
		if (floatResult < minValue) {
			return minValue;
		}
		else {
			return floatResult;
		}
	}
	
	void SaveLevel () {
		Save.Level level = new Save.Level();
		
		level.levelName = lm.levelName;
		level.size = new Save.Vect2Int(lm.size);
		
		level.desc = lm.desc;
		
		level.blocks = new Save.Vect2Int[lm.blockList.Count];
		for (int i = 0; i < lm.blockList.Count; i++) {
			level.blocks[i] = new Save.Vect2Int(lm.blockList[i].position);
		}
		
		if (lm.enemyCore != null)
			level.enemyCore = new Save.Vect2Int(lm.enemyCore.position);
		
		level.enemies = new Save.Vect2Int[lm.enemies.Count];
		for (int i = 0; i < lm.enemies.Count; i++) {
			level.enemies[i] = new Save.Vect2Int(lm.enemies[i].transform.position);
		}
		
		if (lm.playerShip != null) {
			level.playerPos = new Save.Vect2Int(lm.playerShip.transform.position);
			level.playerFaceDir = new Save.Vect2Int(lm.playerShip.faceDir);
			level.playerLifeCnt = 3;
		}
		
		if (!System.IO.Directory.Exists("Levels")) {
			System.IO.Directory.CreateDirectory("Levels");
		}
		
		Save.Serialize(level);
	}
	
	void UpdateMouseOverGUI (Rect r) {
		if (Event.current.type == EventType.Repaint) {
			if (!mouseOverGUI) {
				mouseOverGUI = r.Contains(Event.current.mousePosition);
			}
		}
	}
}
