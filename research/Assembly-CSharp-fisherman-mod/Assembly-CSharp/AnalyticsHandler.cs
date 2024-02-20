using System;
using ObjectModel;
using ObjectModel.Common;
using UnityEngine;

public class AnalyticsHandler : MonoBehaviour
{
	internal void Start()
	{
		PhotonConnectionFactory.Instance.OnLicensePenalty += this.OnLicensePenalty;
		PhotonConnectionFactory.Instance.OnEndOfDayResult += this.OnEndOfDayResult;
		PhotonConnectionFactory.Instance.OnGotProfile += this.OnGotProfile;
		PhotonConnectionFactory.Instance.OnProductDelivered += this.OnProductDelivered;
		PhotonConnectionFactory.Instance.OnPondStayProlonged += this.OnPondStayProlonged;
		PhotonConnectionFactory.Instance.OnLevelGained += this.OnLevelGained;
		PhotonConnectionFactory.Instance.OnAchivementGained += this.OnAchivementGained;
		PhotonConnectionFactory.Instance.OnBonusGained += this.OnBonusGained;
		PhotonConnectionFactory.Instance.OnResidenceChanged += this.OnResidenceChanged;
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnLicensePenalty -= this.OnLicensePenalty;
		PhotonConnectionFactory.Instance.OnEndOfDayResult -= this.OnEndOfDayResult;
		PhotonConnectionFactory.Instance.OnGotProfile -= this.OnGotProfile;
		PhotonConnectionFactory.Instance.OnProductDelivered -= this.OnProductDelivered;
		PhotonConnectionFactory.Instance.OnPondStayProlonged -= this.OnPondStayProlonged;
		PhotonConnectionFactory.Instance.OnLevelGained -= this.OnLevelGained;
		PhotonConnectionFactory.Instance.OnAchivementGained -= this.OnAchivementGained;
		PhotonConnectionFactory.Instance.OnBonusGained -= this.OnBonusGained;
		PhotonConnectionFactory.Instance.OnResidenceChanged -= this.OnResidenceChanged;
	}

	private void OnLevelGained(LevelInfo level)
	{
		AnalyticsFacade.WriteNewLevel(level.Level);
		int num = 0;
		int num2 = 0;
		if (level.Amount1 != null && level.Amount1.Currency == "SC")
		{
			num2 = level.Amount1.Value;
		}
		if (level.Amount2 != null && level.Amount2.Currency == "SC")
		{
			num2 = level.Amount2.Value;
		}
		if (level.Amount1 != null && level.Amount1.Currency == "GC")
		{
			num = level.Amount1.Value;
		}
		if (level.Amount2 != null && level.Amount2.Currency == "GC")
		{
			num = level.Amount2.Value;
		}
		if (num2 > 0)
		{
			AnalyticsFacade.WriteEarnedSilver("Level", num2);
		}
		if (num > 0)
		{
			AnalyticsFacade.WriteEarnedGold("Level", num);
		}
	}

	private void OnGotProfile(Profile profile)
	{
		if (!PlayerPrefs.HasKey("LevelInited"))
		{
			PlayerPrefs.SetInt("LevelInited", 1);
			AnalyticsFacade.WriteNewLevel(profile.Level);
		}
	}

	private void OnProductDelivered(ProfileProduct product, int count, bool announce)
	{
		AnalyticsFacade.WriteSpentReal(product.Name, (decimal)product.Price);
	}

	private void OnAchivementGained(AchivementInfo achivInfo)
	{
		int num = 0;
		int num2 = 0;
		if (achivInfo.Amount1 != null && achivInfo.Amount1.Currency == "SC")
		{
			num2 = achivInfo.Amount1.Value;
		}
		if (achivInfo.Amount2 != null && achivInfo.Amount2.Currency == "SC")
		{
			num2 = achivInfo.Amount2.Value;
		}
		if (achivInfo.Amount1 != null && achivInfo.Amount1.Currency == "GC")
		{
			num = achivInfo.Amount1.Value;
		}
		if (achivInfo.Amount2 != null && achivInfo.Amount2.Currency == "GC")
		{
			num = achivInfo.Amount2.Value;
		}
		if (num2 > 0)
		{
			AnalyticsFacade.WriteEarnedSilver("Achivement", num2);
		}
		if (num > 0)
		{
			AnalyticsFacade.WriteEarnedGold("Achivement", num);
		}
	}

	internal void OnEndOfDayResult(PeriodStats result)
	{
		int num = 0;
		int num2 = 0;
		int num3 = num2;
		int? fishSilver = result.FishSilver;
		num2 = num3 + ((fishSilver == null) ? 0 : fishSilver.Value);
		int num4 = num;
		int? fishGold = result.FishGold;
		num = num4 + ((fishGold == null) ? 0 : fishGold.Value);
		if (num2 > 0)
		{
			AnalyticsFacade.WriteEarnedSilver("Fish", num2);
		}
		if (num > 0)
		{
			AnalyticsFacade.WriteEarnedGold("Fish", num);
		}
	}

	private void OnLicensePenalty(LicenseBreakingInfo info)
	{
		int num = 0;
		int num2 = 0;
		string currency = info.Currency;
		if (currency != null)
		{
			if (!(currency == "SC"))
			{
				if (currency == "GC")
				{
					num += info.Value;
					AnalyticsFacade.WriteSpentGold("Penalty", num, 1);
				}
			}
			else
			{
				num2 += info.Value;
				AnalyticsFacade.WriteSpentSilver("Penalty", num2, 1);
			}
		}
	}

	private void OnBonusGained(BonusInfo bonus)
	{
		int num = 0;
		int num2 = 0;
		int num3 = bonus.Days - 1;
		if (num3 >= 0 && num3 < bonus.AllDailyBonuses.Length)
		{
			Reward reward = bonus.AllDailyBonuses[num3];
			UIHelper.ParseAmount(new Amount
			{
				Currency = reward.Currency1,
				Value = ((reward.Money1 == null) ? 0 : ((int)reward.Money1.Value))
			}, ref num2, ref num);
			UIHelper.ParseAmount(new Amount
			{
				Currency = reward.Currency2,
				Value = ((reward.Money2 == null) ? 0 : ((int)reward.Money2.Value))
			}, ref num2, ref num);
		}
		if (num2 > 0)
		{
			AnalyticsFacade.WriteEarnedSilver("DailyBonus", num2);
		}
		if (num > 0)
		{
			AnalyticsFacade.WriteEarnedGold("DailyBonus", num);
		}
	}

	private void OnResidenceChanged(ChangeResidenceInfo info)
	{
		int num = 0;
		int num2 = 0;
		string currency = info.Currency;
		if (currency != null)
		{
			if (!(currency == "SC"))
			{
				if (currency == "GC")
				{
					num += info.Amount;
					AnalyticsFacade.WriteSpentGold("ResidenceChanged", num, 1);
				}
			}
			else
			{
				num2 += info.Amount;
				AnalyticsFacade.WriteSpentSilver("ResidenceChanged", num2, 1);
			}
		}
	}

	private void OnPondStayProlonged(ProlongInfo info)
	{
		int num = 0;
		int num2 = 0;
		string currency = info.Currency;
		if (currency != null)
		{
			if (!(currency == "SC"))
			{
				if (currency == "GC")
				{
					AnalyticsFacade.WriteSpentGold("DayProlonged", num, 1);
					num += (int)info.Cost;
				}
			}
			else
			{
				num2 += (int)info.Cost;
				AnalyticsFacade.WriteSpentSilver("DayProlonged", num2, 1);
			}
		}
	}
}
