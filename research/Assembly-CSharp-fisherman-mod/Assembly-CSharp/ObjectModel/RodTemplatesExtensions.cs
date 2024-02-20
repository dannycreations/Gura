using System;
using System.Linq;

namespace ObjectModel
{
	public static class RodTemplatesExtensions
	{
		public static bool IsFloat(this RodTemplate template)
		{
			return template == RodTemplate.Float;
		}

		public static bool IsSpinningFishingTemplate(this RodTemplate template)
		{
			return RodTemplatesExtensions.SpinningTemplates.Contains(template);
		}

		public static bool IsBaitFishingTemplate(this RodTemplate template)
		{
			return RodTemplatesExtensions.BaitTemplates.Contains(template);
		}

		public static bool IsBottomFishingTemplate(this RodTemplate template)
		{
			return RodTemplatesExtensions.BottomTemplates.Contains(template);
		}

		public static bool IsChumFishingTemplate(this RodTemplate template)
		{
			return RodTemplatesExtensions.ChumTemplates.Contains(template);
		}

		public static bool IsSinkerRig(this RodTemplate template)
		{
			return RodTemplatesExtensions.SinkerRigTemplates.Contains(template);
		}

		public static bool IsOffsetHook(this RodTemplate template)
		{
			return RodTemplatesExtensions.OffsetHookTemplates.Contains(template);
		}

		public static bool IsSilicon(this RodTemplate template)
		{
			return RodTemplatesExtensions.SiliconTemplates.Contains(template);
		}

		public static bool IsTails(this RodTemplate template)
		{
			return RodTemplatesExtensions.TailsTemplates.Contains(template);
		}

		public static bool IsLureBait(this RodTemplate template)
		{
			return RodTemplatesExtensions.LureBaitTemplates.Contains(template);
		}

		public static bool IsSwappableLuresType(this ItemTypes type)
		{
			return RodTemplatesExtensions.SwappableLures.Contains(type);
		}

		public static bool IsRodWithBobber(this ItemSubTypes subType)
		{
			return RodTemplatesExtensions.BobberRods.Contains(subType);
		}

		public static bool IsRodWithHook(this ItemSubTypes subType)
		{
			return RodTemplatesExtensions.HookRods.Contains(subType);
		}

		public static bool IsRodWithLure(this ItemSubTypes subType)
		{
			return RodTemplatesExtensions.LureRods.Contains(subType);
		}

		public static bool IsRodWithLeader(this ItemSubTypes subType)
		{
			return RodTemplatesExtensions.LeaderRods.Contains(subType);
		}

		public static bool IsRodWithFeeder(this ItemSubTypes subType)
		{
			return RodTemplatesExtensions.FeederRods.Contains(subType);
		}

		public static bool IsCuttableLeader(this ItemSubTypes subType)
		{
			return RodTemplatesExtensions.CuttableLeaders.Contains(subType);
		}

		public static bool IsUncuttableLeader(this ItemSubTypes subType)
		{
			return RodTemplatesExtensions.UncuttableLeaders.Contains(subType);
		}

		public static bool IsRigLeader(this ItemSubTypes subType)
		{
			return RodTemplatesExtensions.RigLeaders.Contains(subType);
		}

		public static bool IsManyTimeLeader(this ItemSubTypes subType)
		{
			return RodTemplatesExtensions.ManyTimeLeaders.Contains(subType);
		}

		private static readonly RodTemplate[] SpinningTemplates = new RodTemplate[]
		{
			RodTemplate.Jig,
			RodTemplate.OffsetJig,
			RodTemplate.Lure,
			RodTemplate.FlippingRig,
			RodTemplate.SpinnerTails,
			RodTemplate.SpinnerbaitTails,
			RodTemplate.CarolinaRig,
			RodTemplate.TexasRig,
			RodTemplate.ThreewayRig
		};

		private static readonly RodTemplate[] BaitTemplates = new RodTemplate[]
		{
			RodTemplate.Float,
			RodTemplate.Bottom,
			RodTemplate.ClassicCarp,
			RodTemplate.MethodCarp,
			RodTemplate.PVACarp
		};

		private static readonly RodTemplate[] BottomTemplates = new RodTemplate[]
		{
			RodTemplate.Bottom,
			RodTemplate.ClassicCarp,
			RodTemplate.MethodCarp,
			RodTemplate.PVACarp
		};

		private static readonly RodTemplate[] ChumTemplates = new RodTemplate[]
		{
			RodTemplate.Bottom,
			RodTemplate.ClassicCarp,
			RodTemplate.MethodCarp,
			RodTemplate.PVACarp,
			RodTemplate.Spod
		};

		private static readonly RodTemplate[] SinkerRigTemplates = new RodTemplate[]
		{
			RodTemplate.CarolinaRig,
			RodTemplate.TexasRig,
			RodTemplate.ThreewayRig
		};

		private static readonly RodTemplate[] OffsetHookTemplates = new RodTemplate[]
		{
			RodTemplate.CarolinaRig,
			RodTemplate.TexasRig,
			RodTemplate.ThreewayRig,
			RodTemplate.OffsetJig
		};

		private static readonly RodTemplate[] SiliconTemplates = new RodTemplate[]
		{
			RodTemplate.CarolinaRig,
			RodTemplate.TexasRig,
			RodTemplate.ThreewayRig,
			RodTemplate.OffsetJig,
			RodTemplate.FlippingRig,
			RodTemplate.Jig
		};

		private static readonly RodTemplate[] TailsTemplates = new RodTemplate[]
		{
			RodTemplate.SpinnerTails,
			RodTemplate.SpinnerbaitTails
		};

		private static readonly RodTemplate[] LureBaitTemplates = new RodTemplate[]
		{
			RodTemplate.SpinnerTails,
			RodTemplate.SpinnerbaitTails,
			RodTemplate.FlippingRig
		};

		private static readonly ItemTypes[] SwappableLures = new ItemTypes[]
		{
			ItemTypes.JigHead,
			ItemTypes.Lure
		};

		private static readonly ItemSubTypes[] BobberRods = new ItemSubTypes[]
		{
			ItemSubTypes.TelescopicRod,
			ItemSubTypes.MatchRod
		};

		private static readonly ItemSubTypes[] HookRods = new ItemSubTypes[]
		{
			ItemSubTypes.TelescopicRod,
			ItemSubTypes.MatchRod,
			ItemSubTypes.FeederRod,
			ItemSubTypes.BottomRod,
			ItemSubTypes.CarpRod
		};

		private static readonly ItemSubTypes[] LureRods = new ItemSubTypes[]
		{
			ItemSubTypes.SpinningRod,
			ItemSubTypes.CastingRod
		};

		private static readonly ItemSubTypes[] LeaderRods = new ItemSubTypes[]
		{
			ItemSubTypes.FeederRod,
			ItemSubTypes.BottomRod,
			ItemSubTypes.CarpRod
		};

		private static readonly ItemSubTypes[] FeederRods = new ItemSubTypes[]
		{
			ItemSubTypes.FeederRod,
			ItemSubTypes.BottomRod,
			ItemSubTypes.CarpRod,
			ItemSubTypes.SpodRod
		};

		private static readonly ItemSubTypes[] CuttableLeaders = new ItemSubTypes[]
		{
			ItemSubTypes.MonoLeader,
			ItemSubTypes.CarpLeader,
			ItemSubTypes.CarolinaRig,
			ItemSubTypes.TexasRig,
			ItemSubTypes.ThreewayRig
		};

		private static readonly ItemSubTypes[] UncuttableLeaders = new ItemSubTypes[]
		{
			ItemSubTypes.FlurLeader,
			ItemSubTypes.TitaniumLeader,
			ItemSubTypes.SteelLeader
		};

		private static readonly ItemSubTypes[] RigLeaders = new ItemSubTypes[]
		{
			ItemSubTypes.CarolinaRig,
			ItemSubTypes.TexasRig,
			ItemSubTypes.ThreewayRig
		};

		private static readonly ItemSubTypes[] ManyTimeLeaders = new ItemSubTypes[]
		{
			ItemSubTypes.FlurLeader,
			ItemSubTypes.TitaniumLeader,
			ItemSubTypes.SteelLeader
		};
	}
}
