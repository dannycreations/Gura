using System;
using UnityEngine;

namespace mset
{
	public class ShaderIDs
	{
		public ShaderIDs()
		{
			this.SH = new int[9];
		}

		public bool valid
		{
			get
			{
				return this._valid;
			}
		}

		public void Link()
		{
			this.Link(string.Empty);
		}

		public void Link(string postfix)
		{
			this.specCubeIBL = Shader.PropertyToID("_SpecCubeIBL" + postfix);
			this.skyCubeIBL = Shader.PropertyToID("_SkyCubeIBL" + postfix);
			this.skyMatrix = Shader.PropertyToID("_SkyMatrix" + postfix);
			this.invSkyMatrix = Shader.PropertyToID("_InvSkyMatrix" + postfix);
			this.skyMin = Shader.PropertyToID("_SkyMin" + postfix);
			this.skyMax = Shader.PropertyToID("_SkyMax" + postfix);
			this.exposureIBL = Shader.PropertyToID("_ExposureIBL" + postfix);
			this.exposureLM = Shader.PropertyToID("_ExposureLM" + postfix);
			for (int i = 0; i < 9; i++)
			{
				this.SH[i] = Shader.PropertyToID("_SH" + i + postfix);
			}
			this.blendWeightIBL = Shader.PropertyToID("_BlendWeightIBL");
			this._valid = true;
		}

		public int specCubeIBL = -1;

		public int skyCubeIBL = -1;

		public int skyMatrix = -1;

		public int invSkyMatrix = -1;

		public int skySize = -1;

		public int skyMin = -1;

		public int skyMax = -1;

		public int[] SH;

		public int exposureIBL = -1;

		public int exposureLM = -1;

		public int oldExposureIBL = -1;

		public int blendWeightIBL = -1;

		private bool _valid;
	}
}
