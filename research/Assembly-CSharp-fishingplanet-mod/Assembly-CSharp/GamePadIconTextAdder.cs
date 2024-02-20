using System;
using InControl;
using TMPro;
using UnityEngine;

public class GamePadIconTextAdder : ActivityStateControlled
{
	private void Awake()
	{
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
		this.OnInputTypeChanged(InputModuleManager.GameInputType);
	}

	protected override void SetHelp()
	{
		if (this.shouldCall)
		{
			this.shouldCall = false;
			this.OnInputTypeChanged(this.lastType);
		}
		else
		{
			this.Localize();
		}
	}

	public void SetTermAndText(string text, InputControlType term)
	{
		this.TextData = text;
		this.SetTerm(term);
	}

	public void SetTerm(InputControlType term)
	{
		this._localizationTerm = term;
		this.Localize();
	}

	private void OnInputTypeChanged(InputModuleManager.InputType inputType)
	{
		if (!base.ShouldUpdate())
		{
			this.shouldCall = true;
			this.lastType = inputType;
			return;
		}
		if (((inputType == InputModuleManager.InputType.Mouse && this._mouseOnly) || (inputType == InputModuleManager.InputType.GamePad && this._gamePadOnly) || this._pcOnly) && !this._consoleOnly)
		{
			this._isShow = true;
			this.Localize();
		}
		else
		{
			this._isShow = false;
			this.Localize();
		}
	}

	public void Localize()
	{
		string text;
		if (HotkeyIcons.KeyMappings.TryGetValue(this._localizationTerm, out text) && this.text != null)
		{
			this.text.text = ((!this._isShow) ? this.TextData : string.Format("{0} {1}", text, this.TextData));
		}
	}

	[SerializeField]
	private InputControlType _localizationTerm;

	[SerializeField]
	private bool _gamePadOnly = true;

	[SerializeField]
	private bool _mouseOnly;

	[SerializeField]
	private bool _consoleOnly;

	[SerializeField]
	private bool _pcOnly;

	public TextMeshProUGUI text;

	public string TextData;

	private const string format = "{0} {1}";

	private bool _isShow = true;

	private bool shouldCall;

	private InputModuleManager.InputType lastType;
}
