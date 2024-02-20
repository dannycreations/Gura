using System;
using CodeStage.AntiCheat.ObscuredTypes;
using Phy;

public class LineOnPodBehaviour : LineBehaviour
{
	public LineOnPodBehaviour(LineController owner, IAssembledRod rodAssembly, GameFactory.RodSlot slot)
		: base(owner, rodAssembly, slot)
	{
	}

	public RodOnPodBehaviour RodOnPod
	{
		get
		{
			return (RodOnPodBehaviour)base.Rod;
		}
	}

	public override ObscuredFloat SecuredLineLength
	{
		get
		{
			return this.RodOnPod.LineLength;
		}
	}

	public override float FullLineLength
	{
		get
		{
			if (base.RodSlot.Tackle.TackleType == FishingRodSimulation.TackleType.Float || base.RodSlot.Tackle.TackleType == FishingRodSimulation.TackleType.Float)
			{
				return this.RodOnPod.LeaderLength + this.RodOnPod.LineLength;
			}
			return this.RodOnPod.LineLength;
		}
	}

	public override bool IsSlacked
	{
		get
		{
			return this.RodOnPod.DetectLineSlack && this.RodOnPod.RodTipToTackleDistance < this.RodOnPod.LineLength;
		}
	}
}
