using System;

public struct PodSlotData
{
	public IOutline Outline
	{
		get
		{
			return this.Rod.Behaviour;
		}
	}

	public RodController Rod;

	public LineBehaviour Line;

	public TackleBehaviour Tackle;

	public ReelBehaviour Reel;

	public BellBehaviour Bell;

	public ReelTypes ReelType;
}
