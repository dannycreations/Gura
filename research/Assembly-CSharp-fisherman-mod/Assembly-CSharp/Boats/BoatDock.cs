using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;

namespace Boats
{
	public class BoatDock : MonoBehaviour
	{
		private bool IsBoatType(ItemSubTypes type)
		{
			return BoatDock._itemSubType2BoatTypeMap.ContainsKey((int)type);
		}

		public Transform PlayerRespawnPos
		{
			get
			{
				return this._playerRespawnPos;
			}
		}

		public List<IBoatController> Boats
		{
			get
			{
				return this._boats;
			}
		}

		public List<BoatDesc> BoatsForRentDescriptions
		{
			get
			{
				return this._boatsForRentDescriptions;
			}
		}

		public bool IsBoatsAvailable
		{
			get
			{
				return PhotonConnectionFactory.Instance.Profile.Tournament == null || PhotonConnectionFactory.Instance.Profile.Tournament.EquipmentAllowed == null || (PhotonConnectionFactory.Instance.Profile.Tournament.EquipmentAllowed.BoatTypes != null && PhotonConnectionFactory.Instance.Profile.Tournament.EquipmentAllowed.BoatTypes.Length > 0);
			}
		}

		private void OnGotBoats(IEnumerable<BoatDesc> boats)
		{
			this._allBoats = boats.ToList<BoatDesc>();
			this.CollectPondBoats();
			this.UnsubscribeFromBoatsList();
		}

		public void CollectPondBoats()
		{
			if (this._allBoats == null)
			{
				return;
			}
			List<BoatRent> rents = PhotonConnectionFactory.Instance.Profile.ActiveBoatRents;
			ItemSubTypes itemSubTypes = ItemSubTypes.All;
			this._playerBoatItem = PhotonConnectionFactory.Instance.Profile.Inventory.Items.FirstOrDefault((InventoryItem item) => item.Storage == StoragePlaces.Doll && (rents == null || rents.All((BoatRent b) => b.BoatType != item.ItemSubType)) && this.IsBoatType(item.ItemSubType)) as Boat;
			if (this._playerBoatItem != null && this._playerBoatItem.Durability > 0)
			{
				if (this.GetSpawnSettingsForType(this._playerBoatItem.ItemSubType) == null)
				{
					BoatDock.SpawnSettings anyTypeSettings = this.GetAnyTypeSettings();
					if (anyTypeSettings != null)
					{
						itemSubTypes = anyTypeSettings.ItemSubType;
					}
					else
					{
						this._playerBoatItem = null;
					}
				}
			}
			else
			{
				this._playerBoatItem = null;
			}
			this._boatsForRentDescriptions.Clear();
			for (int i = 0; i < this._allBoats.Count; i++)
			{
				BoatDesc b = this._allBoats[i];
				if (b.Prices != null)
				{
					BoatPriceDesc boatPriceDesc = b.Prices.FirstOrDefault((BoatPriceDesc bp) => bp.PondId == PhotonConnectionFactory.Instance.CurrentPondId);
					if (boatPriceDesc != null && this.GetSpawnSettingsForType((ItemSubTypes)b.BoatCategoryId) != null)
					{
						if (this._boatsForRentDescriptions.FirstOrDefault((BoatDesc bb) => bb.BoatCategoryId == b.BoatCategoryId) == null)
						{
							if (this._playerBoatItem == null || (itemSubTypes != (ItemSubTypes)b.BoatCategoryId && this._playerBoatItem.ItemSubType != (ItemSubTypes)b.BoatCategoryId))
							{
								this._boatsForRentDescriptions.Add(b);
							}
						}
						else
						{
							LogHelper.Error("It's impossible to add more then one {0}", new object[] { (ItemSubTypes)b.BoatCategoryId });
						}
					}
				}
			}
		}

		public void TrySpawnBoat(bool force = false)
		{
			if (this.IsBoatsAvailable && this._allBoats != null && (this._boats.Count == 0 || force))
			{
				ProfileTournament tournament = PhotonConnectionFactory.Instance.Profile.Tournament;
				ItemSubTypes[] array = ((tournament == null || tournament.EquipmentAllowed == null) ? null : tournament.EquipmentAllowed.BoatTypes);
				if (this._playerBoatItem != null && (array == null || array.Contains(this._playerBoatItem.ItemSubType)) && !this._boats.Any((IBoatController b) => b.Category == this._playerBoatItem.ItemSubType))
				{
					IBoatController boatController = this.CreateBoat(this._playerBoatItem, null);
					if (boatController != null)
					{
						this._boats.Add(boatController);
					}
				}
				List<BoatRent> activeBoatRents = PhotonConnectionFactory.Instance.Profile.ActiveBoatRents;
				for (int i = 0; i < this._boatsForRentDescriptions.Count; i++)
				{
					BoatDesc boatDesc = this._boatsForRentDescriptions[i];
					ItemSubTypes boatType = (ItemSubTypes)boatDesc.BoatCategoryId;
					if ((array == null || array.Contains(boatType)) && !this._boats.Any((IBoatController b) => b.Category == boatType))
					{
						IBoatController boatController2 = this.CreateBoat(boatDesc.BoatModel, boatDesc);
						this._boats.Add(boatController2);
						if (activeBoatRents != null && activeBoatRents.Any((BoatRent r) => r.BoatType == boatType))
						{
							boatController2.Rent();
						}
					}
				}
			}
		}

		public void RefreshBoats(ItemSubTypes? currentType)
		{
			for (int i = this._boats.Count - 1; i >= 0; i--)
			{
				if (!this.IsBoatTypeAllowed(this._boats[i].Category))
				{
					if (this._boats[i].Category == currentType)
					{
						this._boats[i].HiddenLeave();
					}
					this._boats[i].Destroy();
					this._boats.RemoveAt(i);
				}
			}
			this.TrySpawnBoat(true);
		}

		public void RecreateAllBoats()
		{
			for (int i = 0; i < this._boats.Count; i++)
			{
				this._boats[i].Destroy();
			}
			this._boats.Clear();
			if (this.IsBoatsAvailable)
			{
				this.CollectPondBoats();
				this.TrySpawnBoat(false);
			}
		}

		public bool IsBoatTypeAllowed(ItemSubTypes type)
		{
			ProfileTournament tournament = PhotonConnectionFactory.Instance.Profile.Tournament;
			if (tournament == null)
			{
				return true;
			}
			ItemSubTypes[] array = ((tournament.EquipmentAllowed == null) ? null : tournament.EquipmentAllowed.BoatTypes);
			return array != null && array.Contains(type);
		}

		private void OnCantGetBoats(Failure f)
		{
			LogHelper.Error("PhotonConnectionFactory.Instance.OnGettingBoatsFailed({0})", new object[] { f });
			this.UnsubscribeFromBoatsList();
		}

		private void UnsubscribeFromBoatsList()
		{
			PhotonConnectionFactory.Instance.OnGotBoats -= this.OnGotBoats;
			PhotonConnectionFactory.Instance.OnGettingBoatsFailed -= this.OnCantGetBoats;
		}

		private void Awake()
		{
			if (GameFactory.BoatDock == null)
			{
				GameFactory.BoatDock = this;
			}
			if (this._allBoats == null)
			{
				PhotonConnectionFactory.Instance.GetBoats();
				PhotonConnectionFactory.Instance.OnGotBoats += this.OnGotBoats;
				PhotonConnectionFactory.Instance.OnGettingBoatsFailed += this.OnCantGetBoats;
			}
			else
			{
				this.OnGotBoats(this._allBoats);
			}
			PhotonConnectionFactory.Instance.OnItemBought += this.OnItemBought;
			PhotonConnectionFactory.Instance.OnItemGained += this.OnItemGained;
		}

		public bool IsRefreshRequest
		{
			get
			{
				return this._isRefreshRequest;
			}
		}

		private void OnItemBought(InventoryItem item)
		{
			if (item.ItemType == ItemTypes.Boat)
			{
				this._isRefreshRequest = true;
			}
		}

		private void OnItemGained(IEnumerable<InventoryItem> items, bool announce)
		{
			if (items.Any((InventoryItem i) => i.ItemType == ItemTypes.Boat))
			{
				this._isRefreshRequest = true;
			}
		}

		public void OnPlayerReady()
		{
			this._isRefreshRequest = false;
			this._wasItemsUnsubscribed = true;
			PhotonConnectionFactory.Instance.OnItemBought -= this.OnItemBought;
			PhotonConnectionFactory.Instance.OnItemGained -= this.OnItemGained;
		}

		private IBoatController CreateBoat(Boat boatData, BoatDesc description = null)
		{
			BoatDock.SpawnSettings spawnSettings = this.GetSpawnSettingsForType(boatData.ItemSubType);
			if (spawnSettings == null)
			{
				if (description == null)
				{
					spawnSettings = this.GetAnyTypeSettings();
				}
				if (spawnSettings == null)
				{
					LogHelper.Error("Can't find prefab for type {0}", new object[] { boatData.ItemSubType });
					return null;
				}
			}
			ushort id = this._factory.GetID(boatData.Asset, boatData.Material);
			if (id == 65535)
			{
				LogHelper.Error("Can't find prefab for asset {0}", new object[] { boatData.Asset });
				return null;
			}
			ushort num = id;
			BoatFactoryData data = this._factory.GetData(num);
			GameObject gameObject = Resources.Load<GameObject>(data.Prefab1p);
			if (gameObject == null)
			{
				LogHelper.Error("Can't find prefab {0} to load", new object[] { data.Prefab1p });
			}
			GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject);
			BoatSettings component = gameObject2.GetComponent<BoatSettings>();
			if (data.Materials != null)
			{
				for (int i = 0; i < component.Renderers.Length; i++)
				{
					Material material = Resources.Load<Material>(data.Materials[i]);
					component.Renderers[i].material = material;
				}
			}
			bool flag = description == null;
			IBoatController boatController = null;
			if (boatData is Kayak)
			{
				boatController = KayakController.Create(spawnSettings.spawnPos, component, boatData as Kayak, flag, num);
			}
			else if (boatData.ItemSubType == ItemSubTypes.BassBoat)
			{
				boatController = BassBoatController.Create(spawnSettings.spawnPos, component, boatData as BassBoat, flag, num);
			}
			else if (boatData is MotorBoat)
			{
				boatController = BoatWithEngineController.Create(spawnSettings.spawnPos, component, boatData as MotorBoat, flag, num);
			}
			if (boatController != null)
			{
				int index = 0;
				if (flag && boatData.NestedItems != null && boatData.NestedItems.Length > 0)
				{
					for (int j = 0; j < boatData.NestedItems.Length; j++)
					{
						InventoryItem inventoryItem = PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.ItemId == boatData.NestedItems[index].ItemId && x.ItemSubType == ItemSubTypes.EchoSounder && x.ParentItemInstanceId != null && x.ParentItemInstanceId.Value == boatData.InstanceId);
						if (inventoryItem != null)
						{
							this.CreateFishFinder(inventoryItem as EchoSounder, component, boatController, index++);
						}
					}
				}
				else if (!flag && description.BoatInventory != null)
				{
					foreach (InventoryItem inventoryItem2 in description.BoatInventory)
					{
						if (inventoryItem2.ItemSubType == ItemSubTypes.EchoSounder)
						{
							this.CreateFishFinder(inventoryItem2 as EchoSounder, component, boatController, index++);
						}
					}
				}
			}
			if (boatController == null)
			{
				LogHelper.Error("{0} is not supported description type", new object[] { boatData.ItemSubType });
			}
			return boatController;
		}

		private void CreateFishFinder(EchoSounder item, BoatSettings bs, IBoatController bc, int index = 0)
		{
			FishFinder fishFinder = Resources.Load<FishFinder>(item.Asset);
			if (fishFinder != null && bs.FishFinderPivots != null && bs.FishFinderPivots.Length > index)
			{
				FishFinder fishFinder2 = Object.Instantiate<FishFinder>(fishFinder, bs.FishFinderPivots[index] ?? bs.OarAnchor);
				TransformHelper.ChangeLayersRecursively(fishFinder2.transform, GlobalConsts.UnimportantLayer);
				fishFinder2.SetBoatController(bc);
				fishFinder2.InitRenderTextures();
				fishFinder2.SetInstanceId(item.InstanceId.ToString());
				fishFinder2.SetType(item.Kind, index == 0);
				if (item.MaxSpeed != null)
				{
					fishFinder2.SetMaxSpeed(item.MaxSpeed.Value);
				}
			}
			else if (fishFinder == null)
			{
				Debug.LogError("Wrong asset path for echosouder");
			}
			else
			{
				Debug.LogError("No fishfinder pivot assigned for index: " + index);
			}
		}

		public TPMBoatSettings GetTPMBoat(ushort factoryID)
		{
			BoatFactoryData data = this._factory.GetData(factoryID);
			GameObject gameObject = Resources.Load<GameObject>(data.Prefab3p);
			GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject);
			TPMBoatSettings component = gameObject2.GetComponent<TPMBoatSettings>();
			if (data.Materials != null)
			{
				for (int i = 0; i < component.Renderers.Length; i++)
				{
					Material material = Resources.Load<Material>(data.Materials[i]);
					component.Renderers[i].material = material;
				}
			}
			return component;
		}

		public ItemSubTypes GetAnyAvailableBoatType()
		{
			if (this._prefabs.Length > 0)
			{
				BoatDock.SpawnSettings s = this._prefabs[0];
				return (ItemSubTypes)BoatDock._itemSubType2BoatTypeMap.FirstOrDefault((KeyValuePair<int, BoatType> p) => p.Value == s.type).Key;
			}
			return ItemSubTypes.All;
		}

		public BoatDock.SpawnSettings GetSpawnSettingsForType(ItemSubTypes itemType)
		{
			if (!BoatDock._itemSubType2BoatTypeMap.ContainsKey((int)itemType))
			{
				LogHelper.Error("ItemSubTypes.{0} is not supported", new object[] { itemType });
			}
			BoatType type = BoatDock._itemSubType2BoatTypeMap[(int)itemType];
			return this._prefabs.FirstOrDefault((BoatDock.SpawnSettings s) => s.type == type);
		}

		public BoatDock.SpawnSettings GetSpawnSettings(ItemSubTypes itemType)
		{
			BoatDock.SpawnSettings spawnSettings = this.GetSpawnSettingsForType(itemType);
			if (spawnSettings == null)
			{
				spawnSettings = this.GetAnyTypeSettings();
			}
			return spawnSettings;
		}

		private BoatDock.SpawnSettings GetAnyTypeSettings()
		{
			return this._prefabs.FirstOrDefault((BoatDock.SpawnSettings s) => s.isAnyTypeSlot);
		}

		public BoatType GetTypeByItemType(ItemSubTypes itemType)
		{
			return BoatDock._itemSubType2BoatTypeMap[(int)itemType];
		}

		private void OnDestroy()
		{
			if (this._wasItemsUnsubscribed)
			{
				PhotonConnectionFactory.Instance.OnItemBought -= this.OnItemBought;
				PhotonConnectionFactory.Instance.OnItemGained -= this.OnItemGained;
			}
			for (int i = 0; i < this._boats.Count; i++)
			{
				this._boats[i].Destroy();
			}
			GameFactory.BoatDock = null;
			this._allBoats = null;
			this._boats = null;
			this._boatsForRentDescriptions.Clear();
		}

		private static Dictionary<int, BoatType> _itemSubType2BoatTypeMap = new Dictionary<int, BoatType>
		{
			{
				98,
				BoatType.KAYAK
			},
			{
				100,
				BoatType.ZODIAC
			},
			{
				102,
				BoatType.METAL
			},
			{
				103,
				BoatType.BASS
			}
		};

		[SerializeField]
		private BoatDock.SpawnSettings[] _prefabs;

		[SerializeField]
		private Transform _playerRespawnPos;

		[SerializeField]
		private BoatsFactory _factory;

		private List<BoatDesc> _allBoats;

		private List<IBoatController> _boats = new List<IBoatController>();

		private readonly List<BoatDesc> _boatsForRentDescriptions = new List<BoatDesc>();

		private Boat _playerBoatItem;

		private bool _isRefreshRequest;

		private bool _wasItemsUnsubscribed;

		[Serializable]
		public class SpawnSettings
		{
			public ItemSubTypes ItemSubType
			{
				get
				{
					if (BoatDock._itemSubType2BoatTypeMap.ContainsValue(this.type))
					{
						return (ItemSubTypes)BoatDock._itemSubType2BoatTypeMap.FirstOrDefault((KeyValuePair<int, BoatType> rr) => rr.Value == this.type).Key;
					}
					return ItemSubTypes.All;
				}
			}

			public BoatType type;

			public Transform spawnPos;

			public bool isAnyTypeSlot;
		}
	}
}
