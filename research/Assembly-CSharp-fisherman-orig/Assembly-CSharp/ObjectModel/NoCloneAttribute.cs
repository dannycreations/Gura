using System;

namespace ObjectModel
{
	public class NoCloneAttribute : Attribute
	{
		public NoCloneAttribute(bool noClone = true)
		{
			this.NoClone = noClone;
		}

		public bool NoClone;
	}
}
