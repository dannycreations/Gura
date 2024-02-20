using System;
using System.Diagnostics;
using UnityEngine;

namespace TPM
{
	public class TPMRecieverDataCache : TPMDataCache, IDataCache
	{
		public TPMRecieverDataCache(float minCacheLength = 1f)
		{
			this._minCacheLength = minCacheLength;
			this.rnd = new Random();
			int num = (int)(10f * minCacheLength / 0.1f);
			this._dataQueue = new QueuePool<ThirdPersonData>(num);
		}

		public bool IsPaused
		{
			get
			{
				return this._isPaused;
			}
		}

		public bool IsPlayer
		{
			get
			{
				return false;
			}
		}

		public float PlaybackTime
		{
			get
			{
				return Time.realtimeSinceStartup - this._myClientStartTime;
			}
		}

		public void AppendData(Package package)
		{
			for (int i = 0; i < package.Length; i++)
			{
				this._dataQueue.Enqueue(package[i]);
			}
			ThirdPersonData last = package.Last;
			this._lastReceivedFractionTime = last.Time;
			if (this._isPaused && last.isPaused)
			{
				this._isPaused = false;
			}
		}

		private bool IsNewFractionTime(double serverTime)
		{
			return serverTime <= (double)(this._otherClientStartTime + this.PlaybackTime);
		}

		public ThirdPersonData Update()
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if (this._isPaused)
			{
				if (this._dataQueue.Count > 0 && this._lastReceivedFractionTime - this._dataQueue.Peek().Time >= this._minCacheLength)
				{
					this._isPaused = false;
					return this.StartSimulation();
				}
			}
			else if (this._wasStarted)
			{
				if (this._nextDebugPrintAt < realtimeSinceStartup)
				{
					this._nextDebugPrintAt = realtimeSinceStartup + 3f;
				}
				if (this._dataQueue.Count > 0 && this.IsNewFractionTime((double)this._dataQueue.Peek().Time))
				{
					return this.GetBestFraction();
				}
			}
			else if ((float)this._dataQueue.Count * 0.1f >= this._minCacheLength)
			{
				this._wasStarted = true;
				this._nextDebugPrintAt = realtimeSinceStartup + 3f;
				return this.StartSimulation();
			}
			return null;
		}

		private ThirdPersonData StartSimulation()
		{
			ThirdPersonData fraction = this.GetFraction();
			this._otherClientStartTime = fraction.Time;
			this._myClientStartTime = Time.realtimeSinceStartup;
			return fraction;
		}

		private ThirdPersonData GetBestFraction()
		{
			ThirdPersonData thirdPersonData = this.GetFraction();
			while (!thirdPersonData.isPaused && this._dataQueue.Count > 0 && this.IsNewFractionTime((double)this._dataQueue.Peek().Time))
			{
				ThirdPersonData thirdPersonData2 = this._dataQueue.Peek();
				if (thirdPersonData2.Time < this._lastProcessedFractionTime)
				{
					LogHelper.Error("process package in indirect order", new object[0]);
				}
				if (!this.isSameNotSkipableFraction(thirdPersonData, thirdPersonData2))
				{
					break;
				}
				thirdPersonData = this.GetFraction();
			}
			if (thirdPersonData.isPaused)
			{
				this._isPaused = thirdPersonData.isPaused;
			}
			this._lastProcessedFractionTime = thirdPersonData.Time;
			if (this._dataQueue.Count != 0 || !thirdPersonData.isPaused)
			{
			}
			return thirdPersonData;
		}

		private bool isSameNotSkipableFraction(ThirdPersonData curFraction, ThirdPersonData nextFraction)
		{
			for (int i = 0; i < nextFraction.ByteParameters.Length; i++)
			{
				if (curFraction.ByteParameters[i] != nextFraction.ByteParameters[i])
				{
					return false;
				}
			}
			for (int j = 0; j < nextFraction.BoolParameters.Length; j++)
			{
				if (curFraction.BoolParameters[j] != nextFraction.BoolParameters[j])
				{
					return false;
				}
			}
			return true;
		}

		private ThirdPersonData GetFraction()
		{
			return this._dataQueue.Dequeue();
		}

		[Conditional("LOG_HELPER")]
		private void Log(string text)
		{
		}

		[Conditional("LOG_HELPER")]
		private void Log(string textPattern, params object[] args)
		{
		}

		private const float MIN_CACHE_LENGTH = 1f;

		private const float DEBUG_PRINT_INTERVAL = 3f;

		private const float QUEUE_CAPACITY_K = 10f;

		private readonly QueuePool<ThirdPersonData> _dataQueue;

		private bool _wasStarted;

		private bool _isPaused;

		private float _nextDebugPrintAt;

		private float _lastProcessedFractionTime;

		private float _lastReceivedFractionTime;

		private float _otherClientStartTime;

		private float _myClientStartTime;

		private float _minCacheLength;

		private Random rnd;
	}
}
