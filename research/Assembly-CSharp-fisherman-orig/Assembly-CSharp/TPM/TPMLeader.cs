using System;
using System.IO;

namespace TPM
{
	[Serializable]
	public class TPMLeader : ILeader
	{
		public TPMLeader(ILeader leader)
		{
			this._itemId = leader.ItemId;
			this._color = leader.Color;
		}

		public TPMLeader(int itemId, string color)
		{
			this._itemId = itemId;
			this._color = color;
		}

		public int ItemId
		{
			get
			{
				return this._itemId;
			}
		}

		public string Color
		{
			get
			{
				return this._color;
			}
		}

		public void ReplaceData(ILeader leader)
		{
			this._itemId = leader.ItemId;
			this._color = leader.Color;
		}

		public static void WriteToStream(Stream stream, TPMLeader leader)
		{
			Serializer.WriteBool(stream, leader != null);
			if (leader != null)
			{
				Serializer.WriteInt(stream, leader._itemId);
				Serializer.WriteInt(stream, TPMLine.LineColorToInt(leader.Color));
			}
		}

		public static TPMLeader ReadFromStream(Stream stream)
		{
			if (Serializer.ReadBool(stream))
			{
				int num = Serializer.ReadInt(stream);
				string text = TPMLine.LineColorFromInt(Serializer.ReadInt(stream));
				return new TPMLeader(num, text);
			}
			return null;
		}

		private int _itemId;

		private string _color;
	}
}
