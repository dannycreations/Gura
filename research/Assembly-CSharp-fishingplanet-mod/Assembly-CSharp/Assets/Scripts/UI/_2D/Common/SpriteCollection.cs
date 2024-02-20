using System;
using UnityEngine;

namespace Assets.Scripts.UI._2D.Common
{
	public class SpriteCollection
	{
		public SpriteCollection(string spritesheet)
		{
			this.sprites = Resources.LoadAll<Sprite>(spritesheet);
			this.names = new string[this.sprites.Length];
			for (int i = 0; i < this.names.Length; i++)
			{
				this.names[i] = this.sprites[i].name;
			}
		}

		public Sprite GetSprite(string name)
		{
			if (name == null || !Array.Exists<string>(this.names, (string x) => x == name) || this.sprites[Array.IndexOf<string>(this.names, name)] == null)
			{
				return this.sprites[Array.IndexOf<string>(this.names, "default")];
			}
			return this.sprites[Array.IndexOf<string>(this.names, name)];
		}

		private Sprite[] sprites;

		private string[] names;
	}
}
