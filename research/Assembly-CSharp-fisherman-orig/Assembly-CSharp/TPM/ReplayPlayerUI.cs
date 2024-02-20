using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TPM
{
	public class ReplayPlayerUI : MonoBehaviour
	{
		private void Awake()
		{
			this._playGroup = this._lblCameraMode.transform.parent.GetComponent<CanvasGroup>();
			this._recordingGroup = this._lblRecording.transform.parent.GetComponent<CanvasGroup>();
		}

		public void Init(int framesCount, IEnumerable<int> keyFrames)
		{
			this._totalFramesCount = framesCount;
			for (int i = 0; i < this._frames.Count; i++)
			{
				this.DestroyMarkerObject(i);
			}
			this._frames.Clear();
			this._currentFrame = 0;
			this._progressValue.fillAmount = 0f;
			foreach (int num in keyFrames)
			{
				this._frames.Add(this.CreateKeyFrameRecord(num));
			}
		}

		public void SetPlayVisibility(bool flag)
		{
			this._playGroup.alpha = (float)((!flag) ? 0 : 1);
		}

		public void SetRecordingVisibility(bool flag)
		{
			this._recordingGroup.alpha = (float)((!flag) ? 0 : 1);
		}

		public void SwitchPlayVisibility()
		{
			this._playGroup.alpha = 1f - this._playGroup.alpha;
		}

		public void SetCameraMode(bool isFree)
		{
			this._lblCameraMode.text = ((!isFree) ? "Locked camera" : "Free camera");
		}

		public void SetPause(bool flag)
		{
			this._lblPause.enabled = flag;
		}

		public void SetPlaybackSpeed(float speed)
		{
			this._lblPlaybackSpeed.text = string.Format("Playback speed: {0:f1}x", speed);
		}

		public void SetMovementSpeed(float speed)
		{
			this._lblMovementSpeed.text = string.Format("Movement speed: {0:f1}x", speed);
		}

		public void SetRotationSpeed(float speed)
		{
			this._lblRotationSpeed.text = string.Format("Rotation speed: {0:f1}x", speed);
		}

		public void SetFocusTarget(ReplayDofTarget target, bool isAvailable = true)
		{
			this._lblFocalDistance.enabled = target == ReplayDofTarget.None;
			this._lblFocusOnTarget.enabled = target != ReplayDofTarget.None;
			this._lblFocusOnTarget.text = string.Format("Focus target: {0}{1}", target, (!isAvailable) ? "(N/A)" : string.Empty);
		}

		public void SetFocalSettings(float distance, float size)
		{
			this._lblFocalDistance.text = string.Format("Focal distance: {0:f1}", distance);
			this._lblFocalSize.text = string.Format("Focal size: {0:f1}", size);
		}

		public void SetAperture(float value)
		{
			this._lblAperture.text = string.Format("DoF intensity: {0:f0}", value);
		}

		public void SetHighQualityMode(bool flag)
		{
			this._lblHighQuuality.enabled = flag;
		}

		public void SetCurrentFrame(int frame)
		{
			int num;
			if (this._currentFrame != frame)
			{
				num = this._frames.FindIndex((ReplayPlayerUI.KeyFrame f) => f.Frame == this._currentFrame);
				if (num != -1)
				{
					this._frames[num].Label.Select(false);
				}
				this._currentFrame = frame;
			}
			num = this._frames.FindIndex((ReplayPlayerUI.KeyFrame f) => f.Frame == frame);
			if (num != -1)
			{
				this._frames[num].Label.Select(true);
			}
			this._progressValue.fillAmount = ((float)frame + 1f) / (float)this._totalFramesCount;
		}

		public void AddKeyFrame(int frame)
		{
			int num = this._frames.FindIndex((ReplayPlayerUI.KeyFrame f) => f.Frame == frame);
			if (num == -1)
			{
				ReplayPlayerUI.KeyFrame keyFrame = this.CreateKeyFrameRecord(frame);
				num = this._frames.FindIndex((ReplayPlayerUI.KeyFrame f) => f.Frame > frame);
				if (num != -1)
				{
					this._frames.Insert(num, keyFrame);
				}
				else
				{
					this._frames.Add(keyFrame);
				}
				if (this._currentFrame == frame)
				{
					keyFrame.Label.Select(true);
				}
			}
		}

		public void ShiftKeyFrame(int targetFrame)
		{
			if (this._frames.FindIndex((ReplayPlayerUI.KeyFrame f) => f.Frame == targetFrame) != -1)
			{
				return;
			}
			this.DelKeyFrame(this._currentFrame);
			this.AddKeyFrame(targetFrame);
		}

		public void DelKeyFrame(int frame)
		{
			int num = this._frames.FindIndex((ReplayPlayerUI.KeyFrame f) => f.Frame == frame);
			if (num != -1)
			{
				this.DestroyMarkerObject(num);
				this._frames.RemoveAt(num);
			}
		}

		public void DelAllKeyFrames()
		{
			for (int i = 0; i < this._frames.Count; i++)
			{
				this.DestroyMarkerObject(i);
			}
			this._frames.Clear();
		}

		public void SetHelpVisibility(bool flag)
		{
			this._lblHelp.enabled = flag;
		}

		private void DestroyMarkerObject(int i)
		{
			Object.Destroy(this._frames[i].Label.gameObject);
		}

		private ReplayPlayerUI.KeyFrame CreateKeyFrameRecord(int frame)
		{
			ReplayPlayerUI.KeyFrame keyFrame = new ReplayPlayerUI.KeyFrame
			{
				Frame = frame,
				Label = Object.Instantiate<ReplayUIKeyframe>(this._pKeyFrame, this._keyFramesRoot)
			};
			keyFrame.Label.SetPosition(this._progressValue.rectTransform.rect.width * (float)frame / (float)this._totalFramesCount);
			keyFrame.Label.Select(false);
			return keyFrame;
		}

		[SerializeField]
		private Text _lblPause;

		[SerializeField]
		private Text _lblPlaybackSpeed;

		[SerializeField]
		private Text _lblCameraMode;

		[SerializeField]
		private Text _lblMovementSpeed;

		[SerializeField]
		private Text _lblRotationSpeed;

		[SerializeField]
		private Text _lblFocusOnTarget;

		[SerializeField]
		private Text _lblFocalDistance;

		[SerializeField]
		private Text _lblFocalSize;

		[SerializeField]
		private Text _lblAperture;

		[SerializeField]
		private Text _lblHighQuuality;

		[SerializeField]
		private Text _lblHelp;

		[SerializeField]
		private Text _lblRecording;

		[SerializeField]
		private Image _progressValue;

		[SerializeField]
		private RectTransform _keyFramesRoot;

		[SerializeField]
		private ReplayUIKeyframe _pKeyFrame;

		private CanvasGroup _playGroup;

		private CanvasGroup _recordingGroup;

		private int _totalFramesCount;

		private List<ReplayPlayerUI.KeyFrame> _frames = new List<ReplayPlayerUI.KeyFrame>();

		private int _currentFrame;

		private class KeyFrame
		{
			public int Frame;

			public ReplayUIKeyframe Label;
		}
	}
}
