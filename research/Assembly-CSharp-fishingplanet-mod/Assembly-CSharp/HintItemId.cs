using System;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

public class HintItemId : MonoBehaviour
{
	private void Awake()
	{
		this.Add();
	}

	private void Add()
	{
		if (this.Item != null)
		{
			HintSystem.RegisterItemId(base.transform, this.Item);
			if (this.Categories != null && this.Categories.Count > 0)
			{
				HintSystem.RegisterCategories(this.Categories, base.transform, this.Item);
			}
			this.added = true;
		}
	}

	public void SetRemoveOnDisable(bool remove)
	{
		this.removeOnDisable = remove;
	}

	public void Remove()
	{
		if (this.Item != null && this.added)
		{
			HintSystem.UnregisterItemId(this.Item.ItemId, base.transform);
			if (this.Categories != null && this.Categories.Count > 0)
			{
				HintSystem.UnregisterCategories(this.Categories, base.transform);
			}
			this.Clear();
			this.added = false;
		}
	}

	private void Clear()
	{
		ManagedHintObject[] componentsInChildren = base.GetComponentsInChildren<ManagedHintObject>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!componentsInChildren[i].BeenActive)
			{
				componentsInChildren[i].RemoveManual();
			}
			Object.Destroy(componentsInChildren[i].gameObject);
		}
		this.Item = null;
	}

	private void FixedUpdate()
	{
		if (!this.added)
		{
			this.Add();
		}
	}

	public void SetItemId(InventoryItem item, List<string> cats)
	{
		this.Remove();
		this.Item = item;
		this.Categories = cats;
		this.Add();
	}

	private void OnDestroy()
	{
		this.Remove();
	}

	private void OnDisable()
	{
		if (this.removeOnDisable)
		{
			this.Remove();
		}
	}

	protected List<string> Categories;

	private bool added;

	private bool removeOnDisable;

	private InventoryItem Item;
}
