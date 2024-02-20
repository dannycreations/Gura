using System;

namespace ObjectModel
{
	public class SounderBattery : ToolItem
	{
		[JsonConfig]
		public float Charge { get; set; }
	}
}
