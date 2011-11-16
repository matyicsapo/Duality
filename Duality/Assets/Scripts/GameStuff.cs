using UnityEngine;

public class GameStuff : MonoBehaviour {
	public GUISkin mSkin;
	
	public Vector2 trgtRes;
	
	//[System.NonSerialized]
	public string levelName;
	
	LevelManager lm;
	GameManager gm;
	
	enum GUIState {None, Pause};
	GUIState guiState;
	
	[System.Serializable]
	public class GUIPause {
		public Vector2 btnSize;
		public float verticalSpacing;
	}
	public GUIPause guiPause;
	
	public int mainMenuLevelIndex;
	
	void OnLevelWasLoaded () {
		enabled = true;
		
		guiState = GUIState.None;
	}
	
	void Start () {
		lm = GameObject.FindWithTag("LevelManager").GetComponent<LevelManager>();
		lm.LoadLevel(levelName);
	}
	
	void OnGUI () {
		if (Event.current.type == EventType.KeyDown) {
			if (Event.current.keyCode == KeyCode.Escape) {
				if (guiState != GUIState.None) {
					guiState = GUIState.None;
					Time.timeScale = 1;
				}
				else {
					guiState = GUIState.Pause;
					Time.timeScale = 0;
				}
			}
		}
		
		if (guiState != GUIState.None) {
			GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
				new Vector3(Screen.width / trgtRes.x, Screen.height / trgtRes.y, 1));
		
			GUI.skin = mSkin;
			
			switch (guiState) {
				case GUIState.Pause:
					GUIStatePause();
				break;
			}
		}
	}
	
	void GUIStatePause () {
		int btnCnt = 2;
	
		Vector2 centerGroupSize = new Vector2(guiPause.btnSize.x,
			guiPause.btnSize.y * btnCnt + guiPause.verticalSpacing * (btnCnt - 1));
		Rect r = new Rect(trgtRes.x / 2 - centerGroupSize.x / 2,
			trgtRes.y / 2 - centerGroupSize.y / 2, centerGroupSize.x, centerGroupSize.y);
	
		GUI.BeginGroup(r);
			r = new Rect(0, 0, guiPause.btnSize.x, guiPause.btnSize.y);
		
			if (GUI.Button(r, "Resume")) {
				OnButtonResume();
			}
	
			r.y += guiPause.btnSize.y + guiPause.verticalSpacing;
			if (GUI.Button(r, "Quit")) {
				OnButtonQuit();
			}
		GUI.EndGroup();
	}
	
	void OnButtonResume () {
		guiState = GUIState.None;
		Time.timeScale = 1;
	}
	
	void OnButtonQuit () {
		Destroy(gameObject);
		Application.LoadLevel(mainMenuLevelIndex);
	}
}
