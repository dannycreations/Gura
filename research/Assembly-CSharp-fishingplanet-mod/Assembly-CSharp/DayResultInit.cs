using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayResultInit : MonoBehaviour
{
	protected virtual void Awake()
	{
		if (this.NonPremiumMoneyGold != null)
		{
			this.NonPremiumMoneyGold.SetActive(false);
		}
	}

	public virtual void Init(PeriodStats result)
	{
		this._result = result;
		this.PremiumExpValue.text = "0";
		this.PremiumMoneyValue.text = "0";
		if (PhotonConnectionFactory.Instance.Profile.HasPremium)
		{
			for (int i = 0; i < this.NonPremiumTextElements.Count; i++)
			{
				if (this.NonPremiumTextElements[i] != null)
				{
					this.NonPremiumTextElements[i].color = Color.gray;
				}
			}
			for (int j = 0; j < this.NonPremiumImageElements.Count; j++)
			{
				if (this.NonPremiumImageElements[j] != null)
				{
					this.NonPremiumImageElements[j].color = Color.gray;
				}
			}
		}
		if (!PhotonConnectionFactory.Instance.Profile.HasPremium)
		{
			for (int k = 0; k < this.PremiumTextElements.Count; k++)
			{
				if (this.PremiumTextElements[k] != null)
				{
					this.PremiumTextElements[k].color = Color.gray;
				}
			}
			for (int l = 0; l < this.PremiumImageElements.Count; l++)
			{
				if (this.PremiumImageElements[l] != null)
				{
					this.PremiumImageElements[l].color = Color.gray;
				}
			}
		}
		base.GetComponent<AlphaFade>().ShowFinished += this.DayResultInit_ShowFinished;
	}

	private void DayResultInit_ShowFinished(object sender, EventArgs e)
	{
		if (PhotonConnectionFactory.Instance.Profile.HasPremium)
		{
			base.StartCoroutine(this.SetupValue(this._result.Experience, this.PremiumExpValue));
			int? fishSilver = this._result.FishSilver;
			base.StartCoroutine(this.SetupValue((fishSilver == null) ? 0 : fishSilver.Value, this.PremiumMoneyValue));
			base.StartCoroutine(this.SetupValue((int)((double)this._result.Experience / 1.5), this.NonPremiumExpValue));
			base.StartCoroutine(this.SetupValue((this._result.FishSilver == null) ? 0 : ((int)((double)this._result.FishSilver.Value / 1.5)), this.NonPremiumMoneyValue));
		}
		if (!PhotonConnectionFactory.Instance.Profile.HasPremium)
		{
			base.StartCoroutine(this.SetupValue((int)((double)this._result.Experience * 1.5), this.PremiumExpValue));
			base.StartCoroutine(this.SetupValue((this._result.FishSilver == null) ? 0 : ((int)((double)this._result.FishSilver.Value * 1.5)), this.PremiumMoneyValue));
			base.StartCoroutine(this.SetupValue(this._result.Experience, this.NonPremiumExpValue));
			int? fishSilver2 = this._result.FishSilver;
			base.StartCoroutine(this.SetupValue((fishSilver2 == null) ? 0 : fishSilver2.Value, this.NonPremiumMoneyValue));
		}
	}

	private IEnumerator SetupValue(int value, Text text)
	{
		float maxTime = 1f;
		float currentTime = 0f;
		int currentValue = 0;
		while (value != 0)
		{
			currentTime += Time.deltaTime;
			if (currentTime > maxTime)
			{
				currentTime = maxTime;
			}
			float t = currentTime / maxTime;
			t = Mathf.Sin(t * 3.1415927f * 0.5f);
			currentValue = (int)Mathf.Lerp(0f, (float)value, t);
			text.text = ((value <= 0) ? currentValue.ToString() : ("+" + currentValue));
			if (currentValue == value)
			{
				IL_158:
				yield break;
			}
			yield return 0;
		}
		text.text = "0";
		goto IL_158;
	}

	public void Close()
	{
		AlphaFade component = base.GetComponent<AlphaFade>();
		if (component != null && !component.IsHiding)
		{
			component.HidePanel();
		}
	}

	[SerializeField]
	protected GameObject NonPremiumMoneyGold;

	[SerializeField]
	protected Text NonPremiumMoneyValueGold;

	[SerializeField]
	protected Text NonPremiumMoneyIcoGold;

	[SerializeField]
	protected GameObject PremiumContent;

	[SerializeField]
	protected RectTransform NoPremiumContent;

	[SerializeField]
	protected RectTransform Window;

	public List<Text> NonPremiumTextElements;

	public List<Text> PremiumTextElements;

	public List<Image> NonPremiumImageElements;

	public List<Image> PremiumImageElements;

	public Text NonPremiumExpValue;

	public Text NonPremiumMoneyValue;

	public Text PremiumExpValue;

	public Text PremiumMoneyValue;

	private PeriodStats _result;
}
