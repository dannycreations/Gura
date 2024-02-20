using System;

namespace ObjectModel
{
	public class Glasses : OutfitItem
	{
		[JsonConfig]
		public float DimmingMultiplier { get; set; }

		[JsonConfig]
		public float ReflectedLightMultiplier { get; set; }

		[JsonConfig]
		public float SunIntensityMultiplier { get; set; }

		[JsonConfig]
		public float ContrastMultiplier { get; set; }

		[JsonConfig]
		public string ParamsModel { get; set; }

		[JsonConfig]
		public string ParamsColor { get; set; }

		[JsonConfig]
		public string ParamsMaterial { get; set; }
	}
}
