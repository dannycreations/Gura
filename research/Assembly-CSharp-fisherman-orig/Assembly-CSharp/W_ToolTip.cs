using System;
using UnityEngine;

public class W_ToolTip : PropertyAttribute
{
	public W_ToolTip(string tooltip)
	{
		this.tooltip = tooltip;
	}

	public readonly string tooltip;
}
