using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class DebugDialog : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<DebugDialogType, int> OnClose = delegate
	{
	};

	public DebugDialogType? DialogType
	{
		get
		{
			return this._dialogType;
		}
	}

	public string Message
	{
		set
		{
			this._message.text = value;
		}
	}

	private void Awake()
	{
		for (int i = 0; i < this._buttons.Length; i++)
		{
			int i1 = i;
			Button button = this._buttons[i];
			button.onClick.AddListener(delegate
			{
				this.OnBtnClick(i1);
			});
			this._btnTitles.Add(button.transform.GetChild(0).GetComponent<Text>());
		}
	}

	private void OnBtnClick(int i)
	{
		ControlsController.ControlsActions.UnBlockAxis();
		CursorManager.HideCursor();
		base.gameObject.SetActive(false);
		this.OnClose(this._dialogType.Value, i);
		this._dialogType = null;
	}

	[SerializeField]
	private Text _title;

	[SerializeField]
	private Text _message;

	[SerializeField]
	private Button[] _buttons;

	private List<Text> _btnTitles = new List<Text>();

	private DebugDialogType? _dialogType;
}
