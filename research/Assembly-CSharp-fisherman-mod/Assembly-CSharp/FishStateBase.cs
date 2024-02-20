using System;
using System.Linq;
using ObjectModel;

public abstract class FishStateBase : FsmBaseState<IFishController>
{
	protected IFishController Fish
	{
		get
		{
			return this._owner;
		}
	}

	protected bool IsInHands
	{
		get
		{
			return this.RodSlot.IsInHands;
		}
	}

	protected GameFactory.RodSlot RodSlot
	{
		get
		{
			return this.Fish.RodSlot;
		}
	}

	protected GameActionAdapter Adapter
	{
		get
		{
			return PhotonConnectionFactory.Instance.GetGameSlot(this.Fish.SlotId).Adapter;
		}
	}

	public virtual TPMFishState State
	{
		get
		{
			return TPMFishState.None;
		}
	}

	protected bool IsBellActive
	{
		get
		{
			return FishStateBase._rodsWithBellFeature.Contains(this.RodSlot.Rod.RodAssembly.RodTemplate);
		}
	}

	private static RodTemplate[] _rodsWithBellFeature = new RodTemplate[]
	{
		RodTemplate.Lure,
		RodTemplate.Float
	};
}
