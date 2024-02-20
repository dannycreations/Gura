using System;
using Phy;
using UnityEngine;

public class PhyTestRoom : MonoBehaviour
{
	public void OnStart()
	{
		this.sim = new FishingRodSimulation("PhyTestRoomSource", true);
		if (this.simThread != null)
		{
			this.simThread.ForceStop();
		}
		this.simThread = new SimulationThread("TestRoom", this.sim, new FishingRodSimulation("PhyTestRoomThread", false));
		this.sim.PhyActionsListener = this.simThread;
		this.InitSim();
		this.sim.RefreshObjectArrays(true);
		if (this.simThread != null)
		{
			this.simThread.Start();
		}
		this.phydebugtool.OverrideSim = this.sim;
	}

	public virtual void InitSim()
	{
		if (this.rootMass != null)
		{
			this.rootMass.IsKinematic = true;
			this.rootMass.Position = this.rootPosition.position;
			this.rootKinematicSpline = new KinematicSpline(this.rootMass, false);
			this.sim.Connections.Add(this.rootKinematicSpline);
		}
	}

	public virtual void UpdateSim()
	{
		if (this.rootKinematicSpline != null)
		{
			this.rootKinematicSpline.SetNextPointAndRotation(this.rootPosition.position, this.rootPosition.rotation);
		}
	}

	public void OnLateUpdate()
	{
		this.UpdateSim();
		if (this.simThread != null)
		{
			this.simThread.SyncMain();
		}
		else
		{
			this.sim.Update(Time.deltaTime);
		}
		this.sim.DebugDraw();
	}

	public void OnQuit()
	{
		if (this.sim != null)
		{
			this.sim.SaveReplayRecorder();
		}
		if (this.simThread != null)
		{
			this.simThread.InternalSim.SaveReplayRecorder();
			this.simThread.ForceStop();
		}
		DebugPlotter.AutoSave();
	}

	public PhyDebugTool phydebugtool;

	public Transform rootPosition;

	public Mass rootMass;

	protected FishingRodSimulation sim;

	protected SimulationThread simThread;

	protected KinematicSpline rootKinematicSpline;
}
