using System;

public class DamageThreeBandsHandler : DamageBandsHandlerBase
{
	private void Update()
	{
		if (!this.IsEnabled || base.IsHintActive)
		{
			return;
		}
		this.Init();
		this.Refresh(this.Items, this.DurabilityImages, this.DamageTexts, null, this.RullerTypes, null);
	}
}
