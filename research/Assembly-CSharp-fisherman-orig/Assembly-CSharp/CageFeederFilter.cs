using System;
using System.Collections.Generic;

public class CageFeederFilter : FeederFilter
{
	internal override void Init()
	{
		base.Init();
		base.AddSingle<float>(this.CellSizes, "CellSize", "TypeFilter", true);
	}

	protected readonly Dictionary<string, float> CellSizes = new Dictionary<string, float>
	{
		{ "ClosedFeederCaption", 0.4f },
		{ "OpenedFeederCaption", 0.8f }
	};
}
