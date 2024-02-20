using System;
using ObjectModel;
using UnityEngine;

public static class HudTooltips
{
	public static readonly HudTooltip[] Tooltips = new HudTooltip[]
	{
		new HudTooltip
		{
			ElementId = "HUDFishKeepnetPanelTooltip",
			Text = "HudFishKeepnetTooltip",
			Side = HintSide.Right,
			Scale = new Vector3(0.7f, 0.7f, 0.7f)
		},
		new HudTooltip
		{
			ElementId = "HudOneBandFightTooltip",
			Text = "HudBandFightTooltip",
			Scale = new Vector3(0.9f, 0.9f, 0.9f)
		},
		new HudTooltip
		{
			ElementId = "HudThreeBandTooltip",
			Text = "HudBandFightTooltip",
			Scale = new Vector3(0.9f, 0.9f, 0.9f)
		}
	};
}
