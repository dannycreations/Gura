using System;
using System.Collections.Generic;
using ObjectModel;

public static class HintSettings
{
	public static readonly Dictionary<HintArrowType3D, string> IcoDictionary = new Dictionary<HintArrowType3D, string>
	{
		{
			HintArrowType3D.Pointer,
			"\ue802"
		},
		{
			HintArrowType3D.Fish,
			"\ue634"
		},
		{
			HintArrowType3D.Ring,
			"\ue787"
		},
		{
			HintArrowType3D.Swim,
			"\ue733"
		},
		{
			HintArrowType3D.Hand,
			"\ue795"
		}
	};
}
