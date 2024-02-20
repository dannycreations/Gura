using System;
using System.Diagnostics;
using UnityEngine;

namespace FSMAnimator
{
	public class AdditiveAnimAxisSequence
	{
		public AdditiveAnimAxisSequence(string controllerName, Animation[] animBanks, int mixingLayer, AdditiveAnimAxisSequence.UpdateF updateF, string forwardClip, string backwardClip)
		{
			this._controllerName = controllerName;
			this._updateF = updateF;
			this._forwardClip = forwardClip;
			this._backwardClip = backwardClip;
			for (int i = 0; i < animBanks.Length; i++)
			{
				this.SetupMixingAnimation(animBanks[i][forwardClip], mixingLayer);
				this.SetupMixingAnimation(animBanks[i][backwardClip], mixingLayer);
			}
		}

		private void SetupMixingAnimation(AnimationState state, int layer)
		{
			if (state != null)
			{
				state.layer = layer;
				state.blendMode = 1;
			}
		}

		public void Update(float prc)
		{
			string text;
			if (Mathf.Sign(this._prevPrc) != Mathf.Sign(prc))
			{
				text = ((this._prevPrc < 0f) ? this._backwardClip : this._forwardClip);
				this._updateF(text, 0f);
				if (prc == 0f)
				{
				}
			}
			text = ((prc < 0f) ? this._backwardClip : this._forwardClip);
			this._updateF(text, Mathf.Abs(prc));
			this._prevPrc = prc;
		}

		public void Stop()
		{
			this._updateF(this._forwardClip, 0f);
			this._updateF(this._backwardClip, 0f);
			this._prevPrc = 0f;
		}

		[Conditional("LOG_HELPER")]
		private void Log(string msg, params object[] args)
		{
		}

		private string _controllerName;

		private AdditiveAnimAxisSequence.UpdateF _updateF;

		private string _forwardClip;

		private string _backwardClip;

		private float _prevPrc;

		public delegate void UpdateF(string clipName, float prc);
	}
}
