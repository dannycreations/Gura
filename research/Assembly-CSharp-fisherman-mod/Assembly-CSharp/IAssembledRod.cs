using System;
using ObjectModel;

public interface IAssembledRod
{
	Rod Rod { get; }

	IRod RodInterface { get; }

	IReel ReelInterface { get; }

	IBell BellInterface { get; }

	ILine LineInterface { get; }

	ILeader LeaderInterface { get; }

	IBobber BobberInterface { get; }

	ISinker SinkerInterface { get; }

	IFeeder FeederInterface { get; }

	IHook HookInterface { get; }

	IBait BaitInterface { get; }

	IChum ChumInterface { get; }

	IChum[] ChumInterfaceAll { get; }

	IQuiverTip QuiverTipInterface { get; }

	ReelTypes ReelType { get; }

	RodTemplate RodTemplate { get; }

	int Slot { get; }

	bool IsRodDisassembled { get; }
}
