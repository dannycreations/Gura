using System;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Inventory;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoatRentHandler : MessageBoxBase
{
	private float DiscountMultiplier
	{
		get
		{
			return (!PhotonConnectionFactory.Instance.Profile.HasPremium) ? 1f : 0.5f;
		}
	}

	private void Update()
	{
		if (HudTournamentHandler.IsWarningOfEnd)
		{
			this.CloseWindow();
			return;
		}
		this.UpdateMaxDays();
		this.SetDays();
		this.AddDay.interactable = this.daysRequested < this.maxAllowedDays;
		this.SubDay.interactable = this.daysRequested > 1;
	}

	private void SetPrice()
	{
		this.Price.text = string.Format("{0} {1}", MeasuringSystemManager.GetCurrencyIcon(this.prices.Currency), (this.DiscountMultiplier * (float)this.prices.PricePerDay * (float)this.daysRequested).ToString());
	}

	private void UpdateMaxDays()
	{
		if (TimeAndWeatherManager.CurrentTime != null)
		{
			TimeSpan value = TimeAndWeatherManager.CurrentTime.Value;
			DateTime dateTime = new DateTime(2000, 1, 1, 0, 0, 0);
			dateTime = dateTime.Add(value);
			this.maxAllowedDays = PhotonConnectionFactory.Instance.Profile.PondStayTime.Value - (value.Days - ((value.Hours >= PhotonConnectionFactory.Instance.PondDayStart) ? 0 : 1));
		}
	}

	public void Init(BoatDesc boatForRent, Action<BoatDesc, int> rentABoat, Action<BoatDesc> onRentNotExtended, bool extend)
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		this._boatDescription = boatForRent;
		this.onBoatRent = rentABoat;
		this.onRentExpired = onRentNotExtended;
		if (extend)
		{
			for (int i = 0; i < this.rentObjects.Length; i++)
			{
				this.rentObjects[i].SetActive(false);
			}
		}
		else
		{
			for (int j = 0; j < this.extendRentObjects.Length; j++)
			{
				this.extendRentObjects[j].SetActive(false);
			}
		}
		this.UpdateMaxDays();
		this.prices = boatForRent.Prices.FirstOrDefault((BoatPriceDesc x) => x.PondId == StaticUserData.CurrentPond.PondId);
		this.Name.text = boatForRent.Name;
		this.Description.text = InventoryParamsHelper.SplitWithCaptionOutlineForBoats(string.Format(boatForRent.Params, this.DiscountMultiplier * (float)this.prices.PricePerDay), ScriptLocalization.Get("SpecificationsCaption"));
		this.daysRequested = 1;
		this.SetPrice();
		this.SubDay.interactable = false;
		if (this.maxAllowedDays == 1)
		{
			this.AddDay.interactable = false;
		}
		if (boatForRent.ThumbnailBID != 0)
		{
			this.ThumbnailLdbl.Image = this.Thumbnail;
			this.ThumbnailLdbl.Load(string.Format("Textures/Inventory/{0}", boatForRent.ThumbnailBID));
		}
		this.SetDayText();
		ProfileTournament tournament = profile.Tournament;
		if (tournament != null && !tournament.IsEnded && tournament.IsStarted && (tournament.PrimaryScoring.FishOrigin == TournamentFishOrigin.Boat || (tournament.EquipmentAllowed.BoatTypes != null && tournament.EquipmentAllowed.BoatTypes.Length != 0)))
		{
			this.QuantityController.SetActive(false);
			this.RentCompetition.gameObject.SetActive(true);
			this.RentCompetition.text = string.Format("{0} <b>{1}</b>", ScriptLocalization.Get("RentFor"), tournament.Name);
			Text price = this.Price;
			string text = "{0} <b>{1}</b>";
			object currencyIcon = MeasuringSystemManager.GetCurrencyIcon(this.prices.Currency);
			int? pricePerHour = this.prices.PricePerHour;
			float? num = ((pricePerHour == null) ? null : new float?((float)pricePerHour.Value));
			float? num2 = ((num == null) ? null : new float?(this.DiscountMultiplier * num.GetValueOrDefault()));
			price.text = string.Format(text, currencyIcon, ((num2 == null) ? null : new float?(num2.GetValueOrDefault() * (float)tournament.InGameDuration)).ToString());
		}
	}

	public void AddDayPressed()
	{
		this.daysRequested++;
		this.daysRequested = Mathf.Clamp(this.daysRequested, 0, this.maxAllowedDays);
		this.SetDayText();
		this.SetPrice();
	}

	public void SubDayPressed()
	{
		this.daysRequested--;
		this.daysRequested = Mathf.Clamp(this.daysRequested, 0, this.maxAllowedDays);
		this.SetDayText();
		this.SetPrice();
	}

	public void RentABoat()
	{
		if (this.prices.Currency == "GC" && PhotonConnectionFactory.Instance.Profile.GoldCoins < (double)(this.DiscountMultiplier * (float)this.prices.PricePerDay * (float)this.daysRequested))
		{
			this.ShowMessage(ScriptLocalization.Get("HaventMoney"));
			return;
		}
		if (this.prices.Currency == "SC" && PhotonConnectionFactory.Instance.Profile.SilverCoins < (double)(this.DiscountMultiplier * (float)this.prices.PricePerDay * (float)this.daysRequested))
		{
			this.ShowMessage(ScriptLocalization.Get("TravelNotEnoughMoney"));
			return;
		}
		if (this.onBoatRent != null && !HudTournamentHandler.IsWarningOfEnd)
		{
			base.GetComponent<AlphaFade>().CanvasGroup.interactable = false;
			this.onBoatRent(this._boatDescription, this.daysRequested);
		}
		this.CloseWindow();
	}

	private void SetDayText()
	{
		this.DaysText.text = ((this.daysRequested != 1) ? ScriptLocalization.Get("DaysCountMulti") : ScriptLocalization.Get("DaysCount"));
	}

	private void SetDays()
	{
		this.Days.text = string.Format("{0}/{1}", this.daysRequested, this.maxAllowedDays);
	}

	private void ShowMessage(string v)
	{
		this.messageBox = this.helpers.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), v, true, false, false, null);
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
		this.messageBox.GetComponent<EventAction>().ActionCalled += this.CompleteMessage_ActionCalled;
	}

	private void CompleteMessage_ActionCalled(object sender, EventArgs e)
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
	}

	private void CloseWindow()
	{
		if (!base.GetComponent<AlphaFade>().IsHiding)
		{
			base.GetComponent<AlphaFade>().HidePanel();
		}
	}

	public override void Close()
	{
		if (this.onRentExpired != null)
		{
			this.onRentExpired(this._boatDescription);
		}
		base.Close();
	}

	[SerializeField]
	private GameObject _rentOtherBoatError;

	[SerializeField]
	private Image Thumbnail;

	private ResourcesHelpers.AsyncLoadableImage ThumbnailLdbl = new ResourcesHelpers.AsyncLoadableImage();

	[SerializeField]
	private TextMeshProUGUI Description;

	[SerializeField]
	private Text Name;

	[SerializeField]
	private Text Price;

	[SerializeField]
	private Text Days;

	[SerializeField]
	private Text RentCompetition;

	[SerializeField]
	private Text DaysText;

	[SerializeField]
	private Button AddDay;

	[SerializeField]
	private Button SubDay;

	[SerializeField]
	private GameObject[] rentObjects;

	[SerializeField]
	private GameObject[] extendRentObjects;

	[SerializeField]
	private GameObject QuantityController;

	private int maxAllowedDays;

	private int daysRequested = 1;

	private Action<BoatDesc, int> onBoatRent;

	private Action<BoatDesc> onRentExpired;

	private BoatPriceDesc prices;

	private MessageBox messageBox;

	private MenuHelpers helpers = new MenuHelpers();

	private BoatDesc _boatDescription;
}
