using System;

namespace ObjectModel
{
	public interface IMissionClientCondition
	{
		MissionTaskTrackedOnClient Task { get; set; }

		event MissionClientConditionHintsChangedHandler HintsChanged;

		event MissionClientConditionCompletedEventHandler Completed;

		bool CheckMonitoredDependencies(string dependency);

		void Start();

		void Update(MissionsContext context);

		void ForceGenerateHints(MissionsContext context);

		void Destroy();
	}
}
