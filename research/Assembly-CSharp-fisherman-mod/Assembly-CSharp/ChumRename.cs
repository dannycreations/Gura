using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class ChumRename : WindowBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<string> OnRenamed = delegate(string s)
	{
	};

	protected override void Awake()
	{
		base.Awake();
		this._input.placeholder.GetComponent<Text>().text = null;
		this._input.characterLimit = this._chumNameMaxCharacters;
		InputField input2 = this._input;
		input2.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(input2.onValidateInput, new InputField.OnValidateInput((string input, int charIndex, char addedChar) => this.ValidateInput(addedChar)));
	}

	public void Init(string v)
	{
		this._title.text = ScriptLocalization.Get("SaveRecipeCaption");
		this._input.text = v;
		this.OnInputTypeChanged(SettingsManager.InputType);
	}

	public void Init(string v, string title, string titleLocalized, bool allowedEmpty, int? chumNameMaxCharacters = null, List<char> chars = null, bool onlyNumbers = false)
	{
		this.Init(v);
		this._onlyNumbers = onlyNumbers;
		if (chumNameMaxCharacters != null)
		{
			this._chumNameMaxCharacters = chumNameMaxCharacters.Value;
		}
		if (chars != null)
		{
			this._chars = chars;
		}
		this._allowedEmpty = allowedEmpty;
		this._title.text = ((!string.IsNullOrEmpty(titleLocalized)) ? titleLocalized : ScriptLocalization.Get(title));
	}

	public override void Accept()
	{
		if (this._allowedEmpty || !string.IsNullOrEmpty(this._input.text))
		{
			base.Accept();
		}
		else
		{
			if (this._msgBox)
			{
				return;
			}
			this._msgBox = true;
			UIHelper.ShowCanceledMsg(ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("UGC_FieldCannotBeEmpty"), TournamentCanceledInit.MessageTypes.Error, delegate
			{
				this._msgBox = false;
			}, false);
		}
	}

	protected override void AcceptActionCalled()
	{
		this.OnRenamed(this._input.text);
	}

	protected override void OnInputTypeChanged(InputModuleManager.InputType type)
	{
		base.StartCoroutine(this.StartEdit());
	}

	private void ActivateInputField()
	{
		UINavigation.SetSelectedGameObject(this._input.gameObject);
	}

	private IEnumerator StartEdit()
	{
		yield return new WaitForEndOfFrame();
		this._input.GetComponent<ScreenKeyboard>().OnInputFieldSelect(string.Empty, false, ScreenKeyboard.VirtualKeyboardScope.Default);
		this.ActivateInputField();
		this._input.Select();
		this._input.ActivateInputField();
		this._input.MoveTextEnd(true);
		yield break;
	}

	private char ValidateInput(char charToValidate)
	{
		if ((char.IsLetter(charToValidate) && !this._onlyNumbers) || char.IsNumber(charToValidate))
		{
			return charToValidate;
		}
		if (!this._chars.Contains(charToValidate))
		{
			charToValidate = '\0';
		}
		return charToValidate;
	}

	[SerializeField]
	private Text _title;

	[SerializeField]
	private InputField _input;

	private int _chumNameMaxCharacters = 31;

	private List<char> _chars = new List<char> { '-', '_', '.', ' ' };

	private bool _allowedEmpty = true;

	private bool _onlyNumbers;

	private bool _msgBox;
}
