using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Phy;
using UnityEngine;

public class PhysicsSandbox : MonoBehaviour
{
	private protected int frameIndex { protected get; private set; }

	private protected List<float> energyPlot { protected get; private set; }

	private protected List<float> motorPlot { protected get; private set; }

	private protected List<float> forcePlot { protected get; private set; }

	private protected List<float> avgforcePlot { protected get; private set; }

	private void Start()
	{
		this.plots = new Dictionary<string, List<float>>();
		Dictionary<string, List<float>> dictionary = this.plots;
		string text = "energy";
		List<float> list = new List<float>();
		this.energyPlot = list;
		dictionary[text] = list;
		Dictionary<string, List<float>> dictionary2 = this.plots;
		string text2 = "motor";
		list = new List<float>();
		this.motorPlot = list;
		dictionary2[text2] = list;
		Dictionary<string, List<float>> dictionary3 = this.plots;
		string text3 = "force";
		list = new List<float>();
		this.forcePlot = list;
		dictionary3[text3] = list;
		Dictionary<string, List<float>> dictionary4 = this.plots;
		string text4 = "avgforce";
		list = new List<float>();
		this.avgforcePlot = list;
		dictionary4[text4] = list;
		this.externalActions = new List<PhysicsSandbox.FishingRodExternalAction>();
		this.externalActions.AddRange(this.ConstantForceExternalActions);
		this.externalActions.AddRange(this.TranslateMassExternalActions);
		this.OnStart();
		for (int i = 0; i < this.externalActions.Count; i++)
		{
			this.externalActions[i].Init(this.simulation);
		}
		if (this._PerformanceTest.Enabled)
		{
			this._PerformanceTest.OnStart(this.simulation);
		}
	}

	private void Update()
	{
		this.OnUpdate();
	}

	private void LateUpdate()
	{
		this.OnLateUpdatePre();
		this.simulation.Update(this.FrameFixedDeltaTime);
		this.OnLateUpdatePost();
	}

	protected virtual void OnStart()
	{
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual void OnLateUpdatePre()
	{
		this.addPlotsFrame();
		for (int i = 0; i < this.externalActions.Count; i++)
		{
			this.externalActions[i].Update((float)this.frameIndex * this.FrameFixedDeltaTime);
		}
	}

	protected virtual void OnLateUpdatePost()
	{
		this.energyPlot[this.frameIndex] = this.measureSystemKineticEnergy(this.simulation);
		this.motorPlot[this.frameIndex] = this.getSystemMotorSum(this.simulation);
		this.forcePlot[this.frameIndex] = this.getSystemForceSum(this.simulation);
		this.avgforcePlot[this.frameIndex] = this.getSystemAvgForceSum(this.simulation);
		this.frameIndex++;
	}

	private float measureSystemKineticEnergy(Simulation sim)
	{
		float num = 0f;
		for (int i = 0; i < sim.Masses.Count; i++)
		{
			float magnitude = sim.Masses[i].Velocity.magnitude;
			num += magnitude * magnitude * 0.5f * sim.Masses[i].MassValue;
		}
		return num;
	}

	private float getSystemMotorSum(Simulation sim)
	{
		float num = 0f;
		for (int i = 0; i < sim.Masses.Count; i++)
		{
			num += sim.Masses[i].Motor.magnitude;
		}
		return num;
	}

	private float getSystemForceSum(Simulation sim)
	{
		float num = 0f;
		for (int i = 0; i < sim.Masses.Count; i++)
		{
			num += sim.Masses[i].Force.magnitude;
		}
		return num;
	}

	private float getSystemAvgForceSum(Simulation sim)
	{
		float num = 0f;
		for (int i = 0; i < sim.Masses.Count; i++)
		{
			num += sim.Masses[i].AvgForce.magnitude;
		}
		return num;
	}

	private void OnApplicationQuit()
	{
		this.savePlots();
	}

	private void savePlots()
	{
		using (StreamWriter streamWriter = new StreamWriter(Application.dataPath + this.PlotOutputPath))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("#");
			foreach (string text in this.plots.Keys)
			{
				stringBuilder.Append(text + "\t");
			}
			stringBuilder.AppendLine(string.Empty);
			for (int i = 0; i < this.plotsFramesCount; i++)
			{
				foreach (string text2 in this.plots.Keys)
				{
					stringBuilder.Append(this.plots[text2][i] + "\t");
				}
				stringBuilder.AppendLine(string.Empty);
			}
			streamWriter.Write(stringBuilder.ToString());
		}
	}

	private void addPlotsFrame()
	{
		foreach (string text in this.plots.Keys)
		{
			this.plots[text].Add(0f);
		}
		this.plotsFramesCount++;
	}

	public string PlotOutputPath;

	public float FrameFixedDeltaTime;

	public PhysicsSandbox.ConstantForceExternalAction[] ConstantForceExternalActions;

	public PhysicsSandbox.TranslateMassExternalAction[] TranslateMassExternalActions;

	public PhysicsSandbox.PerformanceTest _PerformanceTest;

	protected Simulation simulation;

	protected Dictionary<string, List<float>> plots;

	private int plotsFramesCount;

	protected List<PhysicsSandbox.FishingRodExternalAction> externalActions;

	public enum ObjectMassSelector
	{
		First,
		Middle,
		Last
	}

	[Serializable]
	public class ExternalAction
	{
		public Mass Target { get; protected set; }

		public Simulation Sim { get; private set; }

		private protected float phaseStart { protected get; private set; }

		private protected float phaseFinish { protected get; private set; }

		private protected int phaseCounter { protected get; private set; }

		public virtual void Init(Simulation sim)
		{
			this.Sim = sim;
			this.Target = this.Sim.Masses[this.TargetMassIndexsOffset];
			this.phaseStart = this.StartTime;
			this.phaseFinish = this.phaseStart + this.PhaseDuration;
			this.phaseCounter = 0;
			this.phaseActive = false;
		}

		public void Update(float time)
		{
			if (this.Target != null && this.Enabled && this.phaseCounter <= this.RepeatCount)
			{
				if (time >= this.phaseStart && time <= this.phaseFinish)
				{
					if (!this.phaseActive)
					{
						this.phaseActive = true;
						this.Start();
					}
					this.Continue();
				}
				if (this.phaseActive && time > this.phaseFinish)
				{
					this.phaseActive = false;
					this.Finish();
					this.phaseCounter++;
					this.phaseStart = this.phaseFinish + this.PhaseCooldown;
					this.phaseFinish = this.phaseStart + this.PhaseDuration;
				}
			}
		}

		public virtual void Start()
		{
		}

		public virtual void Continue()
		{
		}

		public virtual void Finish()
		{
		}

		public bool Enabled;

		public float StartTime;

		public float PhaseDuration;

		public float PhaseCooldown;

		public int RepeatCount;

		public int TargetMassIndexsOffset;

		protected bool phaseActive;
	}

	[Serializable]
	public class FishingRodExternalAction : PhysicsSandbox.ExternalAction
	{
		public override void Init(Simulation sim)
		{
			base.Init(sim);
			PhyObject phyObject = (base.Sim as FishingRodSimulation).Objects.FirstOrDefault((PhyObject i) => i.Type == this.TargetObject);
			if (phyObject != null)
			{
				PhysicsSandbox.ObjectMassSelector targetMassSelector = this.TargetMassSelector;
				if (targetMassSelector != PhysicsSandbox.ObjectMassSelector.First)
				{
					if (targetMassSelector != PhysicsSandbox.ObjectMassSelector.Middle)
					{
						if (targetMassSelector == PhysicsSandbox.ObjectMassSelector.Last)
						{
							base.Target = phyObject.Masses[phyObject.Masses.Count - 1 + this.TargetMassIndexsOffset];
						}
					}
					else
					{
						base.Target = phyObject.Masses[phyObject.Masses.Count / 2 + this.TargetMassIndexsOffset];
					}
				}
				else
				{
					base.Target = phyObject.Masses[this.TargetMassIndexsOffset];
				}
			}
		}

		public PhyObjectType TargetObject;

		public PhysicsSandbox.ObjectMassSelector TargetMassSelector;
	}

	[Serializable]
	public class PerformanceTest
	{
		public void OnStart(Simulation sim)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			sim.Update(0.0004f * (float)this.IterationsCount);
			stopwatch.Stop();
			Debug.Log(string.Concat(new object[] { "Sandbox PerformanceTest: sim ", this.IterationsCount, " iterations in ", stopwatch.ElapsedMilliseconds, " ms." }));
		}

		public bool Enabled;

		public int IterationsCount;
	}

	[Serializable]
	public class ConstantForceExternalAction : PhysicsSandbox.FishingRodExternalAction
	{
		public override void Start()
		{
			base.Target.Motor += this.Force;
		}

		public override void Finish()
		{
			base.Target.Motor -= this.Force;
		}

		public Vector3 Force;
	}

	[Serializable]
	public class TranslateMassExternalAction : PhysicsSandbox.FishingRodExternalAction
	{
		public override void Start()
		{
			base.Target.Position += this.Translate;
		}

		public override void Finish()
		{
			base.Target.Position -= this.Translate;
		}

		public Vector3 Translate;
	}
}
