using System;
using System.Collections;
using System.Diagnostics;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputAreaWnd : WindowBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<string> OnOk = delegate(string s)
	{
	};

	protected override void Awake()
	{
		base.Awake();
		this._originWndHeight = this._wnd.rect.height;
		this._originIfHeight = this._ifLe.preferredHeight;
		this._input.onValueChanged.AddListener(delegate(string v)
		{
			if (this._originWndHeight <= 0f)
			{
				this._originWndHeight = this._wnd.rect.height;
			}
			bool flag = string.IsNullOrEmpty(this._input.text.Trim());
			if (this._input.textComponent.preferredHeight > 384f && !flag)
			{
				return;
			}
			float num = ((this._input.textComponent.preferredHeight <= 160f || flag) ? 0f : (this._input.textComponent.preferredHeight - 160f));
			this._ifLe.preferredHeight = this._originIfHeight + num;
			this._wnd.sizeDelta = new Vector2(this._wnd.rect.width, this._originWndHeight + num);
			float num2 = (this._ifLe.preferredHeight - this._scrollbar.rect.height) / 2f;
			this._scrollbar.sizeDelta = new Vector2(this._scrollbar.rect.width, this._ifLe.preferredHeight);
			this._scrollbar.anchoredPosition = new Vector2(this._scrollbar.anchoredPosition.x, this._scrollbar.anchoredPosition.y - num2);
		});
	}

	protected override void Update()
	{
		if (Input.GetKeyDown(27))
		{
			this.Close();
		}
	}

	public void Init(string title, bool allowedEmpty = true, bool isScreenKeyboard = true)
	{
		this._allowedEmpty = allowedEmpty;
		this._isScreenKeyboard = isScreenKeyboard;
		this._title.text = title;
		if (this._isScreenKeyboard)
		{
			this.OnInputTypeChanged(SettingsManager.InputType);
		}
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
		this.OnOk(this._input.text);
	}

	protected override void OnInputTypeChanged(InputModuleManager.InputType type)
	{
		if (this._isScreenKeyboard)
		{
			base.StartCoroutine(this.StartEdit());
		}
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

	[SerializeField]
	private Text _title;

	[SerializeField]
	private TMP_InputField _input;

	[SerializeField]
	private RectTransform _wnd;

	[SerializeField]
	private LayoutElement _ifLe;

	[SerializeField]
	private RectTransform _scrollbar;

	private const float PreferredHeightForResize = 160f;

	private const float PreferredHeightForResizeMax = 384f;

	private bool _allowedEmpty = true;

	private bool _isScreenKeyboard = true;

	private float _originWndHeight;

	private float _originIfHeight;

	private bool _msgBox;
}
