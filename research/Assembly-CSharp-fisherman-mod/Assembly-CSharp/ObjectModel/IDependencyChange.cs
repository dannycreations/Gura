using System;

namespace ObjectModel
{
	public interface IDependencyChange
	{
		string Name { get; set; }

		object CurrentValue { get; }

		bool IsChanged { get; }
	}
}
