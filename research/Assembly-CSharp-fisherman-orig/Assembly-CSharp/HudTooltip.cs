using System;
using ObjectModel;
using UnityEngine;

public class HudTooltip
{
	public HudTooltip()
	{
		this.Scale = Vector3.one;
		this.Side = HintSide.Top;
	}

	public string ElementId { get; set; }

	public HintSide Side { get; set; }

	public string Text { get; set; }

	public Vector3 Scale { get; set; }
}
