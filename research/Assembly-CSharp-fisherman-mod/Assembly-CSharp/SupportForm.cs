using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SupportForm : MonoBehaviour
{
	private void OnEnable()
	{
		for (int i = 0; i < this._toggles.Count; i++)
		{
			this._toggles[i].isOn = i == 0;
		}
	}

	private void Awake()
	{
		this._togglesRoot.allowSwitchOff = true;
		for (int i = 0; i < this._togglesRoot.transform.childCount; i++)
		{
			Toggle component = this._togglesRoot.transform.GetChild(i).GetComponent<Toggle>();
			if (component != null)
			{
				component.group = this._togglesRoot;
				this._toggles.Add(component);
				component.isOn = false;
				SupportCategory i1 = (SupportCategory)i;
				component.onValueChanged.AddListener(delegate(bool flag)
				{
					if (flag)
					{
						this.SetPage(i1);
					}
				});
			}
		}
		this._toggles[0].isOn = true;
		this._togglesRoot.allowSwitchOff = false;
	}

	private void SetPage(SupportCategory pageType)
	{
		if (this._curCategory != null)
		{
			if (this._curCategory.Value == pageType)
			{
				return;
			}
			if (this._curCategory.Value == SupportCategory.Info)
			{
				this._infoForm.Deactivate();
			}
			else if (pageType == SupportCategory.Info)
			{
				this._submitionForm.Deactivate();
			}
		}
		this._curCategory = new SupportCategory?(pageType);
		((pageType != SupportCategory.Info) ? this._submitionForm : this._infoForm).SetupWindow(pageType);
	}

	private void OnDestroy()
	{
		for (int i = 0; i < this._toggles.Count; i++)
		{
			this._toggles[i].onValueChanged.RemoveAllListeners();
		}
	}

	[SerializeField]
	private SupportBaseForm _infoForm;

	[SerializeField]
	private SupportBaseForm _submitionForm;

	[Tooltip("Node with ToggleGroup component and Toggle as children")]
	[SerializeField]
	private ToggleGroup _togglesRoot;

	private List<Toggle> _toggles = new List<Toggle>();

	private SupportCategory? _curCategory;
}
