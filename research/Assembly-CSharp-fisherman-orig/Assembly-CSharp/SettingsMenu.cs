using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsMenu : MenuBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> OnFonstSizeActive = delegate
	{
	};

	public void OpenFiltersMenu()
	{
		if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
		{
			base.StartCoroutine(this.OpenPanel());
		}
	}

	public void CloseFiltersMenu()
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			this.Filters.SetActive(false);
			this.SettingNavigation.enabled = base.IsActive;
		}
	}

	private IEnumerator OpenPanel()
	{
		yield return null;
		this.Filters.SetActive(true);
		this.FiltersNavigation.SetFirstUpperActive();
		this.SettingNavigation.enabled = false;
		yield break;
	}

	private void Awake()
	{
		this.FontSizeActions.OnDeselected.AddListener(delegate
		{
			this.OnFonstSizeActive(false);
		});
		this.FontSizeActions.OnSelected.AddListener(delegate
		{
			this.OnFonstSizeActive(true);
		});
	}

	private void Update()
	{
		if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
		{
			this.CheckPanel();
		}
	}

	protected override GameObject GetSelectedGameObject()
	{
		return this.button.gameObject;
	}

	protected override bool IsAnySelected()
	{
		return !this._buttons.Any((Selectable p) => p.gameObject == EventSystem.current.currentSelectedGameObject);
	}

	protected override void ActivePanel(bool flag)
	{
		base.ActivePanel(flag);
		if (!flag)
		{
			this.CloseFiltersMenu();
			if (InputModuleManager.GameInputType == InputModuleManager.InputType.Mouse)
			{
				UINavigation.SetSelectedGameObject(null);
			}
		}
	}

	public Button button;

	public List<Selectable> _buttons;

	public GameObject Filters;

	public UINavigation FiltersNavigation;

	public UINavigation SettingNavigation;

	public PointerActionHandler FontSizeActions;
}
