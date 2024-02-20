using System;
using UnityEngine;

public abstract class PlayerRollBase : PlayerStateBase
{
	protected bool IsRollCircleFinished
	{
		get
		{
			if (this.AnimationState == null)
			{
				return true;
			}
			if (this.AnimationState.speed == 0f)
			{
				return true;
			}
			float num = 1f;
			if (base.Player.ReelType == ReelTypes.Baitcasting)
			{
				num = 2f;
			}
			float num2 = this.AnimationState.length - Mathf.Abs(this.AnimationState.time) * num % this.AnimationState.length * this.AnimationState.length;
			float num3 = Mathf.Floor(Mathf.Abs(this.AnimationState.time) * num / this.AnimationState.length);
			bool flag = num2 < this.AnimationState.length * 0.2f;
			if (num3 > this.lastCycle && this.lastCycle != -1f)
			{
				flag = true;
			}
			this.lastCycle = num3;
			return flag;
		}
	}

	public float lastCycle = -1f;
}
