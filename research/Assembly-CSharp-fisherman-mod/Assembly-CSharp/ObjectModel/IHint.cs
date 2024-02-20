using System;
using System.Collections.Generic;

namespace ObjectModel
{
	public interface IHint
	{
		IEnumerable<HintMessage> Check(MissionsContext context);
	}
}
