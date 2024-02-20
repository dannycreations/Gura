using System;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

public class HintElementId : MonoBehaviour
{
	protected virtual void Awake()
	{
		this.Add();
	}

	protected virtual void FixedUpdate()
	{
		if (!this.added)
		{
			this.Add();
		}
	}

	protected virtual void OnDestroy()
	{
		this.Remove();
	}

	public string GetElementId()
	{
		return this.ElementId;
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}

	public void SetElementId(string elementId, List<string> cats = null, InventoryItem item = null)
	{
		this.Remove();
		this.ElementId = elementId;
		this.Categories = cats;
		this.Item = item;
		this.Add();
	}

	protected virtual void Add()
	{
		this.CachedTransform = base.transform;
		if (!string.IsNullOrEmpty(this.ElementId) && !HintSystem.ElementIds.ContainsKey(this.ElementId))
		{
			HintSystem.ElementIds.Add(this.ElementId, this);
			if (this.Categories != null && this.Categories.Count > 0)
			{
				HintSystem.RegisterCategories(this.Categories, base.transform, this.Item);
			}
			this.added = true;
		}
	}

	protected virtual void Remove()
	{
		if (!string.IsNullOrEmpty(this.ElementId) && this.added)
		{
			HintSystem.ElementIds.Remove(this.ElementId);
			if (this.Categories != null && this.Categories.Count > 0)
			{
				HintSystem.UnregisterCategories(this.Categories, this.CachedTransform);
			}
			this.Clear();
			this.added = false;
		}
	}

	protected virtual void Clear()
	{
		ManagedHintObject[] componentsInChildren = base.GetComponentsInChildren<ManagedHintObject>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy(componentsInChildren[i].gameObject);
		}
	}

	[SerializeField]
	protected string ElementId;

	protected InventoryItem Item;

	protected List<string> Categories;

	public HintSide PreferredSide;

	public Transform CachedTransform;

	public bool IsPondPin;

	public bool IsLocationPin;

	public bool IsCirclePin;

	public bool IsColorHighlightedTab;

	protected bool added;
}
