using System;
using System.IO;
using ObjectModel;

namespace TPM
{
	[Serializable]
	public class TPMQuiverTip : IQuiverTip
	{
		public TPMQuiverTip(IQuiverTip quiver)
		{
			this._itemId = quiver.ItemId;
			this._color = quiver.Color;
		}

		public TPMQuiverTip(int itemId, QuiverTipColor color)
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

		public QuiverTipColor Color
		{
			get
			{
				return this._color;
			}
		}

		public void ReplaceData(IQuiverTip quiver)
		{
			this._itemId = quiver.ItemId;
			this._color = quiver.Color;
		}

		public static void WriteToStream(Stream stream, TPMQuiverTip quiver)
		{
			Serializer.WriteBool(stream, quiver != null);
			if (quiver != null)
			{
				Serializer.WriteInt(stream, quiver._itemId);
				Serializer.WriteInt(stream, (int)quiver._color);
			}
		}

		public static TPMQuiverTip ReadFromStream(Stream stream)
		{
			return (!Serializer.ReadBool(stream)) ? null : new TPMQuiverTip(Serializer.ReadInt(stream), (QuiverTipColor)Serializer.ReadInt(stream));
		}

		private int _itemId;

		private QuiverTipColor _color;
	}
}
