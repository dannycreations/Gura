using System;
using UnityEngine;

namespace TPM
{
	public class HandsLayersController
	{
		public HandsLayersController(Animator animator)
		{
			this.m_Animator = animator;
			this._curWeight = 0f;
			this.m_Animator.SetLayerWeight(1, this._curWeight);
		}

		public void Restart()
		{
			this._curState = HandsLayersController.HandsState.FREE;
			this._curWeight = 0f;
			this.m_Animator.SetLayerWeight(1, this._curWeight);
		}

		public void Update()
		{
			if (this._transitionActive)
			{
				float num = Mathf.Clamp01(Time.deltaTime / ((this._curState != HandsLayersController.HandsState.POD_INTERACTION) ? 0.5f : 0.1f));
				this._curWeight += (float)this._weightSign * num;
				if ((this._weightSign == 1 && this._curWeight >= this._targetWeight) || (this._weightSign == -1 && this._curWeight <= this._targetWeight))
				{
					this._transitionActive = false;
					this._curWeight = this._targetWeight;
				}
				this.m_Animator.SetLayerWeight(1, this._curWeight);
			}
		}

		public void UpdateData(bool isMove, bool isThrowedTackle, bool isSailing, bool isPodInteraction, bool isChumUsage)
		{
			this._isSailing = isSailing;
			this._isMove = isMove;
			this._isThrowedTackle = isThrowedTackle;
			this._isPodInteraction = isPodInteraction;
			this._isChumUsage = isChumUsage;
			this.CheckTransition();
		}

		public void SetRodActive(bool flag)
		{
			this._isRodActive = flag;
			this.CheckTransition();
		}

		private void CheckTransition()
		{
			HandsLayersController.HandsState handsState = HandsLayersController.HandsState.FREE;
			if (this._isChumUsage)
			{
				handsState = HandsLayersController.HandsState.CHUM;
			}
			else if (this._isPodInteraction)
			{
				handsState = HandsLayersController.HandsState.POD_INTERACTION;
			}
			else if (this._isSailing)
			{
				handsState = HandsLayersController.HandsState.SAILING;
			}
			else if (this._isThrowedTackle)
			{
				handsState = HandsLayersController.HandsState.THROWED_TACKLE;
			}
			else if (this._isRodActive)
			{
				handsState = ((!this._isMove) ? HandsLayersController.HandsState.ROD : HandsLayersController.HandsState.MOVEMENT_WITH_ROD);
			}
			if (handsState != this._curState)
			{
				this._targetWeight = this._layerWeights[(int)((byte)handsState)];
				if (!Mathf.Approximately(this._curWeight, this._targetWeight))
				{
					this._weightSign = ((this._curWeight >= this._targetWeight) ? (-1) : 1);
					this._transitionActive = true;
				}
				this._curState = handsState;
			}
		}

		private const int LAYER_INDEX = 1;

		private const float LAYER_SWITCH_TRANSITION_TIME = 0.5f;

		private const float LAYER_FAST_TRANSITION_TIME = 0.1f;

		private readonly float[] _layerWeights = new float[] { 0f, 1f, 0f, 1f, 1f, 1f, 1f };

		private HandsLayersController.HandsState _curState;

		private Animator m_Animator;

		private float _curWeight;

		private float _targetWeight;

		private int _weightSign;

		private bool _transitionActive;

		private bool _isMove;

		private bool _isRodActive;

		private bool _isThrowedTackle;

		private bool _isSailing;

		private bool _isPodInteraction;

		private bool _isChumUsage;

		public enum HandsState
		{
			FREE,
			ROD,
			MOVEMENT_WITH_ROD,
			THROWED_TACKLE,
			SAILING,
			POD_INTERACTION,
			CHUM
		}
	}
}
