using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;

public class AssembledRod : IEqualityComparer<AssembledRod>, IEquatable<AssembledRod>, ICloneable, IAssembledRod
{
	public AssembledRod()
	{
		this.RodTemplate = RodTemplate.UnEquiped;
	}

	public AssembledRod(Guid? instanceId)
	{
		this.RefreshRod(instanceId);
	}

	public AssembledRod(AssembledRod source)
	{
		this.Rod = source.Rod;
		this.Line = source.Line;
		this.Reel = source.Reel;
		this.Bell = source.Bell;
		this.Bobber = source.Bobber;
		this.Sinker = source.Sinker;
		this.Feeder = source.Feeder;
		this.Hook = source.Hook;
		this.Bait = source.Bait;
		this.Chum = source.Chum;
		this.ChumAll = source.ChumAll;
		this.Quiver = source.Quiver;
		this.RodTemplate = source.RodTemplate;
		this.IsRodDisassembled = source.IsRodDisassembled;
		this.Leader = source.Leader;
	}

	public IRod RodInterface
	{
		get
		{
			return this.Rod;
		}
	}

	public IReel ReelInterface
	{
		get
		{
			return this.Reel;
		}
	}

	public IBell BellInterface
	{
		get
		{
			return this.Bell;
		}
	}

	public ILine LineInterface
	{
		get
		{
			return this.Line;
		}
	}

	public ILeader LeaderInterface
	{
		get
		{
			return this.Leader;
		}
	}

	public IBobber BobberInterface
	{
		get
		{
			return this.Bobber;
		}
	}

	public ISinker SinkerInterface
	{
		get
		{
			return this.Sinker;
		}
	}

	public IFeeder FeederInterface
	{
		get
		{
			return this.Feeder;
		}
	}

	public IHook HookInterface
	{
		get
		{
			return this.Hook;
		}
	}

	public IBait BaitInterface
	{
		get
		{
			return this.Bait;
		}
	}

	public IChum ChumInterface
	{
		get
		{
			return this.Chum;
		}
	}

	public IChum[] ChumInterfaceAll
	{
		get
		{
			return this.ChumAll;
		}
	}

	public IQuiverTip QuiverTipInterface
	{
		get
		{
			return this.Quiver;
		}
	}

	public int Slot
	{
		get
		{
			return (this.Rod == null) ? (-1) : this.Rod.Slot;
		}
	}

	public Rod Rod { get; set; }

	public Line Line { get; private set; }

	public Reel Reel { get; private set; }

	public Bell Bell { get; private set; }

	public Bobber Bobber { get; private set; }

	public Sinker Sinker { get; private set; }

	public Feeder Feeder { get; private set; }

	public Hook Hook { get; private set; }

	public Bait Bait { get; private set; }

	public Chum Chum { get; private set; }

	public Chum[] ChumAll { get; private set; }

	public QuiverTip Quiver { get; private set; }

	public Leader Leader { get; private set; }

	public ReelTypes ReelType
	{
		get
		{
			return this.Reel.ReelType;
		}
	}

	public RodTemplate RodTemplate { get; private set; }

	public bool IsBottom
	{
		get
		{
			return this.Sinker != null && this.Feeder == null;
		}
	}

	public bool RefreshRod(Guid? instanceId)
	{
		if (instanceId == null)
		{
			return false;
		}
		Rod rod = (Rod)PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault(delegate(InventoryItem x)
		{
			Guid? instanceId2 = x.InstanceId;
			return instanceId2 != null == (instanceId != null) && (instanceId2 == null || instanceId2.GetValueOrDefault() == instanceId.GetValueOrDefault()) && x.ItemType == ItemTypes.Rod;
		});
		if (rod == null)
		{
			return false;
		}
		if (!rod.IsRodOnDoll)
		{
			this.IsRodDisassembled = !this.HasAssembledRod;
			return false;
		}
		RodTemplate rodTemplate = PhotonConnectionFactory.Instance.Profile.Inventory.GetRodTemplate(rod);
		if (rodTemplate == RodTemplate.UnEquiped)
		{
			this.IsRodDisassembled = !this.HasAssembledRod;
			return false;
		}
		Line line = (Line)PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault(delegate(InventoryItem x)
		{
			Guid? parentItemInstanceId = x.ParentItemInstanceId;
			bool flag = parentItemInstanceId != null;
			Guid? instanceId3 = rod.InstanceId;
			return flag == (instanceId3 != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId3.GetValueOrDefault()) && x.ItemType == ItemTypes.Line;
		});
		Leader leader = (Leader)PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault(delegate(InventoryItem x)
		{
			Guid? parentItemInstanceId2 = x.ParentItemInstanceId;
			bool flag2 = parentItemInstanceId2 != null;
			Guid? instanceId4 = rod.InstanceId;
			return flag2 == (instanceId4 != null) && (parentItemInstanceId2 == null || parentItemInstanceId2.GetValueOrDefault() == instanceId4.GetValueOrDefault()) && x.ItemType == ItemTypes.Leader;
		});
		Reel reel = (Reel)PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault(delegate(InventoryItem x)
		{
			Guid? parentItemInstanceId3 = x.ParentItemInstanceId;
			bool flag3 = parentItemInstanceId3 != null;
			Guid? instanceId5 = rod.InstanceId;
			return flag3 == (instanceId5 != null) && (parentItemInstanceId3 == null || parentItemInstanceId3.GetValueOrDefault() == instanceId5.GetValueOrDefault()) && x.ItemType == ItemTypes.Reel;
		});
		Bell bell = (Bell)PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault(delegate(InventoryItem x)
		{
			Guid? parentItemInstanceId4 = x.ParentItemInstanceId;
			bool flag4 = parentItemInstanceId4 != null;
			Guid? instanceId6 = rod.InstanceId;
			return flag4 == (instanceId6 != null) && (parentItemInstanceId4 == null || parentItemInstanceId4.GetValueOrDefault() == instanceId6.GetValueOrDefault()) && x.ItemType == ItemTypes.Bell;
		});
		Bobber bobber = (Bobber)PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault(delegate(InventoryItem x)
		{
			Guid? parentItemInstanceId5 = x.ParentItemInstanceId;
			bool flag5 = parentItemInstanceId5 != null;
			Guid? instanceId7 = rod.InstanceId;
			return flag5 == (instanceId7 != null) && (parentItemInstanceId5 == null || parentItemInstanceId5.GetValueOrDefault() == instanceId7.GetValueOrDefault()) && x.ItemType == ItemTypes.Bobber;
		});
		Sinker sinker = (Sinker)PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault(delegate(InventoryItem x)
		{
			Guid? parentItemInstanceId6 = x.ParentItemInstanceId;
			bool flag6 = parentItemInstanceId6 != null;
			Guid? instanceId8 = rod.InstanceId;
			return flag6 == (instanceId8 != null) && (parentItemInstanceId6 == null || parentItemInstanceId6.GetValueOrDefault() == instanceId8.GetValueOrDefault()) && x.ItemType == ItemTypes.Sinker;
		});
		Feeder feeder = (Feeder)PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault(delegate(InventoryItem x)
		{
			Guid? parentItemInstanceId7 = x.ParentItemInstanceId;
			bool flag7 = parentItemInstanceId7 != null;
			Guid? instanceId9 = rod.InstanceId;
			return flag7 == (instanceId9 != null) && (parentItemInstanceId7 == null || parentItemInstanceId7.GetValueOrDefault() == instanceId9.GetValueOrDefault()) && x.ItemType == ItemTypes.Feeder;
		});
		if (sinker == null)
		{
			sinker = feeder;
		}
		Hook hook = (Hook)PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault(delegate(InventoryItem x)
		{
			Guid? parentItemInstanceId8 = x.ParentItemInstanceId;
			bool flag8 = parentItemInstanceId8 != null;
			Guid? instanceId10 = rod.InstanceId;
			return flag8 == (instanceId10 != null) && (parentItemInstanceId8 == null || parentItemInstanceId8.GetValueOrDefault() == instanceId10.GetValueOrDefault()) && (x.ItemType == ItemTypes.Hook || x.ItemType == ItemTypes.JigHead || x.ItemType == ItemTypes.Lure);
		});
		if (hook == null)
		{
			hook = new Hook
			{
				Asset = "Tackle/Hooks/DefaultHook/pDefaultHook",
				Weight = new double?(0.01)
			};
		}
		Bait bait;
		if (hook is Lure)
		{
			if (rodTemplate.IsLureBait())
			{
				bait = (Bait)PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault(delegate(InventoryItem x)
				{
					Guid? parentItemInstanceId9 = x.ParentItemInstanceId;
					bool flag9 = parentItemInstanceId9 != null;
					Guid? instanceId11 = rod.InstanceId;
					return flag9 == (instanceId11 != null) && (parentItemInstanceId9 == null || parentItemInstanceId9.GetValueOrDefault() == instanceId11.GetValueOrDefault()) && x.ItemType == ItemTypes.JigBait;
				});
			}
			else
			{
				bait = ((Lure)hook).Bait;
			}
		}
		else
		{
			bait = (Bait)PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault(delegate(InventoryItem x)
			{
				Guid? parentItemInstanceId10 = x.ParentItemInstanceId;
				bool flag10 = parentItemInstanceId10 != null;
				Guid? instanceId12 = rod.InstanceId;
				return flag10 == (instanceId12 != null) && (parentItemInstanceId10 == null || parentItemInstanceId10.GetValueOrDefault() == instanceId12.GetValueOrDefault()) && (x.ItemType == ItemTypes.Bait || x.ItemType == ItemTypes.JigBait);
			});
			if (bait == null)
			{
				bait = new Bait
				{
					Asset = "Baits/Pea/pPeas",
					Weight = new double?(0.01)
				};
			}
		}
		Chum[] array = PhotonConnectionFactory.Instance.Profile.Inventory.OfType<Chum>().Where(delegate(Chum x)
		{
			Guid? parentItemInstanceId11 = x.ParentItemInstanceId;
			bool flag11 = parentItemInstanceId11 != null;
			Guid? instanceId13 = rod.InstanceId;
			return flag11 == (instanceId13 != null) && (parentItemInstanceId11 == null || parentItemInstanceId11.GetValueOrDefault() == instanceId13.GetValueOrDefault());
		}).ToArray<Chum>();
		Chum chum = array.FirstOrDefault<Chum>();
		QuiverTip quiverTip = null;
		FeederRod feederRod = rod as FeederRod;
		if (feederRod != null && feederRod.QuiverTips != null)
		{
			quiverTip = feederRod.QuiverTips.FirstOrDefault((QuiverTip x) => x.ItemId == feederRod.QuiverId);
		}
		if (line == null || reel == null || hook == null)
		{
			return false;
		}
		this.Rod = rod;
		this.Line = line;
		this.Leader = leader;
		this.Reel = reel;
		this.Bell = bell;
		this.Hook = hook;
		this.Bobber = bobber;
		this.Sinker = sinker;
		this.Feeder = feeder;
		this.Bait = bait;
		this.ChumAll = array;
		this.Chum = chum;
		this.Quiver = quiverTip;
		this.RodTemplate = rodTemplate;
		this.IsRodDisassembled = !this.HasAssembledRod;
		return true;
	}

	public float TackleWeight
	{
		get
		{
			return (float)this.Hook.Weight.Value;
		}
	}

	public bool IsOvercasting
	{
		get
		{
			return this.Rod.CastWeightMax < this.TackleWeight;
		}
	}

	public bool IsRodDisassembled { get; set; }

	public bool HasAssembledRod
	{
		get
		{
			return this.Rod != null && this.Rod.IsRodOnDoll && PhotonConnectionFactory.Instance.Profile.Inventory.GetRodTemplate(this.Rod) != RodTemplate.UnEquiped;
		}
	}

	public void ClearRod()
	{
		this.Rod = null;
		this.Line = null;
		this.Reel = null;
		this.Bell = null;
		this.Bobber = null;
		this.Sinker = null;
		this.Feeder = null;
		this.Hook = null;
		this.Bait = null;
		this.Chum = null;
		this.ChumAll = null;
		this.Quiver = null;
		this.Leader = null;
		this.IsRodDisassembled = true;
	}

	public bool Equals(AssembledRod other)
	{
		return this.GetHashCode(this) == other.GetHashCode(other);
	}

	public object Clone()
	{
		return base.MemberwiseClone();
	}

	public bool Equals(AssembledRod x, AssembledRod y)
	{
		return x.GetHashCode(x) == y.GetHashCode(y);
	}

	public int GetHashCode(AssembledRod obj)
	{
		int num = ((obj.Rod == null) ? 0 : obj.Rod.GetHashCode(obj.Rod)) ^ ((obj.Line == null) ? 0 : obj.Line.GetHashCode(obj.Line)) ^ ((obj.Reel == null) ? 0 : obj.Reel.GetHashCode(obj.Reel)) ^ ((obj.Bell == null) ? 0 : obj.Bell.GetHashCode(obj.Bell)) ^ ((obj.Bobber == null) ? 0 : obj.Bobber.GetHashCode(obj.Bobber)) ^ ((obj.Sinker == null) ? 0 : obj.Sinker.GetHashCode(obj.Sinker)) ^ ((obj.Feeder == null) ? 0 : obj.Feeder.GetHashCode(obj.Feeder)) ^ ((obj.Hook == null) ? 0 : obj.Hook.GetHashCode(obj.Hook)) ^ ((obj.Bait == null) ? 0 : obj.Bait.GetHashCode(obj.Bait));
		int num2;
		if (obj.ChumAll != null)
		{
			num2 = string.Join(";", obj.ChumAll.Select((Chum c) => c.InstanceId.ToString()).ToArray<string>()).GetHashCode();
		}
		else
		{
			num2 = 0;
		}
		return num ^ num2 ^ ((obj.Quiver == null) ? 0 : obj.Quiver.GetHashCode(obj.Quiver)) ^ ((obj.Leader == null) ? 0 : obj.Leader.GetHashCode(obj.Leader));
	}
}
