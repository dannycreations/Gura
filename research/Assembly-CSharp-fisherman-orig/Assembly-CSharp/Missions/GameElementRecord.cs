using System;
using ObjectModel;

namespace Missions
{
	public class GameElementRecord
	{
		public GameElementRecord(GameElementType type, string value, int? itemId = null)
		{
			this._type = type;
			this._value = value;
			this._itemId = itemId;
		}

		public GameElementType Type
		{
			get
			{
				return this._type;
			}
		}

		public string Value
		{
			get
			{
				return this._value;
			}
		}

		public int? ItemId
		{
			get
			{
				return this._itemId;
			}
		}

		private GameElementType _type;

		private string _value;

		private int? _itemId;
	}
}
