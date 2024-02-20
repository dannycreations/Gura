using System;
using System.Diagnostics;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using UnityEngine;
using UnityEngine.UI;

public class WindowLevelMinMax : WindowBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<Range> OnSelected = delegate(Range range)
	{
	};

	protected override void Awake()
	{
		base.Awake();
		this.IfCheckerMinLevel.Init(this.IfMinLevel);
		this.IfCheckerMaxLevel.Init(this.IfMaxLevel);
		for (int i = 0; i < this.Toggles.Length; i++)
		{
			HotkeyPressRedirect hkpr = this.Toggles[i].GetComponent<HotkeyPressRedirect>();
			this.Toggles[i].GetComponent<ToggleColorTransitionChanges>().OnStateChanged += delegate(ToggleColorTransitionChanges.SelectionState s)
			{
				if (hkpr != null)
				{
					hkpr.SetPausedFromScript(s != ToggleColorTransitionChanges.SelectionState.Highlighted);
				}
			};
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		this.IfCheckerMinLevel.Dispose();
		this.IfCheckerMaxLevel.Dispose();
	}

	public void Init(Range current, Range minRange, Range maxRange)
	{
		this.IfCheckerMinLevel.SetRange(minRange, new int?(current.Min));
		this.IfCheckerMaxLevel.SetRange(maxRange, new int?(current.Max));
		this.IfCheckerMinLevel.OnValueChanged += delegate(int v)
		{
			this.CheckBtnsInteractable(v, minRange, this.MinBtns);
		};
		this.IfCheckerMaxLevel.OnValueChanged += delegate(int v)
		{
			this.CheckBtnsInteractable(v, maxRange, this.MaxBtns);
		};
		this.CheckBtnsInteractable(this.IfCheckerMinLevel.Value, minRange, this.MinBtns);
		this.CheckBtnsInteractable(this.IfCheckerMaxLevel.Value, maxRange, this.MaxBtns);
		UINavigation.SetSelectedGameObject(this.Toggles[0].gameObject);
	}

	public virtual void MinInc(int v)
	{
		this.IfCheckerMinLevel.Inc(v);
	}

	public virtual void MaxInc(int v)
	{
		this.IfCheckerMaxLevel.Inc(v);
	}

	protected void CheckBtnsInteractable(int v, Range r, Button[] btns)
	{
		btns[0].interactable = v < r.Max;
		btns[1].interactable = v > r.Min;
		WindowScheduleAndDuration.SetUpDownColor(btns[0]);
		WindowScheduleAndDuration.SetUpDownColor(btns[1]);
	}

	protected override void AcceptActionCalled()
	{
		this.OnSelected(new Range(this.IfCheckerMinLevel.Value, this.IfCheckerMaxLevel.Value));
	}

	[SerializeField]
	protected Button[] MinBtns;

	[SerializeField]
	protected Button[] MaxBtns;

	[SerializeField]
	protected InputField IfMinLevel;

	[SerializeField]
	protected InputField IfMaxLevel;

	[SerializeField]
	protected Toggle[] Toggles;

	protected InputFieldMinMaxChecker IfCheckerMinLevel = new InputFieldMinMaxChecker();

	protected InputFieldMinMaxChecker IfCheckerMaxLevel = new InputFieldMinMaxChecker();
}
