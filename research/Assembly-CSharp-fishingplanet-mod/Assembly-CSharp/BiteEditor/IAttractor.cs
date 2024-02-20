using System;
using ObjectModel;

namespace BiteEditor
{
	public interface IAttractor
	{
		Vector2f Position2D { get; }

		float AttractionR { get; }

		float RandomR { get; }

		float AttractionMinValue { get; }

		float AttractionMaxValue { get; }

		int InitialPhase { get; }

		int FillTime { get; }

		int MaxPowerDuration { get; }

		int MaxPowerRandomDuration { get; }

		int SwitchDelay { get; }

		ushort[] LinkedZones { get; }
	}
}
