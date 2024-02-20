using System;
using System.Diagnostics;
using I2.Loc;
using InControl;
using UnityEngine;
using UnityEngine.UI;

public class InteractiveObjectPanelHandler : MonoBehaviour
{
	public static InteractiveObjectPanelHandler Instance { get; private set; }

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action HideFinished = delegate
	{
	};

	public bool IsPanelShowing()
	{
		return base.gameObject.activeInHierarchy && this._alphaFade != null && (this._alphaFade.IsShowing || this._alphaFade.IsShow);
	}

	private void Awake()
	{
		InteractiveObjectPanelHandler.Instance = this;
		this._alphaFade = base.GetComponent<AlphaFade>();
		this._alphaFade.FastHidePanel();
	}

	private void Start()
	{
		this._alphaFade.HideFinished += this._alphaFade_HideFinished;
	}

	private void OnDestroy()
	{
		this._alphaFade.HideFinished -= this._alphaFade_HideFinished;
	}

	public void Change(string text)
	{
		this.TextInfo.text = text;
	}

	public void ShowPanel(string text = null)
	{
		if (!string.IsNullOrEmpty(text))
		{
			this.TextInfo.text = text;
		}
		else
		{
			string text2 = string.Format("<color=#F79A44FF>{0}</color>", (InputModuleManager.GameInputType != InputModuleManager.InputType.GamePad) ? "[E]" : HotkeyIcons.KeyMappings[InputControlType.Action4]);
			this.TextInfo.text = string.Format(ScriptLocalization.Get("InteractObjectCaption"), text2);
		}
		if (this._alphaFade != null)
		{
			this._alphaFade.ShowPanel();
		}
	}

	public void HidePanel(bool isFast = false)
	{
		if (isFast)
		{
			this.TextInfo.text = string.Empty;
			if (this._alphaFade != null)
			{
				this._alphaFade.FastHidePanel();
			}
		}
		else if (this._alphaFade != null)
		{
			this._alphaFade.HidePanel();
		}
	}

	private void _alphaFade_HideFinished(object sender, EventArgsAlphaFade e)
	{
		this.TextInfo.text = string.Empty;
		this.HideFinished();
	}

	public Text TextInfo;

	private AlphaFade _alphaFade;
}
