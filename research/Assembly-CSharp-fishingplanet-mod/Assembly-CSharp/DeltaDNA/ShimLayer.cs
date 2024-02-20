using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeltaDNA
{
	internal class ShimLayer : Layer
	{
		public void Init(IPopup popup, Dictionary<string, object> config, int depth)
		{
			this._popup = popup;
			this._depth = depth;
			object obj;
			if (config.TryGetValue("mask", out obj))
			{
				bool flag = true;
				Color32[] array = new Color32[1];
				string text = (string)obj;
				if (text != null)
				{
					if (text == "dimmed")
					{
						array[0] = new Color32(0, 0, 0, 128);
						goto IL_9A;
					}
					if (text == "clear")
					{
						array[0] = new Color32(0, 0, 0, 0);
						goto IL_9A;
					}
				}
				flag = false;
				IL_9A:
				if (flag)
				{
					this._texture = new Texture2D(1, 1);
					this._texture.SetPixels32(array);
					this._texture.Apply();
				}
			}
			object obj2;
			if (config.TryGetValue("action", out obj2))
			{
				base.RegisterAction((Dictionary<string, object>)obj2, "shim");
			}
			else
			{
				base.RegisterAction();
			}
		}

		public void OnGUI()
		{
			GUI.depth = this._depth;
			if (this._texture)
			{
				Rect rect;
				rect..ctor(0f, 0f, (float)Screen.width, (float)Screen.height);
				GUI.DrawTexture(rect, this._texture);
				if (GUI.Button(rect, string.Empty, GUIStyle.none) && this._actions.Count > 0)
				{
					this._actions[0]();
				}
			}
		}

		private Texture2D _texture;

		private const byte _dimmedMaskAlpha = 128;
	}
}
