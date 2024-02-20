using System;

public class DragMeDollRodBag : DragMeDoll
{
	public override void Clear()
	{
		base.Clear();
		if (InitRods.Instance != null)
		{
			InitRods.Instance.Refresh(null);
		}
	}
}
