using System;
using System.Collections.Generic;
using InControl;
using UnityEngine;
using UnityEngine.UI;

public class ScrollbarNavigation : ActivityStateControlled
{
	public static void PauseForLayersLess(int layer)
	{
		for (int i = 0; i < ScrollbarNavigation.allNavigation.Count; i++)
		{
			if (ScrollbarNavigation.allNavigation[i]._visibleLayer < layer)
			{
				ScrollbarNavigation.allNavigation[i].paused = true;
			}
		}
	}

	public static void UnpauseForLayersGreater(int layer)
	{
		for (int i = 0; i < ScrollbarNavigation.allNavigation.Count; i++)
		{
			if (ScrollbarNavigation.allNavigation[i]._visibleLayer >= layer)
			{
				ScrollbarNavigation.allNavigation[i].paused = false;
			}
		}
	}

	public void SetScrollRect(ScrollRect newRect)
	{
		this._scrollrect = newRect;
	}

	protected override void OnEnable()
	{
		BlockableRegion componentInParent = base.GetComponentInParent<BlockableRegion>();
		if (componentInParent != null)
		{
			this._visibleLayer = componentInParent.Layer;
		}
		ScrollbarNavigation.allNavigation.Add(this);
		base.OnEnable();
	}

	protected override void SetHelp()
	{
		HelpLinePanel.SetActionHelp(this._leftBinding);
		HelpLinePanel.SetActionHelp(this._rightBinding);
		HelpLinePanel.SetActionHelp(this._upBinding);
		HelpLinePanel.SetActionHelp(this._downBinding);
	}

	protected override void HideHelp()
	{
		this.ClearIncreasing();
		HelpLinePanel.HideActionHelp(this._leftBinding);
		HelpLinePanel.HideActionHelp(this._rightBinding);
		HelpLinePanel.HideActionHelp(this._upBinding);
		HelpLinePanel.HideActionHelp(this._downBinding);
	}

	protected override void OnDisable()
	{
		ScrollbarNavigation.allNavigation.Remove(this);
		base.OnDisable();
	}

	private void Update()
	{
		if (this.paused || InputModuleManager.GameInputType != InputModuleManager.InputType.GamePad || !base.ShouldUpdate())
		{
			return;
		}
		InputControl inputControl = InputManager.ActiveDevice.GetControl(this._leftBinding.Hotkey);
		InputControl inputControl2 = InputManager.ActiveDevice.GetControl(this._rightBinding.Hotkey);
		InputControl inputControl3 = InputManager.ActiveDevice.GetControl(this._downBinding.Hotkey);
		InputControl inputControl4 = InputManager.ActiveDevice.GetControl(this._upBinding.Hotkey);
		InputControl inputControl5 = this.GetPressed(inputControl, inputControl2, inputControl3, inputControl4);
		if (!inputControl5.IsPressed)
		{
			inputControl = InputManager.ActiveDevice.GetControl(this._leftBinding.AlternativeHotkey);
			inputControl2 = InputManager.ActiveDevice.GetControl(this._rightBinding.AlternativeHotkey);
			inputControl3 = InputManager.ActiveDevice.GetControl(this._downBinding.AlternativeHotkey);
			inputControl4 = InputManager.ActiveDevice.GetControl(this._upBinding.AlternativeHotkey);
			inputControl5 = this.GetPressed(inputControl, inputControl2, inputControl3, inputControl4);
		}
		if (inputControl5.IsPressed)
		{
			Vector2 vector = Vector2.zero;
			if (inputControl5 == inputControl)
			{
				int num = this.IncreaseValue(this._leftBinding);
				float num2 = this._deltaVal + (float)num;
				vector -= new Vector2(num2 * inputControl5.Value, 0f);
				this.CheckIncreasing(this._leftBinding);
			}
			if (inputControl5 == inputControl2)
			{
				int num3 = this.IncreaseValue(this._rightBinding);
				float num4 = this._deltaVal + (float)num3;
				vector += new Vector2(num4 * inputControl5.Value, 0f);
				this.CheckIncreasing(this._rightBinding);
			}
			if (inputControl5 == inputControl4)
			{
				int num5 = this.IncreaseValue(this._upBinding);
				float num6 = this._deltaVal + (float)num5;
				vector += new Vector2(0f, num6 * inputControl5.Value);
				this.CheckIncreasing(this._upBinding);
			}
			if (inputControl5 == inputControl3)
			{
				int num7 = this.IncreaseValue(this._downBinding);
				float num8 = this._deltaVal + (float)num7;
				vector -= new Vector2(0f, num8 * inputControl5.Value);
				this.CheckIncreasing(this._downBinding);
			}
			if (vector != Vector2.zero)
			{
				Vector2 vector2 = this._scrollrect.velocity - this._scrollrect.scrollSensitivity * vector * Time.deltaTime;
				if (vector2.SqrMagnitude() > this._maxScrollVelocity * this._maxScrollVelocity)
				{
					vector2 = vector2.normalized * this._maxScrollVelocity;
				}
				this._scrollrect.velocity = vector2;
				this._scrollrect.verticalNormalizedPosition = Mathf.Clamp01(this._scrollrect.verticalNormalizedPosition);
			}
		}
		else
		{
			this.ClearIncreasing();
		}
	}

	private InputControl GetPressed(InputControl lb, InputControl rb, InputControl db, InputControl ub)
	{
		InputControl inputControl = ((lb.Value <= rb.Value) ? rb : lb);
		inputControl = ((inputControl.Value <= db.Value) ? db : inputControl);
		return (inputControl.Value <= ub.Value) ? ub : inputControl;
	}

	private void CheckIncreasing(HotkeyBinding b)
	{
		if (b.IsIncreasing)
		{
			int id = b.Id;
			if (!this._increasing.ContainsKey(id))
			{
				this._increasing[id] = new List<DateTime>();
				this._increasingCounter[id] = 0;
			}
			this._increasing[id].Add(DateTime.Now);
			int count = this._increasing[id].Count;
			DateTime dateTime = this._increasing[id][count - 1];
			double totalSeconds = (dateTime - this._increasing[id][0]).TotalSeconds;
			double num = 0.0;
			if (count > 1)
			{
				num = (dateTime - this._increasing[id][count - 2]).TotalSeconds;
			}
			if (num > 2.0)
			{
				this._increasingCounter[id] = 0;
				this._increasing[id].Clear();
				this._increasing[id].Add(DateTime.Now);
			}
			else if (totalSeconds >= 1.5)
			{
				Dictionary<int, int> increasingCounter;
				int num2;
				(increasingCounter = this._increasingCounter)[num2 = id] = increasingCounter[num2] + 1;
			}
		}
	}

	private void ClearIncreasing()
	{
		this._increasing.Clear();
		this._increasingCounter.Clear();
	}

	private int IncreaseValue(HotkeyBinding b)
	{
		return (!b.IsIncreasing || !this._increasingCounter.ContainsKey(b.Id)) ? 0 : this._increasingCounter[b.Id];
	}

	[SerializeField]
	private float _deltaVal = 10f;

	[SerializeField]
	private float _maxScrollVelocity = 300f;

	[SerializeField]
	private HotkeyBinding _leftBinding;

	[SerializeField]
	private HotkeyBinding _rightBinding;

	[SerializeField]
	private HotkeyBinding _upBinding;

	[SerializeField]
	private HotkeyBinding _downBinding;

	[SerializeField]
	private ScrollRect _scrollrect;

	private int _visibleLayer;

	private bool paused;

	private static List<ScrollbarNavigation> allNavigation = new List<ScrollbarNavigation>();

	private const int IncreasingSpeed = 1;

	private const float TimeForIncreasing = 1.5f;

	private const float TimeForStopIncreasing = 2f;

	private Dictionary<int, List<DateTime>> _increasing = new Dictionary<int, List<DateTime>>();

	private Dictionary<int, int> _increasingCounter = new Dictionary<int, int>();
}
