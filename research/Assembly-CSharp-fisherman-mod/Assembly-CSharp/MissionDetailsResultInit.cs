using System;
using System.Globalization;
using UnityEngine.UI;

public class MissionDetailsResultInit : DayDetailsResultInit
{
	protected override void FillFishInKeepnet()
	{
	}

	public override void InitMissionResult(PeriodStats result)
	{
		base.InitMissionResult(result);
		Text travelGoldCost = this.TravelGoldCost;
		int? travelCostGold = result.TravelCostGold;
		travelGoldCost.text = ((travelCostGold == null) ? 0 : travelCostGold.Value).ToString(CultureInfo.InvariantCulture);
		Text travelSilverCost = this.TravelSilverCost;
		int? travelCostSilver = result.TravelCostSilver;
		travelSilverCost.text = ((travelCostSilver == null) ? 0 : travelCostSilver.Value).ToString(CultureInfo.InvariantCulture);
		Text stayGoldCost = this.StayGoldCost;
		int? stayCostGold = result.StayCostGold;
		stayGoldCost.text = ((stayCostGold == null) ? 0 : stayCostGold.Value).ToString(CultureInfo.InvariantCulture);
		Text staySilverCost = this.StaySilverCost;
		int? stayCostSilver = result.StayCostSilver;
		staySilverCost.text = ((stayCostSilver == null) ? 0 : stayCostSilver.Value).ToString(CultureInfo.InvariantCulture);
		this.StayPeriod.text = result.StayPeriod.ToString(CultureInfo.InvariantCulture);
	}

	protected override void FillDayResult(PeriodStats result)
	{
		this.ExpTotal.text = result.Experience.ToString(CultureInfo.InvariantCulture);
		int num = 0;
		int? fishSilver = result.FishSilver;
		if (fishSilver != null)
		{
			num += result.FishSilver.Value;
		}
		int? silver = result.Silver;
		if (silver != null)
		{
			num += result.Silver.Value;
		}
		if (result.Penalties != null)
		{
			num -= (int)StatPenalty.GetSumOfPenalty(result.Penalties, "SC");
		}
		int? stayCostSilver = result.StayCostSilver;
		if (stayCostSilver != null)
		{
			num -= result.StayCostSilver.Value * result.StayPeriod;
		}
		int? travelCostSilver = result.TravelCostSilver;
		if (travelCostSilver != null)
		{
			num -= result.TravelCostSilver.Value;
		}
		int num2 = 0;
		int? fishGold = result.FishGold;
		if (fishGold != null)
		{
			num2 += result.FishGold.Value;
		}
		int? gold = result.Gold;
		if (gold != null)
		{
			num2 += result.Gold.Value;
		}
		if (result.Penalties != null)
		{
			num2 -= (int)StatPenalty.GetSumOfPenalty(result.Penalties, "GC");
		}
		int? stayCostGold = result.StayCostGold;
		if (stayCostGold != null)
		{
			num2 -= result.StayCostGold.Value * result.StayPeriod;
		}
		int? travelCostGold = result.TravelCostGold;
		if (travelCostGold != null)
		{
			num2 -= result.TravelCostGold.Value;
		}
		this.SilversTotal.text = num.ToString(CultureInfo.InvariantCulture);
		this.GoldsTotal.text = num2.ToString(CultureInfo.InvariantCulture);
	}

	public Text TravelGoldCost;

	public Text TravelSilverCost;

	public Text StayGoldCost;

	public Text StaySilverCost;

	public Text StayPeriod;
}
