using System;
using ObjectModel;

[TriggerName(Name = "Float rod cast number")]
[Serializable]
public class FloatRodCastedTimesTrigger : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
		if (arguments == null || arguments.Length != 1)
		{
			throw new ArgumentException("arguments array for FloatRodCastedTimesTrigger has to contain 1 argument");
		}
		this.CastNumber = int.Parse(arguments[0]);
	}

	public override bool IsTriggering()
	{
		return GameFactory.Player != null && GameFactory.Player.InFishingZone && this.IsFloatRod() == this._isFloatRod && ((FloatRodCastedTimesTrigger._castedFloatNumber >= this.CastNumber && this._isFloatRod) || (FloatRodCastedTimesTrigger._castedCastingNumber >= this.CastNumber && !this._isFloatRod)) && !this.IsFishing();
	}

	public override void Update()
	{
		base.Update();
		if (StaticUserData.CurrentPond == null)
		{
			FloatRodCastedTimesTrigger._castedFloatNumber = 0;
			FloatRodCastedTimesTrigger._castedCastingNumber = 0;
		}
		if (GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.IsInWater)
		{
			if (this.IsFloatRod() == this._isFloatRod && !FloatRodCastedTimesTrigger._wasInWater)
			{
				if (this._isFloatRod)
				{
					FloatRodCastedTimesTrigger._castedFloatNumber++;
				}
				else
				{
					FloatRodCastedTimesTrigger._castedCastingNumber++;
				}
			}
		}
		else
		{
			FloatRodCastedTimesTrigger._wasInWater = false;
		}
	}

	private bool IsFishing()
	{
		return GameFactory.Player != null && GameFactory.Player.State != typeof(PlayerIdle) && GameFactory.Player.State != typeof(PlayerIdlePitch) && GameFactory.Player.State != typeof(PlayerDrawIn) && GameFactory.Player.State != typeof(PlayerEmpty) && GameFactory.Player.State != typeof(ToolIdle);
	}

	private bool IsFloatRod()
	{
		return StaticUserData.RodInHand.Rod != null && StaticUserData.RodInHand.Rod.ItemSubType != ItemSubTypes.CastingRod && StaticUserData.RodInHand.Rod.ItemSubType != ItemSubTypes.SpinningRod;
	}

	[TriggerVariable(Name = "Number")]
	public int CastNumber;

	protected bool _isFloatRod = true;

	private static bool _wasInWater;

	private static int _castedFloatNumber;

	private static int _castedCastingNumber;
}
