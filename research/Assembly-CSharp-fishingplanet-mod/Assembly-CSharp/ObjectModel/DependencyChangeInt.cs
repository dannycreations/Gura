using System;

namespace ObjectModel
{
	public class DependencyChangeInt : DependencyChangeValue<int>
	{
		public float Delta
		{
			get
			{
				return (float)(base.NewValue - base.OldValue);
			}
		}
	}
}
