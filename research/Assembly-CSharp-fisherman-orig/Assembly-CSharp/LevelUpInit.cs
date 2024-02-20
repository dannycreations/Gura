using System;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpInit : MonoBehaviour
{
	public void Init(LevelInfo level)
	{
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
		this.LevelName.text = string.Format(ScriptLocalization.Get((!level.IsLevel) ? "LockedRank" : "LockedLevel").ToUpper(), level.Level);
		this.MoneyValue.text = num2.ToString();
		this.GoldValue.text = num.ToString();
	}

	public Text LevelName;

	public Text MoneyValue;

	public Text GoldValue;
}
