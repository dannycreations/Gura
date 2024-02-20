using System;

namespace ObjectModel
{
	public delegate void MissionClientConditionCompletedEventHandler(IMissionClientCondition sender, bool completed, string progress);
}
