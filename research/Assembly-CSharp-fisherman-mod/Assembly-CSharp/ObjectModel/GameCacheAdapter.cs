using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectModel
{
	public class GameCacheAdapter
	{
		public MissionsContext Context { get; set; }

		public GameCacheAdapter.GetInventoryHandler GetInventory { get; set; }

		public GameCacheAdapter.GetInventoryItemsInGlobalShopHandler GetInventoryItemsInGlobalShop { get; set; }

		public GameCacheAdapter.GetInventoryItemsInLocalShopHandler GetInventoryItemsInLocalShop { get; set; }

		public GameCacheAdapter.GetInventoryCategoryHandler GetInventoryCategory { get; set; }

		public InventoryItem ResolveItemInventory(int itemId)
		{
			InventoryItem inventoryItem = this.Context.GetProfile().Inventory.FirstOrDefault((InventoryItem i) => i.ItemId == itemId);
			if (inventoryItem != null)
			{
				return inventoryItem;
			}
			return null;
		}

		public InventoryItem ResolveItemGloablly(int itemId)
		{
			if (this.GetInventory != null)
			{
				return this.GetInventory(itemId, this.Context.GetProfile().LanguageId);
			}
			return new InventoryItem
			{
				ItemId = itemId
			};
		}

		public InventoryItem ResolveItemInventoryOrGlobally(int itemId)
		{
			return this.ResolveItemInventory(itemId) ?? this.ResolveItemGloablly(itemId);
		}

		public bool IsInLocalShop(int itemId, MissionInventoryFilter filter = null)
		{
			if (this.Context.PondId == 0)
			{
				return false;
			}
			if (this.GetInventoryItemsInLocalShop == null)
			{
				return false;
			}
			InventoryItem[] array = this.GetInventoryItemsInLocalShop(this.Context.PondId);
			Pond pond = this.ResolvePond(this.Context.PondId);
			bool isPondUnlocked = pond != null && pond.MinLevel > this.Context.Level;
			if (itemId < 0)
			{
				int categoryId = -itemId;
				return (from item in array
					where (int)item.ItemType == categoryId || (int)item.ItemSubType == categoryId
					where item.MinLevel <= this.Context.Level || (isPondUnlocked && item.RaretyId == 3)
					where MissionInventoryContext.CheckInventoryFilter(item, filter, true)
					select item).Any<InventoryItem>();
			}
			return (from item in array
				where item.ItemId == itemId
				where item.MinLevel <= this.Context.Level || (isPondUnlocked && item.RaretyId == 3)
				where MissionInventoryContext.CheckInventoryFilter(item, filter, true)
				select item).Any<InventoryItem>();
		}

		public bool IsInGlobalShop(int itemId, MissionInventoryFilter filter = null)
		{
			if (this.GetInventoryItemsInGlobalShop == null)
			{
				return false;
			}
			InventoryItem[] array = this.GetInventoryItemsInGlobalShop();
			if (itemId < 0)
			{
				int categoryId = -itemId;
				return (from item in array
					where (int)item.ItemType == categoryId || (int)item.ItemSubType == categoryId
					where item.MinLevel <= this.Context.Level
					where MissionInventoryContext.CheckInventoryFilter(item, filter, true)
					select item).Any<InventoryItem>();
			}
			return (from item in array
				where item.ItemId == itemId
				where item.MinLevel <= this.Context.Level
				where MissionInventoryContext.CheckInventoryFilter(item, filter, true)
				select item).Any<InventoryItem>();
		}

		public InventoryCategory ResolveInventoryCategory(int categoryId)
		{
			if (this.GetInventoryCategory != null)
			{
				return this.GetInventoryCategory(categoryId, this.Context.GetProfile().LanguageId);
			}
			return new InventoryCategory
			{
				CategoryId = categoryId
			};
		}

		public GameCacheAdapter.GetAllPondIdsHandler GetAllPonds { get; set; }

		public GameCacheAdapter.GetPondHandler GetPond { get; set; }

		public GameCacheAdapter.GetLocationHandler GetLocation { get; set; }

		public GameCacheAdapter.GetLicenseHandler GetLicense { get; set; }

		public int? GetPondStateId(int pondId)
		{
			if (this.GetPond == null)
			{
				return null;
			}
			Pond pond = this.GetPond(pondId, this.Context.GetProfile().LanguageId);
			return (pond == null || pond.State == null) ? null : new int?(pond.State.StateId);
		}

		public int ResolvePondIdByAsset(string asset)
		{
			if (this.GetPond == null)
			{
				return 0;
			}
			if (this.GetAllPonds == null)
			{
				return 0;
			}
			foreach (int num in this.GetAllPonds())
			{
				Pond pond = this.GetPond(num, this.Context.GetProfile().LanguageId);
				if (pond.Asset == asset)
				{
					return num;
				}
			}
			return 0;
		}

		public Pond ResolvePond(int pondId)
		{
			if (pondId == 0)
			{
				return null;
			}
			if (this.GetPond == null)
			{
				return null;
			}
			return this.GetPond(pondId, this.Context.GetProfile().LanguageId);
		}

		public string ResolvePondName(int pondId)
		{
			if (this.GetPond == null)
			{
				return null;
			}
			Pond pond = this.GetPond(pondId, this.Context.GetProfile().LanguageId);
			return (pond == null) ? null : pond.Name;
		}

		public string ResolvePondAsset(int pondId)
		{
			if (pondId == 0)
			{
				return null;
			}
			if (this.GetPond == null)
			{
				return null;
			}
			Pond pond = this.GetPond(pondId, this.Context.GetProfile().LanguageId);
			return (pond == null) ? null : pond.Asset;
		}

		public string ResolveLicenseName(int licenseId)
		{
			if (this.GetLicense == null)
			{
				return null;
			}
			PlayerLicense playerLicense = this.GetLicense(licenseId, this.Context.GetProfile().LanguageId);
			return (playerLicense == null) ? null : playerLicense.Name;
		}

		public string ResolveLocationName(int pondId, string asset)
		{
			if (this.GetLocation == null)
			{
				return null;
			}
			Location location = this.GetLocation(pondId, asset, this.Context.GetProfile().LanguageId);
			return (location == null) ? null : location.Name;
		}

		public static GameCacheAdapter.GetFishCategoryHandler GetFishCategory { get; set; }

		public static int? GetFishParentCategory(int categoryId)
		{
			if (GameCacheAdapter.GetFishCategory == null)
			{
				return null;
			}
			FishCategory fishCategory = GameCacheAdapter.GetFishCategory(categoryId);
			if (fishCategory == null)
			{
				return null;
			}
			return fishCategory.ParentCategoryId;
		}

		public static GameCacheAdapter.GetFishCategoryIdHandler GetFishCategoryId { get; set; }

		public static int? GetFishCategoryIdByCode(string categoryCode)
		{
			if (GameCacheAdapter.GetFishCategoryId == null)
			{
				return null;
			}
			return new int?(GameCacheAdapter.GetFishCategoryId(categoryCode));
		}

		public static GameCacheAdapter.GetFishByCodeHandler GetFishByCode { get; set; }

		public static int GetFishId(string codeName)
		{
			if (GameCacheAdapter.GetFishByCode == null)
			{
				return 0;
			}
			return GameCacheAdapter.GetFishByCode(codeName);
		}

		public GameCacheAdapter.GetFishHandler GetFish { get; set; }

		public string ResolveFishName(int fishId)
		{
			if (this.GetFish == null)
			{
				return null;
			}
			Fish fish = this.GetFish(fishId, this.Context.GetProfile().LanguageId);
			return (fish == null) ? null : fish.Name;
		}

		public GameCacheAdapter.GetAchievementHandler GetAchievement { get; set; }

		public GameCacheAdapter.GetAchievementStageHandler GetAchievementStage { get; set; }

		public GameCacheAdapter.GetHasAchievementHandler GetHasAchievement { get; set; }

		public GameCacheAdapter.GetHasAchievementStageHandler GetHasAchievementStage { get; set; }

		public GameCacheAdapter.GetHasAchievementOrderHandler GetHasAchievementOrder { get; set; }

		public GameCacheAdapter.Achievement ResolveAchievement(int achievementId)
		{
			if (this.GetAchievement == null)
			{
				return null;
			}
			return this.GetAchievement(achievementId, this.Context.GetProfile().LanguageId);
		}

		public GameCacheAdapter.Achievement ResolveAchievementStage(int stageId)
		{
			if (this.GetAchievementStage == null)
			{
				return null;
			}
			return this.GetAchievementStage(stageId, this.Context.GetProfile().LanguageId);
		}

		public string ResolveAchievementName(int achievementId)
		{
			if (this.GetAchievement == null)
			{
				return null;
			}
			GameCacheAdapter.Achievement achievement = this.GetAchievement(achievementId, this.Context.GetProfile().LanguageId);
			return (achievement == null) ? null : achievement.Name;
		}

		public bool HasCompletedAchievement(int achievementId, out int count)
		{
			count = 0;
			return this.GetHasAchievement != null && this.GetHasAchievement(achievementId, out count);
		}

		public bool HasCompletedAchievementStage(int stageid, out int count)
		{
			count = 0;
			return this.GetHasAchievementStage != null && this.GetHasAchievementStage(stageid, out count);
		}

		public bool HasCompletedAchievementOrder(int achievementId, int orderId, out int count)
		{
			count = 0;
			return this.GetHasAchievementOrder != null && this.GetHasAchievementOrder(achievementId, orderId, out count);
		}

		public bool ShopCachePropertiesLoaded
		{
			get
			{
				return this.itemsGlobalLoaded && (this.Context.PondId == 0 || this.itemsLocalLoaded);
			}
		}

		public void EnsureCaches()
		{
			if (!this.itemsGlobalRequested && StaticUserData.AllCategories != null)
			{
				this.itemsGlobalRequested = true;
				int[] array = (from c in StaticUserData.AllCategories
					where c.ParentCategoryId == null
					select c.CategoryId).ToArray<int>();
				CacheLibrary.GlobalShopCacheInstance.GetItemsFromCategory(array);
			}
			if (this.itemsGlobalLoaded && !this.itemsLocalRequested && StaticUserData.AllCategories != null && this.Context.PondId != 0)
			{
				this.itemsLocalRequested = true;
				int[] array2 = (from c in StaticUserData.AllCategories
					where c.ParentCategoryId == null
					select c.CategoryId).ToArray<int>();
				CacheLibrary.LocalCacheInstance.GetItemsFromCategory(array2, this.Context.PondId);
			}
		}

		public void OnPondChanged()
		{
			CacheLibrary.LocalCacheInstance.EnsureCacheCorrespondsPondId();
			this.itemsLocalRequested = false;
			this.itemsLocalLoaded = false;
		}

		public void SubscribeEvents()
		{
			CacheLibrary.GlobalShopCacheInstance.OnGetItems += this.GlobalShopCacheInstance_OnGetItems;
			CacheLibrary.GlobalShopCacheInstance.OnUpdatedItems += this.GlobalShopCacheInstance_OnUpdatedItems;
			CacheLibrary.LocalCacheInstance.OnGetItems += this.LocalCacheInstance_OnGetItems;
			CacheLibrary.LocalCacheInstance.OnUpdatedItems += this.LocalCacheInstance_OnUpdatedItems;
		}

		public void UnsubscribeEvents()
		{
			CacheLibrary.GlobalShopCacheInstance.OnGetItems -= this.GlobalShopCacheInstance_OnGetItems;
			CacheLibrary.GlobalShopCacheInstance.OnUpdatedItems -= this.GlobalShopCacheInstance_OnUpdatedItems;
			CacheLibrary.LocalCacheInstance.OnGetItems -= this.LocalCacheInstance_OnGetItems;
			CacheLibrary.LocalCacheInstance.OnUpdatedItems -= this.LocalCacheInstance_OnUpdatedItems;
		}

		private void GlobalShopCacheInstance_OnGetItems(object sender, GlobalCacheResponceEventArgs e)
		{
			this.itemsGlobalLoaded = true;
			this.Context.OnDependencyChanged("GlobalShopInventory", DependencyChange.Updated<bool>(false, true), true);
		}

		private void GlobalShopCacheInstance_OnUpdatedItems(object sender, GlobalCacheResponceEventArgs e)
		{
			this.Context.OnDependencyChanged("GlobalShopInventory", DependencyChange.Updated<bool>(false, true), true);
		}

		private void LocalCacheInstance_OnGetItems(object sender, GlobalCacheResponceEventArgs e)
		{
			this.itemsLocalLoaded = true;
			this.Context.OnDependencyChanged("LocalShopInventory", DependencyChange.Updated<bool>(false, true), true);
		}

		private void LocalCacheInstance_OnUpdatedItems(object sender, GlobalCacheResponceEventArgs e)
		{
			this.Context.OnDependencyChanged("LocalShopInventory", DependencyChange.Updated<bool>(false, true), true);
		}

		private bool itemsGlobalRequested;

		private bool itemsLocalRequested;

		private bool itemsGlobalLoaded;

		private bool itemsLocalLoaded;

		public class Achievement
		{
			public int AchievementId { get; set; }

			public int? OrderId { get; set; }

			public string Name { get; set; }

			public string Desc { get; set; }

			public List<GameCacheAdapter.AchievementStage> Stages { get; set; }
		}

		public class AchievementStage
		{
			public int StageId { get; set; }

			public int OrderId { get; set; }

			public int Count { get; set; }
		}

		public delegate InventoryItem GetInventoryHandler(int itemId, int languageId);

		public delegate InventoryItem[] GetInventoryItemsInGlobalShopHandler();

		public delegate InventoryItem[] GetInventoryItemsInLocalShopHandler(int pondId);

		public delegate InventoryCategory GetInventoryCategoryHandler(int categoryId, int languageId);

		public delegate PlayerLicense GetLicenseHandler(int licenseId, int languageId);

		public delegate int[] GetAllPondIdsHandler();

		public delegate Pond GetPondHandler(int pondId, int languageId);

		public delegate Location GetLocationHandler(int pondId, string asset, int languageId);

		public delegate FishCategory GetFishCategoryHandler(int categoryId);

		public delegate int GetFishCategoryIdHandler(string categoryCode);

		public delegate int GetFishByCodeHandler(string codeName);

		public delegate Fish GetFishHandler(int fishId, int languageId);

		public delegate GameCacheAdapter.Achievement GetAchievementHandler(int achievementId, int languageId);

		public delegate GameCacheAdapter.Achievement GetAchievementStageHandler(int stageId, int languageId);

		public delegate bool GetHasAchievementHandler(int achievementId, out int count);

		public delegate bool GetHasAchievementStageHandler(int stageId, out int count);

		public delegate bool GetHasAchievementOrderHandler(int achievementId, int orderId, out int count);
	}
}
