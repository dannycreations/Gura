using System;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.Util
{
	public class SimpleFPSCounter : MonoBehaviour
	{
		private void Update()
		{
			this.deltaTime += (Time.deltaTime - this.deltaTime) * 0.1f;
		}

		private void OnGUI()
		{
			int width = Screen.width;
			int height = Screen.height;
			GUIStyle guistyle = new GUIStyle();
			Rect rect;
			rect..ctor((float)(width / 2 - 100), 0f, (float)(width / 2), (float)(height * 2 / 100));
			guistyle.alignment = 0;
			guistyle.fontSize = height * 2 / 80;
			guistyle.normal.textColor = new Color(0f, 0f, 0.5f, 1f);
			float num = this.deltaTime * 1000f;
			float num2 = 1f / this.deltaTime;
			string text = string.Format("{0:0.0} ms ({1:0.} fps)", num, num2);
			GUI.Label(rect, text, guistyle);
		}

		private float deltaTime;
	}
}
