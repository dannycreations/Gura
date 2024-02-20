using System;
using System.Collections.Generic;
using frame8.ScrollRectItemsAdapter.Util;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.MainExample
{
	[Serializable]
	public class MyParams : BaseParamsWithPrefabAndLazyData<ClientModel>
	{
		internal void InitTextures()
		{
			this.sampleAvatarsDownsized = new List<Sprite>(this.sampleAvatars.Count);
			foreach (Sprite sprite in this.sampleAvatars)
			{
				Color32[] pixels = sprite.texture.GetPixels32(Mathf.Min(2, sprite.texture.mipmapCount));
				int num = (int)Math.Sqrt((double)pixels.Length);
				Texture2D texture2D = new Texture2D(num, num, 4, false);
				texture2D.SetPixels32(pixels);
				texture2D.Apply();
				Sprite sprite2 = Sprite.Create(texture2D, new Rect(0f, 0f, (float)num, (float)num), Vector2.one * 0.5f);
				this.sampleAvatarsDownsized.Add(sprite2);
				Texture2D texture2D2 = new Texture2D(sprite.texture.width, sprite.texture.height, 4, false);
				texture2D2.SetPixels32(sprite.texture.GetPixels32());
				texture2D2.Apply();
				this.sampleAvatars[this.sampleAvatars.IndexOf(sprite)] = Sprite.Create(texture2D2, sprite.textureRect, Vector2.one * 0.5f);
			}
		}

		internal void ReleaseTextures()
		{
			foreach (Sprite sprite in this.sampleAvatars)
			{
				if (sprite)
				{
					Object.Destroy(sprite);
				}
			}
			foreach (Sprite sprite2 in this.sampleAvatarsDownsized)
			{
				if (sprite2)
				{
					Object.Destroy(sprite2);
				}
			}
		}

		public List<Sprite> sampleAvatars;

		public string[] sampleFirstNames;

		public string[] sampleLocations;

		public bool itemsAreExpandable;

		[NonSerialized]
		public List<Sprite> sampleAvatarsDownsized;
	}
}
