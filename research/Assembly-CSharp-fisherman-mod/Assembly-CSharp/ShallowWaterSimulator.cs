using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Flowmap;
using UnityEngine;

[AddComponentMenu("Flowmaps/Simulators/Shallow Water")]
[ExecuteInEditMode]
public class ShallowWaterSimulator : FlowSimulator
{
	private Material SimulationMaterial
	{
		get
		{
			if (!this.simulationMaterial)
			{
				this.simulationMaterial = new Material(Shader.Find("Hidden/ShallowWaterFlowmapSimulator"));
				this.simulationMaterial.hideFlags = 61;
			}
			return this.simulationMaterial;
		}
	}

	public Texture HeightFluidTexture
	{
		get
		{
			if (FlowmapGenerator.SimulationPath == SimulationPath.CPU)
			{
				return this.heightFluidCpu;
			}
			return this.heightFluidRT;
		}
	}

	public Texture VelocityAccumulatedTexture
	{
		get
		{
			if (FlowmapGenerator.SimulationPath == SimulationPath.CPU && this.velocityAccumulatedCpu)
			{
				return this.velocityAccumulatedCpu;
			}
			if (this.velocityAccumulatedRT)
			{
				return this.velocityAccumulatedRT;
			}
			return null;
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event VoidEvent OnRenderTextureReset;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event VoidEvent OnMaxStepsReached;

	public override void Init()
	{
		base.Init();
		this.Cleanup();
		SimulationPath simulationPath = FlowmapGenerator.SimulationPath;
		if (simulationPath != SimulationPath.GPU)
		{
			if (simulationPath == SimulationPath.CPU)
			{
				this.ResetCpuData();
				this.BakeFieldsCpu();
				for (int i = 0; i < this.resolutionX; i++)
				{
					for (int j = 0; j < this.resolutionY; j++)
					{
						this.simulationDataCpu[i][j].fluid = ((this.fluidDepth != FluidDepth.DeepWater) ? this.initialFluidAmount : ((1f - Mathf.Ceil(this.simulationDataCpu[i][j].height)) * this.initialFluidAmount));
					}
				}
				this.initializedCpu = true;
			}
		}
		else
		{
			this.heightFluidRT = new RenderTexture(this.resolutionX, this.resolutionY, 0, FlowmapGenerator.GetTwoChannelRTFormat, 1);
			this.heightFluidRT.hideFlags = 61;
			this.heightFluidRT.name = "HeightFluid";
			this.heightFluidRT.Create();
			this.fluidAddRT = new RenderTexture(this.resolutionX, this.resolutionY, 0, FlowmapGenerator.GetSingleChannelRTFormat, 1);
			this.fluidAddRT.hideFlags = 61;
			this.fluidAddRT.name = "FluidAdd";
			this.fluidAddRT.Create();
			this.fluidRemoveRT = new RenderTexture(this.resolutionX, this.resolutionY, 0, FlowmapGenerator.GetSingleChannelRTFormat, 1);
			this.fluidRemoveRT.hideFlags = 61;
			this.fluidRemoveRT.name = "FluidRemove";
			this.fluidRemoveRT.Create();
			this.fluidForceRT = new RenderTexture(this.resolutionX, this.resolutionY, 0, FlowmapGenerator.GetFourChannelRTFormat, 1);
			this.fluidForceRT.hideFlags = 61;
			this.fluidForceRT.name = "FluidForce";
			this.fluidForceRT.Create();
			this.outflowRT = new RenderTexture(this.resolutionX, this.resolutionY, 0, FlowmapGenerator.GetFourChannelRTFormat, 1);
			this.outflowRT.hideFlags = 61;
			this.outflowRT.name = "Outflow";
			this.outflowRT.Create();
			this.velocityRT = new RenderTexture(this.resolutionX, this.resolutionY, 0, FlowmapGenerator.GetTwoChannelRTFormat, 1);
			this.velocityRT.hideFlags = 61;
			this.velocityRT.name = "Velocity";
			this.velocityRT.Create();
			this.velocityAccumulatedRT = new RenderTexture(this.resolutionX, this.resolutionY, 0, FlowmapGenerator.GetFourChannelRTFormat, 1);
			this.velocityAccumulatedRT.hideFlags = 61;
			this.velocityAccumulatedRT.name = "VelocityAccumulated";
			this.velocityAccumulatedRT.Create();
			this.bufferRT1 = new RenderTexture(this.resolutionX, this.resolutionY, 0, FlowmapGenerator.GetFourChannelRTFormat, 1);
			this.bufferRT1.hideFlags = 61;
			this.bufferRT1.name = "Buffer1";
			this.bufferRT1.Create();
			this.fieldRenderCamera = new GameObject("Field Renderer", new Type[] { typeof(Camera) }).GetComponent<Camera>();
			this.fieldRenderCamera.gameObject.hideFlags = 61;
			this.fieldRenderCamera.orthographic = true;
			this.fieldRenderCamera.orthographicSize = Mathf.Max(base.Generator.Dimensions.x, base.Generator.Dimensions.y) * 0.5f;
			this.fieldRenderCamera.renderingPath = 1;
			this.fieldRenderCamera.cullingMask = 1 << FlowmapGenerator.GpuRenderLayer.value;
			this.fieldRenderCamera.clearFlags = 2;
			this.fieldRenderCamera.backgroundColor = Color.black;
			this.fieldRenderCamera.enabled = false;
			this.fieldRenderCamera.useOcclusionCulling = false;
			this.ResetGPUData();
			this.initializedGpu = true;
		}
		if (base.Generator.Heightmap is FlowRenderHeightmap)
		{
			(base.Generator.Heightmap as FlowRenderHeightmap).UpdateHeightmap();
		}
	}

	private void DestroyProperly(Object obj)
	{
		if (Application.isEditor || !Application.isPlaying)
		{
			Object.DestroyImmediate(obj);
		}
		else if (Application.isPlaying && !Application.isEditor)
		{
			Object.Destroy(obj);
		}
	}

	private void Cleanup()
	{
		RenderTexture.active = null;
		if (this.heightFluidRT)
		{
			this.DestroyProperly(this.heightFluidRT);
		}
		if (this.fluidAddRT)
		{
			this.DestroyProperly(this.fluidAddRT);
		}
		if (this.fluidRemoveRT)
		{
			this.DestroyProperly(this.fluidRemoveRT);
		}
		if (this.fluidForceRT)
		{
			this.DestroyProperly(this.fluidForceRT);
		}
		if (this.outflowRT)
		{
			this.DestroyProperly(this.outflowRT);
		}
		if (this.velocityRT)
		{
			this.DestroyProperly(this.velocityRT);
		}
		if (this.velocityAccumulatedRT)
		{
			this.DestroyProperly(this.velocityAccumulatedRT);
		}
		if (this.bufferRT1)
		{
			this.DestroyProperly(this.bufferRT1);
		}
		if (this.fieldRenderCamera)
		{
			this.DestroyProperly(this.fieldRenderCamera.gameObject);
		}
		if (this.simulationMaterial)
		{
			this.DestroyProperly(this.simulationMaterial);
		}
		this.initializedGpu = false;
		this.simulationDataCpu = null;
		if (this.heightFluidCpu)
		{
			this.DestroyProperly(this.heightFluidCpu);
		}
		if (this.velocityAccumulatedCpu)
		{
			this.DestroyProperly(this.heightFluidCpu);
		}
		this.initializedCpu = false;
	}

	private void OnDestroy()
	{
		this.Cleanup();
	}

	public override void Reset()
	{
		this.Init();
		base.Reset();
		this.AssignToMaterials();
	}

	private void ResetGPUData()
	{
		this.SimulationMaterial.SetColor("_Color", new Color(0.5f, 0.5f, 0f, 1f));
		Graphics.Blit(null, this.velocityRT, this.SimulationMaterial, 4);
		Graphics.Blit(null, this.velocityAccumulatedRT, this.SimulationMaterial, 4);
		Graphics.Blit(null, this.fluidForceRT, this.SimulationMaterial, 4);
		this.SimulationMaterial.SetColor("_Color", Color.black);
		Graphics.Blit(null, this.fluidAddRT, this.SimulationMaterial, 4);
		Graphics.Blit(null, this.fluidRemoveRT, this.SimulationMaterial, 4);
		Graphics.Blit(null, this.bufferRT1, this.SimulationMaterial, 4);
		this.SimulationMaterial.SetColor("_Color", new Color(0f, 0f, 0f, 0f));
		Graphics.Blit(null, this.outflowRT, this.SimulationMaterial, 4);
		if (base.Generator.Heightmap)
		{
			Graphics.Blit(base.Generator.Heightmap.HeightmapTexture, this.heightFluidRT, this.SimulationMaterial, 6);
		}
		else
		{
			this.SimulationMaterial.SetColor("_Color", new Color(0f, 0f, 0f, 0f));
			Graphics.Blit(null, this.heightFluidRT, this.SimulationMaterial, 4);
		}
		if (this.initialFluidAmount > 0f)
		{
			this.SimulationMaterial.SetFloat("_DeepWater", (float)((this.fluidDepth != FluidDepth.DeepWater) ? 0 : 1));
			this.SimulationMaterial.SetFloat("_FluidAmount", this.initialFluidAmount);
			Graphics.Blit(this.heightFluidRT, this.bufferRT1, this.SimulationMaterial, 14);
			Graphics.Blit(this.bufferRT1, this.heightFluidRT);
		}
	}

	private void ResetCpuData()
	{
		if (this.simulationDataCpu == null)
		{
			this.simulationDataCpu = new SimulationData[this.resolutionX][];
		}
		for (int i = 0; i < this.resolutionX; i++)
		{
			if (this.simulationDataCpu[i] == null)
			{
				this.simulationDataCpu[i] = new SimulationData[this.resolutionY];
			}
			for (int j = 0; j < this.resolutionY; j++)
			{
				this.simulationDataCpu[i][j] = default(SimulationData);
				this.simulationDataCpu[i][j].velocity = new Vector3(0.5f, 0.5f, 0f);
				this.simulationDataCpu[i][j].velocityAccumulated = new Vector3(0.5f, 0.5f, 0f);
			}
		}
	}

	public override void StartSimulating()
	{
		base.StartSimulating();
		SimulationPath simulationPath = FlowmapGenerator.SimulationPath;
		if (simulationPath != SimulationPath.CPU)
		{
			if (simulationPath == SimulationPath.GPU)
			{
				if (!this.initializedGpu)
				{
					this.Init();
				}
			}
		}
		else
		{
			if (this.simulationDataCpu == null)
			{
				this.initializedCpu = false;
			}
			if (!this.initializedCpu)
			{
				this.Init();
			}
		}
	}

	public override void Tick()
	{
		base.Tick();
		if (!base.Simulating)
		{
			return;
		}
		SimulationPath simulationPath = FlowmapGenerator.SimulationPath;
		if (simulationPath != SimulationPath.GPU)
		{
			if (simulationPath == SimulationPath.CPU)
			{
				if (FlowmapGenerator.ThreadCount > 1)
				{
					int num = Mathf.CeilToInt((float)this.resolutionX / (float)FlowmapGenerator.ThreadCount);
					ManualResetEvent[] array = new ManualResetEvent[FlowmapGenerator.ThreadCount];
					ArrayThreadedInfo[] array2 = new ArrayThreadedInfo[FlowmapGenerator.ThreadCount];
					for (int i = 0; i < FlowmapGenerator.ThreadCount; i++)
					{
						array[i] = new ManualResetEvent(false);
						array2[i] = new ArrayThreadedInfo(0, 0, null);
					}
					for (int j = 0; j < FlowmapGenerator.ThreadCount; j++)
					{
						array[j].Reset();
						array2[j].start = j * num;
						array2[j].length = ((j != FlowmapGenerator.ThreadCount - 1) ? num : (this.resolutionX - 1 - j * num));
						array2[j].resetEvent = array[j];
						ThreadPool.QueueUserWorkItem(new WaitCallback(this.AddRemoveFluidThreaded), array2[j]);
					}
					WaitHandle.WaitAll(array);
					for (int k = 0; k < FlowmapGenerator.ThreadCount; k++)
					{
						int num2 = k * num;
						int num3 = ((k != FlowmapGenerator.ThreadCount - 1) ? num : (this.resolutionX - 1 - k * num));
						array[k] = new ManualResetEvent(false);
						ThreadPool.QueueUserWorkItem(new WaitCallback(this.OutflowThreaded), new ArrayThreadedInfo(num2, num3, array[k]));
					}
					WaitHandle.WaitAll(array);
					for (int l = 0; l < FlowmapGenerator.ThreadCount; l++)
					{
						int num4 = l * num;
						int num5 = ((l != FlowmapGenerator.ThreadCount - 1) ? num : (this.resolutionX - 1 - l * num));
						array[l] = new ManualResetEvent(false);
						ThreadPool.QueueUserWorkItem(new WaitCallback(this.UpdateVelocityThreaded), new ArrayThreadedInfo(num4, num5, array[l]));
					}
					WaitHandle.WaitAll(array);
					if (this.simulateFoam)
					{
						for (int m = 0; m < FlowmapGenerator.ThreadCount; m++)
						{
							int num6 = m * num;
							int num7 = ((m != FlowmapGenerator.ThreadCount - 1) ? num : (this.resolutionX - 1 - m * num));
							array[m] = new ManualResetEvent(false);
							ThreadPool.QueueUserWorkItem(new WaitCallback(this.FoamThreaded), new ArrayThreadedInfo(num6, num7, array[m]));
						}
						WaitHandle.WaitAll(array);
					}
					if (this.outputFilterStrength > 0f)
					{
						for (int n = 0; n < FlowmapGenerator.ThreadCount; n++)
						{
							int num8 = n * num;
							int num9 = ((n != FlowmapGenerator.ThreadCount - 1) ? num : (this.resolutionX - 1 - n * num));
							array[n] = new ManualResetEvent(false);
							ThreadPool.QueueUserWorkItem(new WaitCallback(this.BlurVelocityAccumulatedHorizontalThreaded), new ArrayThreadedInfo(num8, num9, array[n]));
						}
						WaitHandle.WaitAll(array);
						for (int num10 = 0; num10 < FlowmapGenerator.ThreadCount; num10++)
						{
							int num11 = num10 * num;
							int num12 = ((num10 != FlowmapGenerator.ThreadCount - 1) ? num : (this.resolutionX - 1 - num10 * num));
							array[num10] = new ManualResetEvent(false);
							ThreadPool.QueueUserWorkItem(new WaitCallback(this.BlurVelocityAccumulatedVerticalThreaded), new ArrayThreadedInfo(num11, num12, array[num10]));
						}
						WaitHandle.WaitAll(array);
					}
				}
				else
				{
					for (int num13 = 0; num13 < this.resolutionX; num13++)
					{
						for (int num14 = 0; num14 < this.resolutionY; num14++)
						{
							this.AddRemoveFluidCpu(num13, num14);
						}
					}
					for (int num15 = 0; num15 < this.resolutionX; num15++)
					{
						for (int num16 = 0; num16 < this.resolutionY; num16++)
						{
							this.OutflowCpu(num15, num16);
						}
					}
					for (int num17 = 0; num17 < this.resolutionX; num17++)
					{
						for (int num18 = 0; num18 < this.resolutionY; num18++)
						{
							this.UpdateVelocityCpu(num17, num18);
						}
					}
					if (this.simulateFoam)
					{
						for (int num19 = 0; num19 < this.resolutionX; num19++)
						{
							for (int num20 = 0; num20 < this.resolutionY; num20++)
							{
								this.FoamCpu(num19, num20);
							}
						}
					}
					if (this.outputFilterStrength > 0f)
					{
						for (int num21 = 0; num21 < this.resolutionX; num21++)
						{
							for (int num22 = 0; num22 < this.resolutionY; num22++)
							{
								this.BlurVelocityAccumulatedHorizontalCpu(num21, num22);
							}
						}
						for (int num23 = 0; num23 < this.resolutionX; num23++)
						{
							for (int num24 = 0; num24 < this.resolutionY; num24++)
							{
								this.BlurVelocityAccumulatedVerticalCpu(num23, num24);
							}
						}
					}
				}
				if (base.SimulationStepsCount % this.updateTextureDelayCPU == 0)
				{
					this.WriteCpuDataToTexture();
				}
			}
		}
		else
		{
			if (!this.heightFluidRT.IsCreated() || !this.outflowRT.IsCreated() || !this.velocityRT.IsCreated() || !this.velocityAccumulatedRT.IsCreated())
			{
				if (this.OnRenderTextureReset != null)
				{
					this.OnRenderTextureReset();
				}
				this.Init();
			}
			float num25 = base.Generator.transform.position.y;
			float num26 = base.Generator.transform.position.y;
			for (int num27 = 0; num27 < base.Generator.Fields.Length; num27++)
			{
				num25 = Mathf.Max(num25, base.Generator.Fields[num27].transform.position.y);
				num26 = Mathf.Min(num26, base.Generator.Fields[num27].transform.position.y);
			}
			this.fieldRenderCamera.transform.localPosition = base.Generator.transform.position;
			this.fieldRenderCamera.transform.position = new Vector3(this.fieldRenderCamera.transform.position.x, num25 + 1f, this.fieldRenderCamera.transform.position.z);
			this.fieldRenderCamera.farClipPlane = num25 - num26 + 2f;
			this.fieldRenderCamera.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
			this.fieldRenderCamera.useOcclusionCulling = false;
			this.SimulationMaterial.SetVector("_Resolution", new Vector4((float)this.resolutionX, (float)this.resolutionY, 0f, 0f));
			this.SimulationMaterial.SetFloat("_Timestep", this.timestep);
			this.SimulationMaterial.SetFloat("_Gravity", this.gravity);
			this.SimulationMaterial.SetFloat("_VelocityScale", this.velocityScale);
			foreach (FlowSimulationField flowSimulationField in base.Generator.Fields)
			{
				if (flowSimulationField.Pass == FieldPass.AddFluid)
				{
					flowSimulationField.TickStart();
				}
			}
			this.fieldRenderCamera.backgroundColor = Color.black;
			this.fieldRenderCamera.targetTexture = this.fluidAddRT;
			this.fieldRenderCamera.RenderWithShader(Shader.Find("Hidden/FlowmapOffscreenRender"), "Offscreen");
			foreach (FlowSimulationField flowSimulationField2 in base.Generator.Fields)
			{
				if (flowSimulationField2.Pass == FieldPass.AddFluid)
				{
					flowSimulationField2.TickEnd();
				}
			}
			foreach (FlowSimulationField flowSimulationField3 in base.Generator.Fields)
			{
				if (flowSimulationField3.Pass == FieldPass.RemoveFluid)
				{
					flowSimulationField3.TickStart();
				}
			}
			this.fieldRenderCamera.backgroundColor = Color.black;
			this.fieldRenderCamera.targetTexture = this.fluidRemoveRT;
			this.fieldRenderCamera.RenderWithShader(Shader.Find("Hidden/FlowmapOffscreenRender"), "Offscreen");
			foreach (FlowSimulationField flowSimulationField4 in base.Generator.Fields)
			{
				if (flowSimulationField4.Pass == FieldPass.RemoveFluid)
				{
					flowSimulationField4.TickEnd();
				}
			}
			foreach (FlowSimulationField flowSimulationField5 in base.Generator.Fields)
			{
				if (flowSimulationField5.Pass == FieldPass.Force)
				{
					flowSimulationField5.TickStart();
				}
			}
			this.fieldRenderCamera.backgroundColor = new Color(Mathf.LinearToGammaSpace(0.5f), Mathf.LinearToGammaSpace(0.5f), 0f, 1f);
			this.fieldRenderCamera.targetTexture = this.fluidForceRT;
			this.fieldRenderCamera.RenderWithShader(Shader.Find("Hidden/FlowmapOffscreenRender"), "Offscreen");
			foreach (FlowSimulationField flowSimulationField6 in base.Generator.Fields)
			{
				if (flowSimulationField6.Pass == FieldPass.Force)
				{
					flowSimulationField6.TickEnd();
				}
			}
			if (base.Generator.Heightmap && base.Generator.Heightmap is FlowRenderHeightmap && (base.Generator.Heightmap as FlowRenderHeightmap).dynamicUpdating)
			{
				(base.Generator.Heightmap as FlowRenderHeightmap).UpdateHeightmap();
			}
			if (base.Generator.Heightmap)
			{
				if (base.Generator.Heightmap is FlowTextureHeightmap && (base.Generator.Heightmap as FlowTextureHeightmap).isRaw)
				{
					this.SimulationMaterial.SetFloat("_IsFloatRGBA", 1f);
				}
				else
				{
					this.SimulationMaterial.SetFloat("_IsFloatRGBA", 0f);
				}
				this.SimulationMaterial.SetTexture("_NewHeightTex", base.Generator.Heightmap.HeightmapTexture);
				Graphics.Blit(this.heightFluidRT, this.bufferRT1, this.SimulationMaterial, 9);
				Graphics.Blit(this.bufferRT1, this.heightFluidRT);
			}
			foreach (FlowSimulationField flowSimulationField7 in base.Generator.Fields)
			{
				if (flowSimulationField7.Pass == FieldPass.Heightmap)
				{
					flowSimulationField7.TickStart();
				}
			}
			this.fieldRenderCamera.backgroundColor = Color.black;
			RenderTexture temporary = RenderTexture.GetTemporary(this.resolutionX, this.resolutionY, 0, FlowmapGenerator.GetSingleChannelRTFormat, 1);
			this.fieldRenderCamera.targetTexture = temporary;
			this.fieldRenderCamera.RenderWithShader(Shader.Find("Hidden/FlowmapOffscreenRender"), "Offscreen");
			foreach (FlowSimulationField flowSimulationField8 in base.Generator.Fields)
			{
				if (flowSimulationField8.Pass == FieldPass.Heightmap)
				{
					flowSimulationField8.TickEnd();
				}
			}
			this.SimulationMaterial.SetTexture("_HeightmapFieldsTex", temporary);
			Graphics.Blit(this.heightFluidRT, this.bufferRT1, this.SimulationMaterial, 11);
			Graphics.Blit(this.bufferRT1, this.heightFluidRT);
			RenderTexture.ReleaseTemporary(temporary);
			this.SimulationMaterial.SetTexture("_FluidAddTex", this.fluidAddRT);
			this.SimulationMaterial.SetTexture("_FluidRemoveTex", this.fluidRemoveRT);
			this.SimulationMaterial.SetFloat("_Evaporation", this.evaporationRate);
			this.SimulationMaterial.SetFloat("_FluidAddMultiplier", this.fluidAddMultiplier);
			this.SimulationMaterial.SetFloat("_FluidRemoveMultiplier", this.fluidRemoveMultiplier);
			Graphics.Blit(this.heightFluidRT, this.bufferRT1, this.SimulationMaterial, 0);
			Graphics.Blit(this.bufferRT1, this.heightFluidRT);
			this.SimulationMaterial.SetTexture("_FluidForceTex", this.fluidForceRT);
			this.SimulationMaterial.SetFloat("_FluidForceMultiplier", this.fluidForceMultiplier);
			this.SimulationMaterial.SetTexture("_OutflowTex", this.outflowRT);
			this.SimulationMaterial.SetTexture("_VelocityTex", this.velocityRT);
			this.SimulationMaterial.SetFloat("_BorderCollision", (float)((this.borderCollision != SimulationBorderCollision.Collide) ? 0 : 1));
			Graphics.Blit(this.heightFluidRT, this.bufferRT1, this.SimulationMaterial, 1);
			Graphics.Blit(this.bufferRT1, this.outflowRT);
			this.SimulationMaterial.SetTexture("_OutflowTex", this.outflowRT);
			Graphics.Blit(this.heightFluidRT, this.bufferRT1, this.SimulationMaterial, 2);
			Graphics.Blit(this.bufferRT1, this.heightFluidRT);
			this.SimulationMaterial.SetTexture("_OutflowTex", this.outflowRT);
			Graphics.Blit(this.heightFluidRT, this.bufferRT1, this.SimulationMaterial, 3);
			Graphics.Blit(this.bufferRT1, this.velocityRT);
			this.SimulationMaterial.SetFloat("_Delta", this.outputAccumulationRate);
			this.SimulationMaterial.SetTexture("_VelocityTex", this.velocityRT);
			this.SimulationMaterial.SetTexture("_VelocityAccumTex", this.velocityAccumulatedRT);
			Graphics.Blit(this.heightFluidRT, this.bufferRT1, this.SimulationMaterial, 5);
			Graphics.Blit(this.bufferRT1, this.velocityAccumulatedRT);
			if (this.simulateFoam)
			{
				this.SimulationMaterial.SetFloat("_Delta", this.outputAccumulationRate);
				this.SimulationMaterial.SetTexture("_FluidAddTex", this.fluidAddRT);
				this.SimulationMaterial.SetTexture("_VelocityAccumTex", this.velocityAccumulatedRT);
				this.SimulationMaterial.SetFloat("_FoamVelocityScale", this.foamVelocityScale);
				Graphics.Blit(this.heightFluidRT, this.bufferRT1, this.SimulationMaterial, 10);
				Graphics.Blit(this.bufferRT1, this.velocityAccumulatedRT);
			}
			if (this.writeFluidDepthInAlpha)
			{
				this.SimulationMaterial.SetTexture("_HeightFluidTex", this.heightFluidRT);
				Graphics.Blit(this.velocityAccumulatedRT, this.bufferRT1, this.SimulationMaterial, 15);
				Graphics.Blit(this.bufferRT1, this.velocityAccumulatedRT);
			}
			if (this.outputFilterStrength > 0f)
			{
				this.SimulationMaterial.SetFloat("_BlurSpread", (float)this.outputFilterSize);
				this.SimulationMaterial.SetFloat("_Strength", this.outputFilterStrength);
				Graphics.Blit(this.velocityAccumulatedRT, this.bufferRT1, this.SimulationMaterial, 7);
				Graphics.Blit(this.bufferRT1, this.velocityAccumulatedRT, this.SimulationMaterial, 8);
			}
		}
	}

	private void BakeFieldsCpu()
	{
		if (base.Generator.Heightmap)
		{
			Texture2D texture2D = null;
			bool flag = false;
			bool flag2 = false;
			if (base.Generator.Heightmap is FlowTextureHeightmap && (base.Generator.Heightmap as FlowTextureHeightmap).HeightmapTexture as Texture2D != null)
			{
				texture2D = (base.Generator.Heightmap as FlowTextureHeightmap).HeightmapTexture as Texture2D;
				flag = (base.Generator.Heightmap as FlowTextureHeightmap).isRaw;
			}
			else if (base.Generator.Heightmap is FlowRenderHeightmap && FlowRenderHeightmap.Supported)
			{
				(this.generator.Heightmap as FlowRenderHeightmap).UpdateHeightmap();
				RenderTexture renderTexture = (this.generator.Heightmap as FlowRenderHeightmap).HeightmapTexture as RenderTexture;
				RenderTexture temporary = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, 0, 0, 1);
				Graphics.Blit(renderTexture, temporary);
				texture2D = new Texture2D(renderTexture.width, renderTexture.height);
				texture2D.hideFlags = 61;
				RenderTexture.active = temporary;
				texture2D.ReadPixels(new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), 0, 0);
				texture2D.Apply();
				flag2 = true;
				RenderTexture.ReleaseTemporary(temporary);
			}
			if (texture2D != null)
			{
				Color[] pixels = texture2D.GetPixels();
				for (int i = 0; i < this.resolutionX; i++)
				{
					for (int j = 0; j < this.resolutionY; j++)
					{
						if (flag)
						{
							this.simulationDataCpu[i][j].height = TextureUtilities.DecodeFloatRGBA(TextureUtilities.SampleColorBilinear(pixels, texture2D.width, texture2D.height, (float)i / (float)this.resolutionX, (float)j / (float)this.resolutionY));
						}
						else
						{
							this.simulationDataCpu[i][j].height = TextureUtilities.SampleColorBilinear(pixels, texture2D.width, texture2D.height, (float)i / (float)this.resolutionX, (float)j / (float)this.resolutionY).r;
						}
					}
				}
				if (flag2)
				{
					if (Application.isPlaying)
					{
						Object.Destroy(texture2D);
					}
					else
					{
						Object.DestroyImmediate(texture2D);
					}
				}
			}
		}
		if (FlowmapGenerator.ThreadCount > 1)
		{
			int num = Mathf.CeilToInt((float)this.resolutionX / (float)FlowmapGenerator.ThreadCount);
			ManualResetEvent[] array = new ManualResetEvent[FlowmapGenerator.ThreadCount];
			FlowSimulationField[] array2 = base.Generator.Fields.Where((FlowSimulationField f) => f is FluidAddField && f.enabled).ToArray<FlowSimulationField>();
			foreach (FlowSimulationField flowSimulationField in array2)
			{
				flowSimulationField.TickStart();
			}
			for (int l = 0; l < FlowmapGenerator.ThreadCount; l++)
			{
				int num2 = l * num;
				int num3 = ((l != FlowmapGenerator.ThreadCount - 1) ? num : (this.resolutionX - 1 - l * num));
				array[l] = new ManualResetEvent(false);
				ThreadPool.QueueUserWorkItem(new WaitCallback(this.BakeAddFluidThreaded), new ThreadedFieldBakeInfo(num2, num3, array[l], array2, base.Generator));
			}
			WaitHandle.WaitAll(array);
			foreach (FlowSimulationField flowSimulationField2 in array2)
			{
				flowSimulationField2.TickEnd();
			}
			FlowSimulationField[] array5 = base.Generator.Fields.Where((FlowSimulationField f) => f is FluidRemoveField && f.enabled).ToArray<FlowSimulationField>();
			foreach (FlowSimulationField flowSimulationField3 in array5)
			{
				flowSimulationField3.TickStart();
			}
			for (int num4 = 0; num4 < FlowmapGenerator.ThreadCount; num4++)
			{
				int num5 = num4 * num;
				int num6 = ((num4 != FlowmapGenerator.ThreadCount - 1) ? num : (this.resolutionX - 1 - num4 * num));
				array[num4] = new ManualResetEvent(false);
				ThreadPool.QueueUserWorkItem(new WaitCallback(this.BakeRemoveFluidThreaded), new ThreadedFieldBakeInfo(num5, num6, array[num4], array5, base.Generator));
			}
			WaitHandle.WaitAll(array);
			foreach (FlowSimulationField flowSimulationField4 in array5)
			{
				flowSimulationField4.TickEnd();
			}
			FlowSimulationField[] array8 = base.Generator.Fields.Where((FlowSimulationField f) => f is FlowForceField && f.enabled).ToArray<FlowSimulationField>();
			foreach (FlowSimulationField flowSimulationField5 in array8)
			{
				flowSimulationField5.TickStart();
			}
			for (int num9 = 0; num9 < FlowmapGenerator.ThreadCount; num9++)
			{
				int num10 = num9 * num;
				int num11 = ((num9 != FlowmapGenerator.ThreadCount - 1) ? num : (this.resolutionX - 1 - num9 * num));
				array[num9] = new ManualResetEvent(false);
				ThreadPool.QueueUserWorkItem(new WaitCallback(this.BakeForcesThreaded), new ThreadedFieldBakeInfo(num10, num11, array[num9], array8, base.Generator));
			}
			WaitHandle.WaitAll(array);
			foreach (FlowSimulationField flowSimulationField6 in array8)
			{
				flowSimulationField6.TickEnd();
			}
			FlowSimulationField[] array11 = base.Generator.Fields.Where((FlowSimulationField f) => f is HeightmapField && f.enabled).ToArray<FlowSimulationField>();
			foreach (FlowSimulationField flowSimulationField7 in array11)
			{
				flowSimulationField7.TickStart();
			}
			for (int num14 = 0; num14 < FlowmapGenerator.ThreadCount; num14++)
			{
				int num15 = num14 * num;
				int num16 = ((num14 != FlowmapGenerator.ThreadCount - 1) ? num : (this.resolutionX - 1 - num14 * num));
				array[num14] = new ManualResetEvent(false);
				ThreadPool.QueueUserWorkItem(new WaitCallback(this.BakeHeightmapThreaded), new ThreadedFieldBakeInfo(num15, num16, array[num14], array11, base.Generator));
			}
			WaitHandle.WaitAll(array);
			foreach (FlowSimulationField flowSimulationField8 in array11)
			{
				flowSimulationField8.TickEnd();
			}
		}
		else
		{
			FlowSimulationField[] array14 = base.Generator.Fields.Where((FlowSimulationField f) => f is FluidAddField && f.enabled).ToArray<FlowSimulationField>();
			foreach (FlowSimulationField flowSimulationField9 in array14)
			{
				flowSimulationField9.TickStart();
			}
			for (int num19 = 0; num19 < this.resolutionX; num19++)
			{
				for (int num20 = 0; num20 < this.resolutionY; num20++)
				{
					foreach (FlowSimulationField flowSimulationField10 in array14)
					{
						SimulationData[] array17 = this.simulationDataCpu[num19];
						int num22 = num20;
						array17[num22].addFluid = array17[num22].addFluid + flowSimulationField10.GetStrengthCpu(base.Generator, new Vector2((float)num19 / (float)this.resolutionX, (float)num20 / (float)this.resolutionY));
					}
				}
			}
			foreach (FlowSimulationField flowSimulationField11 in array14)
			{
				flowSimulationField11.TickEnd();
			}
			FlowSimulationField[] array19 = base.Generator.Fields.Where((FlowSimulationField f) => f is FluidRemoveField && f.enabled).ToArray<FlowSimulationField>();
			foreach (FlowSimulationField flowSimulationField12 in array19)
			{
				flowSimulationField12.TickStart();
			}
			for (int num25 = 0; num25 < this.resolutionX; num25++)
			{
				for (int num26 = 0; num26 < this.resolutionY; num26++)
				{
					foreach (FlowSimulationField flowSimulationField13 in array19)
					{
						SimulationData[] array22 = this.simulationDataCpu[num25];
						int num28 = num26;
						array22[num28].removeFluid = array22[num28].removeFluid + flowSimulationField13.GetStrengthCpu(base.Generator, new Vector2((float)num25 / (float)this.resolutionX, (float)num26 / (float)this.resolutionY));
					}
				}
			}
			foreach (FlowSimulationField flowSimulationField14 in array19)
			{
				flowSimulationField14.TickEnd();
			}
			FlowSimulationField[] array24 = base.Generator.Fields.Where((FlowSimulationField f) => f is FlowForceField && f.enabled).ToArray<FlowSimulationField>();
			foreach (FlowSimulationField flowSimulationField15 in array24)
			{
				flowSimulationField15.TickStart();
			}
			for (int num31 = 0; num31 < this.resolutionX; num31++)
			{
				for (int num32 = 0; num32 < this.resolutionY; num32++)
				{
					foreach (FlowSimulationField flowSimulationField16 in array24)
					{
						SimulationData[] array27 = this.simulationDataCpu[num31];
						int num34 = num32;
						array27[num34].force = array27[num34].force + (flowSimulationField16 as FlowForceField).GetForceCpu(base.Generator, new Vector2((float)num31 / (float)this.resolutionX, (float)num32 / (float)this.resolutionY));
						this.simulationDataCpu[num31][num32].force.z = Mathf.Max(this.simulationDataCpu[num31][num32].force.z, 0f);
					}
				}
			}
			foreach (FlowSimulationField flowSimulationField17 in array24)
			{
				flowSimulationField17.TickEnd();
			}
			FlowSimulationField[] array29 = base.Generator.Fields.Where((FlowSimulationField f) => f is HeightmapField && f.enabled).ToArray<FlowSimulationField>();
			foreach (FlowSimulationField flowSimulationField18 in array29)
			{
				flowSimulationField18.TickStart();
			}
			for (int num37 = 0; num37 < this.resolutionX; num37++)
			{
				for (int num38 = 0; num38 < this.resolutionY; num38++)
				{
					foreach (FlowSimulationField flowSimulationField19 in array29)
					{
						float strengthCpu = flowSimulationField19.GetStrengthCpu(base.Generator, new Vector2((float)num37 / (float)this.resolutionX, (float)num38 / (float)this.resolutionY));
						this.simulationDataCpu[num37][num38].height = Mathf.Lerp(this.simulationDataCpu[num37][num38].height, strengthCpu, strengthCpu * (1f - this.simulationDataCpu[num37][num38].height));
					}
				}
			}
			foreach (FlowSimulationField flowSimulationField20 in array29)
			{
				flowSimulationField20.TickEnd();
			}
		}
		this.WriteCpuDataToTexture();
	}

	private void AddRemoveFluidCpu(int x, int y)
	{
		SimulationData[] array = this.simulationDataCpu[x];
		array[y].fluid = array[y].fluid + this.simulationDataCpu[x][y].addFluid * this.timestep * this.fluidAddMultiplier;
		this.simulationDataCpu[x][y].fluid = Mathf.Max(0f, this.simulationDataCpu[x][y].fluid - this.simulationDataCpu[x][y].removeFluid * this.fluidRemoveMultiplier);
		this.simulationDataCpu[x][y].fluid = this.simulationDataCpu[x][y].fluid * (1f - this.evaporationRate * this.timestep);
	}

	private void OutflowCpu(int x, int y)
	{
		int num = Mathf.Min(y + 1, this.resolutionY - 1);
		int num2 = Mathf.Min(x + 1, this.resolutionX - 1);
		int num3 = Mathf.Max(y - 1, 0);
		int num4 = Mathf.Max(x - 1, 0);
		Vector2 vector;
		vector..ctor(this.simulationDataCpu[x][y].force.x, this.simulationDataCpu[x][y].force.y);
		float num5 = Mathf.Max(0f, this.simulationDataCpu[x][y].outflow.x + this.timestep * this.gravity * (this.simulationDataCpu[x][y].height + this.simulationDataCpu[x][y].fluid - this.simulationDataCpu[x][num].height - this.simulationDataCpu[x][num].fluid) + Mathf.Clamp01(Vector2.Dot(vector, new Vector2(0f, 1f))) * this.timestep * this.fluidForceMultiplier);
		float num6 = Mathf.Max(0f, this.simulationDataCpu[x][y].outflow.y + this.timestep * this.gravity * (this.simulationDataCpu[x][y].height + this.simulationDataCpu[x][y].fluid - this.simulationDataCpu[num2][y].height - this.simulationDataCpu[num2][y].fluid) + Mathf.Clamp01(Vector2.Dot(vector, new Vector2(1f, 0f))) * this.timestep * this.fluidForceMultiplier);
		float num7 = Mathf.Max(0f, this.simulationDataCpu[x][y].outflow.z + this.timestep * this.gravity * (this.simulationDataCpu[x][y].height + this.simulationDataCpu[x][y].fluid - this.simulationDataCpu[x][num3].height - this.simulationDataCpu[x][num3].fluid) + Mathf.Clamp01(Vector2.Dot(vector, new Vector2(0f, -1f))) * this.timestep * this.fluidForceMultiplier);
		float num8 = Mathf.Max(0f, this.simulationDataCpu[x][y].outflow.w + this.timestep * this.gravity * (this.simulationDataCpu[x][y].height + this.simulationDataCpu[x][y].fluid - this.simulationDataCpu[num4][y].height - this.simulationDataCpu[num4][y].fluid) + Mathf.Clamp01(Vector2.Dot(vector, new Vector2(-1f, 0f))) * this.timestep * this.fluidForceMultiplier);
		if (this.borderCollision == SimulationBorderCollision.PassThrough)
		{
			if (x == 0)
			{
				num6 = 0f;
			}
			if (x == this.resolutionX - 1)
			{
				num8 = 0f;
			}
			if (y == 0)
			{
				num5 = 0f;
			}
			if (y == this.resolutionY - 1)
			{
				num7 = 0f;
			}
		}
		float num9 = ((num5 + num6 + num7 + num8 <= 0f) ? 0f : Mathf.Min(1f, this.simulationDataCpu[x][y].fluid / (this.timestep * (num5 + num6 + num7 + num8))));
		num9 *= 1f - this.simulationDataCpu[x][y].force.z;
		this.simulationDataCpu[x][y].outflow = new Vector4(num5 * num9, num6 * num9, num7 * num9, num8 * num9);
	}

	private void UpdateVelocityCpu(int x, int y)
	{
		int num = Mathf.Min(y + 1, this.resolutionY - 1);
		int num2 = Mathf.Min(x + 1, this.resolutionX - 1);
		int num3 = Mathf.Max(y - 1, 0);
		int num4 = Mathf.Max(x - 1, 0);
		float z = this.simulationDataCpu[x][num].outflow.z;
		float w = this.simulationDataCpu[num2][y].outflow.w;
		float x2 = this.simulationDataCpu[x][num3].outflow.x;
		float y2 = this.simulationDataCpu[num4][y].outflow.y;
		float num5 = this.timestep * (z + w + x2 + y2 - (this.simulationDataCpu[x][y].outflow.x + this.simulationDataCpu[x][y].outflow.y + this.simulationDataCpu[x][y].outflow.z + this.simulationDataCpu[x][y].outflow.w));
		this.simulationDataCpu[x][y].fluid = this.simulationDataCpu[x][y].fluid + num5;
		float num6 = 0.5f * (y2 - w + (this.simulationDataCpu[x][y].outflow.y - this.simulationDataCpu[x][y].outflow.w));
		float num7 = 0.5f * (x2 - z + (this.simulationDataCpu[x][y].outflow.x - this.simulationDataCpu[x][y].outflow.z));
		float num8 = 0.5f * (this.simulationDataCpu[x][y].fluid + (this.simulationDataCpu[x][y].fluid + num5));
		Vector2 zero = Vector2.zero;
		if (num8 != 0f)
		{
			zero.x = num6 / num8;
			zero.y = num7 / num8;
		}
		zero.x = Mathf.Clamp(zero.x * this.velocityScale, -1f, 1f) * 0.5f + 0.5f;
		zero.y = Mathf.Clamp(zero.y * this.velocityScale, -1f, 1f) * 0.5f + 0.5f;
		this.simulationDataCpu[x][y].velocity = zero;
		float z2 = this.simulationDataCpu[x][y].velocityAccumulated.z;
		this.simulationDataCpu[x][y].velocityAccumulated = Vector3.Lerp(this.simulationDataCpu[x][y].velocityAccumulated, this.simulationDataCpu[x][y].velocity, this.outputAccumulationRate);
		this.simulationDataCpu[x][y].velocityAccumulated.z = z2;
	}

	private void BlurVelocityAccumulatedHorizontalCpu(int x, int y)
	{
		Vector3 velocityAccumulated = this.simulationDataCpu[Mathf.Max(0, x - 1)][y].velocityAccumulated;
		Vector3 velocityAccumulated2 = this.simulationDataCpu[Mathf.Min(this.resolutionX - 1, x + 1)][y].velocityAccumulated;
		this.simulationDataCpu[x][y].velocityAccumulated = Vector3.Lerp(this.simulationDataCpu[x][y].velocityAccumulated, velocityAccumulated * 0.25f + this.simulationDataCpu[x][y].velocityAccumulated * 0.5f + velocityAccumulated2 * 0.25f, this.outputFilterStrength);
	}

	private void BlurVelocityAccumulatedVerticalCpu(int x, int y)
	{
		Vector3 velocityAccumulated = this.simulationDataCpu[x][Mathf.Max(0, y - 1)].velocityAccumulated;
		Vector3 velocityAccumulated2 = this.simulationDataCpu[x][Mathf.Min(this.resolutionY - 1, y + 1)].velocityAccumulated;
		this.simulationDataCpu[x][y].velocityAccumulated = Vector3.Lerp(this.simulationDataCpu[x][y].velocityAccumulated, velocityAccumulated * 0.25f + this.simulationDataCpu[x][y].velocityAccumulated * 0.5f + velocityAccumulated2 * 0.25f, this.outputFilterStrength);
	}

	private void FoamCpu(int x, int y)
	{
		int num = Mathf.Min(y + 1, this.resolutionY - 1);
		int num2 = Mathf.Min(x + 1, this.resolutionX - 1);
		int num3 = Mathf.Max(y - 1, 0);
		int num4 = Mathf.Max(x - 1, 0);
		Vector2 vector;
		vector..ctor(this.simulationDataCpu[x][y].velocityAccumulated.x * 2f - 1f, this.simulationDataCpu[x][y].velocityAccumulated.y * 2f - 1f);
		float magnitude = vector.magnitude;
		Vector2 vector2;
		vector2..ctor(this.simulationDataCpu[x][num].velocityAccumulated.x * 2f - 1f, this.simulationDataCpu[x][num].velocityAccumulated.y * 2f - 1f);
		float magnitude2 = vector2.magnitude;
		Vector2 vector3;
		vector3..ctor(this.simulationDataCpu[num2][y].velocityAccumulated.x * 2f - 1f, this.simulationDataCpu[num2][y].velocityAccumulated.y * 2f - 1f);
		float magnitude3 = vector3.magnitude;
		Vector2 vector4;
		vector4..ctor(this.simulationDataCpu[x][num3].velocityAccumulated.x * 2f - 1f, this.simulationDataCpu[x][num3].velocityAccumulated.y * 2f - 1f);
		float magnitude4 = vector4.magnitude;
		Vector2 vector5;
		vector5..ctor(this.simulationDataCpu[num4][y].velocityAccumulated.x * 2f - 1f, this.simulationDataCpu[num4][y].velocityAccumulated.y * 2f - 1f);
		float magnitude5 = vector5.magnitude;
		float num5 = 100f * (magnitude2 - magnitude + (magnitude3 - magnitude) + (magnitude4 - magnitude) + (magnitude5 - magnitude));
		float num6 = 1f;
		Vector2 vector6;
		vector6..ctor(this.simulationDataCpu[x][y].velocity.x * 2f - 1f, this.simulationDataCpu[x][y].velocity.y * 2f - 1f);
		float num7 = Mathf.Pow(num6 - Mathf.Clamp01(vector6.magnitude * this.foamVelocityScale), 2f);
		num7 *= 1f - this.simulationDataCpu[x][y].addFluid;
		num7 = (Mathf.Clamp01((num7 * 1.2f - 0.5f) * 4f) + 0.5f) * Mathf.Clamp01(num5);
		this.simulationDataCpu[x][y].velocityAccumulated.z = Mathf.Lerp(this.simulationDataCpu[x][y].velocityAccumulated.z, num7, this.outputAccumulationRate);
	}

	private void BakeAddFluidThreaded(object threadContext)
	{
		ThreadedFieldBakeInfo threadedFieldBakeInfo = threadContext as ThreadedFieldBakeInfo;
		try
		{
			for (int i = threadedFieldBakeInfo.start; i < threadedFieldBakeInfo.start + threadedFieldBakeInfo.length; i++)
			{
				for (int j = 0; j < this.resolutionY; j++)
				{
					foreach (FlowSimulationField flowSimulationField in threadedFieldBakeInfo.fields)
					{
						SimulationData[] array = this.simulationDataCpu[i];
						int num = j;
						array[num].addFluid = array[num].addFluid + flowSimulationField.GetStrengthCpu(threadedFieldBakeInfo.generator, new Vector2((float)i / (float)this.resolutionX, (float)j / (float)this.resolutionY));
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.ToString());
		}
		threadedFieldBakeInfo.resetEvent.Set();
	}

	private void BakeRemoveFluidThreaded(object threadContext)
	{
		ThreadedFieldBakeInfo threadedFieldBakeInfo = threadContext as ThreadedFieldBakeInfo;
		try
		{
			for (int i = threadedFieldBakeInfo.start; i < threadedFieldBakeInfo.start + threadedFieldBakeInfo.length; i++)
			{
				for (int j = 0; j < this.resolutionY; j++)
				{
					foreach (FlowSimulationField flowSimulationField in threadedFieldBakeInfo.fields)
					{
						SimulationData[] array = this.simulationDataCpu[i];
						int num = j;
						array[num].removeFluid = array[num].removeFluid + flowSimulationField.GetStrengthCpu(threadedFieldBakeInfo.generator, new Vector2((float)i / (float)this.resolutionX, (float)j / (float)this.resolutionY));
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.ToString());
		}
		threadedFieldBakeInfo.resetEvent.Set();
	}

	private void BakeForcesThreaded(object threadContext)
	{
		ThreadedFieldBakeInfo threadedFieldBakeInfo = threadContext as ThreadedFieldBakeInfo;
		try
		{
			for (int i = threadedFieldBakeInfo.start; i < threadedFieldBakeInfo.start + threadedFieldBakeInfo.length; i++)
			{
				for (int j = 0; j < this.resolutionY; j++)
				{
					foreach (FlowSimulationField flowSimulationField in threadedFieldBakeInfo.fields)
					{
						SimulationData[] array = this.simulationDataCpu[i];
						int num = j;
						array[num].force = array[num].force + (flowSimulationField as FlowForceField).GetForceCpu(threadedFieldBakeInfo.generator, new Vector2((float)i / (float)this.resolutionX, (float)j / (float)this.resolutionY));
						this.simulationDataCpu[i][j].force.z = Mathf.Max(this.simulationDataCpu[i][j].force.z, 0f);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.ToString());
		}
		threadedFieldBakeInfo.resetEvent.Set();
	}

	private void BakeHeightmapThreaded(object threadContext)
	{
		ThreadedFieldBakeInfo threadedFieldBakeInfo = threadContext as ThreadedFieldBakeInfo;
		try
		{
			for (int i = threadedFieldBakeInfo.start; i < threadedFieldBakeInfo.start + threadedFieldBakeInfo.length; i++)
			{
				for (int j = 0; j < this.resolutionY; j++)
				{
					foreach (FlowSimulationField flowSimulationField in threadedFieldBakeInfo.fields)
					{
						float strengthCpu = flowSimulationField.GetStrengthCpu(threadedFieldBakeInfo.generator, new Vector2((float)i / (float)this.resolutionX, (float)j / (float)this.resolutionY));
						this.simulationDataCpu[i][j].height = Mathf.Lerp(this.simulationDataCpu[i][j].height, strengthCpu, strengthCpu * (1f - this.simulationDataCpu[i][j].height));
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.ToString());
		}
		threadedFieldBakeInfo.resetEvent.Set();
	}

	private void AddRemoveFluidThreaded(object threadContext)
	{
		ArrayThreadedInfo arrayThreadedInfo = threadContext as ArrayThreadedInfo;
		try
		{
			for (int i = arrayThreadedInfo.start; i < arrayThreadedInfo.start + arrayThreadedInfo.length; i++)
			{
				for (int j = 0; j < this.resolutionY; j++)
				{
					this.AddRemoveFluidCpu(i, j);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.ToString());
		}
		arrayThreadedInfo.resetEvent.Set();
	}

	private void OutflowThreaded(object threadContext)
	{
		ArrayThreadedInfo arrayThreadedInfo = threadContext as ArrayThreadedInfo;
		try
		{
			for (int i = arrayThreadedInfo.start; i < arrayThreadedInfo.start + arrayThreadedInfo.length; i++)
			{
				for (int j = 0; j < this.resolutionY; j++)
				{
					this.OutflowCpu(i, j);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.ToString());
		}
		arrayThreadedInfo.resetEvent.Set();
	}

	private void UpdateVelocityThreaded(object threadContext)
	{
		ArrayThreadedInfo arrayThreadedInfo = threadContext as ArrayThreadedInfo;
		try
		{
			for (int i = arrayThreadedInfo.start; i < arrayThreadedInfo.start + arrayThreadedInfo.length; i++)
			{
				for (int j = 0; j < this.resolutionY; j++)
				{
					this.UpdateVelocityCpu(i, j);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.ToString());
		}
		arrayThreadedInfo.resetEvent.Set();
	}

	private void BlurVelocityAccumulatedHorizontalThreaded(object threadContext)
	{
		ArrayThreadedInfo arrayThreadedInfo = threadContext as ArrayThreadedInfo;
		try
		{
			for (int i = arrayThreadedInfo.start; i < arrayThreadedInfo.start + arrayThreadedInfo.length; i++)
			{
				for (int j = 0; j < this.resolutionY; j++)
				{
					this.BlurVelocityAccumulatedHorizontalCpu(i, j);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.ToString());
		}
		arrayThreadedInfo.resetEvent.Set();
	}

	private void BlurVelocityAccumulatedVerticalThreaded(object threadContext)
	{
		ArrayThreadedInfo arrayThreadedInfo = threadContext as ArrayThreadedInfo;
		try
		{
			for (int i = arrayThreadedInfo.start; i < arrayThreadedInfo.start + arrayThreadedInfo.length; i++)
			{
				for (int j = 0; j < this.resolutionY; j++)
				{
					this.BlurVelocityAccumulatedVerticalCpu(i, j);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.ToString());
		}
		arrayThreadedInfo.resetEvent.Set();
	}

	private void FoamThreaded(object threadContext)
	{
		ArrayThreadedInfo arrayThreadedInfo = threadContext as ArrayThreadedInfo;
		try
		{
			for (int i = arrayThreadedInfo.start; i < arrayThreadedInfo.start + arrayThreadedInfo.length; i++)
			{
				for (int j = 0; j < this.resolutionY; j++)
				{
					this.FoamCpu(i, j);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.ToString());
		}
		arrayThreadedInfo.resetEvent.Set();
	}

	protected override void Update()
	{
		base.Update();
		this.AssignToMaterials();
	}

	private void WriteCpuDataToTexture()
	{
		if (this.heightFluidCpu == null || this.heightFluidCpu.width != this.resolutionX || this.heightFluidCpu.height != this.resolutionY)
		{
			if (this.heightFluidCpu)
			{
				this.DestroyProperly(this.heightFluidCpu);
			}
			this.heightFluidCpu = new Texture2D(this.resolutionX, this.resolutionY, 5, true, true);
			this.heightFluidCpu.hideFlags = 61;
			this.heightFluidCpu.name = "HeightFluidCpu";
		}
		Color[] array = new Color[this.resolutionX * this.resolutionY];
		for (int i = 0; i < this.resolutionY; i++)
		{
			for (int j = 0; j < this.resolutionX; j++)
			{
				array[j + i * this.resolutionX] = new Color(this.simulationDataCpu[j][i].height, this.simulationDataCpu[j][i].fluid, 0f, 1f);
			}
		}
		this.heightFluidCpu.SetPixels(array);
		this.heightFluidCpu.Apply();
		if (this.velocityAccumulatedCpu == null || this.velocityAccumulatedCpu.width != this.resolutionX || this.velocityAccumulatedCpu.height != this.resolutionY)
		{
			if (this.velocityAccumulatedCpu)
			{
				this.DestroyProperly(this.velocityAccumulatedCpu);
			}
			this.velocityAccumulatedCpu = new Texture2D(this.resolutionX, this.resolutionY, 5, true, true);
			this.velocityAccumulatedCpu.hideFlags = 61;
			this.velocityAccumulatedCpu.name = "VelocityAccumulatedCpu";
		}
		for (int k = 0; k < this.resolutionY; k++)
		{
			for (int l = 0; l < this.resolutionX; l++)
			{
				array[l + k * this.resolutionX] = new Color(this.simulationDataCpu[l][k].velocityAccumulated.x, this.simulationDataCpu[l][k].velocityAccumulated.y, this.simulationDataCpu[l][k].velocityAccumulated.z, 1f);
			}
		}
		this.velocityAccumulatedCpu.SetPixels(array);
		this.velocityAccumulatedCpu.Apply();
	}

	private void AssignToMaterials()
	{
		if (this.assignFlowmapToMaterials != null)
		{
			foreach (Material material in this.assignFlowmapToMaterials)
			{
				if (!(material == null))
				{
					if (this.assignFlowmap)
					{
						material.SetTexture(this.assignedFlowmapName, (FlowmapGenerator.SimulationPath != SimulationPath.GPU) ? this.velocityAccumulatedCpu : this.velocityAccumulatedRT);
					}
					if (this.assignHeightAndFluid)
					{
						material.SetTexture(this.assignedHeightAndFluidName, (FlowmapGenerator.SimulationPath != SimulationPath.GPU) ? this.heightFluidCpu : this.heightFluidRT);
					}
					if (this.assignUVScaleTransform)
					{
						if (base.Generator.Dimensions.x < base.Generator.Dimensions.y)
						{
							float num = base.Generator.Dimensions.y / base.Generator.Dimensions.x;
							material.SetVector(this.assignUVCoordsName, new Vector4(base.Generator.Dimensions.x * num, base.Generator.Dimensions.y, base.Generator.Position.x, base.Generator.Position.z));
						}
						else
						{
							float num2 = base.Generator.Dimensions.x / base.Generator.Dimensions.y;
							material.SetVector(this.assignUVCoordsName, new Vector4(base.Generator.Dimensions.x, base.Generator.Dimensions.y * num2, base.Generator.Position.x, base.Generator.Position.z));
						}
					}
				}
			}
		}
	}

	public void WriteTextureToDisk(ShallowWaterSimulator.OutputTexture textureToWrite, string path)
	{
		SimulationPath simulationPath = FlowmapGenerator.SimulationPath;
		if (simulationPath != SimulationPath.GPU)
		{
			if (simulationPath == SimulationPath.CPU)
			{
				if (textureToWrite != ShallowWaterSimulator.OutputTexture.HeightAndFluid)
				{
					if (textureToWrite != ShallowWaterSimulator.OutputTexture.Flowmap)
					{
						if (textureToWrite == ShallowWaterSimulator.OutputTexture.Foam)
						{
							Texture2D foamTextureCPU = this.GetFoamTextureCPU();
							TextureUtilities.WriteTexture2DToFile(foamTextureCPU, path, TextureUtilities.SupportedFormats[this.generator.outputFileFormat]);
							if (Application.isPlaying)
							{
								Object.Destroy(foamTextureCPU);
							}
							else
							{
								Object.DestroyImmediate(foamTextureCPU);
							}
						}
					}
					else if (this.writeFoamSeparately)
					{
						Texture2D flowmapWithoutFoamTextureCPU = this.GetFlowmapWithoutFoamTextureCPU();
						TextureUtilities.WriteTexture2DToFile(flowmapWithoutFoamTextureCPU, path, TextureUtilities.SupportedFormats[this.generator.outputFileFormat]);
						if (Application.isPlaying)
						{
							Object.Destroy(flowmapWithoutFoamTextureCPU);
						}
						else
						{
							Object.DestroyImmediate(flowmapWithoutFoamTextureCPU);
						}
					}
					else
					{
						TextureUtilities.WriteTexture2DToFile(this.velocityAccumulatedCpu, path, TextureUtilities.SupportedFormats[this.generator.outputFileFormat]);
					}
				}
				else
				{
					TextureUtilities.WriteTexture2DToFile(this.heightFluidCpu, path, TextureUtilities.SupportedFormats[this.generator.outputFileFormat]);
				}
			}
		}
		else if (textureToWrite != ShallowWaterSimulator.OutputTexture.HeightAndFluid)
		{
			if (textureToWrite != ShallowWaterSimulator.OutputTexture.Flowmap)
			{
				if (textureToWrite == ShallowWaterSimulator.OutputTexture.Foam)
				{
					RenderTexture foamRT = this.GetFoamRT();
					TextureUtilities.WriteRenderTextureToFile(foamRT, path, true, TextureUtilities.SupportedFormats[this.generator.outputFileFormat]);
					if (Application.isPlaying)
					{
						Object.Destroy(foamRT);
					}
					else
					{
						Object.DestroyImmediate(foamRT);
					}
				}
			}
			else if (this.writeFoamSeparately)
			{
				RenderTexture flowmapWithoutFoamRT = this.GetFlowmapWithoutFoamRT();
				TextureUtilities.WriteRenderTextureToFile(flowmapWithoutFoamRT, path, true, TextureUtilities.SupportedFormats[this.generator.outputFileFormat]);
				if (Application.isPlaying)
				{
					Object.Destroy(flowmapWithoutFoamRT);
				}
			}
			else
			{
				TextureUtilities.WriteRenderTextureToFile(this.velocityAccumulatedRT, path, true, TextureUtilities.SupportedFormats[this.generator.outputFileFormat]);
			}
		}
		else
		{
			TextureUtilities.WriteRenderTextureToFile(this.heightFluidRT, path, true, TextureUtilities.SupportedFormats[this.generator.outputFileFormat], "Hidden/WriteHeightFluid");
		}
	}

	private Texture2D GetFoamTextureCPU()
	{
		Texture2D texture2D = new Texture2D(this.resolutionX, this.resolutionY, 5, true);
		texture2D.hideFlags = 61;
		Color[] array = new Color[this.resolutionX * this.resolutionY];
		for (int i = 0; i < this.resolutionY; i++)
		{
			for (int j = 0; j < this.resolutionX; j++)
			{
				array[j + i * this.resolutionX] = new Color(this.simulationDataCpu[j][i].velocityAccumulated.z, this.simulationDataCpu[j][i].velocityAccumulated.z, this.simulationDataCpu[j][i].velocityAccumulated.z, 1f);
			}
		}
		texture2D.SetPixels(array);
		texture2D.Apply();
		return texture2D;
	}

	private Texture2D GetFlowmapWithoutFoamTextureCPU()
	{
		Texture2D texture2D = new Texture2D(this.resolutionX, this.resolutionY, 5, true);
		texture2D.hideFlags = 61;
		Color[] array = new Color[this.resolutionX * this.resolutionY];
		for (int i = 0; i < this.resolutionY; i++)
		{
			for (int j = 0; j < this.resolutionX; j++)
			{
				array[j + i * this.resolutionX] = new Color(this.simulationDataCpu[j][i].velocityAccumulated.x, this.simulationDataCpu[j][i].velocityAccumulated.y, 0f, 1f);
			}
		}
		texture2D.SetPixels(array);
		texture2D.Apply();
		return texture2D;
	}

	private RenderTexture GetFoamRT()
	{
		RenderTexture renderTexture = new RenderTexture(this.resolutionX, this.resolutionY, 0, 0, 1);
		Graphics.Blit(this.velocityAccumulatedRT, renderTexture, this.SimulationMaterial, 12);
		return renderTexture;
	}

	private RenderTexture GetFlowmapWithoutFoamRT()
	{
		RenderTexture renderTexture = new RenderTexture(this.resolutionX, this.resolutionY, 0, 0, 1);
		Graphics.Blit(this.velocityAccumulatedRT, renderTexture, this.SimulationMaterial, 13);
		return renderTexture;
	}

	protected override void MaxStepsReached()
	{
		base.MaxStepsReached();
		if (this.writeToFileOnMaxSimulationSteps && !string.IsNullOrEmpty(this.outputFolderPath) && Directory.Exists(this.outputFolderPath))
		{
			this.WriteAllTextures();
		}
		if (this.OnMaxStepsReached != null)
		{
			this.OnMaxStepsReached();
		}
	}

	public void WriteAllTextures()
	{
		SimulationPath simulationPath = FlowmapGenerator.SimulationPath;
		if (simulationPath != SimulationPath.GPU)
		{
			if (simulationPath == SimulationPath.CPU)
			{
				if (this.simulationDataCpu == null)
				{
					this.Init();
				}
				this.WriteCpuDataToTexture();
				if (this.writeHeightAndFluid)
				{
					TextureUtilities.WriteTexture2DToFile(this.heightFluidCpu, this.outputFolderPath + "/" + this.outputPrefix + "HeightAndFluid", TextureUtilities.SupportedFormats[this.generator.outputFileFormat]);
				}
				if (this.writeFoamSeparately)
				{
					Texture2D foamTextureCPU = this.GetFoamTextureCPU();
					TextureUtilities.WriteTexture2DToFile(foamTextureCPU, this.outputFolderPath + "/" + this.outputPrefix + "Foam", TextureUtilities.SupportedFormats[this.generator.outputFileFormat]);
					if (Application.isPlaying)
					{
						Object.Destroy(foamTextureCPU);
					}
					else
					{
						Object.DestroyImmediate(foamTextureCPU);
					}
					Texture2D flowmapWithoutFoamTextureCPU = this.GetFlowmapWithoutFoamTextureCPU();
					TextureUtilities.WriteTexture2DToFile(flowmapWithoutFoamTextureCPU, this.outputFolderPath + "/" + this.outputPrefix + "Flowmap", TextureUtilities.SupportedFormats[this.generator.outputFileFormat]);
					if (Application.isPlaying)
					{
						Object.Destroy(flowmapWithoutFoamTextureCPU);
					}
					else
					{
						Object.DestroyImmediate(flowmapWithoutFoamTextureCPU);
					}
				}
				else
				{
					TextureUtilities.WriteTexture2DToFile(this.velocityAccumulatedCpu, this.outputFolderPath + "/" + this.outputPrefix + "Flowmap", TextureUtilities.SupportedFormats[this.generator.outputFileFormat]);
				}
			}
		}
		else
		{
			if (this.writeHeightAndFluid)
			{
				TextureUtilities.WriteRenderTextureToFile(this.heightFluidRT, this.outputFolderPath + "/" + this.outputPrefix + "HeightAndFluid", true, TextureUtilities.SupportedFormats[this.generator.outputFileFormat], "Hidden/WriteHeightFluid");
			}
			if (this.writeFoamSeparately)
			{
				RenderTexture foamRT = this.GetFoamRT();
				TextureUtilities.WriteRenderTextureToFile(foamRT, this.outputFolderPath + "/" + this.outputPrefix + "Foam", TextureUtilities.SupportedFormats[this.generator.outputFileFormat]);
				if (Application.isPlaying)
				{
					Object.Destroy(foamRT);
				}
				else
				{
					Object.DestroyImmediate(foamRT);
				}
				RenderTexture flowmapWithoutFoamRT = this.GetFlowmapWithoutFoamRT();
				TextureUtilities.WriteRenderTextureToFile(flowmapWithoutFoamRT, this.outputFolderPath + "/" + this.outputPrefix + "Flowmap", TextureUtilities.SupportedFormats[this.generator.outputFileFormat]);
				if (Application.isPlaying)
				{
					Object.Destroy(flowmapWithoutFoamRT);
				}
				else
				{
					Object.DestroyImmediate(flowmapWithoutFoamRT);
				}
			}
			else
			{
				TextureUtilities.WriteRenderTextureToFile(this.velocityAccumulatedRT, this.outputFolderPath + "/" + this.outputPrefix + "Flowmap", TextureUtilities.SupportedFormats[this.generator.outputFileFormat]);
			}
		}
	}

	public int updateTextureDelayCPU = 10;

	public float timestep = 0.4f;

	public float evaporationRate = 0.001f;

	public float gravity = 1f;

	public float velocityScale = 1f;

	public float fluidAddMultiplier = 0.01f;

	public float fluidRemoveMultiplier = 0.01f;

	public float fluidForceMultiplier = 0.01f;

	public float initialFluidAmount;

	public FluidDepth fluidDepth;

	public float outputAccumulationRate = 0.02f;

	private int outputFilterSize = 1;

	public float outputFilterStrength = 1f;

	public bool simulateFoam;

	public float foamVelocityScale = 1f;

	public bool simulateFirstFluidHit;

	public float firstFluidHitTimeMax = 30f;

	public Material[] assignFlowmapToMaterials;

	public bool assignFlowmap;

	public string assignedFlowmapName = "_FlowmapTex";

	public bool assignHeightAndFluid;

	public string assignedHeightAndFluidName = "_HeightFluidTex";

	public bool assignUVScaleTransform;

	public string assignUVCoordsName = "_FlowmapUV";

	public bool writeHeightAndFluid;

	public bool writeFoamSeparately;

	public bool writeFluidDepthInAlpha;

	private RenderTexture heightFluidRT;

	private RenderTexture heightPreviewRT;

	private RenderTexture fluidPreviewRT;

	private RenderTexture fluidAddRT;

	private RenderTexture fluidRemoveRT;

	private RenderTexture fluidForceRT;

	private RenderTexture heightmapFieldsRT;

	private RenderTexture outflowRT;

	private RenderTexture bufferRT1;

	private RenderTexture velocityRT;

	private RenderTexture velocityAccumulatedRT;

	private Material simulationMaterial;

	private Camera fieldRenderCamera;

	private bool initializedGpu;

	[HideInInspector]
	[SerializeField]
	private SimulationData[][] simulationDataCpu;

	private Texture2D heightFluidCpu;

	private Texture2D velocityAccumulatedCpu;

	private bool initializedCpu;

	public enum OutputTexture
	{
		Flowmap,
		HeightAndFluid,
		Foam
	}
}
