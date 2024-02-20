using System;
using System.Collections;
using I2.Loc;
using InControl;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StatsInit : ActivityStateControlled
{
	private void Awake()
	{
		this._selectBinding = new HotkeyBinding
		{
			Hotkey = InputControlType.Action1,
			LocalizationKey = "Select"
		};
	}

	protected override void SetHelp()
	{
		this._canRunUI = true;
		HelpLinePanel.SetActionHelp(this._selectBinding);
		PhotonConnectionFactory.Instance.OnGotStats += this.OnGotStats;
		PhotonConnectionFactory.Instance.OnPromoCodeGenerated += this.OnPromoCodeGenerated;
		this.PromocodeButton.onClick.AddListener(new UnityAction(this.GetPromoCode));
		if (StaticUserData.CurrentPond != null || this._stats == null)
		{
			PhotonConnectionFactory.Instance.RequestStats();
		}
		else if (this._stats != null && this.CommonInfo != null)
		{
			this.CommonInfo.Refresh(PhotonConnectionFactory.Instance.Profile, this._stats);
		}
	}

	protected override void HideHelp()
	{
		PhotonConnectionFactory.Instance.OnGotStats -= this.OnGotStats;
		PhotonConnectionFactory.Instance.OnPromoCodeGenerated -= this.OnPromoCodeGenerated;
		this.PromocodeButton.onClick.RemoveAllListeners();
		this.PromocodeButton.GetComponentInChildren<Text>().text = ScriptLocalization.Get("GetPromoCodeCaption");
		this.PromocodeField.gameObject.SetActive(false);
		this._canRunUI = false;
		HelpLinePanel.HideActionHelp(this._selectBinding);
	}

	private void GetPromoCode()
	{
		this.PromocodeButton.interactable = false;
		if (string.IsNullOrEmpty(PhotonConnectionFactory.Instance.Profile.OwnPromoCode))
		{
			PhotonConnectionFactory.Instance.GeneratePromoCode();
		}
		else
		{
			this.OnPromoCodeGenerated();
		}
	}

	private void OnPromoCodeGenerated()
	{
		this.PromocodeButton.onClick.RemoveListener(new UnityAction(this.GetPromoCode));
		this.PromocodeButton.onClick.AddListener(new UnityAction(this.CopyPromoCode));
		this.PromocodeButton.GetComponentInChildren<Text>().text = ScriptLocalization.Get("CopyCodeCaption");
		this.PromocodeField.gameObject.SetActive(true);
		this.PromocodeField.text = PhotonConnectionFactory.Instance.Profile.OwnPromoCode;
		this.PromocodeButton.interactable = true;
	}

	private void CopyPromoCode()
	{
		GUIUtility.systemCopyBuffer = PhotonConnectionFactory.Instance.Profile.OwnPromoCode;
	}

	internal void OnGotStats(PlayerStats stats)
	{
		base.StartCoroutine(this.UpdateScreen(0.1f, stats));
	}

	private IEnumerator UpdateScreen(float waitTime, PlayerStats stats)
	{
		while (!this._canRunUI)
		{
			yield return new WaitForSeconds(waitTime);
		}
		this._stats = stats;
		if (this.CommonInfo != null)
		{
			this.CommonInfo.Refresh(PhotonConnectionFactory.Instance.Profile, stats);
		}
		if (this.AchivementsInit != null)
		{
			this.AchivementsInit.Clear();
			this.AchivementsInit.Refresh(stats);
		}
		if (this.TrophiesInit != null)
		{
			this.TrophiesInit.Clear();
			this.TrophiesInit.Refresh(stats.FishStats);
		}
		yield break;
	}

	public InitCommonInfo CommonInfo;

	public AchivementsInit AchivementsInit;

	public TrophiesInit TrophiesInit;

	public Button PromocodeButton;

	public InputField PromocodeField;

	private bool _canRunUI;

	private HotkeyBinding _selectBinding;

	private PlayerStats _stats;
}
