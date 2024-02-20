using System;
using UnityEngine;

public class MB2_LODManagerGUI : MonoBehaviour
{
	private void OnGUI()
	{
		MB2_LODManager component = base.GetComponent<MB2_LODManager>();
		if (GUI.Button(new Rect(0f, 0f, 100f, 20f), "LOD Stats"))
		{
			if (component != null)
			{
				this.text = component.GetStats();
			}
			else
			{
				this.text = "Could not find LODManager";
			}
			Debug.Log(this.text);
		}
		GUI.Label(new Rect(0f, 20f, 300f, 600f), this.text);
	}

	private string text;
}
