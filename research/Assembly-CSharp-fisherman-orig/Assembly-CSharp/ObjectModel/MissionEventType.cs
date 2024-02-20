using System;

namespace ObjectModel
{
	public enum MissionEventType : byte
	{
		MissionStarted = 1,
		MissionArchived,
		MissionActivated,
		MissionDeactivated,
		MissionCompleted,
		MissionCancelled,
		MissionRestored,
		MissionCommand,
		MissionDisabled,
		MissionFailed,
		MissionTaskHintMessageAdded,
		MissionTaskHintMessageRemoved,
		MissionTaskStarted,
		MissionTaskCompleted,
		MissionTaskCanceled,
		MissionTaskFailed,
		MissionTaskRestored
	}
}
