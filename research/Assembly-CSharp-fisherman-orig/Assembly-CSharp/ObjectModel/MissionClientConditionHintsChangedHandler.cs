using System;
using System.Collections.Generic;

namespace ObjectModel
{
	public delegate void MissionClientConditionHintsChangedHandler(IMissionClientCondition sender, List<HintMessage> newMessages);
}
