using System;
using System.Collections.Generic;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TournamentDetailFish : MonoBehaviour
{
	private void OnDestroy()
	{
		CacheLibrary.MapCache.OnFishCategoriesLoaded -= this.MapCache_OnFishCategoriesLoaded;
	}

	public void Init(TournamentDetailFish.TournamentDetailFishData f)
	{
		this.ImgLoadable.Load(f.Fish.ThumbnailBID, this._fishImage, "Textures/Inventory/{0}");
		if (CacheLibrary.MapCache.HasFishCategories)
		{
			this.SetFishName(f.Fish.CategoryId);
		}
		else
		{
			this._result = f;
			CacheLibrary.MapCache.OnFishCategoriesLoaded += this.MapCache_OnFishCategoriesLoaded;
			CacheLibrary.MapCache.GetFishCategories();
		}
		for (int i = 0; i < this._fishTypes.Length; i++)
		{
			if (!f.FishTypesEnable.Contains(this._fishTypes[i].Type))
			{
				this._fishTypes[i].Image.color = new Color(this._fishTypes[i].Image.color.r, this._fishTypes[i].Image.color.g, this._fishTypes[i].Image.color.b, 0f);
			}
		}
	}

	private void MapCache_OnFishCategoriesLoaded()
	{
		CacheLibrary.MapCache.OnFishCategoriesLoaded -= this.MapCache_OnFishCategoriesLoaded;
		this.SetFishName(this._result.Fish.CategoryId);
	}

	private void SetFishName(int categoryId)
	{
		this._fishName.text = CacheLibrary.MapCache.GetFishCategory(categoryId).Name;
	}

	[SerializeField]
	private Image _fishImage;

	[SerializeField]
	private TextMeshProUGUI _fishName;

	[SerializeField]
	private TournamentDetailFish.FishType[] _fishTypes;

	private ResourcesHelpers.AsyncLoadableImage ImgLoadable = new ResourcesHelpers.AsyncLoadableImage();

	private TournamentDetailFish.TournamentDetailFishData _result;

	[Serializable]
	public class FishType
	{
		public UIHelper.FishTypes Type;

		public Image Image;
	}

	public class TournamentDetailFishData
	{
		public TournamentDetailFishData(FishBrief fish)
		{
			this.Fish = fish;
			this.FishTypesEnable = new List<UIHelper.FishTypes>();
		}

		public FishBrief Fish { get; private set; }

		public List<UIHelper.FishTypes> FishTypesEnable { get; private set; }

		public void AddFishTypeEnabled(UIHelper.FishTypes ft)
		{
			this.FishTypesEnable.Add(ft);
		}
	}
}
