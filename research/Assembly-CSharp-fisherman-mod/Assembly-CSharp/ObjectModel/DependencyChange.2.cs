using System;

namespace ObjectModel
{
	public class DependencyChange<T> : DependencyChange, IDependencyChange
	{
		public T OldValue { get; set; }

		public T NewValue { get; set; }

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
				if (this.OldValue == null && this.NewValue == null)
				{
					return false;
				}
				if (this.OldValue == null || this.NewValue == null)
				{
					return true;
				}
				T newValue = this.NewValue;
				return !newValue.Equals(this.OldValue);
			}
		}

		public override string ToString()
		{
			return string.Format("DependencyChange '{0}': old = {1}, new = {2}", base.Name, this.OldValue, this.NewValue);
		}
	}
}
