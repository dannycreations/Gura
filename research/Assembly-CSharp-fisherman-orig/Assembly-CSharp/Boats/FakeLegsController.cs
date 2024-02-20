using System;
using UnityEngine;

namespace Boats
{
	public class FakeLegsController
	{
		public FakeLegsController(Animation objAnimator)
		{
			this._objAnimator = objAnimator;
			this._curState = FakeLegsController.State.IDLE;
			this._objAnimator.Play("idle");
		}

		public void SetActive(bool flag)
		{
			this._objAnimator.gameObject.SetActive(flag);
		}

		public void SetState(FakeLegsController.State state)
		{
			if (this._curState != state)
			{
				this._curState = state;
				this._objAnimator.CrossFade(this._stateToAnimation[(int)((byte)state)]);
			}
		}

		private string[] _stateToAnimation = new string[] { "idle", "row" };

		private Animation _objAnimator;

		private FakeLegsController.State _curState;

		public enum State
		{
			IDLE,
			MOVEMENT
		}
	}
}
