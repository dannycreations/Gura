using System;
using UnityEngine;

public class MoWaterController : MOControllerBase
{
	public override void Init()
	{
		if (string.IsNullOrEmpty(base.ResourceKey))
		{
			base.ResourceKey = "@" + base.gameObject.name;
		}
	}

	public override void Activate(bool flag)
	{
		if ((base.gameObject.activeSelf && flag) || (!base.gameObject.activeSelf && !flag))
		{
			return;
		}
		base.Activate(flag);
		if (flag)
		{
			GameFactory.Water.SetColor(this.BaseColor, this.AbyssColor, this.ChangeColorTime);
		}
		else
		{
			GameFactory.Water.ResetColors(this.ChangeColorTime);
		}
	}

	protected override void Update()
	{
	}

	protected override void OnInteract()
	{
	}

	[SerializeField]
	protected Color BaseColor = Color.red;

	[SerializeField]
	protected Color AbyssColor = Color.red;

	[SerializeField]
	protected float ChangeColorTime = 5f;
}
