using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class TooltipManager : IDisposable
{
	public TooltipManager(int maxCount = 1)
	{
		this._maxCount = maxCount;
	}

	public void Add(string elementId, Vector3 scale, string text, HintSide side = HintSide.Undefined)
	{
	}

	private void TooltipHint_OnDestroyed(ManagedHintObject obj)
	{
		TooltipHint tooltipHint = (TooltipHint)obj;
		KeyValuePair<string, TooltipHint> keyValuePair = this._tooltipHints.FirstOrDefault((KeyValuePair<string, TooltipHint> p) => p.Value == tooltipHint);
		if (!keyValuePair.Equals(default(KeyValuePair<string, TooltipHint>)))
		{
			this.Remove(keyValuePair.Key);
		}
	}

	public void SetActive(string elementId, bool flag)
	{
		if (this._tooltipHints.ContainsKey(elementId))
		{
			if (flag)
			{
				if (this._activeTooltipHints.Count == this._maxCount)
				{
					this._activeTooltipHints.RemoveAt(0);
				}
				this._activeTooltipHints.Add(elementId);
			}
			else if (this._activeTooltipHints.Contains(elementId))
			{
				this._activeTooltipHints.Remove(elementId);
			}
		}
	}

	public bool IsActive(string elementId)
	{
		return this._activeTooltipHints.Contains(elementId);
	}

	public bool IsAdded(string elementId)
	{
		return this._tooltipHints.ContainsKey(elementId);
	}

	public void Remove(string elementId)
	{
		if (this._activeTooltipHints.Contains(elementId))
		{
			this._activeTooltipHints.Remove(elementId);
		}
		if (this._tooltipHints.ContainsKey(elementId))
		{
			this._tooltipHints.Remove(elementId);
		}
	}

	public void Dispose()
	{
		foreach (KeyValuePair<string, TooltipHint> keyValuePair in this._tooltipHints)
		{
			Object.Destroy(keyValuePair.Value.gameObject);
		}
		this._tooltipHints.Clear();
		this._activeTooltipHints.Clear();
	}

	private Dictionary<string, TooltipHint> _tooltipHints = new Dictionary<string, TooltipHint>();

	private List<string> _activeTooltipHints = new List<string>();

	private readonly int _maxCount;
}
