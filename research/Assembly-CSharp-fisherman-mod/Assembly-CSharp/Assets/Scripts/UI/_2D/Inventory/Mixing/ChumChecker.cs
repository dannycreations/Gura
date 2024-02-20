using System;
using System.Linq;
using ObjectModel;

namespace Assets.Scripts.UI._2D.Inventory.Mixing
{
	public class ChumChecker
	{
		public static bool IsOk(Types type, float prc, Chum chum, int skippedItemId)
		{
			if (type != Types.Base)
			{
				return (type != Types.Aroma && type != Types.Particle) || ChumChecker.GetPrc(type, chum, skippedItemId) < (double)prc;
			}
			return ChumChecker.GetPrc(type, chum, -1) >= (double)prc;
		}

		public static double GetPrc(Types type, Chum chum, int skippedItemId)
		{
			if (chum.Ingredients == null)
			{
				return 0.0;
			}
			if (type == Types.Base)
			{
				return (double)chum.PercentageBase;
			}
			if (type == Types.Aroma)
			{
				double num;
				if (skippedItemId == -1)
				{
					num = (double)chum.PercentageAroma;
				}
				else
				{
					num = (double)chum.ChumAroma.Where((ChumAroma p) => p.ItemId != skippedItemId).ToList<ChumAroma>().SumFloat((ChumAroma i) => i.Percentage);
				}
				return num;
			}
			if (type != Types.Particle)
			{
				return 0.0;
			}
			double num2;
			if (skippedItemId == -1)
			{
				num2 = (double)chum.PercentageParticle;
			}
			else
			{
				num2 = (double)chum.ChumParticle.Where((ChumParticle p) => p.ItemId != skippedItemId).ToList<ChumParticle>().SumFloat((ChumParticle i) => i.Percentage);
			}
			return num2;
		}
	}
}
