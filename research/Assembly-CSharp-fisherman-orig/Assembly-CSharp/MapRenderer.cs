using System;
using UnityEngine;
using UnityEngine.UI;

public class MapRenderer
{
	public MapRenderer(FlowRenderHeightmap flowRenderHeightmap, Terrain[] terrains, float lowestDepth, RawImage mapImage)
	{
		this.simplifiedLayer = 1 << LayerMask.NameToLayer("SimplifiedTerrain");
		this.terrainLayer = 1 << LayerMask.NameToLayer("Terrain");
		LogHelper.Check(flowRenderHeightmap != null, "Flow Render Heightmap");
		LogHelper.Check(mapImage != null, "Map Image");
		this.generator = flowRenderHeightmap.GetComponent<FlowmapGenerator>();
		LogHelper.Check(this.generator != null, "Flow Map Generator");
		this.waterMapShader = Shader.Find("Hidden/WaterMap");
		this.map = mapImage;
		this._terrains = terrains;
		float num = -lowestDepth;
		this.rt = new RenderTexture(flowRenderHeightmap.resolutionX, flowRenderHeightmap.resolutionY, 0, 0, 1);
		GameObject gameObject = new GameObject("camera");
		this.camera = gameObject.AddComponent<Camera>();
		this.camera.renderingPath = 0;
		this.camera.clearFlags = 2;
		this.camera.backgroundColor = Color.black;
		this.camera.orthographic = true;
		this.camera.useOcclusionCulling = false;
		this.camera.allowHDR = false;
		this.camera.allowMSAA = false;
		this.camera.depthTextureMode = 0;
		this.camera.cullingMask = this.simplifiedLayer;
		this.camera.nearClipPlane = 0f;
		this.camera.farClipPlane = Mathf.Abs(num) + 1f;
		this.camera.targetTexture = this.rt;
		Vector2 vector = this.generator.Dimensions * 1.4f;
		this.camera.orthographicSize = Mathf.Max(vector.x, vector.y) * 0.5f;
		this.minZoom = this.camera.orthographicSize;
		this.targetSize = this.minZoom;
		this.initialOffset = this.generator.transform.position;
		gameObject.AddComponent<IsobarRenderPostProcess>();
		this.camera.enabled = false;
		this.cameraTransform = gameObject.transform;
		this.cameraTransform.SetPositionAndRotation(this.generator.transform.position - new Vector3(0f, 0.001f, 0f), Quaternion.LookRotation(Vector3.down, Vector3.forward));
		this.lastRenderPosition = this.cameraTransform.position;
		this.rt.wrapMode = 1;
		this.rt.filterMode = 0;
		this.rt.anisoLevel = 0;
		Shader.SetGlobalFloat("_HeightmapRenderDepthMax", this.generator.transform.position.y + num + 1f);
		Shader.SetGlobalFloat("_HeightmapRenderDepthMin", this.generator.transform.position.y);
		Shader.SetGlobalFloat("_Contrast", flowRenderHeightmap.contrast);
		Shader.SetGlobalFloat("_Brightness", flowRenderHeightmap.brightness);
		this.map.texture = this.rt;
		if (StaticUserData.CurrentPond != null && StaticUserData.CurrentPond.PondId != 2 && StaticUserData.CurrentPond.PondId != 119)
		{
			this.IsobarDepthStep = Mathf.Abs(this.generator.transform.position.y + num);
		}
		else
		{
			Shader.SetGlobalFloat("_DepthStep", this._isobarDepthStep);
		}
		this.pixelError = new float[terrains.Length];
		this.grassDistance = new float[terrains.Length];
	}

	public float IsobarDepthStep
	{
		get
		{
			return this._isobarDepthStep;
		}
		set
		{
			this._isobarDepthStep = value;
			Shader.SetGlobalFloat("_DepthStep", this._isobarDepthStep);
		}
	}

	public Vector3 MapOffset
	{
		get
		{
			return this.cameraTransform.position;
		}
	}

	public float ScaleRatio
	{
		get
		{
			return this.camera.orthographicSize / this.minZoom;
		}
	}

	public float ZoomScale
	{
		get
		{
			return (this.minZoom - this.maxZoom) * 0.3f;
		}
	}

	public float SizeRatio
	{
		get
		{
			return this.camera.orthographicSize / this.map.rectTransform.rect.width;
		}
	}

	public Vector3 GetInGameCoord(float x, float y)
	{
		Vector3 vector = this.InitialMin();
		Vector3 vector2 = this.InitialMax();
		return new Vector3(x * (vector2.x - vector.x) + vector.x, 0f, y * (vector2.z - vector.z) + vector.z);
	}

	public Vector3 InitialMin()
	{
		return this.initialOffset - this.minZoom * this.xz;
	}

	public Vector3 InitialMax()
	{
		return this.initialOffset + this.minZoom * this.xz;
	}

	public Vector3 GetMin()
	{
		return this.MapOffset - this.xz * this.camera.orthographicSize;
	}

	public Vector3 GetMax()
	{
		return this.MapOffset + this.xz * this.camera.orthographicSize;
	}

	public void Zoom(float value)
	{
		this.targetSize = Mathf.Clamp(this.targetSize - value, this.maxZoom, this.minZoom);
		this.zoomed = true;
	}

	internal void AlignWithPlayer()
	{
		this.SetTargetPosition(GameFactory.PlayerTransform.position);
		this.isPriorityMoving = true;
	}

	public bool ShouldIgnoreMovement()
	{
		return this.isPriorityMoving;
	}

	public void SetTargetPosition(Vector3 newOffset)
	{
		if (this.isPriorityMoving || Vector3.Distance(newOffset, this.targetPosition) < 10f * Mathf.Epsilon)
		{
			return;
		}
		this.targetPosition = new Vector3(Mathf.Clamp(newOffset.x, this.initialOffset.x - this.minZoom, this.initialOffset.x + this.minZoom), 0f, Mathf.Clamp(newOffset.z, this.initialOffset.z - this.minZoom, this.initialOffset.z + this.minZoom));
		this.moved = true;
	}

	public void Move(Vector2 direction)
	{
		this.targetPosition += this.cameraTransform.TransformDirection(direction.x, direction.y, 0f);
		this.targetPosition = new Vector3(Mathf.Clamp(this.targetPosition.x, this.initialOffset.x - this.minZoom, this.initialOffset.x + this.minZoom), 0f, Mathf.Clamp(this.targetPosition.z, this.initialOffset.z - this.minZoom, this.initialOffset.z + this.minZoom));
		this.moved = true;
	}

	public void Rotate(float angle)
	{
		this.targetRotation = Quaternion.Euler(0f, 0f, angle);
		this.rotated = true;
	}

	public bool Update()
	{
		bool flag = false;
		if (this.moved)
		{
			this.cameraTransform.position = Vector3.Lerp(this.cameraTransform.position, this.targetPosition, Mathf.Min(0.8f, 25f * Time.deltaTime));
			if (Vector3.Distance(this.cameraTransform.position, this.targetPosition) <= 2f * Mathf.Epsilon)
			{
				this.cameraTransform.position = this.targetPosition;
				this.moved = false;
				this.isPriorityMoving = false;
				flag = true;
			}
		}
		if (this.rotated)
		{
			this.cameraTransform.rotation = Quaternion.Lerp(this.cameraTransform.rotation, this.targetRotation, Mathf.Min(0.8f, 25f * Time.deltaTime));
			if (Vector3.Distance(this.targetRotation.eulerAngles, this.cameraTransform.rotation.eulerAngles) <= 2f * Mathf.Epsilon)
			{
				this.cameraTransform.rotation = this.targetRotation;
				this.rotated = false;
				flag = true;
			}
		}
		if (this.zoomed)
		{
			this.camera.orthographicSize = Mathf.Lerp(this.camera.orthographicSize, this.targetSize, Mathf.Min(0.8f, 25f * Time.deltaTime));
			if (Mathf.Abs(this.camera.orthographicSize - this.targetSize) <= 2f * Mathf.Epsilon)
			{
				this.camera.orthographicSize = this.targetSize;
				this.zoomed = false;
				flag = true;
			}
		}
		bool flag2 = this.moved || this.zoomed || this.rotated || flag;
		this.lastRenderOffset = this.cameraTransform.position - this.lastRenderPosition;
		this.lastRenderSizeRatio = this.camera.orthographicSize / this.lastRenderSize;
		float num = this.camera.orthographicSize * 0.2f;
		bool flag3 = this.lastRenderOffset.sqrMagnitude < num * num;
		bool flag4 = this.rotated || flag;
		bool flag5 = Mathf.Abs(this.lastRenderSizeRatio - 1f) < 0.2f;
		bool flag6 = flag3 && flag5 && !flag4;
		if (flag2)
		{
			this.Render(flag6);
		}
		return flag2;
	}

	public void Render(bool fastRender = false)
	{
		if (this.generator == null || this.map == null)
		{
			return;
		}
		if (fastRender)
		{
			float num = (1f - this.lastRenderSizeRatio) * 0.5f;
			this.map.uvRect = new Rect(new Vector2(this.lastRenderOffset.x / (this.lastRenderSize * 2f) + num, this.lastRenderOffset.z / (this.lastRenderSize * 2f) + num), Vector2.one * this.lastRenderSizeRatio);
			return;
		}
		this.map.uvRect = new Rect(Vector2.zero, Vector2.one);
		bool flag = this.camera.orthographicSize > 100f;
		if (flag)
		{
			if (this.camera.cullingMask != this.simplifiedLayer)
			{
				this.camera.cullingMask = this.simplifiedLayer;
			}
			this.camera.RenderWithShader(this.waterMapShader, "RenderType");
		}
		else
		{
			if (this.camera.cullingMask != this.terrainLayer)
			{
				this.camera.cullingMask = this.terrainLayer;
			}
			for (int i = 0; i < this._terrains.Length; i++)
			{
				this.pixelError[i] = this._terrains[i].heightmapPixelError;
				this.grassDistance[i] = this._terrains[i].detailObjectDistance;
				this._terrains[i].heightmapPixelError = 0f;
				this._terrains[i].detailObjectDistance = 0f;
			}
			float shadowDistance = QualitySettings.shadowDistance;
			bool softVegetation = QualitySettings.softVegetation;
			QualitySettings.shadowDistance = 0f;
			QualitySettings.softVegetation = false;
			this.camera.RenderWithShader(this.waterMapShader, "RenderType");
			QualitySettings.shadowDistance = shadowDistance;
			QualitySettings.softVegetation = softVegetation;
			for (int j = 0; j < this._terrains.Length; j++)
			{
				this._terrains[j].heightmapPixelError = this.pixelError[j];
				this._terrains[j].detailObjectDistance = this.grassDistance[j];
			}
		}
		this.lastRenderPosition = this.cameraTransform.position;
		this.lastRenderSize = this.camera.orthographicSize;
	}

	public void Clean()
	{
		this._terrains = null;
	}

	public const float Contrast = 2.2f;

	public const float Brightness = 0.2f;

	public const float PartOfMapToSkipRendering = 0.2f;

	private FlowmapGenerator generator;

	private RawImage map;

	private Terrain[] _terrains;

	private RenderTexture rt;

	private Transform cameraTransform;

	private Camera camera;

	private Shader waterMapShader;

	private float minDepthStep = 2f;

	private float maxDepthStep = 0.5f;

	private float _isobarDepthStep = 0.5f;

	private float minZoom = 300f;

	private float maxZoom = 40f;

	private float targetSize;

	private Vector3 initialOffset;

	private float initialSize;

	private float[] pixelError;

	private float[] grassDistance;

	private LayerMask simplifiedLayer;

	private LayerMask terrainLayer;

	private Vector3 xz = new Vector3(1f, 0f, 1f);

	private bool moved;

	private bool zoomed;

	private bool rotated;

	private bool isPriorityMoving;

	private Vector3 targetPosition;

	private Quaternion targetRotation;

	private float zoomSpeed = 90f;

	private Vector3 lastRenderPosition;

	private Vector3 lastRenderOffset;

	private float lastRenderSize;

	private float lastRenderSizeRatio;
}
