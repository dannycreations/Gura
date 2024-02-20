using System;
using System.Collections.Generic;
using UnityEngine;

public class PhotomodeHUD : MonoBehaviour
{
	private PhotoModeScreen GetScreen(int i)
	{
		return (!InputModuleManager.IsConsoleMode) ? this._screens[i].PCScreen : this._screens[i].ConsoleScreen;
	}

	private void OnEnable()
	{
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
	}

	private void OnDisable()
	{
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
	}

	private void Awake()
	{
		this._pcHints.gameObject.SetActive(!InputModuleManager.IsConsoleMode);
		this._consoleHints.gameObject.SetActive(InputModuleManager.IsConsoleMode);
	}

	private void Start()
	{
		for (int i = 0; i < this._screens.Count; i++)
		{
			this._screens[i].ConsoleScreen.FastHide();
			this._screens[i].PCScreen.FastHide();
		}
		this._currentIndex = 0;
		this._current = this.GetScreen(this._currentIndex);
		this._current.Show();
	}

	public void Init(bool cantStidown)
	{
		this._cantSitdownWarning.SetActive(cantStidown);
		this._hideCantSitdownWarningAt = ((!cantStidown) ? (-1f) : (Time.time + 3f));
	}

	private void Update()
	{
		if (this._hideCantSitdownWarningAt > 0f && this._hideCantSitdownWarningAt < Time.time)
		{
			this._hideCantSitdownWarningAt = -1f;
			this._cantSitdownWarning.SetActive(false);
		}
		if (ControlsController.ControlsActions.PHMChangeModeDownUI.WasPressed)
		{
			this.ShowNextMenu();
		}
		if (ControlsController.ControlsActions.PHMChangeModeUpUI.WasPressed)
		{
			this.ShowPrevMenu();
		}
		if (ControlsController.ControlsActions.PHMHideUI.WasPressed)
		{
			if (this._hudRoot.Alpha >= 1f)
			{
				this.HideUI();
			}
			if (this._hudRoot.Alpha <= 0f)
			{
				this.ShowUI();
			}
		}
		if (Input.GetKeyUp(27) && this._hudRoot.Alpha <= 0f)
		{
			this.ShowUI();
		}
	}

	private void HideUI()
	{
		this._logo.ShowPanel();
		this._hudRoot.HidePanel();
	}

	private void ShowUI()
	{
		this._logo.HidePanel();
		this._hudRoot.ShowPanel();
	}

	private void ShowMenu()
	{
		if (this._current == this.GetScreen(this._currentIndex) && (this._current.IsShow || this._current.IsShowing))
		{
			return;
		}
		this._current.Hide();
		this._current = this.GetScreen(this._currentIndex);
		this._current.Show();
	}

	private void ShowPrevMenu()
	{
		this._currentIndex = ((this._currentIndex > 0) ? (this._currentIndex - 1) : (this._screens.Count - 1));
		if (!this._current.IsShowing && !this._current.IsHiding)
		{
			this.ShowMenu();
		}
	}

	private void ShowNextMenu()
	{
		this._currentIndex = ((this._currentIndex < this._screens.Count - 1) ? (this._currentIndex + 1) : 0);
		if (!this._current.IsShowing && !this._current.IsHiding)
		{
			this.ShowMenu();
		}
	}

	private void OnInputTypeChanged(InputModuleManager.InputType obj)
	{
		this.ShowMenu();
		this._pcHints.gameObject.SetActive(!InputModuleManager.IsConsoleMode);
		this._consoleHints.gameObject.SetActive(InputModuleManager.IsConsoleMode);
	}

	[SerializeField]
	private Sprite _logoRetail;

	[SerializeField]
	private AlphaFade _logo;

	[SerializeField]
	private List<PhotomodeHUD.Record> _screens;

	[SerializeField]
	private RectTransform _pcHints;

	[SerializeField]
	private RectTransform _consoleHints;

	[SerializeField]
	private AlphaFade _hudRoot;

	[SerializeField]
	private GameObject _cantSitdownWarning;

	private PhotoModeScreen _current;

	private int _currentIndex;

	private const float WARNING_SHOW_TIME = 3f;

	private float _hideCantSitdownWarningAt = -1f;

	[Serializable]
	private class Record
	{
		public PhotoModeScreen PCScreen;

		public PhotoModeScreen ConsoleScreen;
	}
}
