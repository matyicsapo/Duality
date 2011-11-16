using UnityEngine;

public class ShipTrianglePlayerMouse : ShipBase {
	/* === color changing */
	public string changeColorBtn;
	/* color changing === */
	
	/* === movement */
	public string horizontalMoveAxis;
	public string verticalMoveAxis;
	/* movement === */
	
	public Vector2 faceDir;
	
	/* === shoot */
	public string shootBtn;
	/* shoot === */
	
	protected override void Start () {
		base.Start();
		
		health = gm.health.playerMax;
		
		mTransform.rotation = Quaternion.LookRotation(new Vector3(-faceDir.x, faceDir.y), Vector3.forward);
	}
	
	protected override void InputHandling () {
		mInput = new BaseInput();
		
		InputColorChange();
		
		InputMovement();
		
		InputShoot();
	}
	
	void InputColorChange () {
		if (energy >= gm.energy.costColorChange && Time.time - lastTimeColorChange > gm.color.interval) {
			mInput.colorChange = Input.GetButtonDown(changeColorBtn);
		}
	}
	
	void InputMovement () {
		if (iTween.Count(gameObject, "MoveBy") == 0 && energy >= gm.energy.costMove) {
			// no transversal motion allowed so
				// if 2 axes are pushed
			if (Input.GetAxisRaw(horizontalMoveAxis) != 0 && Input.GetAxisRaw(verticalMoveAxis) != 0) {
				float horizontal = Input.GetAxis(horizontalMoveAxis);
				float vertical = Input.GetAxis(verticalMoveAxis);
				
				// then go in the direction of the one pushed more
					// based on the built-in input smoothing of Unity // or on the raw analog stick values
				if (Mathf.Abs(horizontal) > Mathf.Abs(vertical))
					mInput.moveAxes = Vector2.right * Mathf.Sign(horizontal);
				else
					mInput.moveAxes = Vector2.up * Mathf.Sign(vertical); // important !! invert Y axis
			}
			else if (Input.GetAxisRaw(horizontalMoveAxis) != 0 || Input.GetAxisRaw(verticalMoveAxis) != 0) {
				// nothing special to do - GetAxisRaw is sufficient for keyboard input
					// "Since input is not smoothed, keyboard input will always be either -1, 0 or 1." - the doc	
				mInput.moveAxes = new Vector2(Input.GetAxisRaw(horizontalMoveAxis), Input.GetAxisRaw(verticalMoveAxis));
			}
			
			mInput.moveAxes.y *= -1;
			
			if (!lm.CanTraverse(trgtPos + mInput.moveAxes)) {
				mInput.moveAxes = Vector2.zero;
			}
		}
	}
	
	void InputShoot () {
		if (Input.GetButtonDown(shootBtn) && energy >= gm.energy.costShoot && Time.time - lastTimeShoot > gm.shoot.interval) {
			Vector3 wMouse = GameObject.FindWithTag("MainCamera").camera.ScreenToWorldPoint(Input.mousePosition);	
			Vector3 shootTargetPos = new Vector3(wMouse.x, wMouse.y, shotPrefab.position.z);
			
			if (Mathf.Round(shootTargetPos.x) != Mathf.Round(mTransform.position.x)
					|| Mathf.Round(shootTargetPos.y) != Mathf.Round(mTransform.position.y)) {
				// can't target self
				
				float dX = shootTargetPos.x - mTransform.position.x;
				float dY = shootTargetPos.y - mTransform.position.y;
				
				if (Mathf.Abs(dX) > Mathf.Abs(dY))
					mInput.shootDir = Vector3.right * dX; // horizontal
				else // if (Mathf.Abs(dX) < Mathf.Abs(dY)) // not even exact bisector is a deadzone
					mInput.shootDir = Vector3.up * dY; // vertical
			}
		}
	}
	
	protected override void ActionMovement () {
		base.ActionMovement();
		
		OrientShip();
	}
	
	// ain't this shitty, ehh? ********************************************************************************
	protected void OrientShip () {
		//if (iTween.Count(gameObject, "RotateAdd") == 0) { // not needed <= we're turning only when moving
			float angle = 0;
			
			if (HlpVect.EqualInt(Vector2.Scale(faceDir, mInput.moveAxes), Vector2.zero)) {
				angle = 90;
				
				if (HlpVect.EqualInt(faceDir, -Vector2.right)) {
					//print("facing left");
					angle *= HlpVect.EqualInt(mInput.moveAxes, Vector2.up) ? -1 : 1;
				}
				else if (HlpVect.EqualInt(faceDir, Vector2.right)) {
					//print("facing right");
					angle *= HlpVect.EqualInt(mInput.moveAxes, Vector2.up) ? 1 : -1;
				}
				else if (HlpVect.EqualInt(faceDir, -Vector2.up)) {
					//print("facing up");
					angle *= HlpVect.EqualInt(mInput.moveAxes, Vector2.right) ? 1 : -1;
				}
				else if (HlpVect.EqualInt(faceDir, Vector2.up)) {
					//print("facing down");
					angle *= HlpVect.EqualInt(mInput.moveAxes, Vector2.right) ? -1 : 1;
				}
			}
			else {
				if (faceDir != mInput.moveAxes)
					angle = 180;
			}
			
			if (angle != 0) {
				iTween.RotateAdd(gameObject, iTween.Hash("z", angle, "time", gm.moveTime,
					"easetype", iTween.EaseType.easeInOutSine , "space", Space.World));
				
				faceDir = mInput.moveAxes;
			}
		//}
	}
}
