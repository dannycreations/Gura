using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DeltaDNA
{
	internal class BackgroundLayer : Layer
	{
		public void Init(IPopup popup, Dictionary<string, object> layout, Texture texture, int depth)
		{
			this._popup = popup;
			this._texture = texture;
			this._depth = depth;
			object obj;
			if (layout.TryGetValue("background", out obj))
			{
				Dictionary<string, object> dictionary = obj as Dictionary<string, object>;
				object obj2;
				if (dictionary.TryGetValue("action", out obj2))
				{
					base.RegisterAction((Dictionary<string, object>)obj2, "background");
				}
				else
				{
					base.RegisterAction();
				}
				object obj3;
				if (dictionary.TryGetValue("cover", out obj3))
				{
					this._position = this.RenderAsCover((Dictionary<string, object>)obj3);
				}
				else if (dictionary.TryGetValue("contain", out obj3))
				{
					this._position = this.RenderAsContain((Dictionary<string, object>)obj3);
				}
				else
				{
					Logger.LogError("Invalid layout");
				}
			}
			else
			{
				base.RegisterAction();
			}
		}

		public Rect Position
		{
			get
			{
				return this._position;
			}
		}

		public float Scale
		{
			get
			{
				return this._scale;
			}
		}

		public void OnGUI()
		{
			GUI.depth = this._depth;
			if (this._texture)
			{
				GUI.DrawTexture(this._position, this._texture);
				if (GUI.Button(this._position, string.Empty, GUIStyle.none) && this._actions.Count > 0)
				{
					this._actions[0]();
				}
			}
		}

		private Rect RenderAsCover(Dictionary<string, object> rules)
		{
			this._scale = Math.Max((float)Screen.width / (float)this._texture.width, (float)Screen.height / (float)this._texture.height);
			float num = (float)this._texture.width * this._scale;
			float num2 = (float)this._texture.height * this._scale;
			float num3 = (float)Screen.height / 2f - num2 / 2f;
			float num4 = (float)Screen.width / 2f - num / 2f;
			object obj;
			if (rules.TryGetValue("valign", out obj))
			{
				string text = (string)obj;
				if (text != null)
				{
					if (!(text == "top"))
					{
						if (text == "bottom")
						{
							num3 = (float)Screen.height - num2;
						}
					}
					else
					{
						num3 = 0f;
					}
				}
			}
			object obj2;
			if (rules.TryGetValue("halign", out obj2))
			{
				string text2 = (string)obj2;
				if (text2 != null)
				{
					if (!(text2 == "left"))
					{
						if (text2 == "right")
						{
							num4 = (float)Screen.width - num;
						}
					}
					else
					{
						num4 = 0f;
					}
				}
			}
			return new Rect(num4, num3, num, num2);
		}

		private Rect RenderAsContain(Dictionary<string, object> rules)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			object obj;
			if (rules.TryGetValue("left", out obj))
			{
				num = this.GetConstraintPixels((string)obj, (float)Screen.width);
			}
			object obj2;
			if (rules.TryGetValue("right", out obj2))
			{
				num2 = this.GetConstraintPixels((string)obj2, (float)Screen.width);
			}
			float num5 = ((float)Screen.width - num - num2) / (float)this._texture.width;
			object obj3;
			if (rules.TryGetValue("top", out obj3))
			{
				num3 = this.GetConstraintPixels((string)obj3, (float)Screen.height);
			}
			object obj4;
			if (rules.TryGetValue("bottom", out obj4))
			{
				num4 = this.GetConstraintPixels((string)obj4, (float)Screen.height);
			}
			float num6 = ((float)Screen.height - num3 - num4) / (float)this._texture.height;
			this._scale = Math.Min(num5, num6);
			float num7 = (float)this._texture.width * this._scale;
			float num8 = (float)this._texture.height * this._scale;
			float num9 = ((float)Screen.height - num3 - num4) / 2f - num8 / 2f + num3;
			float num10 = ((float)Screen.width - num - num2) / 2f - num7 / 2f + num;
			object obj5;
			if (rules.TryGetValue("valign", out obj5))
			{
				string text = (string)obj5;
				if (text != null)
				{
					if (!(text == "top"))
					{
						if (text == "bottom")
						{
							num9 = (float)Screen.height - num8 - num4;
						}
					}
					else
					{
						num9 = num3;
					}
				}
			}
			object obj6;
			if (rules.TryGetValue("halign", out obj6))
			{
				string text2 = (string)obj6;
				if (text2 != null)
				{
					if (!(text2 == "left"))
					{
						if (text2 == "right")
						{
							num10 = (float)Screen.width - num7 - num2;
						}
					}
					else
					{
						num10 = num;
					}
				}
			}
			return new Rect(num10, num9, num7, num8);
		}

		private float GetConstraintPixels(string constraint, float edge)
		{
			float num = 0f;
			Regex regex = new Regex("(\\d+)(px|%)", RegexOptions.IgnoreCase);
			Match match = regex.Match(constraint);
			if (match != null && match.Success)
			{
				GroupCollection groups = match.Groups;
				if (float.TryParse(groups[1].Value, out num))
				{
					if (groups[2].Value == "%")
					{
						return edge * num / 100f;
					}
					return num;
				}
			}
			return num;
		}

		private Texture _texture;

		private Rect _position;

		private float _scale;
	}
}
