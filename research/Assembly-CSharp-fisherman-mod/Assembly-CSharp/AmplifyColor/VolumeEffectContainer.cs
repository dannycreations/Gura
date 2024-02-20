using System;
using System.Collections.Generic;
using System.Linq;

namespace AmplifyColor
{
	[Serializable]
	public class VolumeEffectContainer
	{
		public VolumeEffectContainer()
		{
			this.volumes = new List<VolumeEffect>();
		}

		public void AddColorEffect(AmplifyColorBase colorEffect)
		{
			VolumeEffect volumeEffect;
			if ((volumeEffect = this.volumes.Find((VolumeEffect s) => s.gameObject == colorEffect)) != null)
			{
				volumeEffect.UpdateVolume();
			}
			else
			{
				volumeEffect = new VolumeEffect(colorEffect);
				this.volumes.Add(volumeEffect);
				volumeEffect.UpdateVolume();
			}
		}

		public VolumeEffect AddJustColorEffect(AmplifyColorBase colorEffect)
		{
			VolumeEffect volumeEffect = new VolumeEffect(colorEffect);
			this.volumes.Add(volumeEffect);
			return volumeEffect;
		}

		public VolumeEffect GetVolumeEffect(AmplifyColorBase colorEffect)
		{
			return this.volumes.Find((VolumeEffect s) => s.gameObject == colorEffect);
		}

		public void RemoveVolumeEffect(VolumeEffect volume)
		{
			this.volumes.Remove(volume);
		}

		public AmplifyColorBase[] GetStoredEffects()
		{
			return this.volumes.Select((VolumeEffect r) => r.gameObject).ToArray<AmplifyColorBase>();
		}

		public List<VolumeEffect> volumes;
	}
}
