using System;
using UnityEngine;

namespace mset
{
	[Serializable]
	public class SkyBlender
	{
		public float BlendTime
		{
			get
			{
				return this.blendTime;
			}
			set
			{
				this.blendTime = value;
			}
		}

		private float blendTimer
		{
			get
			{
				return this.endStamp - Time.time;
			}
			set
			{
				this.endStamp = Time.time + value;
			}
		}

		public float BlendWeight
		{
			get
			{
				return 1f - Mathf.Clamp01(this.blendTimer / this.currentBlendTime);
			}
		}

		public bool IsBlending
		{
			get
			{
				return Time.time < this.endStamp;
			}
		}

		public bool WasBlending(float secAgo)
		{
			return Time.time - secAgo < this.endStamp;
		}

		public void Apply()
		{
			if (this.IsBlending)
			{
				Sky.EnableGlobalProjection(this.CurrentSky.HasDimensions || this.PreviousSky.HasDimensions);
				Sky.EnableGlobalBlending(true);
				this.CurrentSky.Apply(0);
				this.PreviousSky.Apply(1);
				Sky.SetBlendWeight(this.BlendWeight);
			}
			else
			{
				Sky.EnableGlobalProjection(this.CurrentSky.HasDimensions);
				Sky.EnableGlobalBlending(false);
				this.CurrentSky.Apply(0);
			}
		}

		public void Apply(Material target)
		{
			if (this.IsBlending)
			{
				Sky.EnableBlending(target, true);
				Sky.EnableProjection(target, this.CurrentSky.HasDimensions || this.PreviousSky.HasDimensions);
				this.CurrentSky.Apply(target, 0);
				this.PreviousSky.Apply(target, 1);
				Sky.SetBlendWeight(target, this.BlendWeight);
			}
			else
			{
				Sky.EnableBlending(target, false);
				Sky.EnableProjection(target, this.CurrentSky.HasDimensions);
				this.CurrentSky.Apply(target, 0);
			}
		}

		public void Apply(Renderer target, Material[] materials)
		{
			if (this.IsBlending)
			{
				Sky.EnableBlending(target, materials, true);
				Sky.EnableProjection(target, materials, this.CurrentSky.HasDimensions || this.PreviousSky.HasDimensions);
				this.CurrentSky.ApplyFast(target, 0);
				this.PreviousSky.ApplyFast(target, 1);
				Sky.SetBlendWeight(target, this.BlendWeight);
			}
			else
			{
				Sky.EnableBlending(target, materials, false);
				Sky.EnableProjection(target, materials, this.CurrentSky.HasDimensions);
				this.CurrentSky.ApplyFast(target, 0);
			}
		}

		public void ApplyToTerrain()
		{
			if (this.IsBlending)
			{
				Sky.EnableTerrainBlending(true);
			}
			else
			{
				Sky.EnableTerrainBlending(false);
			}
		}

		public void SnapToSky(Sky nusky)
		{
			if (nusky == null)
			{
				return;
			}
			this.PreviousSky = nusky;
			this.CurrentSky = nusky;
			this.blendTimer = 0f;
		}

		public void BlendToSky(Sky nusky)
		{
			if (nusky == null)
			{
				return;
			}
			if (this.CurrentSky != nusky)
			{
				if (this.CurrentSky == null)
				{
					this.CurrentSky = nusky;
					this.PreviousSky = nusky;
					this.blendTimer = 0f;
				}
				else
				{
					this.PreviousSky = this.CurrentSky;
					this.CurrentSky = nusky;
					this.currentBlendTime = this.blendTime;
					this.blendTimer = this.currentBlendTime;
				}
			}
		}

		public void SkipTime(float sec)
		{
			this.blendTimer -= sec;
		}

		public Sky CurrentSky;

		public Sky PreviousSky;

		[SerializeField]
		private float blendTime = 0.25f;

		private float currentBlendTime = 0.25f;

		private float endStamp;
	}
}
