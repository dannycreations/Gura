using System;
using System.Collections.Generic;
using System.Linq;
using InControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CircleNavigation : ActivityStateControlled
{
	public static void PauseForLayersLess(int layer)
	{
		for (int i = 0; i < CircleNavigation.allNavigations.Count; i++)
		{
			if (CircleNavigation.allNavigations[i]._visibleLayer < layer)
			{
				CircleNavigation.allNavigations[i].paused = true;
			}
		}
	}

	public static void UnpauseForLayersGreater(int layer)
	{
		for (int i = 0; i < CircleNavigation.allNavigations.Count; i++)
		{
			if (CircleNavigation.allNavigations[i]._visibleLayer >= layer)
			{
				CircleNavigation.allNavigations[i].paused = false;
			}
		}
	}

	private void Awake()
	{
		CircleNavigation.allNavigations.Add(this);
		if (this.selectablesRoot != null)
		{
			ChildrenChangedListener[] componentsInChildren = this.selectablesRoot.GetComponentsInChildren<ChildrenChangedListener>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].OnChildrenChanged += this.OnTransformChildrenChanged;
			}
		}
	}

	private void OnTransformChildrenChanged()
	{
		this.needUpdate = true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		CircleNavigation.allNavigations.Remove(this);
	}

	protected override void Start()
	{
		base.Start();
		this.UpdateSelectables();
	}

	private void UpdateSelectables()
	{
		if (this.selectablesRoot != null)
		{
			this.selectables = this.selectablesRoot.GetComponentsInChildren<Selectable>();
		}
		this.region = new TransitionRegion(this.selectables, (!this.horizontalNavigation) ? TransitionRegion.ContentNavigation.CustomVertical : TransitionRegion.ContentNavigation.CustomHorizontal, null, false);
		this.UpdateCurrentSelected();
		this.DisableSelectables(true);
		this.needUpdate = false;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
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

	protected override void SetHelp()
	{
		if (!string.IsNullOrEmpty(this._leftBinding.LocalizationKey))
		{
			HelpLinePanel.SetActionHelp(this._leftBinding);
		}
		if (!string.IsNullOrEmpty(this._rightBinding.LocalizationKey))
		{
			HelpLinePanel.SetActionHelp(this._rightBinding);
		}
	}

	protected override void HideHelp()
	{
		if (this.currentSelected != null && EventSystem.current != null && !EventSystem.current.alreadySelecting)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
		if (!string.IsNullOrEmpty(this._leftBinding.LocalizationKey))
		{
			HelpLinePanel.HideActionHelp(this._leftBinding);
		}
		if (!string.IsNullOrEmpty(this._rightBinding.LocalizationKey))
		{
			HelpLinePanel.HideActionHelp(this._rightBinding);
		}
	}

	private void Update()
	{
		if (this.paused || InputModuleManager.GameInputType != InputModuleManager.InputType.GamePad || !base.ShouldUpdate())
		{
			return;
		}
		if (this.needUpdate)
		{
			this.UpdateSelectables();
			return;
		}
		bool flag = InputManager.ActiveDevice.GetControl(this._leftBinding.Hotkey).WasPressed || InputManager.ActiveDevice.GetControl(this._leftBinding.Hotkey).WasRepeated;
		bool flag2 = InputManager.ActiveDevice.GetControl(this._rightBinding.Hotkey).WasPressed || InputManager.ActiveDevice.GetControl(this._rightBinding.Hotkey).WasRepeated;
		if (flag || flag2)
		{
			this.UpdateCurrentSelected();
			if (this.currentSelected != null)
			{
				this.region.ForceUpdate(this.currentSelected);
				Selectable selectable = ((!this.horizontalNavigation) ? ((!flag) ? this.currentSelected.navigation.selectOnDown : this.currentSelected.navigation.selectOnUp) : ((!flag) ? this.currentSelected.navigation.selectOnRight : this.currentSelected.navigation.selectOnLeft));
				this.currentSelected = selectable;
				if (this.currentSelected != null)
				{
					(this.currentSelected as Toggle).isOn = true;
				}
			}
			this.DisableSelectables(true);
		}
	}

	public void AddSelectable(Selectable item)
	{
		List<Selectable> list = new List<Selectable>(this.selectables);
		if (!list.Contains(item))
		{
			list.Add(item);
			this.selectables = list.ToArray();
			if (this.region != null)
			{
				this.region.UpdateContent(this.selectables);
			}
			return;
		}
	}

	public void RemoveSelectable(Selectable item)
	{
		List<Selectable> list = new List<Selectable>(this.selectables);
		if (list.Contains(item))
		{
			list.Remove(item);
			this.selectables = list.ToArray();
			if (this.region != null)
			{
				this.region.UpdateContent(this.selectables);
			}
			return;
		}
	}

	public void SetSelectablesInteractable(bool flag)
	{
		this.selectables.ToList<Selectable>().ForEach(delegate(Selectable p)
		{
			p.interactable = flag;
		});
		this.disableSelectables = !flag;
	}

	private void UpdateCurrentSelected()
	{
		this.DisableSelectables(false);
		for (int i = 0; i < this.selectables.Length; i++)
		{
			Toggle toggle = this.selectables[i] as Toggle;
			if (toggle != null && toggle.isOn)
			{
				this.currentSelected = toggle;
			}
		}
	}

	private void DisableSelectables(bool disable)
	{
		if (this.disableSelectables)
		{
			for (int i = 0; i < this.selectables.Length; i++)
			{
				this.selectables[i].interactable = !disable;
			}
		}
	}

	private Selectable currentSelected;

	[SerializeField]
	private Transform selectablesRoot;

	private TransitionRegion region;

	[SerializeField]
	private Selectable[] selectables = new Selectable[0];

	[SerializeField]
	private bool disableSelectables;

	[SerializeField]
	private HotkeyBinding _leftBinding;

	[SerializeField]
	private HotkeyBinding _rightBinding;

	[SerializeField]
	private bool horizontalNavigation = true;

	private int _visibleLayer;

	private bool paused;

	private bool needUpdate;

	private static List<CircleNavigation> allNavigations = new List<CircleNavigation>();
}
