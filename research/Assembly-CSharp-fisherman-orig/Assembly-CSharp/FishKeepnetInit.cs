using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class FishKeepnetInit : ActivityStateControlled
{
	protected override void SetHelp()
	{
		this.Clear();
		PhotonConnectionFactory.Instance.OnReleaseFishFromFishCage += this.OnRelease;
		PhotonConnectionFactory.Instance.OnReleaseFishFromFishCageFailed += this.OnReleaseFailed;
		this.InitFishKeepnet();
		UIStatsCollector.ChangeGameScreen(GameScreenType.Fishkeeper, GameScreenTabType.Undefined, null, null, null, null, null);
	}

	protected override void HideHelp()
	{
		PhotonConnectionFactory.Instance.OnReleaseFishFromFishCage -= this.OnRelease;
		PhotonConnectionFactory.Instance.OnReleaseFishFromFishCageFailed -= this.OnReleaseFailed;
	}

	internal void OnRelease(Guid fishInstanceId)
	{
		if (PhotonConnectionFactory.Instance.Profile == null || PhotonConnectionFactory.Instance.Profile.FishCage == null || PhotonConnectionFactory.Instance.Profile.FishCage.Fish == null)
		{
			return;
		}
		CaughtFish caughtFish = PhotonConnectionFactory.Instance.Profile.FishCage.FindFishById(fishInstanceId);
		if (caughtFish != null)
		{
			PhotonConnectionFactory.Instance.Profile.FishCage.RemoveFish(caughtFish);
		}
		this.InitFishKeepnet();
	}

	internal void OnReleaseFailed(Failure failure)
	{
		LogHelper.Log(failure.ErrorMessage);
		this.InitFishKeepnet();
	}

	private void Clear()
	{
		for (int i = 0; i < this.ContentPanel.transform.childCount; i++)
		{
			Object.Destroy(this.ContentPanel.transform.GetChild(i).gameObject);
		}
	}

	private void InitFishKeepnet()
	{
		if (PhotonConnectionFactory.Instance.Profile != null)
		{
			FishCageContents fishCage = PhotonConnectionFactory.Instance.Profile.FishCage;
			if (fishCage != null && fishCage.Cage != null)
			{
				Text info = this.Info;
				string text = ScriptLocalization.Get("FishKeepNetInfo");
				object[] array = new object[7];
				array[0] = MeasuringSystemManager.FishWeight(fishCage.Cage.MaxFishWeight).ToString("n3");
				array[1] = MeasuringSystemManager.FishWeightSufix();
				int num = 2;
				float? weight = fishCage.Weight;
				array[num] = MeasuringSystemManager.FishWeight((weight == null) ? 0f : weight.Value).ToString("n3");
				array[3] = MeasuringSystemManager.FishWeight(fishCage.Cage.TotalWeight).ToString("n3");
				array[4] = fishCage.Count;
				array[5] = fishCage.SilverCost;
				array[6] = fishCage.GoldCost;
				info.text = string.Format(text, array);
				if (fishCage.Fish != null)
				{
					if (fishCage.Fish.Count > 0)
					{
						PhotonConnectionFactory.Instance.OnGotItems += this.OnGotItems;
						PhotonConnectionFactory.Instance.GetItemsByIds(fishCage.Fish.SelectMany((CaughtFish x) => x.AllBaitIds).Distinct<int>().ToArray<int>(), 1, true);
					}
					else
					{
						this.Clear();
					}
				}
				else
				{
					this.Clear();
				}
			}
			else
			{
				this.Info.text = ScriptLocalization.Get("NoFishKeepNetText");
			}
		}
	}

	private void OnGotItems(List<InventoryItem> items, int subscriberId)
	{
		if (subscriberId != 1)
		{
			return;
		}
		FishCageContents fishCage = PhotonConnectionFactory.Instance.Profile.FishCage;
		if (fishCage == null || fishCage.Fish == null)
		{
			return;
		}
		List<CaughtFish> list = fishCage.Fish.OrderBy((CaughtFish x) => x.Fish.Weight).ToList<CaughtFish>();
		this.AddItems(list, items);
		PhotonConnectionFactory.Instance.OnGotItems -= this.OnGotItems;
	}

	private void AddItems(List<CaughtFish> fishes, List<InventoryItem> items)
	{
		this.Clear();
		fishes.ForEach(delegate(CaughtFish f)
		{
			GUITools.AddChild(this.ContentPanel, this.FishItemPrefab).GetComponent<ConcreteFishInFishkeeper>().InitItem(f, items.Where((InventoryItem x) => f.AllBaitIds.Contains(x.ItemId)).ToList<InventoryItem>());
		});
	}

	internal void SortCurrentContent(SortType sortType)
	{
		this.InitFishKeepnet();
	}

	public Text Name;

	public Text Info;

	public GameObject ContentPanel;

	public GameObject FishItemPrefab;

	public SortingFishkeepnet SortingControl;

	private const int ItemSubscriberId = 1;
}
