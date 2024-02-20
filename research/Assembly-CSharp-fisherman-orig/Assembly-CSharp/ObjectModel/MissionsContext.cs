using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ObjectModel
{
	public class MissionsContext
	{
		public MissionsContext()
		{
			this.inventory = new MissionInventoryContext
			{
				Context = this
			};
			this.gameCacheAdapter = new GameCacheAdapter
			{
				Context = this
			};
			this.checkMonitoredDependencies = (string d, bool a) => true;
		}

		public HashSet<string> CapturedDependencies
		{
			get
			{
				return this.capturedDependencies;
			}
		}

		public string CapturedDependenciesString
		{
			get
			{
				return string.Join(",", this.capturedDependencies.ToArray<string>());
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event DependencyChangedEventHandler DependencyChanged;

		public void CaptureDependencies()
		{
			if (!MissionsContext.CalculatedPropertiesLazyMode)
			{
				this.RecalculateAllProperties();
			}
			this.capturedDependencies = new HashSet<string>(this.dependenciesChanged);
			this.dependenciesChanged.Clear();
		}

		public void SetCheckMonitoredDependencies(CheckMonitoredDependenciesHandler checkMonitoredDependencies)
		{
			CheckMonitoredDependenciesHandler checkMonitoredDependenciesHandler = checkMonitoredDependencies;
			if (checkMonitoredDependencies == null)
			{
				checkMonitoredDependenciesHandler = (string d, bool a) => true;
			}
			this.checkMonitoredDependencies = checkMonitoredDependenciesHandler;
		}

		public void OnDependencyChanged(string dependency, IDependencyChange change = null, bool affectProcessing = true)
		{
			object obj = this.lockObject;
			lock (obj)
			{
				bool flag = dependency != "PondTimeSpent" && dependency != "FishCage";
				if (change != null)
				{
					flag = !change.IsChanged;
					if (string.IsNullOrEmpty(change.Name))
					{
						change.Name = dependency;
					}
				}
				bool flag2 = false;
				if (!flag && this.checkMonitoredDependencies(dependency, affectProcessing))
				{
					flag2 = true;
				}
				if (!flag && flag2 && affectProcessing)
				{
					this.dependenciesChanged.Add(dependency);
				}
				if (!flag && this.DependencyChanged != null && affectProcessing)
				{
					this.DependencyChanged(this, dependency, change);
				}
			}
		}

		public bool HasProfile
		{
			get
			{
				return this.profile != null;
			}
		}

		public Profile GetProfile()
		{
			return this.profile;
		}

		public void SetProfile(Profile value)
		{
			if (this.profile != null)
			{
				this.profile.Inventory.OnInventoryChange -= this.Inventory_OnInventoryChange;
			}
			this.profile = value;
			if (this.profile != null)
			{
				if (this.profile.Inventory == null)
				{
					this.profile.Inventory = new Inventory();
				}
				this.profile.Inventory.OnInventoryChange += this.Inventory_OnInventoryChange;
			}
		}

		public GameCacheAdapter GetGameCacheAdapter()
		{
			return this.gameCacheAdapter;
		}

		public void InitCalculatedProperties()
		{
			this.calculatedProperties = new Dictionary<string, Action>
			{
				{
					"AvailableSlots",
					delegate
					{
						this.AvailableSlots = this.GetProfile().Inventory.AvailableInventoryCapacity;
					}
				},
				{
					"AvailableRodSlots",
					delegate
					{
						this.AvailableRodSlots = this.GetProfile().Inventory.GetConstraintAvailableCountDoll(new Rod
						{
							ItemType = ItemTypes.Rod,
							ItemSubType = ItemSubTypes.Rod
						});
					}
				},
				{
					"AvailableReelSlots",
					delegate
					{
						this.AvailableReelSlots = this.GetProfile().Inventory.GetConstraintAvailableCountEquipment(new Reel
						{
							ItemType = ItemTypes.Reel,
							ItemSubType = ItemSubTypes.Reel
						});
					}
				},
				{
					"AvailableLineSlots",
					delegate
					{
						this.AvailableLineSlots = this.GetProfile().Inventory.GetConstraintAvailableCountEquipment(new Line
						{
							ItemType = ItemTypes.Line,
							ItemSubType = ItemSubTypes.Line
						});
					}
				},
				{
					"AvailableTackleSlots",
					delegate
					{
						this.AvailableTackleSlots = this.GetProfile().Inventory.GetConstraintAvailableCountEquipment(new Hook
						{
							ItemType = ItemTypes.Hook,
							ItemSubType = ItemSubTypes.Hook
						});
					}
				},
				{
					"AvailableChumSlots",
					delegate
					{
						this.AvailableChumSlots = this.GetProfile().Inventory.GetConstraintAvailableCountEquipment(new Chum
						{
							ItemType = ItemTypes.Chum,
							ItemSubType = ItemSubTypes.Chum
						});
					}
				}
			};
		}

		public void ForcePropertiesToRecalculate(params string[] properties)
		{
			object obj = this.lockObject;
			lock (obj)
			{
				foreach (string text in properties)
				{
					this.recalculateActions.Add(this.calculatedProperties[text]);
					if (MissionsContext.CalculatedPropertiesLazyMode)
					{
						this.OnDependencyChanged(text, DependencyChange.Updated(0, 1), true);
					}
				}
			}
		}

		public void ForceAllPropertiesToRecalculate()
		{
			foreach (KeyValuePair<string, Action> keyValuePair in this.calculatedProperties)
			{
				this.recalculateActions.Add(keyValuePair.Value);
				if (MissionsContext.CalculatedPropertiesLazyMode)
				{
					this.OnDependencyChanged(keyValuePair.Key, DependencyChange.Updated(0, 1), true);
				}
			}
		}

		private void RecalculateAllProperties()
		{
			while (this.recalculateActions.Any<Action>())
			{
				Action action = this.recalculateActions.First<Action>();
				action();
				this.recalculateActions.Remove(action);
			}
		}

		private void EnsureCalculatedProperty(string propertyName)
		{
			Action action = this.calculatedProperties[propertyName];
			if (this.recalculateActions.Contains(action))
			{
				action();
				this.recalculateActions.Remove(action);
			}
		}

		public int Level
		{
			get
			{
				return this.profile.Level;
			}
		}

		public int Rank
		{
			get
			{
				return this.profile.Rank;
			}
		}

		public int PondId
		{
			get
			{
				int? pondId = this.profile.PondId;
				return (pondId == null) ? 0 : pondId.Value;
			}
		}

		private void Inventory_OnInventoryChange(InventoryChange change)
		{
			string text = null;
			string text2 = null;
			IDependencyChange dependencyChange;
			if (change.New)
			{
				text = "MoveInventory";
				dependencyChange = DependencyChange.ItemAdded<InventoryItem, InventoryChange>(change, change.Item, null, null);
				if (change.Item is Rod)
				{
					text2 = "RodMoveInventory";
				}
			}
			else if (change.Destroyed)
			{
				text = "MoveInventory";
				dependencyChange = DependencyChange.ItemRemoved<InventoryItem, InventoryChange>(change, change.Item, null, null);
				if (change.Item is Rod)
				{
					text2 = "RodMoveInventory";
				}
			}
			else if (change.Updated)
			{
				IDependencyChangeCollectionItem dependencyChangeCollectionItem = DependencyChange.ItemUpdated<InventoryItem, InventoryChange>(change, change.Item, null, null, null);
				dependencyChange = dependencyChangeCollectionItem;
				dependencyChangeCollectionItem.IsAdded = change.Op == "Join" || change.Op == "Combine";
				dependencyChangeCollectionItem.IsRemoved = change.Op == "Split" || change.Op == "Cut";
			}
			else if (change.Repaired)
			{
				dependencyChange = DependencyChange.ItemUpdated<InventoryItem, InventoryChange>(change, change.Item, null, "Repaired", null);
				text = "RepairInventory";
			}
			else if (change.Weared)
			{
				dependencyChange = DependencyChange.ItemUpdated<InventoryItem, InventoryChange>(change, change.Item, null, "Weared", null);
				text = "WearInventory";
				if (change.Item.Durability == 0)
				{
					text2 = "MoveInventory";
				}
			}
			else
			{
				text = "MoveInventory";
				if (change.FromItem != null && change.ToItem == null)
				{
					dependencyChange = DependencyChange.ItemUpdated<InventoryItem, InventoryChange>(change, change.Item, null, "Unequiped", null);
				}
				else if (change.FromItem == null && change.ToItem != null)
				{
					dependencyChange = DependencyChange.ItemUpdated<InventoryItem, InventoryChange>(change, change.Item, null, "Equiped", null);
				}
				else
				{
					dependencyChange = DependencyChange.ItemUpdated<InventoryItem, InventoryChange>(change, change.Item, null, "Moved", null);
					if (change.Item is Rod)
					{
						text2 = "RodMoveInventory";
					}
				}
			}
			this.OnDependencyChanged("Inventory", dependencyChange, true);
			if (text != null)
			{
				this.OnDependencyChanged(text, dependencyChange, true);
			}
			if (text2 != null)
			{
				this.OnDependencyChanged(text2, DependencyChange.Updated<bool>(false, true), true);
			}
			if (text == "MoveInventory")
			{
				this.ForcePropertiesToRecalculate(new string[] { "AvailableSlots", "AvailableRodSlots", "AvailableReelSlots", "AvailableLineSlots", "AvailableTackleSlots", "AvailableChumSlots" });
			}
		}

		public MissionInventoryContext Inventory
		{
			get
			{
				return this.inventory;
			}
		}

		public int AvailableSlots
		{
			get
			{
				this.EnsureCalculatedProperty("AvailableSlots");
				return this.availableSlots;
			}
			set
			{
				int num = this.availableSlots;
				this.availableSlots = value;
				if (num != value)
				{
					this.OnDependencyChanged("AvailableSlots", DependencyChange.Updated(num, value), true);
				}
			}
		}

		public int AvailableRodSlots
		{
			get
			{
				this.EnsureCalculatedProperty("AvailableRodSlots");
				return this.availableRodSlots;
			}
			set
			{
				int num = this.availableRodSlots;
				this.availableRodSlots = value;
				if (num != value)
				{
					this.OnDependencyChanged("AvailableRodSlots", DependencyChange.Updated(num, value), true);
				}
			}
		}

		public int AvailableReelSlots
		{
			get
			{
				this.EnsureCalculatedProperty("AvailableReelSlots");
				return this.availableReelSlots;
			}
			set
			{
				int num = this.availableReelSlots;
				this.availableReelSlots = value;
				if (num != value)
				{
					this.OnDependencyChanged("AvailableReelSlots", DependencyChange.Updated(num, value), true);
				}
			}
		}

		public int AvailableLineSlots
		{
			get
			{
				this.EnsureCalculatedProperty("AvailableLineSlots");
				return this.availableLineSlots;
			}
			set
			{
				int num = this.availableLineSlots;
				this.availableLineSlots = value;
				if (num != value)
				{
					this.OnDependencyChanged("AvailableLineSlots", DependencyChange.Updated(num, value), true);
				}
			}
		}

		public int AvailableTackleSlots
		{
			get
			{
				this.EnsureCalculatedProperty("AvailableTackleSlots");
				return this.availableTackleSlots;
			}
			set
			{
				int num = this.availableTackleSlots;
				this.availableTackleSlots = value;
				if (num != value)
				{
					this.OnDependencyChanged("AvailableTackleSlots", DependencyChange.Updated(num, value), true);
				}
			}
		}

		public int AvailableChumSlots
		{
			get
			{
				this.EnsureCalculatedProperty("AvailableChumSlots");
				return this.availableChumSlots;
			}
			set
			{
				int num = this.availableChumSlots;
				this.availableChumSlots = value;
				if (num != value)
				{
					this.OnDependencyChanged("AvailableChumSlots", DependencyChange.Updated(num, value), true);
				}
			}
		}

		private object lockObject = new object();

		private readonly HashSet<string> dependenciesChanged = new HashSet<string>();

		private CheckMonitoredDependenciesHandler checkMonitoredDependencies;

		private HashSet<string> capturedDependencies;

		private Profile profile;

		private GameCacheAdapter gameCacheAdapter;

		private static readonly bool CalculatedPropertiesLazyMode = false;

		private Dictionary<string, Action> calculatedProperties;

		private static readonly string[] waterStates = new string[] { "Move", "Hitch", "Attack", "BiteConfirmed", "AttackFinished", "FishFight", "Draw" };

		private readonly HashSet<Action> recalculateActions = new HashSet<Action>();

		private static readonly string[] inventoryChangeOperations = new string[] { "Inventory", "MoveInventory", "WearInventory", "RepairInventory", "LeaderLength", "RodMoveInventory" };

		private MissionInventoryContext inventory;

		private int availableSlots;

		private int availableRodSlots;

		private int availableReelSlots;

		private int availableLineSlots;

		private int availableTackleSlots;

		private int availableChumSlots;
	}
}
