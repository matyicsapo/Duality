using UnityEngine;

public class MainMenu : MonoBehaviour {
	public Vector2 trgtRes;
	
	public GUISkin mSkin;
	
	[System.Serializable]
	public class GUIMain {
		public Vector2 btnSize;
		public float verticalSpacing;
	}
	public GUIMain guiMain;
	
	[System.Serializable]
	public class GUIPlay {
		public Vector2 centerGroupOffset;
		public Vector2 scrollviewSize;
		public Vector2 itemBtnSize;
		public Vector2 btnSize;
		public float verticalSpacing;
	}
	public GUIPlay guiPlay;
	
	Vector2 levelScrollPos;
	
	LevelManager.LevelInfo selLevInf;
	
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
	public class GUIOptions {
		public Vector2 optionLabelSize;
		public Vector2 optionBtnSize;
		public Vector2 toggleSize;
		public Vector2 lowBtnSize;
		public float verticalSpacing;
		public float horizontalSpacing;
		public float centerGroupOffsetY;
		public float lowBtnsOffsetY;
	}
	public GUIOptions guiOptions;
	
	[System.Serializable]
	public class GUICredits {
		public Vector2 btnSize;
		public Vector2 labelSize;
		public float verticalSpacing;
	}
	public GUICredits guiCredits;
	
	enum State {Main, Options, Play, Credits};
	State state;
	
	public int gameLevelIndex;
	
	public GameObject editorstuff;
	public GameObject gamestuff;
	
	SettingsManager settingsManager;
	
	void Start () {
		state = State.Main;
		
		settingsManager = GetComponent<SettingsManager>();
		
		selLevInf.levelName = "";
	}
	
	void OnGUI () {
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
			new Vector3(Screen.width / trgtRes.x, Screen.height / trgtRes.y, 1));
		
		GUI.skin = mSkin;
		
		switch (state) {
			case State.Main:
				GUIStateMain();
			break;
			case State.Options:
				GUIStateOptions();
			break;
			case State.Play:
				GUIStatePlay();
			break;
			case State.Credits:
				GUIStateCredits();
			break;
		}
	}
	
	void GUIStateMain () {
# if UNITY_WEBPLAYER
		int btnCnt = 4; // play, editor, options, credits, (exit game)
# else
		int btnCnt = 5; // + exit game
# endif
		
		Vector2 centerGroupSize = new Vector2(guiMain.btnSize.x,
			guiMain.btnSize.y * btnCnt + guiMain.verticalSpacing * (btnCnt - 1));
		Rect r = new Rect(trgtRes.x / 2 - centerGroupSize.x / 2,
			trgtRes.y / 2 - centerGroupSize.y / 2, centerGroupSize.x, centerGroupSize.y);
		
		GUI.BeginGroup(r);
			r = new Rect(0, 0, guiMain.btnSize.x, guiMain.btnSize.y);
		
			if (GUI.Button(r, "Play")) {
				OnButtonPlay();
			}
		
			r.y += guiMain.btnSize.y + guiMain.verticalSpacing;
			if (GUI.Button(r, "Editor")) {
				OnButtonEditor();
			}
		
			r.y += guiMain.btnSize.y + guiMain.verticalSpacing;
			if (GUI.Button(r, "Options")) {
				OnButtonOptions();
			}
		
			r.y += guiMain.btnSize.y + guiMain.verticalSpacing;
			if (GUI.Button(r, "Credits")) {
				OnButtonCredits();
			}
		
# if !UNITY_WEBPLAYER
			r.y += guiMain.btnSize.y + guiMain.verticalSpacing;
			if (GUI.Button(r, "Exit")) {
				Application.Quit();
			}
#endif
		
		GUI.EndGroup();
	}
	
	void OnButtonPlay () {
		state = State.Play;
	}
	
	void OnButtonEditor () {
		LoadEditor();
	}
	
	void OnButtonOptions () {
		state = State.Options;
		settingsManager.Init();
	}
	
	void OnButtonCredits () {
		state = State.Credits;
	}
	
	void GUIStateOptions () {
		Vector2 centerGroupSize =
			new Vector2(guiOptions.optionLabelSize.x + guiOptions.horizontalSpacing + guiOptions.optionBtnSize.x,
				guiOptions.optionBtnSize.y * 2 + guiOptions.toggleSize.y + guiOptions.verticalSpacing * 2);
		Rect r = new Rect(trgtRes.x / 2 - centerGroupSize.x / 2,
			trgtRes.y / 2 - centerGroupSize.y / 2 + guiOptions.centerGroupOffsetY, centerGroupSize.x, centerGroupSize.y);
		
		GUI.BeginGroup(r, mSkin.GetStyle("box"));
			r = new Rect(0, 0, guiOptions.optionLabelSize.x, guiOptions.optionLabelSize.y);
			GUI.Label(r, "Quality");
			r = new Rect(r.x + guiOptions.optionLabelSize.x + guiOptions.horizontalSpacing, r.y,
				guiOptions.optionBtnSize.x, guiOptions.optionBtnSize.y);
			if (GUI.Button(r, settingsManager.settings.quality.ToString())) {
				settingsManager.ChangeQuality(1);
			}
		
			r = new Rect(0, r.y + guiOptions.optionBtnSize.y + guiOptions.verticalSpacing,
				guiOptions.optionLabelSize.x, guiOptions.optionLabelSize.y);
			GUI.Label(r, "Resolution");
			r = new Rect(r.x + guiOptions.optionLabelSize.x + guiOptions.horizontalSpacing, r.y,
				guiOptions.optionBtnSize.x, guiOptions.optionBtnSize.y);
			if (GUI.Button(r, settingsManager.settings.width.ToString() + "*" + settingsManager.settings.height.ToString())) {
				settingsManager.ChangeResolution(1);
			}
		
			r = new Rect(0, r.y + guiOptions.optionBtnSize.y + guiOptions.verticalSpacing,
				guiOptions.toggleSize.x, guiOptions.toggleSize.y);
			settingsManager.settings.fullScreen = GUI.Toggle(r, settingsManager.settings.fullScreen, "Fullscreen");
		GUI.EndGroup();
		
		r = new Rect(trgtRes.x / 2 - (guiOptions.lowBtnSize.x * 2 + guiOptions.horizontalSpacing) / 2,
			trgtRes.y - guiOptions.lowBtnSize.y - guiOptions.lowBtnsOffsetY, guiOptions.lowBtnSize.x, guiOptions.lowBtnSize.y);
		if (GUI.Button(r, "Apply")) {
			settingsManager.ApplySettings();
		}
		
		r.x += guiOptions.lowBtnSize.x + guiOptions.horizontalSpacing;
		if (GUI.Button(r, "Back")) {
			settingsManager.ResetSettings();
			state = State.Main;
		}
	}
	
	void GUIStatePlay () {
		Vector2 centerGroupSize = new Vector2(guiPlay.scrollviewSize.x,
			guiPlay.scrollviewSize.y + guiPlay.verticalSpacing + guiPlay.btnSize.y);
		Rect r = new Rect(trgtRes.x / 2 - centerGroupSize.x / 2 + guiPlay.centerGroupOffset.x,
			trgtRes.y / 2 - centerGroupSize.y / 2 + guiPlay.centerGroupOffset.y, centerGroupSize.x, centerGroupSize.y);
	
		GUI.BeginGroup(r);
			r = new Rect(0, 0, guiPlay.scrollviewSize.x, guiPlay.scrollviewSize.y);
		
			string[] files = System.IO.Directory.GetFiles("Levels/", "*.lev");
		
			Rect viewRect = new Rect(0, 0, 0,
				files.Length * guiPlay.itemBtnSize.y + (files.Length - 1) * guiPlay.verticalSpacing);
		
			levelScrollPos = GUI.BeginScrollView(r, levelScrollPos, viewRect);
				r = new Rect(0, 0, guiPlay.itemBtnSize.x, guiPlay.itemBtnSize.y);
		
				for (int i = 0; i < files.Length; i++) {
					int start = files[i].LastIndexOf("/") + 1;
					int end = files[i].LastIndexOf(".");
					string levelName = files[i].Substring(start, end - start); 
			
					if (GUI.Button(r, levelName)) {
						selLevInf = LoadLevelInfo(levelName);
					}
			
					if (i+1 != files.Length) {
						r.y += guiPlay.itemBtnSize.y + guiPlay.verticalSpacing;
					}
				}
			GUI.EndScrollView();
		
			r = new Rect(0, guiPlay.scrollviewSize.y + guiPlay.verticalSpacing, guiPlay.btnSize.x, guiPlay.btnSize.y);
			if (GUI.Button(r, "Back")) {
				state = State.Main;
				selLevInf.levelName = "";
			}
		GUI.EndGroup();
		
		if (selLevInf.levelName != "") {
			GUIDisplayLevelInfo();
		}
	}
	
	LevelManager.LevelInfo LoadLevelInfo (string levelName) {
		Save.Level level = Save.DeSerialize(levelName);
		
		LevelManager.LevelInfo inf = new LevelManager.LevelInfo();
		inf.levelName = level.levelName;
		inf.size = new Vector2(level.size.x, level.size.y);
		inf.desc = level.desc;
		
		return inf;
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
			if (GUI.Button(r, "Play")) {
				LoadLevel(selLevInf.levelName);
			}
		GUI.EndGroup();
	}
	
	void GUIStateCredits () {
		Rect r = new Rect(trgtRes.x / 2 - guiCredits.labelSize.x / 2,
			trgtRes.y / 2 - guiCredits.labelSize.y / 2, guiCredits.labelSize.x, guiCredits.labelSize.y);
		
		GUI.BeginGroup(r, mSkin.GetStyle("box"));
			r = new Rect(0, 0, r.width, r.height);
			GUI.Label(r, "Matyi Csapo"
		    	+ "\n\nhttp://answers.unity3d.com/users/908/matyi-csapo.html"
		        + "\n\ne-mail: matyicsapo@gmail.com"
		        + "\n\nblablabla ima great guy : D haha");
		GUI.EndGroup();
		
		r = new Rect(trgtRes.x / 2 - guiCredits.labelSize.x / 2,
			trgtRes.y / 2 - guiCredits.labelSize.y / 2 + guiCredits.labelSize.y + guiCredits.verticalSpacing,
			guiCredits.btnSize.x, guiCredits.btnSize.y);
		if (GUI.Button(r, "Back")) {
			state = State.Main;
		}
	}
	
	void LoadEditor () {
		DontDestroyOnLoad(Instantiate(editorstuff, Vector3.zero, Quaternion.identity));
		Application.LoadLevel(gameLevelIndex);
	}
	
	void LoadLevel (string levelName) {
		GameStuff inst_gameStuff =
			(Instantiate(gamestuff, Vector3.zero, Quaternion.identity) as GameObject).GetComponent<GameStuff>();
		inst_gameStuff.levelName = levelName;
		DontDestroyOnLoad(inst_gameStuff.gameObject);
		Application.LoadLevel(gameLevelIndex);
	}
}
