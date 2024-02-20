using System;
using System.Collections.Generic;

namespace BiteEditor
{
	public interface IAttractorGroup
	{
		AttractorsMovement MovementType { get; }

		byte MinActiveCount { get; }

		byte MaxActiveCount { get; }

		SimpleCurve AttractionShape { get; }

		List<IAttractor> Attractors { get; }

		IFishGroup[] FishGroups { get; }

		AttractorResultType ResultType { get; }
	}
}
