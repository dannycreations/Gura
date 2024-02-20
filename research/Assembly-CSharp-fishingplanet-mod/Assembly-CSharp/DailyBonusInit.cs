using System;
using DG.Tweening;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyBonusInit : MonoBehaviour
{
	private void Awake()
	{
		this._alphaFade.ShowFinished += this._alphaFade_ShowFinished;
	}

	private void OnDestroy()
	{
		this._alphaFade.ShowFinished -= this._alphaFade_ShowFinished;
	}

	public void Init(BonusInfo bonus)
	{
		if (!this._isInited)
		{
			this._bonusInfo = bonus;
			return;
		}
		for (int i = 0; i < bonus.AllDailyBonuses.Length; i++)
		{
			string text;
			string text2;
			this.GetDayValue(bonus.AllDailyBonuses[i], out text, out text2);
			Text text3 = this._moneyTextsPrev[i];
			string text4 = text2;
			this._moneyTextsNext[i].text = text4;
			text4 = text4;
			this._moneyTextsCurr[i].text = text4;
			text3.text = text4;
			Text text5 = this._valueTextsPrev[i];
			text4 = text;
			this._valueTextsNext[i].text = text4;
			text4 = text4;
			this._valueTextsCurr[i].text = text4;
			text5.text = text4;
		}
		int num = bonus.Days - 1;
		Sequence sequence = DOTween.Sequence();
		for (int j = 0; j < this._groupsPrev.Length; j++)
		{
			CanvasGroup canvasGroup = this._groupsNext[j];
			if (j == bonus.Days)
			{
				canvasGroup = this._groupsCurr[j];
			}
			else if (j <= num)
			{
				canvasGroup = this._groupsPrev[j];
			}
			TweenSettingsExtensions.Append(sequence, ShortcutExtensions.DOFade(canvasGroup, 1f, 0.1f));
		}
		TweenExtensions.Play<Sequence>(sequence);
	}

	private void GetDayValue(Reward dayReward, out string moneyCount, out string moneySuffix)
	{
		string empty;
		moneySuffix = (empty = string.Empty);
		moneyCount = empty;
		if (dayReward.Money1 != null)
		{
			moneyCount = dayReward.Money1.Value.ToString("F0");
			moneySuffix = "\ue62b";
		}
		if (dayReward.Money2 != null)
		{
			moneyCount = dayReward.Money2.Value.ToString("F0");
			moneySuffix = "\ue62c";
		}
	}

	private void _alphaFade_ShowFinished(object sender, EventArgs e)
	{
		this.OnDestroy();
		this._isInited = true;
		if (this._bonusInfo != null)
		{
			this.Init(this._bonusInfo);
		}
	}

	[SerializeField]
	private AlphaFade _alphaFade;

	[SerializeField]
	private CanvasGroup[] _groupsPrev;

	[SerializeField]
	private CanvasGroup[] _groupsCurr;

	[SerializeField]
	private CanvasGroup[] _groupsNext;

	[SerializeField]
	private Text[] _moneyTextsPrev;

	[SerializeField]
	private Text[] _valueTextsPrev;

	[SerializeField]
	private TextMeshProUGUI[] _moneyTextsCurr;

	[SerializeField]
	private TextMeshProUGUI[] _valueTextsCurr;

	[SerializeField]
	private TextMeshProUGUI[] _moneyTextsNext;

	[SerializeField]
	private TextMeshProUGUI[] _valueTextsNext;

	private bool _isInited;

	private BonusInfo _bonusInfo;
}
