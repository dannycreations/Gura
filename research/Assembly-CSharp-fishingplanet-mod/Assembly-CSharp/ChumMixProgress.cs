using System;
using System.Diagnostics;
using Assets.Scripts.UI._2D.Inventory.Mixing;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChumMixProgress : WindowBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnFinish = delegate
	{
	};

	protected override void Awake()
	{
		base.Awake();
		this._title.text = ScriptLocalization.Get("ChumMixing");
		this._oK.gameObject.SetActive(false);
		this._buttonOk.interactable = false;
	}

	protected override void Update()
	{
		if (this._mixingEmulator != null)
		{
			this._mixingEmulator.Update();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (this._mixingEmulator != null)
		{
			this._mixingEmulator.ChangeState -= this.ChangeStateDuration;
			this._mixingEmulator.ChangeState -= this._mixingEmulator_ChangeState;
			this._mixingEmulator.Dispose();
			this._mixingEmulator = null;
		}
	}

	public void Init(int duration, string title, bool isButtonCancelActive, bool isAutoHide = true)
	{
		this._isAutoHide = isAutoHide;
		this._buttonOk.gameObject.SetActive(false);
		this._buttonCancel.interactable = isButtonCancelActive;
		this._buttonCancel.gameObject.SetActive(isButtonCancelActive);
		this._title.text = title;
		this._premText.gameObject.SetActive(false);
		this._noPremText.gameObject.SetActive(false);
		this._mixingEmulator = new MixingEmulator(this._progressPrc, this._progress, duration, this._buttonCancel, true);
		this._mixingEmulator.ChangeState += this.ChangeStateDuration;
	}

	public void Init(Chum chum, Func<InventoryItem, InventoryItem, bool> transferItemFunc)
	{
		this._mixingEmulator = new MixingEmulator(chum, this._progressPrc, this._progress, this._buttonOk, this._buttonCancel, this._noPremText, this._premText);
		this._mixingEmulator.ChangeState += this._mixingEmulator_ChangeState;
		this._mixingEmulator.Init(transferItemFunc);
		base.Open();
	}

	public override void Close()
	{
		if (this._mixingEmulator != null)
		{
			this._mixingEmulator.FullStop();
		}
		base.Close();
	}

	private void _mixingEmulator_ChangeState()
	{
		if (this._mixingEmulator.InState(MixingEmulator.States.Finish))
		{
			this._progressPrc.gameObject.SetActive(false);
			this._oK.color = this._progress.color;
			this._oK.gameObject.SetActive(true);
		}
	}

	private void ChangeStateDuration()
	{
		bool flag = this._mixingEmulator.InState(MixingEmulator.States.Finish);
		if (flag)
		{
			this.OnFinish();
			if (this._isAutoHide && !this.Alpha.IsHiding)
			{
				this.Alpha.HidePanel();
			}
		}
	}

	[SerializeField]
	private Text _title;

	[SerializeField]
	private Text _progressPrc;

	[SerializeField]
	private Image _progress;

	[SerializeField]
	private BorderedButton _buttonOk;

	[SerializeField]
	private BorderedButton _buttonCancel;

	[SerializeField]
	private TextMeshProUGUI _oK;

	[SerializeField]
	private TextMeshProUGUI _noPremText;

	[SerializeField]
	private TextMeshProUGUI _premText;

	private MixingEmulator _mixingEmulator;

	private bool _isAutoHide;
}
