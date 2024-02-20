using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace TPM
{
	public class ReplayEngine : IDataCache
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action EPlaybackFinished = delegate
		{
		};

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<float> ETargetFrameReached = delegate
		{
		};

		public float SpeedK
		{
			get
			{
				return this._speedK;
			}
		}

		public int FramesCount
		{
			get
			{
				return this._data.Count;
			}
		}

		public int CurrentFrame
		{
			get
			{
				return this._pos;
			}
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
				return true;
			}
		}

		public void Pause()
		{
			this._isPaused = true;
		}

		public void TogglePause()
		{
			this._isPaused = !this._isPaused;
			LogHelper.Log("SetPause = {0}", new object[] { this._isPaused });
			if (!this._isPaused)
			{
				this._nextUpdateAt = Time.time + this._dt;
			}
		}

		public void SetPlaybackSpeed(float speedK)
		{
			this._speedK = speedK;
		}

		public bool PlayToFrame(int frame, float speedK)
		{
			if (frame < this.FramesCount && frame >= this._pos)
			{
				if (this._targetFrame < 0)
				{
					this._lastSpeedK = this._speedK;
					this._speedK = speedK;
				}
				this._targetFrame = frame;
				if (this._isPaused)
				{
					this.TogglePause();
				}
				return true;
			}
			return false;
		}

		public void AppendData(Package package)
		{
		}

		public void PushData(Stream stream, float dt)
		{
			this._speedK = 1f;
			this._dt = dt;
			this._pos = 0;
			this._data.Clear();
			while (stream.Position < stream.Length)
			{
				this._data.Add(ThirdPersonData.DeserializePortion(stream));
			}
			this._nextUpdateAt = Time.time + this._dt;
		}

		public ThirdPersonData Restart()
		{
			this._pos = 0;
			this._nextUpdateAt = Time.time + this._dt;
			return this._data[0];
		}

		public ThirdPersonData Update()
		{
			if (this._isPaused)
			{
				return null;
			}
			if (this._nextUpdateAt < Time.time)
			{
				this._nextUpdateAt += this._dt;
				if (this._pos == this._targetFrame)
				{
					this._targetFrame = -1;
					this.ETargetFrameReached(this._lastSpeedK);
					return this._data[this._pos];
				}
				if (this._pos < this._data.Count)
				{
					return this._data[this._pos++];
				}
				this.EPlaybackFinished();
			}
			return null;
		}

		private float _dt;

		private float _lastSpeedK;

		private float _speedK = 1f;

		private float _nextUpdateAt = -1f;

		private int _pos;

		private int _targetFrame = -1;

		private List<ThirdPersonData> _data = new List<ThirdPersonData>();

		private bool _isPaused;
	}
}
