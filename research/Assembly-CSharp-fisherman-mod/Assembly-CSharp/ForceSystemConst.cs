using System;

public static class ForceSystemConst
{
	public const float NoFishForceDamper = 0.2f;

	public const float HitchForceDamper = 0.2f;

	public const float StrikeForceDamper = 0.3f;

	public const float HookingForceDamper = 0.3f;

	public const float TelescopicForceDamper = 0.2f;

	public const float MovingBackDamper = 0.8f;

	public const float LineSlackMaxRelativeForce = 0.05f;

	public const float PlayerReelingForceMultiplier = 1f;

	public const float IndicatorForceMultiplier = 1f;

	public const float ForceAveragingPeriod = 0.2f;

	public const float ClipStrikeMultiplier = 5f;

	public const float ReelForceSafeMaxLoadDelta = 0.05f;
}
