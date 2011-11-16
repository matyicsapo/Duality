using UnityEngine;

public static class HlpVect {
	public static bool EqualInt (Vector2 lhs, Vector2 rhs) {
		return ((int)lhs.x == (int)rhs.x) && ((int)lhs.y == (int)rhs.y);
	}
}
