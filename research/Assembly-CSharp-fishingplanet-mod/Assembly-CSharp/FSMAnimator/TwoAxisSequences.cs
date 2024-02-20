using System;
using UnityEngine;

namespace FSMAnimator
{
	public class TwoAxisSequences<T> : IAnimationSequence<T> where T : struct, IConvertible
	{
		public TwoAxisSequences(string name, Animation[] animBanks, int additiveAnimLayer, string idle, string forward, string forwardIdle, string backward, string backwardIdle, string axis2Forward, string axis2Backrward, T axis1Property, T axis2Property)
		{
			this._banks = new Animation[animBanks.Length];
			Array.Copy(animBanks, this._banks, this._banks.Length);
			this._mainSequence = new AnimAxisSequence(string.Format("{0}.Main", name), new AnimAxisSequence.PlayAnimationF(this.PlayAnimation), idle, forward, forwardIdle, backward, backwardIdle);
			this._additiveSequence = new AdditiveAnimAxisSequence(string.Format("{0}.Additive", name), this._banks, additiveAnimLayer, new AdditiveAnimAxisSequence.UpdateF(this.PlayAdditiveAnimation), axis2Forward, axis2Backrward);
			this.Properties = new T[] { axis1Property, axis2Property };
		}

		public T[] Properties { get; protected set; }

		public void Update(float[] properties)
		{
			this._mainSequence.Update(properties[0]);
			this._additiveSequence.Update(properties[1]);
		}

		public void Start()
		{
			this._mainSequence.Start();
		}

		public void Stop()
		{
			this._mainSequence.Stop();
			this._additiveSequence.Stop();
		}

		public void RefreshAnimation()
		{
			this._mainSequence.RefreshAnimation();
		}

		private AnimationState PlayAnimation(string clipName, float animSpeed = 1f, float time = 0f, float blendTime = 0f)
		{
			AnimationState animationState = null;
			for (int i = 0; i < this._banks.Length; i++)
			{
				AnimationState animationState2 = this._banks[i][clipName];
				if (animationState2 != null)
				{
					animationState = animationState2;
					if (animSpeed < 0f && time < 0f)
					{
						time = animationState2.length;
					}
					animationState2.speed = animSpeed;
					animationState2.time = time;
					this._banks[i].CrossFade(clipName, blendTime);
				}
			}
			return animationState;
		}

		private void PlayAdditiveAnimation(string clipName, float prc)
		{
			bool flag = !Mathf.Approximately(prc, 0f);
			for (int i = 0; i < this._banks.Length; i++)
			{
				AnimationState animationState = this._banks[i][clipName];
				animationState.normalizedTime = prc;
				animationState.weight = 1f;
				animationState.enabled = flag;
			}
		}

		private Animation[] _banks;

		private AnimAxisSequence _mainSequence;

		private AdditiveAnimAxisSequence _additiveSequence;
	}
}
