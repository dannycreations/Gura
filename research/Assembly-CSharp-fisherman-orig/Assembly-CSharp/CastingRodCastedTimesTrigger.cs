using System;

[TriggerName(Name = "Casting rod cast number")]
[Serializable]
public class CastingRodCastedTimesTrigger : FloatRodCastedTimesTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
		base.AcceptArguments(arguments);
		this._isFloatRod = false;
	}
}
