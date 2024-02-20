using System;
using System.Collections.Generic;
using UnityEngine;

namespace FSMAnimator
{
	public class BlendClip
	{
		public BlendClip(List<Animation> banks, string animation, float weight, float time, bool automaticallyStopAnimation = false)
		{
			this._banks = banks;
			for (int i = 0; i < banks.Count; i++)
			{
				this._banks[i].Blend(animation, weight, time);
			}
			this._animation = animation;
			this.Change(weight, time, automaticallyStopAnimation);
		}

		public string Animation
		{
			get
			{
				return this._animation;
			}
		}

		public void Change(float weight, float time, bool automaticallyStopAnimation)
		{
			this._leftTime = time;
			this._startWeight = this._banks[0][this._animation].weight;
			this._targetWeight = weight;
			this._transitionDuration = time;
			this._automaticallyStopAnimation = automaticallyStopAnimation && Mathf.Approximately(this._targetWeight, 0f);
		}

		public bool Update()
		{
			this._leftTime -= Time.deltaTime;
			if (this._leftTime < 0f)
			{
				for (int i = 0; i < this._banks.Count; i++)
				{
					if (this._automaticallyStopAnimation)
					{
						this._banks[i].Stop(this._animation);
					}
					else
					{
						this._banks[i][this._animation].weight = this._targetWeight;
					}
				}
				return true;
			}
			float num = 1f - this._leftTime / this._transitionDuration;
			float num2 = this._startWeight * (1f - num) + this._targetWeight * num;
			for (int j = 0; j < this._banks.Count; j++)
			{
				this._banks[j][this._animation].weight = num2;
			}
			return false;
		}

		private readonly List<Animation> _banks;

		private string _animation;

		private float _startWeight;

		private float _targetWeight;

		private float _leftTime;

		private float _transitionDuration;

		private bool _automaticallyStopAnimation;
	}
}
