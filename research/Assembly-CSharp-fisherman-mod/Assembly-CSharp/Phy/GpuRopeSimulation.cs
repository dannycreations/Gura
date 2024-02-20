using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Phy
{
	public class GpuRopeSimulation : ISimulation
	{
		public GpuRopeSimulation(Vector3 connectionPoint, float segmentLength, float length, float segmentMass, Material inputMaterial, Material updateMaterial, Material transformMaterial, Texture groundHight)
		{
			this.Masses = new List<Mass>();
			this.Connections = new List<ConnectionBase>();
			this.inputMaterial = inputMaterial;
			this.updateMaterial = updateMaterial;
			this.transformMaterial = transformMaterial;
			this.groundHight = groundHight;
			int num = (int)(length / segmentLength) + 1;
			Mass mass = null;
			for (int i = 0; i < num; i++)
			{
				Mass mass2 = new Mass(GameFactory.Player.RodSlot.Sim, segmentMass, connectionPoint + new Vector3(0f, 0f, segmentLength * (float)i), Mass.MassType.Unknown);
				this.Masses.Add(mass2);
				if (i > 0)
				{
					this.Connections.Add(new Spring(mass, mass2, 5000f, segmentLength, 0.2f));
				}
				mass = mass2;
			}
			this.RopeConnectionPosition = connectionPoint;
			this.RopeConnectionVelocity = Vector3.zero;
			transformMaterial.SetFloat("WaterHeight", 0f);
			transformMaterial.SetFloat("WaterDragConstant", 0.2f);
			transformMaterial.SetFloat("AirDragConstant", 0.02f);
			transformMaterial.SetFloat("GroundRepulsionConstant", 100f);
			transformMaterial.SetFloat("GroundDragConstant", 0.2f);
			transformMaterial.SetFloat("GroundAbsorptionConstant", 2f);
			transformMaterial.SetTexture("GroundHight", groundHight);
		}

		public List<Mass> Masses { get; private set; }

		public List<ConnectionBase> Connections { get; private set; }

		public Vector3 RopeConnectionPosition { get; set; }

		public Vector3 RopeConnectionVelocity { get; set; }

		public void WriteResultsFromTexture(string filename, RenderTexture tex)
		{
			float[] array = new float[65536];
			EncodeFloat.ReadFromRenderTexture(tex, 4, array);
			byte[] array2 = new byte[array.Length * 4];
			Buffer.BlockCopy(array, 0, array2, 0, array2.Length);
			File.WriteAllBytes(filename, array2);
			using (StreamWriter streamWriter = new StreamWriter(filename + ".txt"))
			{
				for (int i = 0; i < array.Length; i++)
				{
					streamWriter.WriteLine(array[i].ToString());
				}
			}
		}

		public void DumpMasses(string filename)
		{
			using (StreamWriter streamWriter = new StreamWriter(filename + ".txt"))
			{
				for (int i = 0; i < this.Masses.Count; i++)
				{
					streamWriter.WriteLine(string.Concat(new string[]
					{
						"Masses[",
						i.ToString(),
						"] mass ",
						this.Masses[i].MassValue.ToString(),
						" position ",
						this.Masses[i].Position.ToString()
					}));
				}
			}
		}

		public float UnPackFloat(float v)
		{
			return v * 10000f * 2f - 10000f;
		}

		public float FixFloat(float v)
		{
			float num = 0f;
			float num2 = (float)Math.Round((double)(v * 10000f * 2f));
			if (num2 >= 10000f)
			{
				num = 1f;
			}
			float num3 = 1f - 2f * num;
			return (num2 - 10000f * num) * num3;
		}

		public float UnPackFloat(float v, float v1)
		{
			float num = 0f;
			float num2 = (float)Math.Round((double)(v * 10000f * 2f));
			if (num2 >= 10000f)
			{
				num = 1f;
			}
			float num3 = 1f - 2f * num;
			return (num2 + v1 - 10000f * num) * num3;
		}

		public float FixFloat2(float v, float v1)
		{
			return this.UnPackFloat(v, v1);
		}

		public float FixFloat3(float v)
		{
			return v * 100f * 2f - 100f;
		}

		public void DumpData(float[] data, string filename)
		{
			using (StreamWriter streamWriter = new StreamWriter(filename + ".txt"))
			{
				for (int i = 0; i < this.Masses.Count; i++)
				{
					streamWriter.WriteLine(i.ToString() + "-->");
					for (int j = 0; j < 7; j++)
					{
						float num = this.FixFloat2(data[j + i * 16], data[j + 7 + i * 16]);
						float num2 = this.FixFloat2(data[j + i * 16 + 16384], data[j + 7 + i * 16 + 16384]);
						float num3 = this.FixFloat2(data[j + i * 16 + 32768], data[j + 7 + i * 16 + 32768]);
						float num4 = this.FixFloat2(data[j + i * 16 + 49152], data[j + 7 + i * 16 + 49152]);
						streamWriter.WriteLine(string.Concat(new string[]
						{
							" ",
							j.ToString(),
							": ",
							num.ToString(),
							" ",
							num2.ToString(),
							" ",
							num3.ToString(),
							" ",
							num4.ToString(),
							" "
						}));
					}
					streamWriter.WriteLine(" Debug-->");
					for (int k = 14; k < 15; k++)
					{
						float num5 = this.FixFloat2(data[k + i * 16], data[k + 1 + i * 16]);
						float num6 = this.FixFloat2(data[k + i * 16 + 16384], data[k + 1 + i * 16 + 16384]);
						float num7 = this.FixFloat2(data[k + i * 16 + 32768], data[k + 1 + i * 16 + 32768]);
						float num8 = this.FixFloat2(data[k + i * 16 + 49152], data[k + 1 + i * 16 + 49152]);
						streamWriter.WriteLine(string.Concat(new string[]
						{
							" ",
							k.ToString(),
							": ",
							num5.ToString(),
							" ",
							num6.ToString(),
							" ",
							num7.ToString(),
							" ",
							num8.ToString(),
							" "
						}));
					}
					for (int l = 14; l < 15; l++)
					{
						float num9 = this.FixFloat(data[l + i * 16]);
						float num10 = this.FixFloat(data[l + i * 16 + 16384]);
						float num11 = this.FixFloat(data[l + i * 16 + 32768]);
						float num12 = this.FixFloat(data[l + i * 16 + 49152]);
						streamWriter.WriteLine(string.Concat(new string[]
						{
							" ",
							l.ToString(),
							": ",
							num9.ToString(),
							" ",
							num10.ToString(),
							" ",
							num11.ToString(),
							" ",
							num12.ToString(),
							" "
						}));
					}
					for (int m = 14; m < 16; m++)
					{
						float num13 = data[m + i * 16];
						float num14 = data[m + i * 16 + 16384];
						float num15 = data[m + i * 16 + 32768];
						float num16 = data[m + i * 16 + 49152];
						streamWriter.WriteLine(string.Concat(new string[]
						{
							" ",
							m.ToString(),
							": ",
							num13.ToString(),
							" ",
							num14.ToString(),
							" ",
							num15.ToString(),
							" ",
							num16.ToString(),
							" "
						}));
					}
				}
			}
		}

		public void Init()
		{
			this.phyTexture = new RenderTexture[2];
			this.phyTexture[0] = new RenderTexture(1024, 16, 0, 11, 1)
			{
				wrapMode = 1,
				filterMode = 0
			};
			this.phyTexture[1] = new RenderTexture(1024, 16, 0, 11, 1)
			{
				wrapMode = 1,
				filterMode = 0
			};
			this.groundHight.wrapMode = 1;
			this.groundHight.filterMode = 0;
			RTUtility.ClearColor(this.phyTexture[0]);
			RTUtility.ClearColor(this.phyTexture[1]);
			this.readableTexture = new Texture2D(1024, 16, 5, false);
			for (int i = 0; i < this.Masses.Count; i++)
			{
				this.inputMaterial.SetFloat("Index", (float)i);
				this.inputMaterial.SetFloat("Mass", this.Masses[i].MassValue);
				this.inputMaterial.SetVector("Position", this.Masses[i].Position);
				this.inputMaterial.SetVector("Rotation", this.Masses[i].Rotation.ToVector4());
				this.inputMaterial.SetVector("Velocity", this.Masses[i].Velocity);
				this.inputMaterial.SetVector("Force", this.Masses[i].Force);
				this.inputMaterial.SetVector("Motor", this.Masses[i].Motor);
				this.inputMaterial.SetFloat("Buoyancy", this.Masses[i].Buoyancy);
				this.inputMaterial.SetFloat("BuoyancySpeedMultiplier", this.Masses[i].BuoyancySpeedMultiplier);
				if (i > 0)
				{
					if (this.Connections[i - 1] is Spring)
					{
						Spring spring = (Spring)this.Connections[i - 1];
						this.inputMaterial.SetFloat("SpringConstant", spring.SpringConstant);
						this.inputMaterial.SetFloat("SpringLength", spring.SpringLength);
						this.inputMaterial.SetFloat("SpringFrictionConstant", spring.FrictionConstant);
					}
				}
				else
				{
					this.inputMaterial.SetFloat("SpringConstant", 0f);
					this.inputMaterial.SetFloat("SpringLength", 0f);
					this.inputMaterial.SetFloat("SpringFrictionConstant", 0f);
				}
				Graphics.Blit(this.phyTexture[0], this.phyTexture[1], this.inputMaterial);
				RTUtility.Swap(this.phyTexture);
				float[] array = new float[65536];
				EncodeFloat.ReadFromRenderTexture(this.phyTexture[0], 4, array);
				this.DumpData(array, "C:\\ScreenShots\\initdata" + i.ToString());
			}
			this.WriteResultsFromTexture("C:\\ScreenShots\\test.png", this.phyTexture[0]);
			this.DumpMasses("C:\\ScreenShots\\initmasses");
			float[] array2 = new float[65536];
			EncodeFloat.ReadFromRenderTexture(this.phyTexture[0], 4, array2);
			this.DumpData(array2, "C:\\ScreenShots\\initdata");
		}

		public void Update(float dt)
		{
			int num = (int)(dt / 0.001f) + 1;
			if (num != 0)
			{
				dt /= (float)num;
			}
			for (int i = 0; i < num; i++)
			{
				this.Operate(dt);
			}
		}

		private void Operate(float dt)
		{
			this.SimulateConnectionPoint(dt);
			this.transformMaterial.SetFloat("dt", dt);
			if (this.frameCount < 4)
			{
				float[] array = new float[65536];
				EncodeFloat.ReadFromRenderTexture(this.phyTexture[0], 4, array);
				this.DumpData(array, "C:\\ScreenShots\\testdata-bf" + this.frameCount.ToString());
			}
			Graphics.Blit(this.phyTexture[0], this.phyTexture[1], this.transformMaterial);
			RTUtility.Swap(this.phyTexture);
			if (!this.writeOnceFlag)
			{
				this.WriteResultsFromTexture("C:\\ScreenShots\\calc_test.png", this.phyTexture[0]);
			}
			this.writeOnceFlag = true;
			if (this.frameCount < 50)
			{
				float[] array2 = new float[65536];
				EncodeFloat.ReadFromRenderTexture(this.phyTexture[0], 4, array2);
				this.DumpData(array2, "C:\\ScreenShots\\testdata" + this.frameCount.ToString());
			}
			this.frameCount++;
		}

		public void SetVelocity(Vector3 connectionVelocity)
		{
			this.RopeConnectionVelocity = connectionVelocity;
		}

		public void SetAngularVelocity(Vector3 connectionVelocity)
		{
		}

		public void SyncMasses()
		{
			RenderTexture renderTexture = this.phyTexture[0];
			try
			{
				EncodeFloat.ReadFromRenderTexture(renderTexture, 4, this.simData);
			}
			finally
			{
			}
			int count = this.Masses.Count;
			for (int i = 0; i < count; i++)
			{
				float num = this.FixFloat3(this.simData[14 + i * 16]);
				float num2 = this.FixFloat3(this.simData[14 + i * 16 + 16384]);
				float num3 = this.FixFloat3(this.simData[14 + i * 16 + 32768]);
				float num4 = this.FixFloat3(this.simData[14 + i * 16 + 49152]);
				this.Masses[i].Position = new Vector3(num, num2, num3);
			}
		}

		private void SimulateConnectionPoint(float dt)
		{
			this.RopeConnectionPosition += this.RopeConnectionVelocity * dt;
			if (this.RopeConnectionPosition.y < 0f)
			{
				this.RopeConnectionPosition = new Vector3(this.RopeConnectionPosition.x, 0f, this.RopeConnectionPosition.z);
				this.RopeConnectionVelocity = new Vector3(this.RopeConnectionVelocity.x, 0f, this.RopeConnectionVelocity.z);
			}
			this.Masses[0].Position = this.RopeConnectionPosition;
			this.Masses[0].Velocity = this.RopeConnectionVelocity;
			this.updateMaterial.SetFloat("Index", 0f);
			this.updateMaterial.SetFloat("Mass", this.Masses[0].MassValue);
			this.updateMaterial.SetVector("Position", this.Masses[0].Position);
			this.updateMaterial.SetVector("Velocity", this.Masses[0].Velocity);
			this.updateMaterial.SetVector("Rotation", this.Masses[0].Rotation.ToVector4());
			Graphics.Blit(this.phyTexture[0], this.phyTexture[1], this.updateMaterial);
			RTUtility.Swap(this.phyTexture);
		}

		public void GpuOperate(float dt)
		{
			GpuRopeSimulation.GpuMassVector gpuMassVector = new GpuRopeSimulation.GpuMassVector();
			GpuRopeSimulation.GpuMassVector gpuMassVector2 = new GpuRopeSimulation.GpuMassVector();
			GpuRopeSimulation.GpuMassVector gpuMassVector3 = new GpuRopeSimulation.GpuMassVector();
			Color color = gpuMassVector.PositionAndMass - gpuMassVector3.PositionAndMass;
			float num = Mathf.Sqrt(color.r * color.r + color.g * color.g + color.b * color.b);
			float num2 = Mathf.Clamp01(1000f * gpuMassVector.PositionAndMass.g);
			float num3 = Mathf.Sqrt(gpuMassVector.Velocity.r * gpuMassVector.Velocity.r + gpuMassVector.Velocity.g * gpuMassVector.Velocity.g + gpuMassVector.Velocity.b * gpuMassVector.Velocity.b);
			float num4 = 0f;
			float num5 = Mathf.Clamp01(1000f * (gpuMassVector.PositionAndMass.g - num4));
			gpuMassVector.Force = gpuMassVector.Motor - Color.Lerp(gpuMassVector.Velocity * 0.2f, gpuMassVector.Velocity * 0.02f, num2) + Color.Lerp(new Color(0f, gpuMassVector.Buoyancy.r + num3 * gpuMassVector.Buoyancy.g, 0f), GpuExtensions.Zero, num2) + Color.Lerp(new Color(-gpuMassVector.Velocity.r, 0f, -gpuMassVector.Velocity.b) * 0.2f + new Color(0f, -gpuMassVector.Velocity.g, 0f) * 2f + new Color(0f, 100f * (num4 - gpuMassVector.PositionAndMass.g), 0f), GpuExtensions.Zero, num5);
			if (gpuMassVector2 != null)
			{
				Color color2 = gpuMassVector2.PositionAndMass - gpuMassVector.PositionAndMass;
				float num6 = Mathf.Sqrt(color2.r * color2.r + color2.g * color2.g + color2.b * color2.b);
				gpuMassVector.Force -= color2 / num6 * (num6 - gpuMassVector2.Spring.g) * -gpuMassVector2.Spring.r - (gpuMassVector2.Velocity - gpuMassVector.Velocity) * gpuMassVector2.Spring.b;
			}
			if (gpuMassVector3 != null)
			{
				gpuMassVector.Force += color / num * (num - gpuMassVector.Spring.g) * -gpuMassVector.Spring.r - (gpuMassVector.Velocity - gpuMassVector3.Velocity) * gpuMassVector.Spring.b + new Color(0f, -gpuMassVector.PositionAndMass.a * 9.81f, 0f);
			}
			gpuMassVector.Velocity += gpuMassVector.Force / gpuMassVector.PositionAndMass.a * dt;
			gpuMassVector.PositionAndMass += new Color(gpuMassVector.Velocity.r, gpuMassVector.Velocity.g, gpuMassVector.Velocity.b) * dt;
		}

		public void SetRotationVelocity(Vector3 velocity)
		{
			throw new NotImplementedException();
		}

		public const float TimeQuant = 0.001f;

		public const float SpringConstant = 5000f;

		public const float SpringFrictionConstant = 0.2f;

		public const float GravityAcceleration = 9.81f;

		public const float WaterHeight = 0f;

		public const float WaterDragConstant = 0.2f;

		public const float AirDragConstant = 0.02f;

		public const float GroundRepulsionConstant = 100f;

		public const float GroundDragConstant = 0.2f;

		public const float GroundAbsorptionConstant = 2f;

		private readonly Material inputMaterial;

		private readonly Material updateMaterial;

		private readonly Material transformMaterial;

		private RenderTexture[] phyTexture;

		private Texture2D readableTexture;

		private readonly Texture groundHight;

		private const int Read = 0;

		private const int Write = 1;

		private const int SimMasses = 1024;

		private const int PhyDataDepth = 16;

		private readonly Rect textureRect = new Rect(0f, 0f, 1f, 1f);

		private bool writeOnceFlag;

		private bool writeUpdateOnceFlag;

		private int frameCount;

		private const int TexturePositionIndex = 0;

		private float[] simData = new float[65536];

		private const float minbound = 10000f;

		private const float maxbound = 100f;

		private const int posAndMassIdx = 0;

		private const int rotationIdx = 1;

		private const int velocityIdx = 2;

		private const int posAndMassIdxFrac = 7;

		private const int posAndMassIdx2 = 14;

		private class GpuMassVector
		{
			public Color PositionAndMass;

			public Vector4 Rotation;

			public Color Velocity;

			public Color Force;

			public Color Motor;

			public Color Buoyancy;

			public Color Spring;
		}
	}
}
