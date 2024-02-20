using System;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class ProlongationOfStay : MonoBehaviour
{
	public static ProlongationOfStay Instance { get; private set; }

	public bool IsActive
	{
		get
		{
			return this._isShowWindow;
		}
	}

	private void Awake()
	{
		ProlongationOfStay.Instance = this;
	}

	internal void Start()
	{
		if (this.HaveMoneyForm != null)
		{
			this.HaveMoneyForm.SetActive(false);
		}
		if (this.DontHaveMoneyForm != null)
		{
			this.DontHaveMoneyForm.SetActive(false);
		}
		if (this.PassEndedForm != null)
		{
			this.PassEndedForm.SetActive(false);
		}
		this.oldTimeScale = Time.timeScale;
		PhotonConnectionFactory.Instance.OnPondStayProlonged += this.OnPondStayProlonged;
		PhotonConnectionFactory.Instance.OnProfileOperationFailed += this.OnProfileOperationFailed;
		PhotonConnectionFactory.Instance.OnPondStayFinish += this.OnPondStayFinish;
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnPondStayProlonged -= this.OnPondStayProlonged;
		PhotonConnectionFactory.Instance.OnProfileOperationFailed -= this.OnProfileOperationFailed;
		PhotonConnectionFactory.Instance.OnPondStayFinish -= this.OnPondStayFinish;
	}

	internal void Update()
	{
		if (PhotonConnectionFactory.Instance.Profile == null)
		{
			return;
		}
		if (!this.HaveMoneyForm.activeSelf && !this.DontHaveMoneyForm.activeSelf && PhotonConnectionFactory.Instance.IsPondStayFinished && !this._isShowWindow && !GameFactory.Player.IsTackleThrown)
		{
			this.OpenProlongationWindow(StaticUserData.CurrentPond.PondLocked());
		}
		if (this._isShowWindow && GameFactory.FishSpawner != null && !GameFactory.FishSpawner.IsGamePaused)
		{
			PhotonConnectionFactory.Instance.Game.Pause();
		}
	}

	public void ProlongationClick()
	{
		if (this.HaveMoneyForm != null)
		{
			this.HaveMoneyForm.SetActive(false);
		}
		PhotonConnectionFactory.Instance.ProlongPondStay(1);
	}

	public void GoHomeClick()
	{
		ControlsController.ControlsActions.UnBlockInput();
		CursorManager.HideCursor();
		GameFactory.ClearPondLocationsInfo();
		StaticUserData.CurrentPond = null;
		StaticUserData.CurrentLocation = null;
		if (this.HaveMoneyForm != null)
		{
			this.HaveMoneyForm.SetActive(false);
		}
		if (this.DontHaveMoneyForm != null)
		{
			this.DontHaveMoneyForm.SetActive(false);
		}
		PhotonConnectionFactory.Instance.RequestEndOfMissionResult();
	}

	private void OnPondStayFinish(PondStayFinish finish)
	{
		if (!this.HaveMoneyForm.activeSelf && !this.DontHaveMoneyForm.activeSelf && !this.PassEndedForm)
		{
			this.OpenProlongationWindow(finish.ForcePlayerToLeavePond);
		}
	}

	private void OpenProlongationWindow(bool forceToLeave)
	{
		if (StaticUserData.CurrentPond == null || GameFactory.ChatInGameController == null)
		{
			return;
		}
		this._isShowWindow = true;
		PhotonConnectionFactory.Instance.Game.Pause();
		CursorManager.ShowCursor();
		ControlsController.ControlsActions.BlockInput(null);
		GameFactory.ChatInGameController.SetActive(false);
		if (forceToLeave)
		{
			this.PassEndedForm.SetActive(true);
		}
		else
		{
			int num;
			if (PhotonConnectionFactory.Instance.Profile.FishCage == null)
			{
				num = 0;
			}
			else
			{
				int? silverCost = PhotonConnectionFactory.Instance.Profile.FishCage.SilverCost;
				num = ((silverCost == null) ? 0 : silverCost.Value);
			}
			int num2 = num;
			if (StaticUserData.CurrentPond.StayFee > 0f)
			{
				float? stayFee = StaticUserData.CurrentPond.StayFee;
				if (((stayFee == null) ? null : new double?((double)stayFee.Value)) > PhotonConnectionFactory.Instance.Profile.SilverCoins + (double)num2)
				{
					this._dontHaveMoneyTxt.text = string.Format(ScriptLocalization.Get("ProlongationGoHomeMessage"), "\n");
					this.DontHaveMoneyForm.SetActive(true);
					return;
				}
			}
			string text = ScriptLocalization.Get("ProlongationAlternativeMessage");
			text = string.Format(text, "\n", "<color=#FFDD77FF>" + StaticUserData.CurrentPond.StayFee + " \ue62b</color>");
			this._haveMoneyTxt.text = text;
			this.HaveMoneyForm.SetActive(true);
		}
	}

	private void OnPondStayProlonged(ProlongInfo info)
	{
		Time.timeScale = this.oldTimeScale;
		this._isShowWindow = false;
		this.HaveMoneyForm.SetActive(false);
		PhotonConnectionFactory.Instance.Game.Resume(true);
		ControlsController.ControlsActions.UnBlockInput();
		CursorManager.HideCursor();
		if (!CursorManager.IsModalWindow())
		{
			GameFactory.ChatInGameController.SetActive(true);
		}
	}

	private void OnProfileOperationFailed(ProfileFailure failure)
	{
		if (failure.SubOperation == 242)
		{
		}
	}

	internal void OnEnable()
	{
		if (GameFactory.Player == null || TimeAndWeatherManager.CurrentTime == null || TimeAndWeatherManager.CurrentTime.Value.TotalSeconds < 1.0)
		{
			return;
		}
		if (PhotonConnectionFactory.Instance.IsPondStayFinished)
		{
			this.OpenProlongationWindow(StaticUserData.CurrentPond.PondLocked());
		}
	}

	[SerializeField]
	private Text _haveMoneyTxt;

	[SerializeField]
	private Text _dontHaveMoneyTxt;

	public GameObject HaveMoneyForm;

	public GameObject DontHaveMoneyForm;

	public GameObject PassEndedForm;

	private float oldTimeScale;

	private bool _isShowWindow;
}
