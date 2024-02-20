using System;
using System.IO;
using Newtonsoft.Json;
using ObjectModel;

namespace BiteEditor.ObjectModel
{
	public class BiteMapPatch
	{
		public BiteMapPatch(string fileName, string name, Vector2f position2D, float width, float height)
		{
			this.FileName = fileName;
			this.UnityName = name;
			this.Position2D = position2D;
			this.Width = width;
			this.Height = height;
		}

		public BiteMapPatch()
		{
		}

		[JsonProperty]
		public string FileName { get; private set; }

		[JsonProperty]
		public string UnityName { get; private set; }

		[JsonProperty]
		public Vector2f Position2D { get; private set; }

		[JsonProperty]
		public float Width { get; private set; }

		[JsonProperty]
		public float Height { get; private set; }

		public void FinishInitialization(Pond pond)
		{
			string imagesFolder = Settings.GetImagesFolder(pond.Name);
			string text = string.Format("{0}{1}", imagesFolder, this.FileName);
			if (!File.Exists(text))
			{
				throw new InvalidOperationException(string.Format("Error - can't find texture {0}", text));
			}
		}

		public byte GetProbability(Vector2f pos)
		{
			Vector2f vector2f = pos - this.Position2D;
			if (float.IsNaN(pos.x) || float.IsNaN(pos.y) || Math.Abs(vector2f.x) > this.Width * 0.5f || Math.Abs(vector2f.y) > this.Height * 0.5f)
			{
				return 0;
			}
			int length = this._data.GetLength(0);
			int length2 = this._data.GetLength(1);
			int num = Math.Min((int)((vector2f.x + this.Width * 0.5f) / this.Width * (float)length2), length2 - 1);
			int num2 = Math.Min((int)((1f - (vector2f.y + this.Height * 0.5f) / this.Height) * (float)length), length - 1);
			return this._data[num2, num];
		}

		private byte[,] _data;
	}
}
