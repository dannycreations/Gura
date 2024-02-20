using System;

namespace ObjectModel
{
	public class DependencyChangeTimeSpan : DependencyChangeValue<TimeSpan>
	{
		public TimeSpan Delta
		{
			get
			{
				return base.NewValue - base.OldValue;
			}
		}
	}
}
