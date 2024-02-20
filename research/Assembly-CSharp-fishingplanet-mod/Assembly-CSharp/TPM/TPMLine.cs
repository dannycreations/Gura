using System;
using System.Globalization;
using System.IO;

namespace TPM
{
	[Serializable]
	public struct TPMLine : ILine
	{
		public int ItemId
		{
			get
			{
				return this._itemId;
			}
		}

		public string Color { get; private set; }

		public string Asset
		{
			get
			{
				return "Lines/DefaultLine/pOtherPlayersLine";
			}
		}

		public string LineOnSpoolAsset
		{
			get
			{
				return "Lines/BobbinLines/Default/Prefab/Bobbin_line";
			}
		}

		public string LineOnBaitcastSpoolAsset
		{
			get
			{
				return "Lines/BobbinLines/Default/Prefab/Bobbin_batecast_line";
			}
		}

		public void ReplaceData(ILine line)
		{
			this._itemId = line.ItemId;
		}

		public void WriteToStream(Stream stream)
		{
			Serializer.WriteInt(stream, this._itemId);
			Serializer.WriteInt(stream, TPMLine.LineColorToInt(this.Color));
		}

		public void ReadFromStream(Stream stream)
		{
			this._itemId = Serializer.ReadInt(stream);
			this.Color = TPMLine.LineColorFromInt(Serializer.ReadInt(stream));
		}

		public static int LineColorToInt(string color)
		{
			if (color != null && color.Length == 7 && color[0] == '#')
			{
				return int.Parse(color.Replace("#", string.Empty), NumberStyles.HexNumber);
			}
			return 0;
		}

		public static string LineColorFromInt(int color)
		{
			if (color != 0)
			{
				return "#" + color.ToString("X6");
			}
			return null;
		}

		private int _itemId;
	}
}
