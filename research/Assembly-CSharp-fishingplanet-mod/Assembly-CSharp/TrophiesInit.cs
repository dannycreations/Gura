using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class TrophiesInit : StatsInitBase
{
	protected override void OnDestroy()
	{
		base.OnDestroy();
		PhotonConnectionFactory.Instance.OnGotItems -= this.OnGotItems;
	}

	public static void FillFishStat(List<FishStat> list, IDictionary<int, FishStat> fishStats)
	{
		list.Clear();
		List<int> fishes = new List<int>();
		(from x in fishStats
			where !Enum.IsDefined(typeof(FishTypes), x.Key)
			select x.Value into x
			orderby x.MaxFish.Weight descending
			select x).ToList<FishStat>().ForEach(delegate(FishStat p)
		{
			if (!fishes.Contains(p.MaxFish.FishId))
			{
				fishes.Add(p.MaxFish.FishId);
				list.Add(p);
			}
		});
	}

	public void Refresh(IDictionary<int, FishStat> fishStats)
	{
		TrophiesInit.FillFishStat(this._list, fishStats);
		PhotonConnectionFactory.Instance.OnGotItems += this.OnGotItems;
		List<int> idsArr = new List<int>();
		this._list.ForEach(delegate(FishStat p)
		{
			idsArr.AddRange(p.MaxFishTackleSet);
		});
		int[] array = idsArr.Distinct<int>().ToArray<int>();
		PhotonConnectionFactory.Instance.GetItemsByIds(array, 2, true);
	}

	private void OnGotItems(List<InventoryItem> items, int subscriberId)
	{
		if (subscriberId != 2)
		{
			return;
		}
		this._items = items;
		this.TrophiesCountText.text = string.Format("({0})", this._list.Count);
		this.Init<FishStat>(this._list, null);
		PhotonConnectionFactory.Instance.OnGotItems -= this.OnGotItems;
	}

	protected override void Add(int i)
	{
		TrophiesInit.<Add>c__AnonStorey2 <Add>c__AnonStorey = new TrophiesInit.<Add>c__AnonStorey2();
		GameObject gameObject = GUITools.AddChild(this.ContentPanel, this.TrophyPrefab);
		<Add>c__AnonStorey.fs = this._list[i];
		List<InventoryItem> list = new List<InventoryItem>();
		int j;
		for (j = 0; j < <Add>c__AnonStorey.fs.MaxFishTackleSet.Length; j++)
		{
			list.Add(this._items.FirstOrDefault((InventoryItem p) => p.ItemId == <Add>c__AnonStorey.fs.MaxFishTackleSet[j]));
		}
		gameObject.GetComponent<ConcreteTrophyInit>().Refresh(this._list[i], list);
		Image component = gameObject.GetComponent<Image>();
		component.enabled = i % 2 == 0;
		component.color = this.ListLineColor;
	}

	private const int ItemSubscriberId = 2;

	public GameObject TrophyPrefab;

	public Color ListLineColor;

	public Text TrophiesCountText;

	private List<FishStat> _list = new List<FishStat>();

	private List<InventoryItem> _items = new List<InventoryItem>();
}
