using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlockableRegion : ActivityStateControlled
{
	public static int GetLayerForTransform(Transform t)
	{
		int num = 0;
		for (int i = 0; i < BlockableRegion._regions.Count; i++)
		{
			BlockableRegion blockableRegion = BlockableRegion._regions[i];
			if (t.IsChildOf(blockableRegion.transform) && num < blockableRegion.Layer)
			{
				num = blockableRegion.Layer;
			}
		}
		return num;
	}

	public GameObject GetSavedLastSelected()
	{
		return this._lastselected;
	}

	public void OverrideLastSelected(GameObject objectToSelect)
	{
		this._lastselected = objectToSelect;
	}

	protected override void SetHelp()
	{
		BlockableRegion._regions.Add(this);
		if (this.Layer >= BlockableRegion.CurrentLayer)
		{
			BlockableRegion.Current = this;
			BlockableRegion.CurrentLayer = this.Layer;
			CircleNavigation.PauseForLayersLess(BlockableRegion.CurrentLayer);
			UINavigation.PauseForLayersLess(BlockableRegion.CurrentLayer);
			BumperControl.PauseForLayersLess(BlockableRegion.CurrentLayer);
			HotkeyPressRedirect.PauseForLayersLess(BlockableRegion.CurrentLayer);
			InGameMap.PauseForLayersLess(BlockableRegion.CurrentLayer);
			if (EventSystem.current != null)
			{
				this._lastselected = EventSystem.current.currentSelectedGameObject;
				UINavigation.SetSelectedGameObject(null);
			}
			else
			{
				this._lastselected = null;
			}
		}
	}

	protected override void HideHelp()
	{
		if (BlockableRegion._regions.Contains(this))
		{
			BlockableRegion._regions.Remove(this);
		}
		if (BlockableRegion.CurrentLayer == this.Layer)
		{
			BlockableRegion._regions.Sort((BlockableRegion x, BlockableRegion y) => y.Layer.CompareTo(x.Layer));
			if (BlockableRegion._regions.Count == 0)
			{
				BlockableRegion.CurrentLayer = 0;
				BlockableRegion.Current = null;
			}
			else
			{
				BlockableRegion.Current = BlockableRegion._regions[0];
				BlockableRegion.CurrentLayer = BlockableRegion.Current.Layer;
			}
			float interfaceVolume = SettingsManager.InterfaceVolume;
			SettingsManager.InterfaceVolume = 0f;
			if (this._lastselected != null && this._lastselected.GetComponent<IgnoredSelectable>() == null)
			{
				UINavigation.SetSelectedGameObject(this._lastselected);
			}
			SettingsManager.InterfaceVolume = interfaceVolume;
			this._lastselected = null;
			InGameMap.UnpauseForLayersGreater(BlockableRegion.CurrentLayer);
			CircleNavigation.UnpauseForLayersGreater(BlockableRegion.CurrentLayer);
			UINavigation.UnpauseForLayersGreater(BlockableRegion.CurrentLayer);
			BumperControl.UnpauseForLayersGreater(BlockableRegion.CurrentLayer);
			HotkeyPressRedirect.UnpauseForLayersGreater(BlockableRegion.CurrentLayer);
		}
	}

	public int Layer;

	public static BlockableRegion Current;

	public static int CurrentLayer = 0;

	private static List<BlockableRegion> _regions = new List<BlockableRegion>();

	private GameObject _lastselected;
}
