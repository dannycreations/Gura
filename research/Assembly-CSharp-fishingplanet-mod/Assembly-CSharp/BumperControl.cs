using System;
using System.Collections.Generic;
using InControl;
using UnityEngine;
using UnityEngine.UI;

public class BumperControl : MonoBehaviour
{
	public static void PauseForLayersLess(int layer)
	{
		for (int i = 0; i < BumperControl.allControls.Count; i++)
		{
			if (BumperControl.allControls[i]._visibleLayer < layer)
			{
				BumperControl.allControls[i].paused = true;
			}
		}
	}

	public static void UnpauseForLayersGreater(int layer)
	{
		for (int i = 0; i < BumperControl.allControls.Count; i++)
		{
			if (BumperControl.allControls[i]._visibleLayer >= layer)
			{
				BumperControl.allControls[i].paused = false;
			}
		}
	}

	private void OnEnable()
	{
		BlockableRegion componentInParent = base.GetComponentInParent<BlockableRegion>();
		if (componentInParent != null)
		{
			this._visibleLayer = componentInParent.Layer;
		}
		if (this._visibleLayer < BlockableRegion.CurrentLayer)
		{
			this.paused = true;
		}
	}

	private void Awake()
	{
		BumperControl.allControls.Add(this);
	}

	private void OnDestroy()
	{
		BumperControl.allControls.Remove(this);
	}

	private void Start()
	{
		this._cg = ActivityState.GetParentActivityState(base.transform).CanvasGroup;
		if (this.selectablesRoot != null)
		{
			this.selectables = this.selectablesRoot.GetComponentsInChildren<Selectable>();
		}
		this.region = new TransitionRegion(this.selectables, TransitionRegion.ContentNavigation.CustomHorizontal, null, false);
	}

	private void Update()
	{
		if (this._cg != null && !this._cg.interactable)
		{
			return;
		}
		if (this.paused || InputModuleManager.GameInputType != InputModuleManager.InputType.GamePad)
		{
			return;
		}
		Selectable selectable = null;
		if (InputManager.ActiveDevice.LeftBumper.WasPressed || InputManager.ActiveDevice.LeftBumper.WasRepeated)
		{
			this.UpdateCurrentSelected();
			this.region.ForceUpdate(this.currentSelected);
			selectable = this.currentSelected.navigation.selectOnLeft;
		}
		if (InputManager.ActiveDevice.RightBumper.WasPressed || InputManager.ActiveDevice.RightBumper.WasRepeated)
		{
			this.UpdateCurrentSelected();
			this.region.ForceUpdate(this.currentSelected);
			selectable = this.currentSelected.navigation.selectOnRight;
		}
		if (selectable != null)
		{
			this.currentSelected = selectable;
			(this.currentSelected as Toggle).isOn = true;
		}
	}

	public void SetSelectables(Selectable[] newSelectables)
	{
		this.selectables = newSelectables;
		this.region = new TransitionRegion(this.selectables, TransitionRegion.ContentNavigation.CustomHorizontal, null, false);
	}

	private void UpdateCurrentSelected()
	{
		for (int i = 0; i < this.selectables.Length; i++)
		{
			Toggle toggle = this.selectables[i] as Toggle;
			if (toggle.isOn)
			{
				this.currentSelected = toggle;
			}
		}
	}

	private Selectable currentSelected;

	[SerializeField]
	private Transform selectablesRoot;

	private TransitionRegion region;

	[SerializeField]
	private Selectable[] selectables = new Selectable[0];

	private int _visibleLayer;

	private bool paused;

	private static List<BumperControl> allControls = new List<BumperControl>();

	private CanvasGroup _cg;
}
