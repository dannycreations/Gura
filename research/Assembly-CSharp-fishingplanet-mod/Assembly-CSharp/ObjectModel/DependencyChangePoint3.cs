using System;

namespace ObjectModel
{
	public class DependencyChangePoint3 : DependencyChange, IDependencyChange
	{
		public Point3 OldValue { get; set; }

		public Point3 NewValue { get; set; }

		public float Distance { get; set; }

		object IDependencyChange.CurrentValue
		{
			get
			{
				return this.NewValue;
			}
		}

		bool IDependencyChange.IsChanged
		{
			get
			{
				return (this.OldValue != null || this.NewValue != null) && (this.OldValue == null || this.NewValue == null || this.NewValue.Distance(this.OldValue) > this.Distance);
			}
		}

		public override string ToString()
		{
			return string.Format("DependencyChange '{0}': old = {1}, new = {2}", base.Name, this.OldValue, this.NewValue);
		}
	}
}
