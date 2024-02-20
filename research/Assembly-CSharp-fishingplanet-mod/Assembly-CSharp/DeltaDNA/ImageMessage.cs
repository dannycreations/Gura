using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DeltaDNA
{
	public class ImageMessage
	{
		private ImageMessage(Dictionary<string, object> configuration, string name, int depth)
		{
			GameObject gameObject = new GameObject(name);
			ImageMessage.SpriteMap spriteMap = gameObject.AddComponent<ImageMessage.SpriteMap>();
			spriteMap.Build(configuration);
			this.configuration = configuration;
			this.gameObject = gameObject;
			this.spriteMap = spriteMap;
			this.depth = depth;
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnDidReceiveResources;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<string> OnDidFailToReceiveResources;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<ImageMessage.EventArgs> OnDismiss;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<ImageMessage.EventArgs> OnAction;

		public static ImageMessage Create(Engagement engagement)
		{
			return ImageMessage.Create(engagement, null);
		}

		public static ImageMessage Create(Engagement engagement, Dictionary<string, object> options)
		{
			if (engagement == null || engagement.JSON == null || !engagement.JSON.ContainsKey("image"))
			{
				return null;
			}
			string text = "Image Message";
			int num = 0;
			if (options != null)
			{
				if (options.ContainsKey("name"))
				{
					text = options["name"] as string;
				}
				if (options.ContainsKey("depth"))
				{
					num = (int)options["depth"];
				}
			}
			ImageMessage imageMessage = null;
			try
			{
				Dictionary<string, object> dictionary = engagement.JSON["image"] as Dictionary<string, object>;
				if (ImageMessage.ValidConfiguration(dictionary))
				{
					imageMessage = new ImageMessage(dictionary, text, num);
					if (engagement.JSON.ContainsKey("parameters"))
					{
						imageMessage.Parameters = engagement.JSON["parameters"] as Dictionary<string, object>;
					}
				}
				else
				{
					Logger.LogWarning("Invalid image message configuration.");
				}
			}
			catch (Exception ex)
			{
				Logger.LogWarning("Failed to create image message: " + ex.Message);
			}
			return imageMessage;
		}

		private static bool ValidConfiguration(Dictionary<string, object> c)
		{
			if (!c.ContainsKey("url") || !c.ContainsKey("height") || !c.ContainsKey("width") || !c.ContainsKey("spritemap") || !c.ContainsKey("layout"))
			{
				return false;
			}
			Dictionary<string, object> dictionary = c["layout"] as Dictionary<string, object>;
			if (!dictionary.ContainsKey("landscape") && !dictionary.ContainsKey("portrait"))
			{
				return false;
			}
			Dictionary<string, object> dictionary2 = c["spritemap"] as Dictionary<string, object>;
			return dictionary2.ContainsKey("background");
		}

		public void FetchResources()
		{
			this.spriteMap.LoadResource(delegate(string error)
			{
				if (error == null)
				{
					this.resourcesLoaded = true;
					if (this.OnDidReceiveResources != null)
					{
						this.OnDidReceiveResources();
					}
				}
				else if (this.OnDidFailToReceiveResources != null)
				{
					this.OnDidFailToReceiveResources(error);
				}
			});
		}

		public bool IsReady()
		{
			return this.resourcesLoaded;
		}

		public void Show()
		{
			if (this.resourcesLoaded)
			{
				try
				{
					if (this.configuration.ContainsKey("shim"))
					{
						ImageMessage.ShimLayer shimLayer = this.gameObject.AddComponent<ImageMessage.ShimLayer>();
						shimLayer.Build(this, this.configuration["shim"] as Dictionary<string, object>, this.depth);
					}
					Dictionary<string, object> dictionary = this.configuration["layout"] as Dictionary<string, object>;
					object obj;
					if (!dictionary.TryGetValue("landscape", out obj) && !dictionary.TryGetValue("portrait", out obj))
					{
						throw new KeyNotFoundException("Layout missing orientation key.");
					}
					ImageMessage.BackgroundLayer backgroundLayer = this.gameObject.AddComponent<ImageMessage.BackgroundLayer>();
					backgroundLayer.Build(this, obj as Dictionary<string, object>, this.spriteMap.Background, this.depth - 1);
					ImageMessage.ButtonsLayer buttonsLayer = this.gameObject.AddComponent<ImageMessage.ButtonsLayer>();
					buttonsLayer.Build(this, obj as Dictionary<string, object>, this.spriteMap.Buttons, backgroundLayer, this.depth - 2);
					this.showing = true;
				}
				catch (KeyNotFoundException ex)
				{
					Logger.LogWarning("Failed to show image message, invalid format: " + ex.Message);
				}
				catch (Exception ex2)
				{
					Logger.LogWarning("Failed to show image message: " + ex2.Message);
				}
			}
		}

		public bool IsShowing()
		{
			return this.showing;
		}

		public void Close()
		{
			if (this.showing)
			{
				foreach (ImageMessage.Layer layer in this.gameObject.GetComponents<ImageMessage.Layer>())
				{
					Object.Destroy(layer);
				}
				this.showing = false;
			}
		}

		public Dictionary<string, object> Parameters { get; private set; }

		private Dictionary<string, object> configuration;

		private GameObject gameObject;

		private ImageMessage.SpriteMap spriteMap;

		private int depth;

		private bool resourcesLoaded;

		private bool showing;

		public class EventArgs : global::System.EventArgs
		{
			public EventArgs(string id, string type, string value)
			{
				this.ID = id;
				this.ActionType = type;
				this.ActionValue = value;
			}

			public string ID { get; set; }

			public string ActionType { get; set; }

			public string ActionValue { get; set; }
		}

		private class SpriteMap : MonoBehaviour
		{
			public string URL { get; private set; }

			public int Width { get; private set; }

			public int Height { get; private set; }

			public void Build(Dictionary<string, object> configuration)
			{
				try
				{
					this.URL = configuration["url"] as string;
					this.Width = (int)((long)configuration["width"]);
					this.Height = (int)((long)configuration["height"]);
					this.configuration = configuration["spritemap"] as Dictionary<string, object>;
				}
				catch (KeyNotFoundException ex)
				{
					Logger.LogError("Invalid format: " + ex.Message);
				}
			}

			public void LoadResource(Action<string> callback)
			{
				this.texture = new Texture2D(this.Width, this.Height);
				base.StartCoroutine(this.LoadResourceCoroutine(this.URL, callback));
			}

			public Texture Texture
			{
				get
				{
					return this.texture;
				}
			}

			public Texture Background
			{
				get
				{
					try
					{
						Dictionary<string, object> dictionary = this.configuration["background"] as Dictionary<string, object>;
						int num = (int)((long)dictionary["x"]);
						int num2 = (int)((long)dictionary["y"]);
						int num3 = (int)((long)dictionary["width"]);
						int num4 = (int)((long)dictionary["height"]);
						return this.GetSubRegion(num, num2, num3, num4);
					}
					catch (KeyNotFoundException ex)
					{
						Logger.LogError("Invalid format, background not found: " + ex.Message);
					}
					return null;
				}
			}

			public List<Texture> Buttons
			{
				get
				{
					List<Texture> list = new List<Texture>();
					if (this.configuration.ContainsKey("buttons"))
					{
						try
						{
							List<object> list2 = this.configuration["buttons"] as List<object>;
							foreach (object obj in list2)
							{
								int num = (int)((long)((Dictionary<string, object>)obj)["x"]);
								int num2 = (int)((long)((Dictionary<string, object>)obj)["y"]);
								int num3 = (int)((long)((Dictionary<string, object>)obj)["width"]);
								int num4 = (int)((long)((Dictionary<string, object>)obj)["height"]);
								list.Add(this.GetSubRegion(num, num2, num3, num4));
							}
						}
						catch (KeyNotFoundException ex)
						{
							Logger.LogError("Invalid format, button not found: " + ex.Message);
						}
					}
					return list;
				}
			}

			public Texture2D GetSubRegion(int x, int y, int width, int height)
			{
				Color[] pixels = this.texture.GetPixels(x, this.texture.height - y - height, width, height);
				Texture2D texture2D = new Texture2D(width, height, this.texture.format, false);
				texture2D.SetPixels(pixels);
				texture2D.Apply();
				return texture2D;
			}

			public Texture2D GetSubRegion(Rect rect)
			{
				return this.GetSubRegion(Mathf.FloorToInt(rect.x), Mathf.FloorToInt(rect.y), Mathf.FloorToInt(rect.width), Mathf.FloorToInt(rect.height));
			}

			private IEnumerator LoadResourceCoroutine(string url, Action<string> callback)
			{
				WWW www = new WWW(url);
				yield return www;
				if (www.error == null)
				{
					www.LoadImageIntoTexture(this.texture);
				}
				else
				{
					Logger.LogWarning("Failed to load resource " + url + " " + www.error);
				}
				callback(www.error);
				yield break;
			}

			private Dictionary<string, object> configuration;

			private Texture2D texture;
		}

		private class Layer : MonoBehaviour
		{
			protected void RegisterAction()
			{
				this.actions.Add(delegate
				{
				});
			}

			protected void RegisterAction(Dictionary<string, object> action, string id)
			{
				object valueObj;
				action.TryGetValue("value", out valueObj);
				object obj;
				if (action.TryGetValue("type", out obj))
				{
					ImageMessage.EventArgs eventArgs = new ImageMessage.EventArgs(id, (string)obj, (string)valueObj);
					string text = (string)obj;
					if (text != null)
					{
						if (text == "none")
						{
							this.actions.Add(delegate
							{
							});
							return;
						}
						if (text == "action")
						{
							this.actions.Add(delegate
							{
								if (valueObj != null && this.imageMessage.OnAction != null)
								{
									this.imageMessage.OnAction(eventArgs);
								}
								this.imageMessage.Close();
							});
							return;
						}
						if (text == "link")
						{
							this.actions.Add(delegate
							{
								if (this.imageMessage.OnAction != null)
								{
									this.imageMessage.OnAction(eventArgs);
								}
								if (valueObj != null)
								{
									Application.OpenURL((string)valueObj);
								}
								this.imageMessage.Close();
							});
							return;
						}
					}
					this.actions.Add(delegate
					{
						if (this.imageMessage.OnDismiss != null)
						{
							this.imageMessage.OnDismiss(eventArgs);
						}
						this.imageMessage.Close();
					});
				}
			}

			protected ImageMessage imageMessage;

			protected List<Action> actions = new List<Action>();

			protected int depth;
		}

		private class ShimLayer : ImageMessage.Layer
		{
			public void Build(ImageMessage imageMessage, Dictionary<string, object> config, int depth)
			{
				this.imageMessage = imageMessage;
				this.depth = depth;
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
							array[0] = new Color32(0, 0, 0, this.dimmedMaskAlpha);
							goto IL_9B;
						}
						if (text == "clear")
						{
							array[0] = new Color32(0, 0, 0, 0);
							goto IL_9B;
						}
					}
					flag = false;
					IL_9B:
					if (flag)
					{
						this.texture = new Texture2D(1, 1);
						this.texture.SetPixels32(array);
						this.texture.Apply();
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
				GUI.depth = this.depth;
				if (this.texture)
				{
					Rect rect;
					rect..ctor(0f, 0f, (float)Screen.width, (float)Screen.height);
					GUI.DrawTexture(rect, this.texture);
					if (GUI.Button(rect, string.Empty, GUIStyle.none) && this.actions.Count > 0)
					{
						this.actions[0]();
					}
				}
			}

			private Texture2D texture;

			private readonly byte dimmedMaskAlpha = 128;
		}

		private class BackgroundLayer : ImageMessage.Layer
		{
			public void Build(ImageMessage imageMessage, Dictionary<string, object> layout, Texture texture, int depth)
			{
				this.imageMessage = imageMessage;
				this.texture = texture;
				this.depth = depth;
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
						this.position = this.RenderAsCover((Dictionary<string, object>)obj3);
					}
					else if (dictionary.TryGetValue("contain", out obj3))
					{
						this.position = this.RenderAsContain((Dictionary<string, object>)obj3);
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
					return this.position;
				}
			}

			public float Scale
			{
				get
				{
					return this.scale;
				}
			}

			public void OnGUI()
			{
				GUI.depth = this.depth;
				if (this.texture)
				{
					GUI.DrawTexture(this.position, this.texture);
					if (GUI.Button(this.position, string.Empty, GUIStyle.none) && this.actions.Count > 0)
					{
						this.actions[0]();
					}
				}
			}

			private Rect RenderAsCover(Dictionary<string, object> rules)
			{
				this.scale = Math.Max((float)Screen.width / (float)this.texture.width, (float)Screen.height / (float)this.texture.height);
				float num = (float)this.texture.width * this.scale;
				float num2 = (float)this.texture.height * this.scale;
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
				float num5 = ((float)Screen.width - num - num2) / (float)this.texture.width;
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
				float num6 = ((float)Screen.height - num3 - num4) / (float)this.texture.height;
				this.scale = Math.Min(num5, num6);
				float num7 = (float)this.texture.width * this.scale;
				float num8 = (float)this.texture.height * this.scale;
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

			private Texture texture;

			private Rect position;

			private float scale;
		}

		private class ButtonsLayer : ImageMessage.Layer
		{
			public void Build(ImageMessage imageMessage, Dictionary<string, object> orientation, List<Texture> textures, ImageMessage.BackgroundLayer content, int depth)
			{
				this.imageMessage = imageMessage;
				this.depth = depth;
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
						this.positions.Add(new Rect(num, num2, (float)textures[i].width * content.Scale, (float)textures[i].height * content.Scale));
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
					this.textures = textures;
				}
			}

			public void OnGUI()
			{
				GUI.depth = this.depth;
				for (int i = 0; i < this.textures.Count; i++)
				{
					GUI.DrawTexture(this.positions[i], this.textures[i]);
					if (GUI.Button(this.positions[i], string.Empty, GUIStyle.none))
					{
						this.actions[i]();
					}
				}
			}

			private List<Texture> textures = new List<Texture>();

			private List<Rect> positions = new List<Rect>();
		}
	}
}
