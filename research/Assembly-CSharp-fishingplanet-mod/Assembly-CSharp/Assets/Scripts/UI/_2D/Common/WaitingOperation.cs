using System;
using System.Diagnostics;
using UnityEngine;

namespace Assets.Scripts.UI._2D.Common
{
	public class WaitingOperation : IUpdateable
	{
		public WaitingOperation()
		{
		}

		public WaitingOperation(float timeForWaitingActive = 0.5f, float timeForWaitingAutohide = 10f)
		{
			this._timeForWaitingActive = timeForWaitingActive;
			this._timeForWaitingAutohide = timeForWaitingAutohide;
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnStopWaiting = delegate
		{
		};

		public void Update()
		{
			this._curMovingTime += Time.deltaTime;
			if (this._curMovingTime > this._timeForWaitingActive && !UIHelper.IsWaiting)
			{
				this._curMovingTime = 0f;
				UIHelper.Waiting(true, null);
			}
			if (this._curMovingTime > this._timeForWaitingAutohide)
			{
				Debug.LogError(string.Format("WaitingOperation:Update - Force stop waiting... time:{0}", this._curMovingTime));
				this.StopWaiting(true);
			}
		}

		public void StopWaiting(bool callback = true)
		{
			this._curMovingTime = 0f;
			if (UIHelper.IsWaiting)
			{
				UIHelper.Waiting(false, null);
			}
			if (callback)
			{
				this.OnStopWaiting();
			}
		}

		private float _curMovingTime;

		private readonly float _timeForWaitingActive = 0.5f;

		private readonly float _timeForWaitingAutohide = 10f;
	}
}
