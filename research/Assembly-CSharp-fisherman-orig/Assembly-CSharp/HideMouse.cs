using System;
using UnityEngine;

public class HideMouse : MonoBehaviour
{
	private void Start()
	{
		Cursor.visible = false;
		Screen.lockCursor = true;
	}
}
