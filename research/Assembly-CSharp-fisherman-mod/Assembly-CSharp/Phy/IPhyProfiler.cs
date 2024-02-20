using System;

namespace Phy
{
	public interface IPhyProfiler
	{
		DebugPlotter ThreadProfiler { get; }

		void StartSegment(string name);

		void StopSegment(string name);
	}
}
