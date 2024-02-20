using System;
using System.Globalization;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DaysChange : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	public float CostValue { get; set; }

	public float CostPerDay { get; set; }

	public string Currency { get; set; }

	public int DaysValue { get; set; }

	public float CostPerDayDiscount { get; set; }

	public float CostValueDiscount { get; set; }

	private void Awake()
	{
		this.DaysValue = 1;
	}

	public void Increase()
	{
		if (this.DaysValue < 30)
		{
			this.UpdateData(this.DaysValue + 1);
		}
	}

	public void Decrease()
	{
		if (this.DaysValue > 1)
		{
			this.UpdateData(this.DaysValue - 1);
		}
		else
		{
			UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
		}
	}

	public void UpdateData(int days)
	{
		this.DaysValue = days;
		if (this.DaysValue < 1)
		{
			this.DaysValue = 1;
		}
		if (this.DaysValue > 30)
		{
			this.DaysValue = 30;
		}
		if (this._incBtn != null)
		{
			this._incBtn.interactable = this.DaysValue < 30;
		}
		if (this._descBtn != null)
		{
			this._descBtn.interactable = this.DaysValue > 1;
		}
		string text = this.DaysValue.ToString(CultureInfo.InvariantCulture);
		if (this.Days != null)
		{
			this.Days.text = text;
		}
		if (this._daysTmp != null)
		{
			string daysLocalization = DateTimeExtensions.GetDaysLocalization(this.DaysValue);
			for (int i = 0; i < this._daysTmp.Length; i++)
			{
				this._daysTmp[i].text = ((!(this._daysTmp[i].gameObject.name == "DaysCount")) ? string.Format("{0} {1}:", text, daysLocalization) : string.Format("{0} {1}", text, daysLocalization));
			}
		}
		this.CostValue = this.CostPerDay * (float)this.DaysValue;
		this.CostValueDiscount = ((this.CostPerDayDiscount <= 0f) ? this.CostValue : (this.CostPerDayDiscount * (float)this.DaysValue));
		string text2 = this.CostValueDiscount.ToString(CultureInfo.InvariantCulture);
		string text3 = this.CostValue.ToString(CultureInfo.InvariantCulture);
		if (this.Cost != null)
		{
			this.Cost.text = text3;
		}
		if (this._costTmp != null)
		{
			for (int j = 0; j < this._costTmp.Length; j++)
			{
				this._costTmp[j].text = text3;
			}
		}
		if (this._costDiscountTmp != null)
		{
			for (int k = 0; k < this._costDiscountTmp.Length; k++)
			{
				this._costDiscountTmp[k].text = text2;
			}
		}
		if (StaticUserData.IS_IN_TUTORIAL)
		{
			return;
		}
		PhotonConnectionFactory.Instance.ChangeIndicator(GameIndicatorType.TravelDays, this.DaysValue);
	}

	public void OnScroll(PointerEventData eventData)
	{
		this.DaysValue += (int)eventData.scrollDelta.y;
		this.UpdateData(this.DaysValue);
	}

	[SerializeField]
	private TextMeshProUGUI[] _daysTmp;

	[SerializeField]
	private TextMeshProUGUI[] _costTmp;

	[SerializeField]
	private TextMeshProUGUI[] _costDiscountTmp;

	[SerializeField]
	private BorderedButton _incBtn;

	[SerializeField]
	private BorderedButton _descBtn;

	public Text Days;

	public Text Cost;

	public const int MinDay = 1;

	private const int MaxDay = 30;
}
