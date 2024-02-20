using System;

namespace FriendsSRIA.Models
{
	public abstract class BaseModel
	{
		public BaseModel()
		{
			this.CachedType = base.GetType();
		}

		public Type CachedType { get; private set; }

		public int id;

		public BaseModel.FriendModelType type;

		public enum FriendModelType
		{
			Normal,
			Request,
			Found,
			Ignored
		}
	}
}
