using System;
using System.Collections.Generic;
using System.Threading;
using Mono.Simd;
using Mono.Simd.Math;
using Phy;
using Phy.Verlet;
using UnityEngine;

public class PhyAsyncTest : MonoBehaviour
{
	private void Start()
	{
		this.sim = new ConnectedBodiesSystemDummy();
		this.initSimSprings();
		if (!this.SingleThreadMode)
		{
			this.simthread = new SimulationThread("Test", this.sim, new ConnectedBodiesSystemDummy());
			this.simthread.Start();
		}
		this.sim.PhyActionsListener = this.simthread;
		if (this.simthread != null)
		{
			this.DebugTool.OverrideSim = this.simthread.InternalSim;
		}
		else
		{
			this.DebugTool.OverrideSim = this.sim;
		}
	}

	private void initSim()
	{
		VerletMass verletMass = new VerletMass(this.sim, 1f, Vector3.zero, Mass.MassType.Unknown);
		verletMass.IsKinematic = true;
		verletMass.StopMass();
		this.sim.Masses.Add(verletMass);
		this.sim.RefreshObjectArrays(true);
	}

	private void initSimSprings()
	{
		Mass[] array = new Mass[10];
		for (int i = 0; i < 10; i++)
		{
			Mass mass = new Mass(this.sim, 2E-05f, Vector3.down * (float)i * 0.1f, Mass.MassType.Line);
			mass.StopMass();
			array[i] = mass;
			this.sim.Masses.Add(mass);
		}
		array[0].IsKinematic = true;
		for (int j = 0; j < 9; j++)
		{
			Spring spring = new Spring(array[j], array[j + 1], 500f, 0.1f, 0.002f);
			this.sim.Connections.Add(spring);
		}
		this.sim.RefreshObjectArrays(true);
	}

	private void initSim1()
	{
		VerletMass[,] array = new VerletMass[10, 10];
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < 10; j++)
			{
				VerletMass verletMass = new VerletMass(this.sim, 1f, new Vector3((float)j, (float)(-(float)i), 0f) * 0.05f, Mass.MassType.Unknown);
				array[i, j] = verletMass;
				verletMass.StopMass();
				this.sim.Masses.Add(verletMass);
			}
		}
		array[0, 0].IsKinematic = true;
		array[0, 0].StopMass();
		array[0, 9].IsKinematic = true;
		array[0, 9].StopMass();
		array[0, 9].Position = new Vector3(9f, 0f, 0f) * 0.05f;
		for (int k = 0; k < 10; k++)
		{
			for (int l = 0; l < 10; l++)
			{
				if (k < 9)
				{
					VerletSpring verletSpring = new VerletSpring(array[k, l], array[k + 1, l], 0.05f, 0.01f, true);
					this.sim.Connections.Add(verletSpring);
				}
				if (l < 9)
				{
					VerletSpring verletSpring2 = new VerletSpring(array[k, l], array[k, l + 1], 0.05f, 0.01f, true);
					this.sim.Connections.Add(verletSpring2);
				}
				if (k < 9 && l < 9)
				{
					VerletSpring verletSpring3 = new VerletSpring(array[k, l], array[k + 1, l + 1], 0.05f * Mathf.Sqrt(2f), 0.01f, true);
					this.sim.Connections.Add(verletSpring3);
				}
			}
		}
		this.sim.RefreshObjectArrays(true);
	}

	private void Update()
	{
		this.PivotA.position = new Vector3(Mathf.Cos(this.rotAngle) + 1f, Mathf.Sin(this.rotAngle) + 5f, 0f) * 0.2f;
		this.rotAngle += Time.deltaTime * 2f;
		if (this.simthread.InternalSim.Masses.Count > 0)
		{
			this.simthread.InternalSim.Masses[0].Position = this.PivotA.position;
		}
		this.simthread.DisableSyncSim = this.DisableSyncSim;
		if (this.simthread != null)
		{
			this.simthread.SyncMain();
		}
		else
		{
			this.sim.RefreshObjectArrays(true);
			this.sim.Update(Time.deltaTime);
		}
	}

	private void OnApplicationQuit()
	{
		if (this.simthread != null)
		{
			this.simthread.ForceStop();
		}
	}

	private void OnDestroy()
	{
		if (this.simthread != null)
		{
			this.simthread.ForceStop();
		}
	}

	public bool SingleThreadMode;

	private ConnectedBodiesSystemDummy sim;

	private SimulationThread simthread;

	public int ActionTest;

	public PhyDebugTool DebugTool;

	public Transform PivotA;

	public bool DisableSyncSim;

	private float rotAngle;

	private const int gridw = 10;

	private const int gridh = 10;

	private const float gridcellsize = 0.05f;

	public class TestSimulation : ConnectedBodiesSystem
	{
		public TestSimulation()
			: base("PhyAsyncTest.TestSimulation")
		{
		}

		public override void ApplyForcesToMass(Mass mass)
		{
			if (mass.IsKinematic || mass.IgnoreEnvForces)
			{
				return;
			}
			Vector4f vector4f = Vector4fExtensions.down * mass.MassValue4f * ConnectedBodiesSystem.GravityAcceleration4f;
			mass.ApplyForce(vector4f, false);
		}
	}

	public class SimThread
	{
		public SimThread(Simulation source)
		{
			this.sim = new PhyAsyncTest.TestSimulation();
			this.sim.RefreshObjectArrays(true);
			this.sourceSim = source;
			this.positionActions = new List<PhyAsyncTest.SimThread.ParticleVectorAction>();
			this.forceActions = new List<PhyAsyncTest.SimThread.ParticleVectorAction>();
			this.snapshot = null;
			this.thread = new Thread(new ThreadStart(this.ThreadProc));
			this.thread.IsBackground = true;
			this.MainThreadCycle = PhyAsyncTest.SimThread.ThreadCycleState.Busy;
			this.SimThreadCycle = PhyAsyncTest.SimThread.ThreadCycleState.Busy;
			this.thread.Start();
		}

		private void ThreadProc()
		{
			while (!this.abortFlag)
			{
				try
				{
					lock (this)
					{
						this.writeSnapshot();
						if (this.MainThreadCycle == PhyAsyncTest.SimThread.ThreadCycleState.Waiting)
						{
							Monitor.Pulse(this);
						}
						this.SimThreadCycle = PhyAsyncTest.SimThread.ThreadCycleState.Waiting;
						if (!Monitor.Wait(this, 1000))
						{
							Debug.LogError("Main thread timeout.");
							this.ForceStop();
						}
						this.SimThreadCycle = PhyAsyncTest.SimThread.ThreadCycleState.Busy;
						this.cachedDeltaTime = Mathf.Min(this.DeltaTime, 0.04f);
						this.readActions();
					}
					this.sim.Update(this.cachedDeltaTime);
				}
				catch (Exception ex)
				{
					Debug.LogError("Thread exception: " + ex.Message);
					this.SimThreadCycle = PhyAsyncTest.SimThread.ThreadCycleState.Busy;
					Monitor.Pulse(this);
					this.ForceStop();
				}
			}
			Debug.LogWarning("Sim thread has finshed");
		}

		private void readActions()
		{
			this.snapshotTest = this.actionTest + 1;
			HashSet<int> hashSet = new HashSet<int>(this.sim.DictMasses.Keys);
			for (int i = 0; i < this.sourceSim.Masses.Count; i++)
			{
				Mass mass = this.sourceSim.Masses[i];
				if (!this.sim.DictMasses.ContainsKey(mass.UID))
				{
					Mass mass2;
					if (mass is VerletMass)
					{
						mass2 = new VerletMass(this.sim, mass as VerletMass);
					}
					else
					{
						mass2 = new Mass(this.sim, mass);
					}
					this.sim.Masses.Add(mass2);
				}
				else
				{
					hashSet.Remove(mass.UID);
				}
			}
			foreach (int num in hashSet)
			{
				this.sim.RemoveMass(this.sim.DictMasses[num]);
			}
			this.sim.RefreshObjectArrays(true);
			HashSet<int> hashSet2 = new HashSet<int>(this.sim.DictConnections.Keys);
			for (int j = 0; j < this.sourceSim.Connections.Count; j++)
			{
				ConnectionBase connectionBase = this.sourceSim.Connections[j];
				if (!this.sim.DictConnections.ContainsKey(connectionBase.UID))
				{
					ConnectionBase connectionBase2 = null;
					if (connectionBase is Spring)
					{
						connectionBase2 = new Spring(this.sim, connectionBase as Spring);
					}
					else if (connectionBase is VerletSpring)
					{
						connectionBase2 = new VerletSpring(this.sim, connectionBase as VerletSpring);
					}
					if (connectionBase2 != null)
					{
						this.sim.Connections.Add(connectionBase2);
					}
				}
				else
				{
					hashSet2.Remove(connectionBase.UID);
				}
			}
			foreach (int num2 in hashSet2)
			{
				this.sim.Connections.Remove(this.sim.DictConnections[num2]);
			}
			this.sim.RefreshObjectArrays(true);
			for (int k = 0; k < this.positionActions.Count; k++)
			{
				PhyAsyncTest.SimThread.ParticleVectorAction particleVectorAction = this.positionActions[k];
				this.sim.Masses[particleVectorAction.uid].Position = particleVectorAction.vector;
			}
			this.positionActions.Clear();
		}

		private void writeSnapshot()
		{
			if (this.snapshot == null || this.snapshot.Length != this.sim.Masses.Count)
			{
				this.snapshot = new PhyAsyncTest.SimThread.ParticleSnapshot[this.sim.Masses.Count];
			}
			for (int i = 0; i < this.sim.Masses.Count; i++)
			{
				Mass mass = this.sim.Masses[i];
				this.snapshot[i].uid = mass.UID;
				this.snapshot[i].position = mass.Position;
				this.snapshot[i].velocity = mass.Velocity;
			}
		}

		public void ForceStop()
		{
			this.abortFlag = true;
		}

		~SimThread()
		{
			this.ForceStop();
		}

		public const int MaxIterationsPerFrame = 100;

		public volatile PhyAsyncTest.SimThread.ThreadCycleState MainThreadCycle;

		public volatile PhyAsyncTest.SimThread.ThreadCycleState SimThreadCycle;

		public volatile float DeltaTime;

		public List<PhyAsyncTest.SimThread.ParticleVectorAction> positionActions;

		public List<PhyAsyncTest.SimThread.ParticleVectorAction> forceActions;

		public PhyAsyncTest.SimThread.ParticleSnapshot[] snapshot;

		public Thread thread;

		private Simulation sourceSim;

		private PhyAsyncTest.TestSimulation sim;

		private volatile bool abortFlag;

		private float cachedDeltaTime;

		public int actionTest;

		public int snapshotTest;

		public enum ThreadCycleState
		{
			Busy,
			Waiting
		}

		public struct ParticleVectorAction
		{
			public int uid;

			public Vector3 vector;
		}

		public struct ParticleSnapshot
		{
			public int uid;

			public Vector3 position;

			public Vector3 velocity;
		}
	}
}
