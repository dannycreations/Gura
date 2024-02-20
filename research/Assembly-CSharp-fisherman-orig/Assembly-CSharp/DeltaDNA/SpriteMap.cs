using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeltaDNA
{
	internal class SpriteMap : MonoBehaviour
	{
		public string URL { get; private set; }

		public int Width { get; private set; }

		public int Height { get; private set; }

		public void Init(Dictionary<string, object> message)
		{
			object obj;
			object obj2;
			object obj3;
			if (message.TryGetValue("url", out obj) && message.TryGetValue("width", out obj2) && message.TryGetValue("height", out obj3))
			{
				this.URL = (string)obj;
				this.Width = (int)((long)obj2);
				this.Height = (int)((long)obj3);
			}
			else
			{
				Logger.LogError("Invalid image message format.");
			}
			object obj4;
			if (message.TryGetValue("spritemap", out obj4))
			{
				this._spriteMapDict = (Dictionary<string, object>)obj4;
			}
			else
			{
				Logger.LogError("Invalid message format, missing 'spritemap' object");
			}
		}

		public void LoadResource(Action callback)
		{
			this._spriteMap = new Texture2D(this.Width, this.Height);
			base.StartCoroutine(this.LoadResourceCoroutine(this.URL, callback));
		}

		public Texture GetSpriteMap()
		{
			return this._spriteMap;
		}

		public Texture GetBackground()
		{
			object obj;
			if (this._spriteMapDict.TryGetValue("background", out obj))
			{
				Dictionary<string, object> dictionary = obj as Dictionary<string, object>;
				object obj2;
				object obj3;
				object obj4;
				object obj5;
				if (dictionary.TryGetValue("x", out obj2) && dictionary.TryGetValue("y", out obj3) && dictionary.TryGetValue("width", out obj4) && dictionary.TryGetValue("height", out obj5))
				{
					return this.GetSubRegion((int)((long)obj2), (int)((long)obj3), (int)((long)obj4), (int)((long)obj5));
				}
			}
			else
			{
				Logger.LogError("Background not found in spritemap object.");
			}
			return null;
		}

		public List<Texture> GetButtons()
		{
			List<Texture> list = new List<Texture>();
			object obj;
			if (this._spriteMapDict.TryGetValue("buttons", out obj))
			{
				List<object> list2 = obj as List<object>;
				foreach (object obj2 in list2)
				{
					Dictionary<string, object> dictionary = obj2 as Dictionary<string, object>;
					object obj3;
					object obj4;
					object obj5;
					object obj6;
					if (dictionary.TryGetValue("x", out obj3) && dictionary.TryGetValue("y", out obj4) && dictionary.TryGetValue("width", out obj5) && dictionary.TryGetValue("height", out obj6))
					{
						list.Add(this.GetSubRegion((int)((long)obj3), (int)((long)obj4), (int)((long)obj5), (int)((long)obj6)));
					}
				}
			}
			return list;
		}

		public Texture2D GetSubRegion(int x, int y, int width, int height)
		{
			Color[] pixels = this._spriteMap.GetPixels(x, this._spriteMap.height - y - height, width, height);
			Texture2D texture2D = new Texture2D(width, height, this._spriteMap.format, false);
			texture2D.SetPixels(pixels);
			texture2D.Apply();
			return texture2D;
		}

		public Texture2D GetSubRegion(Rect rect)
		{
			return this.GetSubRegion(Mathf.FloorToInt(rect.x), Mathf.FloorToInt(rect.y), Mathf.FloorToInt(rect.width), Mathf.FloorToInt(rect.height));
		}

		private IEnumerator LoadResourceCoroutine(string url, Action callback)
		{
			WWW www = new WWW(url);
			yield return www;
			if (www.error == null)
			{
				www.LoadImageIntoTexture(this._spriteMap);
				callback();
				yield break;
			}
			Logger.LogError("Failed to load resource " + url + " " + www.error);
			yield break;
		}

		private Dictionary<string, object> _spriteMapDict;

		private Texture2D _spriteMap;
	}
}
