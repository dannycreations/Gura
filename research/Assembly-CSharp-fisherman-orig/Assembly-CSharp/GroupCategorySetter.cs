using System;
using System.Collections.Generic;
using System.Linq;
using InventorySRIA;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GroupCategorySetter : MonoBehaviour
{
	private void Start()
	{
		if (this.Adapter == null)
		{
			this.Adapter = Object.FindObjectOfType<global::InventorySRIA.InventorySRIA>();
		}
	}

	public void Set(bool isOn)
	{
		this.Start();
		if (isOn)
		{
			this.Adapter.SetSupportedGroups(this.SupportedGroup);
		}
	}

	public void Init(GroupCategoryIcon group, ToggleGroup toggleGroup)
	{
		this.Toggle.onValueChanged.AddListener(new UnityAction<bool>(this.Set));
		this.Toggle.onValueChanged.AddListener(new UnityAction<bool>(this.Sound.OnSetToogle));
		this.ColorTransition.Subscribe();
		this.Toggle.group = toggleGroup;
		this.SupportedGroup = group.type;
		this.Icon.text = group.icon;
		if (this.SupportedGroup != GroupCategoryType.All)
		{
			List<string> list = (from x in global::InventorySRIA.InventorySRIA.GetCategories(this.SupportedGroup)
				select "tab" + x).ToList<string>();
			list.AddRange((from x in global::InventorySRIA.InventorySRIA.GetParentCategories(this.SupportedGroup)
				select "tab" + x).ToList<string>());
			this.HintElement.SetElementId(group.hintElementId, list, null);
		}
		else
		{
			this.HintElement.SetElementId(group.hintElementId, null, null);
		}
	}

	public global::InventorySRIA.InventorySRIA Adapter;

	public ToggleColorTransitionChanges ColorTransition;

	public GroupCategoryType SupportedGroup;

	public HintElementId HintElement;

	public PlayButtonEffect Sound;

	public TextMeshProUGUI Icon;

	public Toggle Toggle;
}
