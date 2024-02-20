using System;
using UnityEngine;

namespace Phy
{
	public class GpuRopeController : RopeController
	{
		public GpuRopeController()
		{
			base.SimulationFactory = new Func<ISimulation>(this.CreateGpuSimulation);
		}

		private ISimulation CreateGpuSimulation()
		{
			GpuRopeSimulation gpuRopeSimulation = new GpuRopeSimulation(new Vector3(0f, 5f, 0f), 0.01f, 3f, 0.01f, this.inputMaterial, this.updateMaterial, this.transformMaterial, this.groundHightTexture);
			gpuRopeSimulation.Init();
			return gpuRopeSimulation;
		}

		public override void Update()
		{
			this.simulation.SyncMasses();
			base.Update();
		}

		public Material inputMaterial;

		public Material updateMaterial;

		public Material transformMaterial;

		public Texture groundHightTexture;
	}
}
