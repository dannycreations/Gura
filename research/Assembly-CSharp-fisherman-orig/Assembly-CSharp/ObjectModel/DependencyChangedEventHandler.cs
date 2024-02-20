using System;

namespace ObjectModel
{
	public delegate void DependencyChangedEventHandler(object sender, string dependency, IDependencyChange change);
}
