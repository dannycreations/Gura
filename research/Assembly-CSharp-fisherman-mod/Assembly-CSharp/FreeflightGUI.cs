using System;
using UnityEngine;

public class FreeflightGUI : MonoBehaviour
{
	private void Start()
	{
		this.freeflight = base.GetComponent<Freeflight>();
	}

	private void OnGUI()
	{
		this.style.normal.background = this.background;
		GUILayout.BeginArea(new Rect(16f, 16f, 192f, 24f));
		if (this.freeflight.enabled)
		{
			GUILayout.Box("Press escape to exit freeflight", new GUILayoutOption[0]);
			if (Input.GetKey(27))
			{
				this.freeflight.enabled = false;
				Cursor.visible = true;
				Cursor.lockState = 0;
			}
		}
		else if (GUILayout.Button("Click to active freeflight", new GUILayoutOption[0]))
		{
			this.freeflight.enabled = true;
			Cursor.visible = false;
			Cursor.lockState = 1;
		}
		GUILayout.EndArea();
		GUI.backgroundColor = new Color(0f, 0f, 0f, 0.8f);
		GUI.Box(new Rect(16f, (float)(Screen.height - 128), 260f, 108f), string.Empty);
		GUILayout.BeginArea(new Rect(16f, (float)(Screen.height - 128), 260f, 108f), this.style);
		GUILayout.Label("Freeflight controls:", new GUILayoutOption[0]);
		GUILayout.Label("WASD: move forward/backward and strafe", new GUILayoutOption[0]);
		GUILayout.Label("QE: move up and down", new GUILayoutOption[0]);
		GUILayout.Label("Mouse Wheel: +/- movement speed", new GUILayoutOption[0]);
		GUILayout.EndArea();
	}

	[SerializeField]
	[HideInInspector]
	private Freeflight freeflight;

	[SerializeField]
	private Texture2D background;

	private GUIStyle style = new GUIStyle();
}
