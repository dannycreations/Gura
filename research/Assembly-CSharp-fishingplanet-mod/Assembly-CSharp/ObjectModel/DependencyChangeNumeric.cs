using System;

namespace ObjectModel
{
	public class DependencyChangeNumeric : DependencyChangeValue<float>
	{
		public float Delta
		{
			get
			{
				return base.NewValue - base.OldValue;
			}
		}
	}
}
