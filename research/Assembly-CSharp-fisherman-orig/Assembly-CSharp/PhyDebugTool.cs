using System;
using System.Collections.Generic;
using ComplexMathFFT;
using Phy;
using Phy.Verlet;
using UnityEngine;

[ExecuteInEditMode]
public class PhyDebugTool : MonoBehaviour
{
	public ConnectedBodiesSystem sim
	{
		get
		{
			if (this.OverrideSim != null)
			{
				return this.OverrideSim;
			}
			if (this.simulationName == PhyDebugTool.SimulationName.Main)
			{
				return GameFactory.Player.RodSlot.Sim;
			}
			if (this.simulationName == PhyDebugTool.SimulationName.Pod)
			{
				return RodOnPodBehaviour.RodPodSim;
			}
			if (this.simulationName == PhyDebugTool.SimulationName.Boat)
			{
				return GameFactory.Player.CurrentBoat.Sim;
			}
			if (this.simulationName == PhyDebugTool.SimulationName.Slot)
			{
				return GameFactory.RodSlots[this.SlotIndex].Sim;
			}
			return null;
		}
	}

	public SimulationThread simThread
	{
		get
		{
			if (this.OverrideSimThread != null)
			{
				return this.OverrideSimThread;
			}
			if (this.simulationName == PhyDebugTool.SimulationName.Main)
			{
				return GameFactory.Player.RodSlot.SimThread;
			}
			if (this.simulationName == PhyDebugTool.SimulationName.Pod)
			{
				return RodOnPodBehaviour.RodPodSimThread;
			}
			if (this.simulationName == PhyDebugTool.SimulationName.Boat)
			{
				return GameFactory.Player.CurrentBoat.SimThread;
			}
			if (this.simulationName == PhyDebugTool.SimulationName.Slot)
			{
				return GameFactory.RodSlots[this.SlotIndex].SimThread;
			}
			return null;
		}
	}

	public static void DrawGizmoPlot(float[] y, float miny, float maxy, Vector3 lbPos, Vector3 rbPos, Vector3 ltPos, Color lineColor, Color frameColor, bool drawFrame = true)
	{
	}

	private void Start()
	{
		PhyDebugTool.Instance = this;
		if (Application.isPlaying)
		{
			Object.DontDestroyOnLoad(base.gameObject);
			this.masses = new Dictionary<int, PhyDebugMass>();
			this.connections = new Dictionary<int, PhyDebugConnection>();
			this.massesNode = base.transform.Find("Masses");
			this.connectionsNode = base.transform.Find("Connections");
			this.objectsNode = base.transform.Find("Objects");
			this.plotter = new DebugPlotter("/monitor_output/" + this.PlotterOutputName + ".adv", new string[0], false);
			this.ScanMasses();
			this.ScanConnections();
		}
	}

	public void InitReplay()
	{
		this.masses = new Dictionary<int, PhyDebugMass>();
		this.connections = new Dictionary<int, PhyDebugConnection>();
		this.massesNode = base.transform.Find("Masses");
		this.connectionsNode = base.transform.Find("Connections");
		this.objectsNode = base.transform.Find("Objects");
	}

	public PhyDebugMass FindMass(int UID)
	{
		if (this.masses.ContainsKey(UID))
		{
			return this.masses[UID];
		}
		return null;
	}

	public PhyDebugConnection FindConnection(ConnectionBase c)
	{
		if (c == null)
		{
			return null;
		}
		if (this.connections.ContainsKey(c.UID))
		{
			return this.connections[c.UID];
		}
		return null;
	}

	public void ScanReplay()
	{
		if (this.replay != null)
		{
			if (this.replay.CurrentReadSegment.Frames != null && this.replay.CurrentReadSegment.Frames.Length > 0 && this.replay.CurrentReadSegment.IsReady)
			{
				if (this.ActualReplaySegment == this.replay.CurrentReadSegmentIndex && this.ActualReplayFrame == this.replay.CurrentFrameIndex)
				{
					return;
				}
				PhyReplay.ReplaySegment currentReadSegment = this.replay.CurrentReadSegment;
				PhyReplay.ReplayParticleState[] array = this.replay.ReadFrame();
				if (array == null)
				{
					return;
				}
				foreach (PhyDebugMass phyDebugMass in this.masses.Values)
				{
					phyDebugMass.destroyedFlag = true;
				}
				HashSet<int> hashSet = new HashSet<int>(currentReadSegment.VerletMasses);
				int num = 0;
				for (int i = 0; i < currentReadSegment.Masses.Length; i++)
				{
					int num2 = currentReadSegment.Masses[i];
					if (!this.masses.ContainsKey(num2))
					{
						GameObject gameObject;
						if (hashSet.Contains(num2))
						{
							gameObject = Object.Instantiate<GameObject>(this.VerletMassPrefab, this.massesNode);
						}
						else
						{
							gameObject = Object.Instantiate<GameObject>(this.MassPrefab, this.massesNode);
						}
						PhyDebugMass component = gameObject.GetComponent<PhyDebugMass>();
						component.Init(array[i], num2, this);
						this.masses[num2] = component;
					}
					else
					{
						this.masses[num2].OnUpdateReplay(array[i]);
						this.masses[num2].destroyedFlag = false;
					}
					if (this.masses[num2] is PhyDebugVerletMass)
					{
						PhyDebugVerletMass phyDebugVerletMass = this.masses[num2] as PhyDebugVerletMass;
						num++;
					}
				}
				List<int> list = new List<int>(this.masses.Keys);
				for (int j = 0; j < list.Count; j++)
				{
					if (this.masses[list[j]].destroyedFlag)
					{
						Object.DestroyImmediate(this.masses[list[j]].gameObject);
						this.masses.Remove(list[j]);
					}
				}
				this.ScanConnectionsReplay(currentReadSegment);
				for (int k = 0; k < currentReadSegment.Masses.Length; k++)
				{
					int num3 = currentReadSegment.Masses[k];
					if (this.masses.ContainsKey(num3))
					{
						this.masses[num3].PriorSpringUID = currentReadSegment.MassConnectionsUIDs[k][0];
						this.masses[num3].NextSpringUID = currentReadSegment.MassConnectionsUIDs[k][1];
						if (this.connections.ContainsKey(currentReadSegment.MassConnectionsUIDs[k][0]))
						{
							this.masses[num3].PriorSpring = this.connections[currentReadSegment.MassConnectionsUIDs[k][0]];
						}
						if (this.connections.ContainsKey(currentReadSegment.MassConnectionsUIDs[k][1]))
						{
							this.masses[num3].NextSpring = this.connections[currentReadSegment.MassConnectionsUIDs[k][1]];
						}
					}
				}
				this.ActualReplaySegment = this.replay.CurrentReadSegmentIndex;
				this.ActualReplayFrame = this.replay.CurrentFrameIndex;
			}
		}
		else
		{
			List<int> list2 = new List<int>(this.masses.Keys);
			for (int l = 0; l < list2.Count; l++)
			{
				Object.DestroyImmediate(this.masses[list2[l]].gameObject);
			}
			List<int> list3 = new List<int>(this.connections.Keys);
			for (int m = 0; m < list3.Count; m++)
			{
				Object.DestroyImmediate(this.connections[list3[m]].gameObject);
			}
			this.masses.Clear();
			this.connections.Clear();
		}
	}

	private void ScanConnectionsReplay(PhyReplay.ReplaySegment seg)
	{
		foreach (PhyDebugConnection phyDebugConnection in this.connections.Values)
		{
			phyDebugConnection.destroyedFlag = true;
		}
		for (int i = 0; i < seg.Connections.Length; i++)
		{
			int num = seg.Connections[i][0];
			if (!this.connections.ContainsKey(num))
			{
				GameObject gameObject;
				if (seg.Springs[i].Constant > 0f)
				{
					gameObject = Object.Instantiate<GameObject>(this.SpringPrefab, this.massesNode);
				}
				else if (seg.Springs[i].Constant < 0f)
				{
					gameObject = Object.Instantiate<GameObject>(this.VerletSpringPrefab, this.massesNode);
				}
				else
				{
					gameObject = Object.Instantiate<GameObject>(this.ConnectionPrefab, this.massesNode);
				}
				PhyDebugConnection component = gameObject.GetComponent<PhyDebugConnection>();
				this.connections[num] = component;
			}
			this.connections[num].OnUpdateReplay(num, seg.Connections[i][1], seg.Connections[i][2], seg.Springs[i]);
			this.connections[num].OnUpdate();
			this.connections[num].destroyedFlag = false;
		}
		List<int> list = new List<int>(this.connections.Keys);
		for (int j = 0; j < list.Count; j++)
		{
			if (this.connections[list[j]].destroyedFlag)
			{
				Object.DestroyImmediate(this.connections[list[j]].gameObject);
				this.connections.Remove(list[j]);
			}
		}
	}

	public void ScanMasses()
	{
		if (this.masses == null || this.sim == null)
		{
			return;
		}
		for (int i = 0; i < this.sim.Masses.Count; i++)
		{
			Mass mass = this.sim.Masses[i];
			if (!this.masses.ContainsKey(mass.UID))
			{
				GameObject gameObject;
				if (mass is VerletMass)
				{
					gameObject = Object.Instantiate<GameObject>(this.VerletMassPrefab, this.massesNode);
				}
				else
				{
					gameObject = Object.Instantiate<GameObject>(this.MassPrefab, this.massesNode);
				}
				PhyDebugMass component = gameObject.GetComponent<PhyDebugMass>();
				component.Init(mass, this.simThread, this);
				this.masses[mass.UID] = component;
			}
			else
			{
				this.masses[mass.UID].destroyedFlag = false;
			}
		}
		List<int> list = new List<int>(this.masses.Keys);
		for (int j = 0; j < list.Count; j++)
		{
			if (this.masses[list[j]].destroyedFlag)
			{
				Object.Destroy(this.masses[list[j]].gameObject);
				this.masses.Remove(list[j]);
			}
		}
	}

	public void ScanConnections()
	{
		if (this.connections == null || this.sim == null)
		{
			return;
		}
		for (int i = 0; i < this.sim.Connections.Count; i++)
		{
			ConnectionBase connectionBase = this.sim.Connections[i];
			if (!this.connections.ContainsKey(connectionBase.UID))
			{
				GameObject gameObject = null;
				if (connectionBase is VerletSpring)
				{
					gameObject = Object.Instantiate<GameObject>(this.VerletSpringPrefab, this.connectionsNode);
				}
				else if (connectionBase is Spring)
				{
					gameObject = Object.Instantiate<GameObject>(this.SpringPrefab, this.connectionsNode);
				}
				else if (connectionBase is VerletBend)
				{
					gameObject = Object.Instantiate<GameObject>(this.VerletBendPrefab, this.connectionsNode);
				}
				else if (connectionBase is Bend)
				{
					gameObject = Object.Instantiate<GameObject>(this.BendPrefab, this.connectionsNode);
				}
				else if (connectionBase is TetrahedronTorsionSpring)
				{
					gameObject = Object.Instantiate<GameObject>(this.TetrahedronTorsionSpringPrefab, this.connectionsNode);
				}
				if (gameObject != null)
				{
					PhyDebugConnection component = gameObject.GetComponent<PhyDebugConnection>();
					component.Init(connectionBase);
					this.connections[connectionBase.UID] = component;
				}
			}
			else
			{
				this.connections[connectionBase.UID].destroyedFlag = false;
			}
		}
		List<int> list = new List<int>(this.connections.Keys);
		for (int j = 0; j < list.Count; j++)
		{
			if (this.connections[list[j]].destroyedFlag)
			{
				Object.Destroy(this.connections[list[j]].gameObject);
				this.connections.Remove(list[j]);
			}
		}
	}

	public void ForceUpdate()
	{
		this.OnUpdate();
		foreach (PhyDebugMass phyDebugMass in this.masses.Values)
		{
			phyDebugMass.OnUpdate();
		}
		foreach (PhyDebugConnection phyDebugConnection in this.connections.Values)
		{
			phyDebugConnection.OnUpdate();
		}
	}

	public void OnUpdate()
	{
		if (Application.isPlaying && !this.DisableScan)
		{
			this.ScanMasses();
			this.ScanConnections();
		}
	}

	private void Update()
	{
		PhyDebugTool.Instance = this;
		this.OnUpdate();
	}

	public void ClearExclusiveActions()
	{
		this.simThread.ExclusiveActions.Clear();
	}

	private void OnDestroy()
	{
		PhyDebugTool.Instance = null;
	}

	private void testFFT(float[] signal)
	{
		FourierAnalysis.WindowFilter.Base @base = new FourierAnalysis.WindowFilter.Lanczos(signal.Length);
		float[] array = new float[signal.Length];
		Array.Copy(signal, array, signal.Length);
		for (int i = 0; i < signal.Length; i++)
		{
			float num = (float)i / (float)signal.Length;
			array[i] += Mathf.Sin(num * 4f * 3.1415927f) * this.sineA1 + Mathf.Sin(num * 8f * 3.1415927f) * this.sineA2 + Mathf.Sin(num * 16f * 3.1415927f) * this.sineA3;
		}
		@base.Apply(array);
		PhyDebugTool.DrawGizmoPlot(array, -1f, 1f, Vector3.zero, Vector3.forward * 5f, Vector3.up * 2f, Color.blue, Color.black, true);
		float[] array2 = new float[array.Length / 2];
		Array.Copy(Complex.GetAmplitudes(FourierAnalysis.R2DITFFT(array, -1, 1, 0)), array2, array2.Length);
		PhyDebugTool.DrawGizmoPlot(array2, 0f, 10f, Vector3.zero, Vector3.forward * 30f, Vector3.up * 2f, Color.red, Color.black, true);
	}

	private void testFFT(float a1, float a2, float a3)
	{
		float[] array = new float[1024];
		for (int i = 0; i < array.Length; i++)
		{
			float num = (float)i / (float)array.Length;
			array[i] = Mathf.Sin(num * 300f * a1) + Mathf.Sin(num * 300f * a2) + Mathf.Sin(num * 300f * a3);
		}
		FourierAnalysis.WindowFilter.Lanczos lanczos = new FourierAnalysis.WindowFilter.Lanczos(array.Length);
		lanczos.Apply(array);
		PhyDebugTool.DrawGizmoPlot(array, -1f, 1f, Vector3.zero, Vector3.forward * 5f, Vector3.up * 2f, Color.blue, Color.black, true);
		float[] amplitudes = Complex.GetAmplitudes(FourierAnalysis.R2DITFFT(array, -1, 1, 0));
		PhyDebugTool.DrawGizmoPlot(amplitudes, 0f, 100f, Vector3.zero, Vector3.forward * 30f, Vector3.up * 2f, Color.red, Color.black, true);
	}

	public void DrawGizmos()
	{
	}

	public void PhyMonitorIteration(int iteration)
	{
		if (this.FrameTracking)
		{
			for (int i = 0; i < this.trackableMasses.Count; i++)
			{
				this.trackableMasses[i].PhyMonitorIteration(iteration);
			}
		}
	}

	public void PhyMonitorPrepareFrame(int numberOfIterations)
	{
		this.numberOfIterations = numberOfIterations;
		if (this.FrameTracking)
		{
			this.trackableMasses = new List<PhyDebugMass>();
			foreach (PhyDebugMass phyDebugMass in this.masses.Values)
			{
				if (phyDebugMass.StoreFrameTrajectory)
				{
					phyDebugMass.PhyMonitorPrepareFrame(numberOfIterations);
					this.trackableMasses.Add(phyDebugMass);
				}
			}
		}
	}

	public int SlotIndex;

	public static PhyDebugTool Instance;

	public ConnectedBodiesSystem OverrideSim;

	public SimulationThread OverrideSimThread;

	public bool DisableScan;

	public GameObject MassPrefab;

	public GameObject VerletMassPrefab;

	public GameObject ConnectionPrefab;

	public GameObject SpringPrefab;

	public GameObject VerletSpringPrefab;

	public GameObject VerletBendPrefab;

	public GameObject BendPrefab;

	public GameObject RigidSpringPrefab;

	public GameObject RigidBodyPrefab;

	public GameObject TetrahedronTorsionSpringPrefab;

	public string PlotterOutputName = "PhyDebugTool";

	public bool FrameTracking;

	public PhyDebugTool.SimulationName simulationName = PhyDebugTool.SimulationName.Main;

	private Transform massesNode;

	private Transform connectionsNode;

	private Transform objectsNode;

	private Dictionary<int, PhyDebugMass> masses;

	private Dictionary<int, PhyDebugConnection> connections;

	private List<PhyDebugMass> trackableMasses;

	private DebugPlotter plotter;

	public PhyReplay replay;

	private int ActualReplaySegment = -1;

	private int ActualReplayFrame = -1;

	public int numberOfIterations;

	public int trajectoryIteration;

	public float sineA1 = 1f;

	public float sineA2 = 1f;

	public float sineA3 = 1f;

	public float[] FFTAnalysisSignal;

	public float[] FFTAmplitudes;

	public enum SimulationName
	{
		None,
		Main,
		Pod,
		Boat,
		Slot
	}
}
