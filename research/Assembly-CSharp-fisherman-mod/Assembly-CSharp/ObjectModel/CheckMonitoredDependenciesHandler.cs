using System;

namespace ObjectModel
{
	public delegate bool CheckMonitoredDependenciesHandler(string dependency, bool affectProcessing);
}
