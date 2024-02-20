using System;
using System.Diagnostics;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using InControl;
using TMPro;
using UnityEngine;

public class WindowBase : MessageBoxBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action CancelActionCalled;

	internal override void Start()
	{
		base.Start();
		this.SetControllerHints();
	}

	protected virtual void Update()
	{
		if (Input.GetKeyDown(13))
		{
			this.Accept();
		}
		else if (Input.GetKeyDown(27))
		{
			this.Close();
		}
	}

	public override void Close()
	{
		if (this.CancelActionCalled != null)
		{
			this.CancelActionCalled();
		}
		base.Close();
	}

	public virtual void Accept()
	{
		if (!this.Alpha.IsHiding)
		{
			this.AcceptActionCalled();
			base.Close();
		}
	}

	public static string HintSelectStr
	{
		get
		{
			return string.Format(ScriptLocalization.Get("UGC_PressSelectHint"), UgcConsts.GetYellowTan(HotkeyIcons.KeyMappings[InputControlType.Action1]));
		}
	}

	public static string HintNavigationStr
	{
		get
		{
			return string.Format("{0} {1}", UgcConsts.GetYellowTan("\ue703"), ScriptLocalization.Get("NavigationCategory"));
		}
	}

	protected virtual void AcceptActionCalled()
	{
	}

	protected virtual void SetControllerHints()
	{
		if (this.HintSelect != null)
		{
			this.HintSelect.text = WindowBase.HintSelectStr;
		}
		if (this.HintNavigation != null)
		{
			this.HintNavigation.text = WindowBase.HintNavigationStr;
		}
	}

	[SerializeField]
	protected AlphaFade Alpha;

	[SerializeField]
	protected BorderedButton OkBtn;

	[SerializeField]
	protected BorderedButton CloseBtn;

	[Space(5f)]
	[SerializeField]
	protected TextMeshProUGUI HintSelect;

	[SerializeField]
	protected TextMeshProUGUI HintNavigation;
}
