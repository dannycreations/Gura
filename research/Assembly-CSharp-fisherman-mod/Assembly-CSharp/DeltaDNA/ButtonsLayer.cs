using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeltaDNA
{
	internal class ButtonsLayer : Layer
	{
		public void Init(IPopup popup, Dictionary<string, object> orientation, List<Texture> textures, BackgroundLayer content, int depth)
		{
			this._popup = popup;
			this._depth = depth;
			object obj;
			if (orientation.TryGetValue("buttons", out obj))
			{
				List<object> list = obj as List<object>;
				for (int i = 0; i < list.Count; i++)
				{
					Dictionary<string, object> dictionary = list[i] as Dictionary<string, object>;
					float num = 0f;
					float num2 = 0f;
					object obj2;
					if (dictionary.TryGetValue("x", out obj2))
					{
						num = (float)((int)((long)obj2)) * content.Scale + content.Position.xMin;
					}
					object obj3;
					if (dictionary.TryGetValue("y", out obj3))
					{
						num2 = (float)((int)((long)obj3)) * content.Scale + content.Position.yMin;
					}
					this._positions.Add(new Rect(num, num2, (float)textures[i].width * content.Scale, (float)textures[i].height * content.Scale));
					object obj4;
					if (dictionary.TryGetValue("action", out obj4))
					{
						base.RegisterAction((Dictionary<string, object>)obj4, "button" + (i + 1));
					}
					else
					{
						base.RegisterAction();
					}
				}
				this._textures = textures;
			}
		}

		public void OnGUI()
		{
			GUI.depth = this._depth;
			for (int i = 0; i < this._textures.Count; i++)
			{
				GUI.DrawTexture(this._positions[i], this._textures[i]);
				if (GUI.Button(this._positions[i], string.Empty, GUIStyle.none))
				{
					this._actions[i]();
				}
			}
		}

		private List<Texture> _textures = new List<Texture>();

		private List<Rect> _positions = new List<Rect>();
	}
}
