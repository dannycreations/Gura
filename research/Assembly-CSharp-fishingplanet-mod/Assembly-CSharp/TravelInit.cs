using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.GlobalMap;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TravelInit : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<Action> OnTravelToPond = delegate(Action func)
	{
	};

	private void Awake()
	{
		this._noMoneyText.text = ScriptLocalization.Get("TravelNotEnoughMoney");
	}

	public string GetPondElementId()
	{
		return (this._pond == null) ? string.Empty : this._pond.Asset;
	}

	private void OnEnable()
	{
		PhotonConnectionFactory.Instance.OnGotPondUnlocks += this.OnGotPondsUnlocks;
		PhotonConnectionFactory.Instance.OnPondUnlocked += this.OnGotPondUnlocked;
		PhotonConnectionFactory.Instance.OnCurrencyExchanged += this.OnCurrencyExchanged;
		PhotonConnectionFactory.Instance.GetPondUnlocks();
		this.SetTravelling(false);
	}

	private void OnDisable()
	{
		PhotonConnectionFactory.Instance.OnGotPondUnlocks -= this.OnGotPondsUnlocks;
		PhotonConnectionFactory.Instance.OnPondUnlocked -= this.OnGotPondUnlocked;
		PhotonConnectionFactory.Instance.OnCurrencyExchanged -= this.OnCurrencyExchanged;
	}

	private void OnCurrencyExchanged(int gold, int silver)
	{
		this.UpdateContentVisibility();
	}

	private void OnGotPondsUnlocks(IEnumerable<PondUnlockInfo> unlocks)
	{
		this._unlocksList = (unlocks as List<PondUnlockInfo>) ?? new List<PondUnlockInfo>();
		this._nextPond = (from p in this._unlocksList
			where p.CanUse
			orderby p.MinLevel descending
			select p).FirstOrDefault<PondUnlockInfo>();
	}

	public void Init(Pond pondInfo, bool forceInit = false)
	{
		if (pondInfo == null || (this._pond == pondInfo && !forceInit))
		{
			return;
		}
		this._pond = pondInfo;
		this._isDiscountTime = this.IsDiscountTime;
		this._transportPriceDiscount = ((!this._isDiscountTime || this._pond.DiscountTravelCost == null) ? 0 : ((int)this._pond.DiscountTravelCost.Value));
		string text = this._transportPriceDiscount.ToString(CultureInfo.InvariantCulture);
		if (this._transportPriceDiscountTmp != null)
		{
			for (int i = 0; i < this._transportPriceDiscountTmp.Length; i++)
			{
				this._transportPriceDiscountTmp[i].text = text;
			}
		}
		float? travelCost = this._pond.TravelCost;
		this._transportPrice = (int)((travelCost == null) ? 0f : travelCost.Value);
		string text2 = this._transportPrice.ToString(CultureInfo.InvariantCulture);
		if (this._transportPriceTmp != null)
		{
			for (int j = 0; j < this._transportPriceTmp.Length; j++)
			{
				this._transportPriceTmp[j].text = text2;
			}
		}
		if (this.TransportPrice != null)
		{
			this.TransportPrice.text = text2;
		}
		DaysChange residenceCost = this.ResidenceCost;
		float? stayFee = this._pond.StayFee;
		residenceCost.CostPerDay = (float)((int)((stayFee == null) ? 0f : stayFee.Value));
		this.ResidenceCost.CostPerDayDiscount = (float)((!this._isDiscountTime || this._pond.DiscountStayFee == null) ? 0 : ((int)this._pond.DiscountStayFee.Value));
		this.ResidenceCost.Currency = this._pond.Currency;
		this.ResidenceCost.UpdateData((!forceInit) ? 1 : this.ResidenceCost.DaysValue);
		this.UpdateTotalPrice((int)this.ResidenceCost.CostValue + this._transportPrice);
		this.UpdateContentVisibility();
		bool flag = this._isDiscountTime && this._pond.DiscountStayFee != null && this._pond.DiscountStayFee > 0f;
		for (int k = 0; k < this._dayDiscounts.Length; k++)
		{
			this._dayDiscounts[k].gameObject.SetActive(flag);
		}
		for (int l = 0; l < this._dayNormal.Length; l++)
		{
			this._dayNormal[l].gameObject.SetActive(!flag);
		}
		bool flag2 = this._isDiscountTime && this._pond.DiscountTravelCost != null && this._pond.DiscountTravelCost > 0f;
		for (int m = 0; m < this._transportDiscounts.Length; m++)
		{
			this._transportDiscounts[m].gameObject.SetActive(flag2);
		}
		for (int n = 0; n < this._transportNormal.Length; n++)
		{
			this._transportNormal[n].gameObject.SetActive(!flag2);
		}
		for (int num = 0; num < this._allDiscounts.Length; num++)
		{
			this._allDiscounts[num].gameObject.SetActive(this._isDiscountTime);
		}
		for (int num2 = 0; num2 < this._allNormal.Length; num2++)
		{
			this._allNormal[num2].gameObject.SetActive(!this._isDiscountTime);
		}
	}

	private void UpdateContentVisibility()
	{
		bool flag = (double)this.Total <= PhotonConnectionFactory.Instance.Profile.SilverCoins || this.Total == 0;
		this._total.gameObject.SetActive(flag && !this._isDiscountTime);
		this._totalDiscount.gameObject.SetActive(flag && this._isDiscountTime);
		this._totalNoMoney.gameObject.SetActive(!flag);
		this._imgNoMoney.gameObject.SetActive(this._isDiscountTime);
		this._travelText.sizeDelta = new Vector2((!this._isDiscountTime) ? 286.3f : 172.91f, this._travelText.rect.height);
	}

	public void Update()
	{
		bool isDiscountTime = this.IsDiscountTime;
		if (isDiscountTime != this._isDiscountTime)
		{
			this.Init(this._pond, true);
		}
		int num = (int)this.ResidenceCost.CostValue + this._transportPrice;
		if (num != this._totalPrice)
		{
			if ((double)num > PhotonConnectionFactory.Instance.Profile.SilverCoins)
			{
				UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
			}
			this.UpdateTotalPrice(num);
			this.UpdateContentVisibility();
		}
	}

	private int Total
	{
		get
		{
			return (!this.IsDiscountTime) ? this._totalPrice : ((int)this.ResidenceCost.CostValueDiscount + ((this._transportPriceDiscount <= 0) ? this._transportPrice : this._transportPriceDiscount));
		}
	}

	private void UpdateTotalPrice(int totalPrice)
	{
		this._totalPrice = totalPrice;
		string text = this._totalPrice.ToString(CultureInfo.InvariantCulture);
		if (this._totalPriceTmp != null)
		{
			for (int i = 0; i < this._totalPriceTmp.Length; i++)
			{
				this._totalPriceTmp[i].text = text;
			}
		}
		string text2 = ((int)this.ResidenceCost.CostValueDiscount + ((this._transportPriceDiscount <= 0) ? this._transportPrice : this._transportPriceDiscount)).ToString(CultureInfo.InvariantCulture);
		if (this._totalPriceDiscountTmp != null)
		{
			for (int j = 0; j < this._totalPriceDiscountTmp.Length; j++)
			{
				this._totalPriceDiscountTmp[j].text = text2;
			}
		}
		if (this.TotalPrice != null)
		{
			this.TotalPrice.text = text;
		}
	}

	private bool IsDiscountTime
	{
		get
		{
			DateTime dateTime = TimeHelper.UtcTime();
			return this._pond != null && this._pond.DiscountStart != null && this._pond.DiscountStart <= dateTime && this._pond.DiscountEnd != null && this._pond.DiscountEnd > dateTime;
		}
	}

	private void SetTravelling(bool value)
	{
		this._traveling = value;
		this._globeNavigation.enabled = !value;
	}

	public void OnClickToTravel()
	{
		if (this._traveling)
		{
			return;
		}
		this.SetTravelling(true);
		ShowPondInfo.Instance.GetComponent<SubMenuControllerNew>().CloseButton();
		if (this._pond.PondLocked())
		{
			this.ShowUnlockPondMessage();
		}
		else if (!GlobalMapHelper.IsActive(this._pond))
		{
			UIHelper.ShowMessage(ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("PondInDeveloping"), true, null, false);
			this.SetTravelling(false);
		}
		else
		{
			this.CheckLicense();
		}
	}

	public void OnClickToTravelConfirmed()
	{
		if (this._pond.PondLocked())
		{
			this.ShowUnlockPondMessage();
		}
		else if (this._totalPrice == 0)
		{
			this.OnClickToTravel();
		}
		else
		{
			int total = this.Total;
			MessageBox _messageBox = MenuHelpers.Instance.ShowMessageSelectable(null, ScriptLocalization.Get("TravelButtonLower"), string.Format(ScriptLocalization.Get("TravelCost") + ": {0}\n{1}", total.ToString(CultureInfo.InvariantCulture), ScriptLocalization.Get("ConfirmSkipText")), ScriptLocalization.Get("YesCaption"), ScriptLocalization.Get("NoCaption"), false, false);
			_messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += delegate(object e, EventArgs obj)
			{
				_messageBox.Close();
			};
			_messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += delegate(object e, EventArgs obj)
			{
				_messageBox.Close();
				this.OnClickToTravel();
			};
		}
	}

	private void CheckLicense()
	{
		PlayerLicense[] array = PhotonConnectionFactory.Instance.Profile.Licenses.Where((PlayerLicense x) => x.StateId == this._pond.State.StateId && (!x.CanExpire || x.End > TimeHelper.UtcTime())).ToArray<PlayerLicense>();
		if (array.Length == 0 && !GlobalConsts.IsDebugLoading)
		{
			this.ShowLicenceMissingMessage();
		}
		else
		{
			this.GetTournaments(null, null);
		}
	}

	private void GetTournaments(object sender, EventArgs e)
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
		this.isTournamentSeries = false;
		this.rodStandAllowed = true;
		this.isHoldingMyTournament = false;
		PhotonConnectionFactory.Instance.OnGotMyTournaments += this.PhotonServerOnGotMyTournaments;
		PhotonConnectionFactory.Instance.OnGettingMyTournamentsFailed += this.InstanceOnGettingMyTournamentsFailed;
		PhotonConnectionFactory.Instance.GetMyTournaments(new int?(ShowPondInfo.Instance.CurrentPond.PondId));
	}

	private void InstanceOnGettingMyTournamentsFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnGotMyTournaments -= this.PhotonServerOnGotMyTournaments;
		PhotonConnectionFactory.Instance.OnGettingMyTournamentsFailed -= this.InstanceOnGettingMyTournamentsFailed;
		Debug.LogError(failure.FullErrorInfo);
	}

	private void PhotonServerOnGotMyTournaments(List<Tournament> tournaments)
	{
		PhotonConnectionFactory.Instance.OnGotMyTournaments -= this.PhotonServerOnGotMyTournaments;
		PhotonConnectionFactory.Instance.OnGettingMyTournamentsFailed -= this.InstanceOnGettingMyTournamentsFailed;
		List<Tournament> list = tournaments.OrderBy((Tournament y) => y.StartDate).ToList<Tournament>();
		if (this.activeTournament == null || list.All((Tournament x) => x.TournamentId != this.activeTournament.TournamentId))
		{
			this.activeTournament = list.FirstOrDefault<Tournament>();
		}
		bool flag = TournamentManager.CurrentUserGeneratedCompetition != null && TournamentManager.CurrentUserGeneratedCompetition.PondId == this._pond.PondId && (TournamentManager.CurrentUserGeneratedCompetition.FixedStartDate == null || this.activeTournament == null || (this.activeTournament != null && TournamentManager.CurrentUserGeneratedCompetition.FixedStartDate.Value.CompareTo(this.activeTournament.StartDate) <= 0));
		if (!flag && (this.activeTournament == null || this.activeTournament.EquipmentAllowed == null))
		{
			this.CheckRodPodsBoatsAndProceed(true);
			return;
		}
		InventoryItem boat = PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.ItemType == ItemTypes.Boat && x.Storage == StoragePlaces.Doll);
		RodStand rodStand = PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem s) => s.ItemSubType == ItemSubTypes.RodStand && s.Storage == StoragePlaces.Doll) as RodStand;
		this.isTournamentSeries = !flag && (this.activeTournament.SerieId != null || this.activeTournament.SerieInstanceId != null);
		this.isHoldingMyTournament = flag || tournaments.Any((Tournament t) => t.TournamentId == this.activeTournament.TournamentId);
		bool flag2 = boat == null;
		this.rodStandAllowed = rodStand == null;
		if (!flag2)
		{
			if (flag)
			{
				flag2 = TournamentManager.CurrentUserGeneratedCompetition.TournamentEquipment.BoatTypes != null && TournamentManager.CurrentUserGeneratedCompetition.TournamentEquipment.BoatTypes.Any((ItemSubTypes x) => x == boat.ItemSubType);
			}
			else
			{
				flag2 = this.activeTournament.EquipmentAllowed.BoatTypes != null && this.activeTournament.EquipmentAllowed.BoatTypes.Any((ItemSubTypes x) => x == boat.ItemSubType);
			}
		}
		if (!this.rodStandAllowed)
		{
			if (flag)
			{
				bool flag3;
				if (TournamentManager.CurrentUserGeneratedCompetition.TournamentEquipment.AllowRodStands)
				{
					if (TournamentManager.CurrentUserGeneratedCompetition.TournamentEquipment.MaxRodsOnStands != null)
					{
						int? maxRodsOnStands = TournamentManager.CurrentUserGeneratedCompetition.TournamentEquipment.MaxRodsOnStands;
						flag3 = rodStand.StandCount * rodStand.RodCount <= maxRodsOnStands;
					}
					else
					{
						flag3 = true;
					}
				}
				else
				{
					flag3 = false;
				}
				this.rodStandAllowed = flag3;
			}
			else
			{
				this.rodStandAllowed = this.activeTournament.EquipmentAllowed.AllowRodStands;
			}
		}
		if (!flag2 && this.isHoldingMyTournament)
		{
			this.ShowBoatNotAllowed(flag || !this.isTournamentSeries);
		}
		else
		{
			this.CheckRodPodsBoatsAndProceed(true);
		}
	}

	private void CheckRodPodsBoatsAndProceed(bool checkStands = true)
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
		if (!this.rodStandAllowed && this.isHoldingMyTournament && checkStands)
		{
			this.ShowRodStandsNotAllowed(!this.isTournamentSeries);
			return;
		}
		if (PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.ItemType == ItemTypes.Boat && x.Storage == StoragePlaces.Doll) != null)
		{
			CacheLibrary.MapCache.OnGetBoatDescs += this.GetBoatDesc;
			CacheLibrary.MapCache.GetBoatDescs();
		}
		else
		{
			this.TravelToPond();
		}
	}

	private void GetBoatDesc(object sender, GlobalMapBoatDescCacheEventArgs e)
	{
		CacheLibrary.MapCache.OnGetBoatDescs -= this.GetBoatDesc;
		IEnumerable<BoatDesc> pondBoatPrices = e.Items.GetPondBoatPrices(this._pond.State.StateId);
		InventoryItem boat = PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.ItemType == ItemTypes.Boat && x.Storage == StoragePlaces.Doll);
		if (pondBoatPrices.Count<BoatDesc>() == 0 && !pondBoatPrices.Any((BoatDesc x) => x.BoatCategoryId == (int)boat.ItemSubType))
		{
			if (this.messageBox != null)
			{
				this.messageBox.Close();
			}
			this.messageBox = this._helpers.ShowMessageSelectable(StaticUserData.CurrentForm.gameObject, ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("TravelToPondWithBoat"), ScriptLocalization.Get(boat.ItemSubType.ToString())), false);
			this.messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += delegate(object ea, EventArgs o)
			{
				if (this.messageBox != null)
				{
					this.messageBox.Close();
				}
				this.TravelToPond();
			};
			this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += delegate(object ea, EventArgs o)
			{
				this.SetTravelling(false);
				if (this.messageBox != null)
				{
					this.messageBox.Close();
				}
			};
		}
		else
		{
			this.TravelToPond();
		}
	}

	private void ShowUnlockPondMessage()
	{
		if (this._nextPond != null)
		{
			this._waitingForUnlock = new List<PondUnlockInfo>();
			if (this._pond.PondId == this._nextPond.PondId)
			{
				this.ShowBuyUnlockMessage();
			}
			else
			{
				List<PondUnlockInfo> list = this._unlocksList.OrderBy((PondUnlockInfo p) => p.MinLevel).ToList<PondUnlockInfo>();
				int num = list.FindIndex((PondUnlockInfo p) => p.PondId == this._pond.PondId);
				for (int i = num; i >= 0; i--)
				{
					PondUnlockInfo pondUnlockInfo = list[i];
					this._waitingForUnlock.Add(pondUnlockInfo);
					if (pondUnlockInfo.CanUse)
					{
						break;
					}
				}
				this._waitingForUnlock.Reverse();
				this.ShowBuyUnlockMessage();
			}
		}
	}

	private void ShowBuyUnlockMessage()
	{
		GameObject gameObject = InfoMessageController.Instance.gameObject;
		this.messageBox = GUITools.AddChild(gameObject, this.helpers.MessageBoxList.messageBoxSelectablePrefab).GetComponent<MessageBox>();
		this.messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		this.messageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		int num = 0;
		if (this._waitingForUnlock.Count > 0)
		{
			this.pondName = string.Empty;
			for (int i = 0; i < this._waitingForUnlock.Count; i++)
			{
				PondUnlockInfo pondUnlockInfo = this._waitingForUnlock[i];
				Pond pondInfoFromCache = CacheLibrary.MapCache.GetPondInfoFromCache(pondUnlockInfo.PondId);
				num += pondUnlockInfo.GoldCost;
				this.pondName += pondInfoFromCache.Name;
				if (i < this._waitingForUnlock.Count - 1)
				{
					this.pondName += ", ";
				}
			}
		}
		else
		{
			this.pondName = this._pond.Name;
			num = this._nextPond.GoldCost;
		}
		this.messageBox.Message = string.Format(ScriptLocalization.Get("UnlockCaption"), "\n" + this.pondName, "\ue62c " + num);
		this.messageBox.Caption = ScriptLocalization.Get("MessageCaption");
		this.messageBox.ConfirmButtonText = ScriptLocalization.Get("AgreeButtonCaption");
		this.messageBox.CancelButtonText = ScriptLocalization.Get("DisagreeButtonCaption");
		this.messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.BuyUnlock;
		this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.CompleteMessage_ActionCalled;
	}

	private void BuyUnlock(object sender, EventArgs args)
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
		bool flag = this._waitingForUnlock.Count > 0;
		int sum = 0;
		if (flag)
		{
			this._waitingForUnlock.ForEach(delegate(PondUnlockInfo p)
			{
				sum += p.GoldCost;
			});
		}
		else
		{
			sum = this._nextPond.GoldCost;
		}
		if ((double)sum > PhotonConnectionFactory.Instance.Profile.GoldCoins)
		{
			this.messageBox = this.helpers.ShowMessage(base.gameObject.transform.root.gameObject, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("HaventMoney"), false, false, false, null);
			UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
			this.messageBox.GetComponent<EventAction>().ActionCalled += this.CompleteMessage_ActionCalled;
		}
		else if (flag)
		{
			this._waitingForUnlock.ForEach(delegate(PondUnlockInfo p)
			{
				PhotonConnectionFactory.Instance.UnlockPond(p.PondId);
			});
		}
		else
		{
			PhotonConnectionFactory.Instance.UnlockPond(this._nextPond.PondId);
		}
	}

	private void OnGotPondUnlocked(int pondId, int accesibleLevel)
	{
		if (this._waitingForUnlock.Count > 0)
		{
			int num = this._waitingForUnlock.FindIndex((PondUnlockInfo p) => p.PondId == pondId);
			if (num != -1)
			{
				this._waitingForUnlock.RemoveAt(num);
				if (this._waitingForUnlock.Count == 0)
				{
					this.ShowUnlockedCaption();
				}
			}
		}
		else
		{
			this.ShowUnlockedCaption();
		}
	}

	private void ShowUnlockedCaption()
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
		this.messageBox = this.helpers.ShowMessage(base.gameObject.transform.root.gameObject, ScriptLocalization.Get("ShopCongratulationCaption"), string.Format(ScriptLocalization.Get("PondUnlocked"), this.pondName), false, false, false, null);
		this.messageBox.GetComponent<EventAction>().ActionCalled += this.CompleteMessage_ActionCalled;
		PhotonConnectionFactory.Instance.GetPondUnlocks();
	}

	private void ShowBoatNotAllowed(bool competition = true)
	{
		this.PrepareThreeSelectable();
		this.messageBox.Message = ScriptLocalization.Get((!competition) ? "UseKayakMessageTournament" : "UseKayakMessage");
		this.messageBox.ConfirmButtonText = ScriptLocalization.Get("OpenInventoryCaption");
		EventConfirmAction component = this.messageBox.GetComponent<EventConfirmAction>();
		component.ConfirmActionCalled += this.OpenInventory;
		component.CancelActionCalled += this.ProceedToPond_ActionCalled;
	}

	private void ShowRodStandsNotAllowed(bool competition = true)
	{
		this.PrepareThreeSelectable();
		this.messageBox.Message = ScriptLocalization.Get((!competition) ? "UseRodStandMessageTournament" : "UseRodStandMessage");
		this.messageBox.ConfirmButtonText = ScriptLocalization.Get("OpenInventoryCaption");
		EventConfirmAction component = this.messageBox.GetComponent<EventConfirmAction>();
		component.ConfirmActionCalled += this.OpenInventory;
		component.CancelActionCalled += this.ProceedToPondRodStands_ActionCalled;
	}

	private void ShowLicenceMissingMessage()
	{
		this.PrepareThreeSelectable();
		this.messageBox.confirmButton.AddComponent<HintElementId>().SetElementId("BuyLicenseOnTravel", null, null);
		this.messageBox.Message = ScriptLocalization.Get("LicenceAbsentText");
		this.messageBox.ConfirmButtonText = ScriptLocalization.Get("ButtonBuyLicensesLower");
		this.messageBox.ThirdButtonText = ScriptLocalization.Get("CancelButton");
		EventConfirmAction component = this.messageBox.GetComponent<EventConfirmAction>();
		component.ConfirmActionCalled += this.LoadShop_ActionCalled;
		component.CancelActionCalled += this.GetTournaments;
	}

	private void PrepareThreeSelectable()
	{
		GameObject gameObject = InfoMessageController.Instance.gameObject;
		this.messageBox = GUITools.AddChild(gameObject, this.helpers.MessageBoxList.messageBoxThreeSelectablePrefab).GetComponent<MessageBox>();
		this.messageBox.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
		this.messageBox.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
		this.messageBox.Caption = ScriptLocalization.Get("MessageCaption");
		this.messageBox.CancelButtonText = ScriptLocalization.Get("TravelButtonLower");
		this.messageBox.ThirdButtonText = ScriptLocalization.Get("CloseButton");
		this.messageBox.GetComponent<EventConfirmAction>().ThirdButtonActionCalled += this.CompleteMessage_ActionCalled;
	}

	private void LoadShop_ActionCalled(object sender, EventArgs e)
	{
		this.CompleteMessage_ActionCalled(null, null);
		ShowPondInfo.Instance.GotoLicenseShop();
	}

	private void LoadBoatShop(object sender, EventArgs e)
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
		ShowPondInfo.Instance.OpenCategory("btnBoats");
	}

	private void OpenInventory(object sender, EventArgs e)
	{
		this.CompleteMessage_ActionCalled(null, null);
		ShowPondInfo.Instance.OpenInventory();
	}

	private void ProceedToPond_ActionCalled(object sender, EventArgs e)
	{
		this.CheckRodPodsBoatsAndProceed(true);
	}

	private void ProceedToPondRodStands_ActionCalled(object sender, EventArgs e)
	{
		this.CheckRodPodsBoatsAndProceed(false);
	}

	private void TravelToPond()
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
		short days = (short)this.ResidenceCost.DaysValue;
		int totalPrice = this.Total;
		if (totalPrice > 0 && (double)totalPrice > PhotonConnectionFactory.Instance.Profile.SilverCoins)
		{
			this.messageBox = this.helpers.ShowMessage(base.gameObject.transform.root.gameObject, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("TravelNotEnoughMoney"), false, false, false, null);
			UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
			this.messageBox.GetComponent<EventAction>().ActionCalled += this.CompleteMessage_ActionCalled;
			return;
		}
		this.OnTravelToPond(delegate
		{
			AnalyticsFacade.WriteSpentSilver("Travel", totalPrice, 1);
			AnalyticsFacade.WriteTravelToPond(this._pond.PondId, PhotonConnectionFactory.Instance.Profile);
			PondTransferInfo pondTransferInfo = new PondTransferInfo
			{
				Pond = this._pond,
				HasInCar = false,
				Days = new int?((int)days)
			};
			CustomPlayerPrefs.SetInt("LastPondId", this._pond.PondId);
			SceneController.CallAction(ScenesList.GlobalMap, SceneStatuses.GotoPond, this, pondTransferInfo);
		});
	}

	private void CompleteMessage_ActionCalled(object sender, EventArgs e)
	{
		this.SetTravelling(false);
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
	}

	[SerializeField]
	private UINavigation _globeNavigation;

	[SerializeField]
	private TextMeshProUGUI _noMoneyText;

	[SerializeField]
	private Transform[] _allDiscounts;

	[SerializeField]
	private Transform[] _allNormal;

	[SerializeField]
	private Transform[] _transportDiscounts;

	[SerializeField]
	private Transform[] _transportNormal;

	[SerializeField]
	private Transform[] _dayDiscounts;

	[SerializeField]
	private Transform[] _dayNormal;

	[SerializeField]
	private Transform _total;

	[SerializeField]
	private Transform _totalDiscount;

	[SerializeField]
	private Transform _totalNoMoney;

	[SerializeField]
	private Image _imgNoMoney;

	[SerializeField]
	private RectTransform _travelText;

	[SerializeField]
	private TextMeshProUGUI[] _transportPriceTmp;

	[SerializeField]
	private TextMeshProUGUI[] _totalPriceTmp;

	[SerializeField]
	private TextMeshProUGUI[] _transportPriceDiscountTmp;

	[SerializeField]
	private TextMeshProUGUI[] _totalPriceDiscountTmp;

	[Space(8f)]
	public DaysChange ResidenceCost;

	public Text TotalPrice;

	public Text TransportPrice;

	private bool _isDiscountTime;

	private int _totalPrice;

	private int _transportPrice;

	private int _totalPriceDiscount;

	private int _transportPriceDiscount;

	private string pondName;

	private MenuHelpers helpers = new MenuHelpers();

	private MessageBox messageBox;

	private PondUnlockInfo _nextPond;

	private bool _traveling;

	private Pond _pond;

	private readonly MenuHelpers _helpers = new MenuHelpers();

	private Tournament activeTournament;

	private const float TravelTextWidth = 286.3f;

	private const float TravelTextWidthDiscount = 172.91f;

	private bool isTournamentSeries;

	private bool rodStandAllowed;

	private bool isHoldingMyTournament;

	private List<PondUnlockInfo> _unlocksList = new List<PondUnlockInfo>();

	private List<PondUnlockInfo> _waitingForUnlock = new List<PondUnlockInfo>();
}
