using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class TransitionsBuilder : MonoBehaviour
{
	public Selectable cachedSelectable
	{
		get
		{
			if (this.thisSelectable == null)
			{
				this.thisSelectable = base.GetComponent<Selectable>();
			}
			return this.thisSelectable;
		}
	}

	public void OnEnable()
	{
		Object.Destroy(base.gameObject);
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			if (StaticUserData.CurrentPond != null && StaticUserData.CurrentPond.PondId == 2)
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				this.cachedSelectable.interactable = true;
				this.active = true;
				TransitionsController.Instance.RebuildBuilders(this, true);
			}
		}
	}

	public void OnDisable()
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			this.cachedSelectable.interactable = false;
			this.active = false;
			TransitionsController.Instance.RebuildBuilders(this, false);
			this.previousSelectable = null;
			this.EnableSelectable(false);
		}
	}

	public void OnPointerDown()
	{
		this.UpdateContent();
	}

	public void EnableSelectable(bool enabled)
	{
		this.selected = enabled;
		if (enabled)
		{
			ColorBlock colors = this.cachedSelectable.colors;
			colors.highlightedColor = this.highlightColor;
			colors.normalColor = this.highlightColor;
			colors.pressedColor = this.highlightColor;
			colors.disabledColor = this.highlightColor;
			this.cachedSelectable.colors = colors;
			this.cachedSelectable.targetGraphic.color = this.highlightColor;
			if (this.eventTrigger != null)
			{
				this.eventTrigger.OnSelect(new BaseEventData(EventSystem.current));
			}
		}
		else
		{
			ColorBlock colors2 = this.cachedSelectable.colors;
			colors2.normalColor = Vector4.zero;
			colors2.highlightedColor = Vector4.zero;
			colors2.disabledColor = Vector4.zero;
			colors2.pressedColor = Vector4.zero;
			this.cachedSelectable.colors = colors2;
			this.cachedSelectable.targetGraphic.color = this.highlightColor;
			if (this.eventTrigger != null)
			{
				this.eventTrigger.OnDeselect(new BaseEventData(EventSystem.current));
			}
		}
	}

	public void OnSelect()
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			this.EnableSelectable(true);
			this.OnPointerDown();
		}
	}

	public void OnDeselect()
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			this.EnableSelectable(false);
		}
	}

	public void SelectGameObject()
	{
		if (EventSystem.current != null)
		{
			EventSystem.current.SetSelectedGameObject(this.region.GetFirstGameObject(this.rememberLast));
		}
	}

	public void UpdateContent()
	{
		List<Selectable> list = new List<Selectable>();
		foreach (Selectable selectable in this.selectablesRoot.GetComponentsInChildren<Selectable>(true))
		{
			if (!object.ReferenceEquals(selectable, this.cachedSelectable) && selectable.GetType() != typeof(Scrollbar) && selectable.GetType() != typeof(Slider))
			{
				list.Add(selectable);
			}
		}
		this.selectables = list.ToArray();
		if (this.region == null)
		{
			this.region = new TransitionRegion(this.selectables, this.contentNavigation, this.cachedSelectable, this.rememberLast);
		}
		else
		{
			this.region.UpdateContent(this.selectables);
		}
		this.SelectGameObject();
		this.SetContentHighlightColor();
	}

	public void Update()
	{
		if (!this.active)
		{
			if (this.selectablesRoot.gameObject.activeInHierarchy && this.selectablesRoot.transform.lossyScale != Vector3.zero)
			{
				this.OnEnable();
			}
		}
		else if (!this.selectablesRoot.gameObject.activeInHierarchy || this.selectablesRoot.transform.lossyScale == Vector3.zero)
		{
			this.OnDisable();
		}
		if (this.selected && this.active && this.needUpdating)
		{
			this.UpdateContent();
			this.needUpdating = false;
		}
	}

	public void UpdateBuilder()
	{
		if (this.region != null && base.enabled)
		{
			this.region.Update((!(EventSystem.current.currentSelectedGameObject == null)) ? EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>() : null);
		}
		if (EventSystem.current.currentSelectedGameObject != null && this.region != null && !this.needUpdating)
		{
			this.region.RememberLast(EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>(), this.rememberLastByPosition);
		}
	}

	public void ForceUpdate()
	{
		if (this.region != null)
		{
			this.region.ForceUpdate((!(EventSystem.current.currentSelectedGameObject == null)) ? EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>() : null);
		}
	}

	private void OnSelectableChanged()
	{
		this.needUpdating = true;
	}

	private void Awake()
	{
		if (InputModuleManager.GameInputType != InputModuleManager.InputType.GamePad)
		{
			base.gameObject.SetActive(false);
		}
	}

	private void Start()
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			ColorBlock colors = this.cachedSelectable.colors;
			colors.highlightedColor = this.highlightColor;
			this.cachedSelectable.colors = colors;
			this.cachedSelectable.targetGraphic.color = this.highlightColor;
			ChildrenChangedListener childrenChangedListener = this.selectablesRoot.gameObject.AddComponent<ChildrenChangedListener>();
			childrenChangedListener.OnChildrenChanged += this.OnSelectableChanged;
			this.eventTrigger = base.GetComponent<EventTrigger>();
			ChildrenChangedListener[] componentsInChildren = this.selectablesRoot.GetComponentsInChildren<ChildrenChangedListener>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].OnChildrenChanged += this.OnSelectableChanged;
			}
		}
	}

	private void SetContentHighlightColor()
	{
		if (this.contentHighlightColor == Color.clear)
		{
			return;
		}
		foreach (Selectable selectable in this.selectables)
		{
			if ((selectable.transition == null || selectable.transition == 1) && selectable.GetComponent<ToggleStateChanges>() == null)
			{
				selectable.transition = 1;
				ColorBlock colors = selectable.colors;
				if (colors.normalColor.r == colors.highlightedColor.r && colors.normalColor.g == colors.highlightedColor.g && colors.normalColor.b == colors.highlightedColor.b)
				{
					colors.highlightedColor = this.contentHighlightColor;
					selectable.colors = colors;
				}
			}
		}
	}

	[SerializeField]
	private Transform selectablesRoot;

	[SerializeField]
	private Color highlightColor = new Color32(240, 207, 175, byte.MaxValue);

	[SerializeField]
	private Color contentHighlightColor = new Color32(195, 152, 109, byte.MaxValue);

	[SerializeField]
	private TransitionRegion.ContentNavigation contentNavigation;

	[SerializeField]
	private bool rememberLast = true;

	[SerializeField]
	private bool rememberLastByPosition;

	private Selectable[] selectables;

	private TransitionRegion region;

	private Selectable previousSelectable;

	private Selectable lastSelectable;

	private Selectable thisSelectable;

	private EventTrigger eventTrigger;

	private bool selected;

	private bool active;

	private bool needUpdating;
}
