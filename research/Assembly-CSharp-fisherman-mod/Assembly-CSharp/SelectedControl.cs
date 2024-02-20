using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectedControl : MonoBehaviour
{
	public void OnDeselect()
	{
		if (this.OnDeselected != null)
		{
			this.OnDeselected.Invoke();
		}
	}

	public void OnSelect()
	{
		if (this.OnSelected != null)
		{
			this.OnSelected.Invoke();
		}
	}

	public void ResetSelected()
	{
		if (EventSystem.current.currentSelectedGameObject != null && this._selectables.Contains(EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>()))
		{
			UINavigation.SetSelectedGameObject(null);
		}
	}

	private void Start()
	{
		this._cg = ActivityState.GetParentActivityState(base.transform).CanvasGroup;
	}

	private void Awake()
	{
		if (this._rootSelectable != null)
		{
			List<Selectable> list = new List<Selectable>();
			foreach (Selectable selectable in this._rootSelectable.GetComponentsInChildren<Selectable>(true))
			{
				if (selectable.GetType() != typeof(Scrollbar) && selectable.GetType() != typeof(Slider))
				{
					list.Add(selectable);
				}
			}
			this._selectables = list.ToArray();
		}
		this.CheckAnySelected();
	}

	private void Update()
	{
		if (this._cg != null && !this._cg.interactable)
		{
			return;
		}
		this.CheckAnySelected();
	}

	public void AddRange(List<Selectable> s)
	{
		List<Selectable> list = this._selectables.ToList<Selectable>();
		list.AddRange(s);
		this._selectables = list.ToArray();
	}

	private void CheckAnySelected()
	{
		if (EventSystem.current.currentSelectedGameObject != null && this._selectables.Contains(EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>()))
		{
			if (!this._selected)
			{
				this.OnSelect();
				this._selected = true;
			}
		}
		else if (this._selected)
		{
			this.OnDeselect();
			this._selected = false;
		}
	}

	[SerializeField]
	private Transform _rootSelectable;

	[SerializeField]
	private Selectable[] _selectables;

	public UnityEvent OnDeselected = new UnityEvent();

	public UnityEvent OnSelected = new UnityEvent();

	private bool _selected;

	private CanvasGroup _cg;
}
