using System;

namespace ObjectModel
{
	public class BoatFishBox : FishCage
	{
		[JsonConfig]
		public override bool Safety
		{
			get
			{
				return false;
			}
		}
	}
}
