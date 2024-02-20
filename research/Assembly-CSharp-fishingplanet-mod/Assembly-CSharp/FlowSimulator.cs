using System;
using Flowmap;
using UnityEngine;

[RequireComponent(typeof(FlowmapGenerator))]
public abstract class FlowSimulator : MonoBehaviour
{
	public int SimulationStepsCount
	{
		get
		{
			return this.simulationStepsCount;
		}
	}

	public bool Simulating
	{
		get
		{
			return this.simulating;
		}
	}

	protected bool Initialized
	{
		get
		{
			return this.initialized;
		}
	}

	public FlowmapGenerator Generator
	{
		get
		{
			if (this.generator == null)
			{
				this.generator = base.GetComponent<FlowmapGenerator>();
			}
			return this.generator;
		}
	}

	protected virtual void Update()
	{
		if (!this.Initialized)
		{
			this.Init();
		}
		if (Application.isPlaying && this.Simulating)
		{
			this.Tick();
		}
	}

	public virtual void Init()
	{
		this.simulationStepsCount = 0;
		this.initialized = true;
	}

	public virtual void StartSimulating()
	{
		if (!this.Initialized || this.SimulationStepsCount == 0)
		{
			this.Init();
		}
		this.simulating = true;
	}

	public virtual void StopSimulating()
	{
		this.simulating = false;
	}

	public virtual void Reset()
	{
		this.simulationStepsCount = 0;
		if (!this.Initialized)
		{
			this.Init();
		}
	}

	public virtual void Tick()
	{
		if (!this.Simulating)
		{
			return;
		}
		this.simulationStepsCount++;
		if (this.simulationStepsCount == this.maxSimulationSteps && this.maxSimulationSteps != 0 && !this.continuousSimulation)
		{
			this.MaxStepsReached();
		}
	}

	protected virtual void MaxStepsReached()
	{
		this.StopSimulating();
	}

	public int resolutionX = 256;

	public int resolutionY = 256;

	public SimulationBorderCollision borderCollision;

	public bool simulateOnPlay;

	public int maxSimulationSteps = 500;

	private int simulationStepsCount;

	public bool continuousSimulation;

	public string outputFolderPath;

	public string outputPrefix;

	public bool writeToFileOnMaxSimulationSteps = true;

	private bool simulating;

	private bool initialized;

	protected FlowmapGenerator generator;
}
