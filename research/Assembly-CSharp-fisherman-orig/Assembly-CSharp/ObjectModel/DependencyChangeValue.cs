using System;

namespace ObjectModel
{
	public class DependencyChangeValue<T> : DependencyChange, IDependencyChange where T : struct, IEquatable<T>
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
