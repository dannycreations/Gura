using System;
using System.Collections.Generic;
using ObjectModel;

public static class ListOfCompatibility
{
	public static List<ItemSubTypes> GetCompatibilityEquipment(ItemSubTypes rodType)
	{
		List<ItemSubTypes> list = null;
		if (ListOfCompatibility.compatibleEquipment.TryGetValue(rodType, out list))
		{
			return list;
		}
		return null;
	}

	public static readonly Dictionary<ItemSubTypes, List<ListOfCompatibility.ConstraintType>> ItemsConstraints = new Dictionary<ItemSubTypes, List<ListOfCompatibility.ConstraintType>>
	{
		{
			ItemSubTypes.TelescopicRod,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Bait
			}
		},
		{
			ItemSubTypes.MatchRod,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Bait
			}
		},
		{
			ItemSubTypes.BottomRod,
			new List<ListOfCompatibility.ConstraintType> { ListOfCompatibility.ConstraintType.Float }
		},
		{
			ItemSubTypes.FeederRod,
			new List<ListOfCompatibility.ConstraintType> { ListOfCompatibility.ConstraintType.Float }
		},
		{
			ItemSubTypes.SpinningRod,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.CastingRod,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.SpinReel,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Bait,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.CastReel,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.SimpleHook,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Bait
			}
		},
		{
			ItemSubTypes.BarblessHook,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Bait
			}
		},
		{
			ItemSubTypes.CommonBait,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Bait
			}
		},
		{
			ItemSubTypes.FreshBait,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Bait
			}
		},
		{
			ItemSubTypes.Bobber,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Bait
			}
		},
		{
			ItemSubTypes.InsectsWormBait,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Bait
			}
		},
		{
			ItemSubTypes.Spoon,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.Spinner,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.Spinnerbait,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.BuzzBait,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.Cranckbait,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.Popper,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.Swimbait,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.Jerkbait,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.BassJig,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.BarblessSpoons,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.BarblessSpinners,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.CommonJigHeads,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.BarblessJigHeads,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.Worm,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.Grub,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.Shad,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.Tube,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.Craw,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.Frog,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.Slug,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.Walker,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.Minnow,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.Tail,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.OffsetHook,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.SpinningSinker,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.DropSinker,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.CarolinaRig,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.TexasRig,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		},
		{
			ItemSubTypes.ThreewayRig,
			new List<ListOfCompatibility.ConstraintType>
			{
				ListOfCompatibility.ConstraintType.Float,
				ListOfCompatibility.ConstraintType.Casting,
				ListOfCompatibility.ConstraintType.Lure
			}
		}
	};

	private static Dictionary<ItemSubTypes, List<ItemSubTypes>> compatibleEquipment = new Dictionary<ItemSubTypes, List<ItemSubTypes>>
	{
		{
			ItemSubTypes.TelescopicRod,
			new List<ItemSubTypes>
			{
				ItemSubTypes.TelescopicRod,
				ItemSubTypes.SpinReel,
				ItemSubTypes.MonoLine,
				ItemSubTypes.BraidLine,
				ItemSubTypes.FlurLine,
				ItemSubTypes.FlurLeader,
				ItemSubTypes.TitaniumLeader,
				ItemSubTypes.SimpleHook,
				ItemSubTypes.BarblessHook,
				ItemSubTypes.Bobber,
				ItemSubTypes.CommonBait,
				ItemSubTypes.InsectsWormBait,
				ItemSubTypes.FreshBait
			}
		},
		{
			ItemSubTypes.MatchRod,
			new List<ItemSubTypes>
			{
				ItemSubTypes.MatchRod,
				ItemSubTypes.SpinReel,
				ItemSubTypes.MonoLine,
				ItemSubTypes.BraidLine,
				ItemSubTypes.FlurLine,
				ItemSubTypes.FlurLeader,
				ItemSubTypes.TitaniumLeader,
				ItemSubTypes.SimpleHook,
				ItemSubTypes.BarblessHook,
				ItemSubTypes.Bobber,
				ItemSubTypes.CommonBait,
				ItemSubTypes.InsectsWormBait,
				ItemSubTypes.FreshBait
			}
		},
		{
			ItemSubTypes.SpinningRod,
			new List<ItemSubTypes>
			{
				ItemSubTypes.SpinningRod,
				ItemSubTypes.SpinReel,
				ItemSubTypes.MonoLine,
				ItemSubTypes.BraidLine,
				ItemSubTypes.FlurLine,
				ItemSubTypes.Spoon,
				ItemSubTypes.Spinner,
				ItemSubTypes.Spinnerbait,
				ItemSubTypes.BuzzBait,
				ItemSubTypes.Cranckbait,
				ItemSubTypes.Popper,
				ItemSubTypes.Minnow,
				ItemSubTypes.Swimbait,
				ItemSubTypes.Jerkbait,
				ItemSubTypes.BassJig,
				ItemSubTypes.BarblessSpoons,
				ItemSubTypes.BarblessSpinners,
				ItemSubTypes.CommonJigHeads,
				ItemSubTypes.BarblessJigHeads,
				ItemSubTypes.Worm,
				ItemSubTypes.Grub,
				ItemSubTypes.Shad,
				ItemSubTypes.Tube,
				ItemSubTypes.Craw,
				ItemSubTypes.Frog,
				ItemSubTypes.Walker,
				ItemSubTypes.Slug,
				ItemSubTypes.SpinningSinker,
				ItemSubTypes.DropSinker,
				ItemSubTypes.OffsetHook,
				ItemSubTypes.Tail,
				ItemSubTypes.CarolinaRig,
				ItemSubTypes.TexasRig,
				ItemSubTypes.ThreewayRig,
				ItemSubTypes.FlurLeader,
				ItemSubTypes.TitaniumLeader
			}
		},
		{
			ItemSubTypes.CastingRod,
			new List<ItemSubTypes>
			{
				ItemSubTypes.CastingRod,
				ItemSubTypes.CastReel,
				ItemSubTypes.MonoLine,
				ItemSubTypes.BraidLine,
				ItemSubTypes.FlurLine,
				ItemSubTypes.Spoon,
				ItemSubTypes.Spinner,
				ItemSubTypes.Spinnerbait,
				ItemSubTypes.BuzzBait,
				ItemSubTypes.Cranckbait,
				ItemSubTypes.Popper,
				ItemSubTypes.Minnow,
				ItemSubTypes.Swimbait,
				ItemSubTypes.Jerkbait,
				ItemSubTypes.BassJig,
				ItemSubTypes.BarblessSpoons,
				ItemSubTypes.BarblessSpinners,
				ItemSubTypes.CommonJigHeads,
				ItemSubTypes.BarblessJigHeads,
				ItemSubTypes.Worm,
				ItemSubTypes.Grub,
				ItemSubTypes.Shad,
				ItemSubTypes.Tube,
				ItemSubTypes.Craw,
				ItemSubTypes.Frog,
				ItemSubTypes.Walker,
				ItemSubTypes.Slug,
				ItemSubTypes.SpinningSinker,
				ItemSubTypes.DropSinker,
				ItemSubTypes.OffsetHook,
				ItemSubTypes.Tail,
				ItemSubTypes.CarolinaRig,
				ItemSubTypes.TexasRig,
				ItemSubTypes.ThreewayRig,
				ItemSubTypes.FlurLeader,
				ItemSubTypes.TitaniumLeader
			}
		},
		{
			ItemSubTypes.FeederRod,
			new List<ItemSubTypes>
			{
				ItemSubTypes.FeederRod,
				ItemSubTypes.Bell,
				ItemSubTypes.CommonBell,
				ItemSubTypes.ElectronicBell,
				ItemSubTypes.SpinReel,
				ItemSubTypes.LineRunningReel,
				ItemSubTypes.MonoLine,
				ItemSubTypes.BraidLine,
				ItemSubTypes.FlurLine,
				ItemSubTypes.SimpleHook,
				ItemSubTypes.BarblessHook,
				ItemSubTypes.CageFeeder,
				ItemSubTypes.Sinker,
				ItemSubTypes.Chum,
				ItemSubTypes.ChumBase,
				ItemSubTypes.Leader,
				ItemSubTypes.FlurLeader,
				ItemSubTypes.BraidLeader,
				ItemSubTypes.MonoLeader,
				ItemSubTypes.SteelLeader,
				ItemSubTypes.TitaniumLeader,
				ItemSubTypes.CommonBait,
				ItemSubTypes.InsectsWormBait,
				ItemSubTypes.FreshBait
			}
		},
		{
			ItemSubTypes.BottomRod,
			new List<ItemSubTypes>
			{
				ItemSubTypes.BottomRod,
				ItemSubTypes.Bell,
				ItemSubTypes.CommonBell,
				ItemSubTypes.ElectronicBell,
				ItemSubTypes.SpinReel,
				ItemSubTypes.LineRunningReel,
				ItemSubTypes.MonoLine,
				ItemSubTypes.BraidLine,
				ItemSubTypes.FlurLine,
				ItemSubTypes.SimpleHook,
				ItemSubTypes.BarblessHook,
				ItemSubTypes.CageFeeder,
				ItemSubTypes.Sinker,
				ItemSubTypes.Chum,
				ItemSubTypes.ChumBase,
				ItemSubTypes.Leader,
				ItemSubTypes.FlurLeader,
				ItemSubTypes.BraidLeader,
				ItemSubTypes.MonoLeader,
				ItemSubTypes.SteelLeader,
				ItemSubTypes.TitaniumLeader,
				ItemSubTypes.CommonBait,
				ItemSubTypes.InsectsWormBait,
				ItemSubTypes.FreshBait
			}
		},
		{
			ItemSubTypes.CarpRod,
			new List<ItemSubTypes>
			{
				ItemSubTypes.CarpRod,
				ItemSubTypes.SpinReel,
				ItemSubTypes.LineRunningReel,
				ItemSubTypes.MonoLine,
				ItemSubTypes.BraidLine,
				ItemSubTypes.FlurLine,
				ItemSubTypes.CarpHook,
				ItemSubTypes.SimpleHook,
				ItemSubTypes.FlatFeeder,
				ItemSubTypes.Sinker,
				ItemSubTypes.PvaFeeder,
				ItemSubTypes.Chum,
				ItemSubTypes.ChumBase,
				ItemSubTypes.CarpLeader,
				ItemSubTypes.BoilBait,
				ItemSubTypes.CommonBait
			}
		},
		{
			ItemSubTypes.SpodRod,
			new List<ItemSubTypes>
			{
				ItemSubTypes.SpodRod,
				ItemSubTypes.SpinReel,
				ItemSubTypes.MonoLine,
				ItemSubTypes.BraidLine,
				ItemSubTypes.FlurLine,
				ItemSubTypes.SpodFeeder,
				ItemSubTypes.Chum,
				ItemSubTypes.ChumBase
			}
		}
	};

	public class KeyValue<T, T1>
	{
		public KeyValue(T key, T1 value)
		{
			this.Key = key;
			this.Value = value;
		}

		public T Key;

		public T1 Value;
	}

	public enum ConstraintType
	{
		Float,
		Casting,
		Bait,
		Lure
	}
}
