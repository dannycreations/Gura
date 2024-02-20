using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace TPM
{
	public class CameraFrames
	{
		public CameraFrames(int framesCount)
		{
			this.KeyFrames = new List<CameraFrames.CameraFrame>();
			this.FramesCount = framesCount;
		}

		[JsonProperty]
		private List<CameraFrames.CameraFrame> KeyFrames { get; set; }

		[JsonIgnore]
		public int FramesCount { get; set; }

		[JsonIgnore]
		public int KeyFramesCount
		{
			get
			{
				return this.KeyFrames.Count;
			}
		}

		public CameraFrames.CameraFrame this[int i]
		{
			get
			{
				return this.KeyFrames[i];
			}
		}

		[JsonIgnore]
		public IEnumerable<int> KeyFramePositions
		{
			get
			{
				return this.KeyFrames.Select((CameraFrames.CameraFrame f) => f.Frame);
			}
		}

		public CameraFrames.CameraFrame GetFrameData(int frame)
		{
			return this.KeyFrames.FirstOrDefault((CameraFrames.CameraFrame f) => f.Frame == frame);
		}

		public void AddFrame(int frame, Vector3 pos, Quaternion rotation, float focalDistance, float aperture, ReplayDofTarget target)
		{
			int num = this.KeyFrames.FindIndex((CameraFrames.CameraFrame f) => f.Frame == frame);
			if (num != -1)
			{
				LogHelper.Log("Update data for {0} frame", new object[] { frame });
				this.KeyFrames[num].Position = pos;
				this.KeyFrames[num].Rotation = rotation;
				this.KeyFrames[num].FocalDistance = focalDistance;
				this.KeyFrames[num].Aperture = aperture;
				this.KeyFrames[num].Target = target;
			}
			else
			{
				num = this.KeyFrames.FindIndex((CameraFrames.CameraFrame f) => f.Frame > frame);
				CameraFrames.CameraFrame cameraFrame = new CameraFrames.CameraFrame
				{
					Frame = frame,
					Position = pos,
					Rotation = rotation,
					FocalDistance = focalDistance,
					Aperture = aperture,
					Target = target
				};
				if (num != -1)
				{
					this.KeyFrames.Insert(num, cameraFrame);
					LogHelper.Log("Insert keyframe at {0} frame ({1}/{2})", new object[]
					{
						frame,
						num,
						this.KeyFrames.Count
					});
				}
				else
				{
					this.KeyFrames.Add(cameraFrame);
					LogHelper.Log("Append keyframe at {0} frame ({1})", new object[]
					{
						frame,
						this.KeyFrames.Count
					});
				}
			}
		}

		public void DelFrame(int frame)
		{
			int num = this.KeyFrames.FindIndex((CameraFrames.CameraFrame f) => f.Frame == frame);
			if (num != -1)
			{
				LogHelper.Log("Remove keyframe at {0} frame. {1} keyframes left", new object[]
				{
					this.KeyFrames[num].Frame,
					this.KeyFrames.Count - 1
				});
				this.KeyFrames.RemoveAt(num);
			}
		}

		public int ShiftFrame(int frame, int dir)
		{
			int num = this.KeyFrames.FindIndex((CameraFrames.CameraFrame f) => f.Frame == frame);
			if (num != -1)
			{
				int num2 = frame + dir;
				if (num2 < 0)
				{
					num2 = 0;
				}
				else if (num2 >= this.FramesCount)
				{
					num2 = this.FramesCount - 1;
				}
				if (num2 != frame)
				{
					LogHelper.Log("Frame {0} shifted to {1} frame", new object[] { frame, num2 });
					CameraFrames.CameraFrame cameraFrame = this.KeyFrames[num];
					this.KeyFrames.RemoveAt(num);
					this.AddFrame(num2, cameraFrame.Position, cameraFrame.Rotation, cameraFrame.FocalDistance, cameraFrame.Aperture, cameraFrame.Target);
					return num2;
				}
			}
			return -1;
		}

		public int FindNextFrame(int frame)
		{
			int num = this.KeyFrames.FindIndex((CameraFrames.CameraFrame f) => f.Frame > frame);
			if (num != -1)
			{
				return this.KeyFrames[num].Frame;
			}
			return -1;
		}

		public int FindPreviousFrame(int frame)
		{
			if (this.KeyFrames.Count > 0)
			{
				int num = this.KeyFrames.FindIndex((CameraFrames.CameraFrame f) => f.Frame >= frame);
				if (num == -1)
				{
					num = this.KeyFrames.Count;
				}
				if (num > 0)
				{
					return this.KeyFrames[num - 1].Frame;
				}
			}
			return -1;
		}

		public void ClearAll()
		{
			if (this.KeyFrames.Count > 0)
			{
				LogHelper.Log("Remove all {0} keyframes", new object[] { this.KeyFrames.Count });
				this.KeyFrames.Clear();
			}
		}

		[JsonIgnore]
		private const float DEFAULT_FOCAL_DISTANCE = 8f;

		public class CameraFrame
		{
			[JsonProperty]
			public int Frame { get; set; }

			[JsonProperty]
			public Vector3 Position { get; set; }

			[JsonProperty]
			public Quaternion Rotation { get; set; }

			[JsonProperty]
			public float FocalDistance { get; set; }

			[JsonProperty]
			public float Aperture { get; set; }

			[JsonProperty]
			public ReplayDofTarget Target { get; set; }
		}
	}
}
