using System;
using System.Diagnostics;

namespace ObjectModel
{
	public class TestClientCondition : IMissionClientCondition
	{
		public MissionTaskTrackedOnClient Task { get; set; }

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event MissionClientConditionHintsChangedHandler HintsChanged;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event MissionClientConditionCompletedEventHandler Completed;

		public bool CheckMonitoredDependencies(string dependency)
		{
			return false;
		}

		public void Start()
		{
			DebugUtility.Missions.Trace("ClientTask START: {0}", new object[] { this.Task });
		}

		public void Update(MissionsContext context)
		{
		}

		public void ForceGenerateHints(MissionsContext context)
		{
		}

		public void Destroy()
		{
			DebugUtility.Missions.Trace("ClientTask DESTROY: {0}", new object[] { this.Task });
		}
	}
}
