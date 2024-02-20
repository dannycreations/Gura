using System;
using UnityEngine;

namespace I2
{
	public class RenameAttribute : PropertyAttribute
	{
		public RenameAttribute(int hspace, string name, string tooltip = null)
		{
			this.Name = name;
			this.Tooltip = tooltip;
			this.HorizSpace = hspace;
		}

		public RenameAttribute(string name, string tooltip = null)
			: this(0, name, tooltip)
		{
		}

		public readonly string Name;

		public readonly string Tooltip;

		public readonly int HorizSpace;
	}
}
