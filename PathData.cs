using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathData : MonoBehaviour {
	//定義方向 方便擴充
	public enum MoveDirection
	{
		Up,
		Down,
		Left,
		Right
	};

	//輸入方向回穿Vector3
	public static Vector3 PathRecognize(MoveDirection dir){
		switch (dir) {
		case MoveDirection.Up:
			return new Vector3 (0, 1, 0);
		case MoveDirection.Down:
			return new Vector3 (0, -1, 0);
		case MoveDirection.Left:
			return new Vector3 (-1, 0, 0);
		case MoveDirection.Right:
			return new Vector3 (1, 0, 0);
		default:
			return new Vector3 (0, 0, 0);
		};
	}
}
