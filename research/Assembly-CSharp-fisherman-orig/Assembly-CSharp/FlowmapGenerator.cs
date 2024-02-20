using System;
using System.Collections.Generic;
using Flowmap;
using UnityEngine;

[AddComponentMenu("Flowmaps/Generator")]
[ExecuteInEditMode]
public class FlowmapGenerator : MonoBehaviour
{
	public static LayerMask GpuRenderLayer
	{
		get
		{
			return LayerMask.NameToLayer("Default");
		}
	}

	public static bool SupportsGPUPath
	{
		get
		{
			return SystemInfo.SupportsRenderTextureFormat(2) && SystemInfo.supportsRenderTextures;
		}
	}

	public static int ThreadCount
	{
		get
		{
			return FlowmapGenerator._threadCount;
		}
		set
		{
			FlowmapGenerator._threadCount = value;
		}
	}

	public static RenderTextureFormat GetSingleChannelRTFormat
	{
		get
		{
			return (!SystemInfo.SupportsRenderTextureFormat(14)) ? 2 : 14;
		}
	}

	public static RenderTextureFormat GetTwoChannelRTFormat
	{
		get
		{
			return (!SystemInfo.SupportsRenderTextureFormat(12)) ? 2 : 12;
		}
	}

	public static RenderTextureFormat GetFourChannelRTFormat
	{
		get
		{
			return (!SystemInfo.SupportsRenderTextureFormat(11)) ? 2 : 11;
		}
	}

	public FlowSimulationField[] Fields
	{
		get
		{
			this.CleanNullFields();
			return this.fields.ToArray();
		}
	}

	public SimulationPath GetSimulationPath()
	{
		return (!this.gpuAcceleration || !FlowmapGenerator.SupportsGPUPath) ? SimulationPath.CPU : SimulationPath.GPU;
	}

	public Vector2 Dimensions
	{
		get
		{
			return this.dimensions;
		}
		set
		{
			this.dimensions = value;
		}
	}

	public Vector3 Position
	{
		get
		{
			return this.cachedPosition;
		}
	}

	public FlowSimulator FlowSimulator
	{
		get
		{
			if (!this.flowSimulator)
			{
				this.flowSimulator = base.GetComponent<FlowSimulator>();
			}
			return this.flowSimulator;
		}
	}

	public FlowHeightmap Heightmap
	{
		get
		{
			if (!this.heightmap)
			{
				this.heightmap = base.GetComponent<FlowHeightmap>();
			}
			return this.heightmap;
		}
	}

	private void Awake()
	{
		if (this.autoAddChildFields)
		{
			FlowSimulationField[] componentsInChildren = base.GetComponentsInChildren<FlowSimulationField>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				this.AddSimulationField(componentsInChildren[i]);
			}
		}
		base.transform.rotation = Quaternion.identity;
		this.cachedPosition = base.transform.position;
		this.UpdateThreadCount();
	}

	private void Start()
	{
		this.UpdateSimulationPath();
		if (this.FlowSimulator)
		{
			this.FlowSimulator.Init();
			if (this.FlowSimulator.simulateOnPlay && Application.isPlaying)
			{
				this.FlowSimulator.StartSimulating();
			}
		}
	}

	public void UpdateSimulationPath()
	{
		FlowmapGenerator.SimulationPath = this.GetSimulationPath();
	}

	public void UpdateThreadCount()
	{
		FlowmapGenerator._threadCount = this.maxThreadCount;
	}

	private void Update()
	{
		base.transform.rotation = Quaternion.identity;
		this.cachedPosition = base.transform.position;
	}

	public void CleanNullFields()
	{
		this.fields.RemoveAll((FlowSimulationField i) => i == null);
	}

	public void AddSimulationField(FlowSimulationField field)
	{
		if (!this.fields.Contains(field))
		{
			this.fields.Add(field);
		}
	}

	public void ClearAllFields()
	{
		this.fields.Clear();
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(base.transform.position, new Vector3(this.Dimensions.x, 0f, this.Dimensions.y));
	}

	public static SimulationPath SimulationPath;

	private static int _threadCount = 1;

	[SerializeField]
	private List<FlowSimulationField> fields = new List<FlowSimulationField>();

	public bool gpuAcceleration;

	public bool autoAddChildFields = true;

	public int maxThreadCount = 1;

	[SerializeField]
	private Vector2 dimensions = Vector2.one;

	private Vector3 cachedPosition;

	public int outputFileFormat;

	private FlowSimulator flowSimulator;

	private FlowHeightmap heightmap;
}
