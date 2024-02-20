using System;
using mset;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SkyNew : MonoBehaviour
{
	public Texture SpecularCube
	{
		get
		{
			return this.specularCube;
		}
		set
		{
			this.specularCube = value;
		}
	}

	public Texture SkyboxCube
	{
		get
		{
			return this.skyboxCube;
		}
		set
		{
			this.skyboxCube = value;
		}
	}

	public Bounds Dimensions
	{
		get
		{
			return this.dimensions;
		}
		set
		{
			this._dirty = true;
			this.dimensions = value;
		}
	}

	public bool Dirty
	{
		get
		{
			return this._dirty;
		}
		set
		{
			this._dirty = value;
		}
	}

	public float MasterIntensity
	{
		get
		{
			return this.masterIntensity;
		}
		set
		{
			this._dirty = true;
			this.masterIntensity = value;
		}
	}

	public float SkyIntensity
	{
		get
		{
			return this.skyIntensity;
		}
		set
		{
			this._dirty = true;
			this.skyIntensity = value;
		}
	}

	public float SpecIntensity
	{
		get
		{
			return this.specIntensity;
		}
		set
		{
			this._dirty = true;
			this.specIntensity = value;
		}
	}

	public float DiffIntensity
	{
		get
		{
			return this.diffIntensity;
		}
		set
		{
			this._dirty = true;
			this.diffIntensity = value;
		}
	}

	public float CamExposure
	{
		get
		{
			return this.camExposure;
		}
		set
		{
			this._dirty = true;
			this.camExposure = value;
		}
	}

	public float SpecIntensityLM
	{
		get
		{
			return this.specIntensityLM;
		}
		set
		{
			this._dirty = true;
			this.specIntensityLM = value;
		}
	}

	public float DiffIntensityLM
	{
		get
		{
			return this.diffIntensityLM;
		}
		set
		{
			this._dirty = true;
			this.diffIntensityLM = value;
		}
	}

	public bool HDRSky
	{
		get
		{
			return this.hdrSky;
		}
		set
		{
			this._dirty = true;
			this.hdrSky = value;
		}
	}

	public bool HDRSpec
	{
		get
		{
			return this.hdrSpec;
		}
		set
		{
			this._dirty = true;
			this.hdrSpec = value;
		}
	}

	public bool LinearSpace
	{
		get
		{
			return this.linearSpace;
		}
		set
		{
			this._dirty = true;
			this.linearSpace = value;
		}
	}

	public bool AutoDetectColorSpace
	{
		get
		{
			return this.autoDetectColorSpace;
		}
		set
		{
			this._dirty = true;
			this.autoDetectColorSpace = value;
		}
	}

	public bool HasDimensions
	{
		get
		{
			return this.hasDimensions;
		}
		set
		{
			this._dirty = true;
			this.hasDimensions = value;
		}
	}

	private Cubemap blackCube
	{
		get
		{
			if (this._blackCube == null)
			{
				this._blackCube = Resources.Load<Cubemap>("blackCube");
			}
			return this._blackCube;
		}
	}

	private Material SkyboxMaterial
	{
		get
		{
			if (this._SkyboxMaterial == null)
			{
				this._SkyboxMaterial = Resources.Load<Material>("skyboxMat");
			}
			return this._SkyboxMaterial;
		}
	}

	private static Material[] getTargetMaterials(Renderer target)
	{
		SkyAnchor component = target.gameObject.GetComponent<SkyAnchor>();
		if (component != null)
		{
			return component.materials;
		}
		return target.sharedMaterials;
	}

	public void Apply()
	{
		this.Apply(0);
	}

	public void Apply(int blendIndex)
	{
		ShaderIDs shaderIDs = this.blendIDs[blendIndex];
		this.ApplyGlobally(shaderIDs);
	}

	public void Apply(Renderer target)
	{
		this.Apply(target, 0);
	}

	public void Apply(Renderer target, int blendIndex)
	{
		if (target && base.enabled && base.gameObject.activeInHierarchy)
		{
			this.ApplyFast(target, blendIndex);
		}
	}

	public void ApplyFast(Renderer target, int blendIndex)
	{
		foreach (Material material in target.sharedMaterials)
		{
			this.Apply(material, blendIndex);
		}
	}

	public void Apply(Material target)
	{
		this.Apply(target, 0);
	}

	public void Apply(Material target, int blendIndex)
	{
		if (target && base.enabled && base.gameObject.activeInHierarchy)
		{
			this.ApplyToMaterial(target, this.blendIDs[blendIndex]);
		}
	}

	private void ApplyToBlock(ref MaterialPropertyBlock block, ShaderIDs bids)
	{
	}

	private void ApplyToMaterial(Material mat, ShaderIDs bids)
	{
		mat.SetVector(bids.exposureIBL, this.exposures);
		mat.SetVector(bids.exposureLM, this.exposuresLM);
		mat.SetMatrix(bids.skyMatrix, this.skyMatrix);
		mat.SetMatrix(bids.invSkyMatrix, this.invMatrix);
		mat.SetVector(bids.skyMin, this.skyMin);
		mat.SetVector(bids.skyMax, this.skyMax);
		if (this.specularCube)
		{
			mat.SetTexture(bids.specCubeIBL, this.specularCube);
		}
		else
		{
			mat.SetTexture(bids.specCubeIBL, this.blackCube);
		}
		if (this.skyboxCube)
		{
			mat.SetTexture(bids.skyCubeIBL, this.skyboxCube);
		}
		for (int i = 0; i < 9; i++)
		{
			mat.SetVector(bids.SH[i], this.SH.cBuffer[i]);
		}
	}

	private void ApplySkyTransform(ShaderIDs bids)
	{
		Shader.SetGlobalMatrix(bids.skyMatrix, this.skyMatrix);
		Shader.SetGlobalMatrix(bids.invSkyMatrix, this.invMatrix);
		Shader.SetGlobalVector(bids.skyMin, this.skyMin);
		Shader.SetGlobalVector(bids.skyMax, this.skyMax);
	}

	private void ApplyGlobally(ShaderIDs bids)
	{
		Shader.SetGlobalMatrix(bids.skyMatrix, this.skyMatrix);
		Shader.SetGlobalMatrix(bids.invSkyMatrix, this.invMatrix);
		Shader.SetGlobalVector(bids.skyMin, this.skyMin);
		Shader.SetGlobalVector(bids.skyMax, this.skyMax);
		Shader.SetGlobalVector(bids.exposureIBL, this.exposures);
		Shader.SetGlobalVector(bids.exposureLM, this.exposuresLM);
		Shader.SetGlobalFloat("_EmissionLM", 1f);
		Shader.SetGlobalVector("_UniformOcclusion", Vector4.one);
		if (this.specularCube)
		{
			Shader.SetGlobalTexture(bids.specCubeIBL, this.specularCube);
		}
		else
		{
			Shader.SetGlobalTexture(bids.specCubeIBL, this.blackCube);
		}
		if (this.skyboxCube)
		{
			Shader.SetGlobalTexture(bids.skyCubeIBL, this.skyboxCube);
		}
		for (int i = 0; i < 9; i++)
		{
			Shader.SetGlobalVector(bids.SH[i], this.SH.cBuffer[i]);
		}
	}

	public static void ScrubGlobalKeywords()
	{
		Shader.DisableKeyword("MARMO_SKY_BLEND_ON");
		Shader.DisableKeyword("MARMO_SKY_BLEND_OFF");
		Shader.DisableKeyword("MARMO_BOX_PROJECTION_ON");
		Shader.DisableKeyword("MARMO_BOX_PROJECTION_OFF");
		Shader.DisableKeyword("MARMO_TERRAIN_BLEND_ON");
		Shader.DisableKeyword("MARMO_TERRAIN_BLEND_OFF");
	}

	public static void ScrubKeywords(Material[] materials)
	{
		foreach (Material material in materials)
		{
			material.DisableKeyword("MARMO_SKY_BLEND_ON");
			material.DisableKeyword("MARMO_SKY_BLEND_OFF");
			material.DisableKeyword("MARMO_BOX_PROJECTION_ON");
			material.DisableKeyword("MARMO_BOX_PROJECTION_OFF");
			material.DisableKeyword("MARMO_TERRAIN_BLEND_ON");
			material.DisableKeyword("MARMO_TERRAIN_BLEND_OFF");
		}
	}

	public static void EnableProjectionSupport(bool enable)
	{
		if (enable)
		{
			Shader.DisableKeyword("MARMO_BOX_PROJECTION_OFF");
		}
		else
		{
			Shader.EnableKeyword("MARMO_BOX_PROJECTION_OFF");
		}
		SkyNew.internalProjectionSupport = enable;
	}

	public static void EnableGlobalProjection(bool enable)
	{
		if (!SkyNew.internalProjectionSupport)
		{
			return;
		}
		if (enable)
		{
			Shader.EnableKeyword("MARMO_BOX_PROJECTION_ON");
		}
		else
		{
			Shader.DisableKeyword("MARMO_BOX_PROJECTION_ON");
		}
	}

	public static void EnableProjection(Renderer target, Material[] mats, bool enable)
	{
		if (!SkyNew.internalProjectionSupport)
		{
			return;
		}
		if (mats == null)
		{
			return;
		}
		if (enable)
		{
			foreach (Material material in mats)
			{
				if (material)
				{
					material.EnableKeyword("MARMO_BOX_PROJECTION_ON");
					material.DisableKeyword("MARMO_BOX_PROJECTION_OFF");
				}
			}
		}
		else
		{
			foreach (Material material2 in mats)
			{
				if (material2)
				{
					material2.DisableKeyword("MARMO_BOX_PROJECTION_ON");
					material2.EnableKeyword("MARMO_BOX_PROJECTION_OFF");
				}
			}
		}
	}

	public static void EnableProjection(Material mat, bool enable)
	{
		if (!SkyNew.internalProjectionSupport)
		{
			return;
		}
		if (enable)
		{
			mat.EnableKeyword("MARMO_BOX_PROJECTION_ON");
			mat.DisableKeyword("MARMO_BOX_PROJECTION_OFF");
		}
		else
		{
			mat.DisableKeyword("MARMO_BOX_PROJECTION_ON");
			mat.EnableKeyword("MARMO_BOX_PROJECTION_OFF");
		}
	}

	public static void EnableBlendingSupport(bool enable)
	{
		if (enable)
		{
			Shader.DisableKeyword("MARMO_SKY_BLEND_OFF");
		}
		else
		{
			Shader.EnableKeyword("MARMO_SKY_BLEND_OFF");
		}
		SkyNew.internalBlendingSupport = enable;
	}

	public static void EnableTerrainBlending(bool enable)
	{
		if (!SkyNew.internalBlendingSupport)
		{
			return;
		}
		if (enable)
		{
			Shader.EnableKeyword("MARMO_TERRAIN_BLEND_ON");
			Shader.DisableKeyword("MARMO_TERRAIN_BLEND_OFF");
		}
		else
		{
			Shader.DisableKeyword("MARMO_TERRAIN_BLEND_ON");
			Shader.EnableKeyword("MARMO_TERRAIN_BLEND_OFF");
		}
	}

	public static void EnableGlobalBlending(bool enable)
	{
		if (!SkyNew.internalBlendingSupport)
		{
			return;
		}
		if (enable)
		{
			Shader.EnableKeyword("MARMO_SKY_BLEND_ON");
		}
		else
		{
			Shader.DisableKeyword("MARMO_SKY_BLEND_ON");
		}
	}

	public static void EnableBlending(Renderer target, Material[] mats, bool enable)
	{
		if (!SkyNew.internalBlendingSupport)
		{
			return;
		}
		if (mats == null)
		{
			return;
		}
		if (enable)
		{
			foreach (Material material in mats)
			{
				if (material)
				{
					material.EnableKeyword("MARMO_SKY_BLEND_ON");
					material.DisableKeyword("MARMO_SKY_BLEND_OFF");
				}
			}
		}
		else
		{
			foreach (Material material2 in mats)
			{
				if (material2)
				{
					material2.DisableKeyword("MARMO_SKY_BLEND_ON");
					material2.EnableKeyword("MARMO_SKY_BLEND_OFF");
				}
			}
		}
	}

	public static void EnableBlending(Material mat, bool enable)
	{
		if (!SkyNew.internalBlendingSupport)
		{
			return;
		}
		if (enable)
		{
			mat.EnableKeyword("MARMO_SKY_BLEND_ON");
			mat.DisableKeyword("MARMO_SKY_BLEND_OFF");
		}
		else
		{
			mat.DisableKeyword("MARMO_SKY_BLEND_ON");
			mat.EnableKeyword("MARMO_SKY_BLEND_OFF");
		}
	}

	public static void SetBlendWeight(float weight)
	{
		Shader.SetGlobalFloat("_BlendWeightIBL", weight);
	}

	public static void SetBlendWeight(Renderer target, float weight)
	{
		Material[] targetMaterials = SkyNew.getTargetMaterials(target);
		foreach (Material material in targetMaterials)
		{
			material.SetFloat("_BlendWeightIBL", weight);
		}
	}

	public static void SetBlendWeight(Material mat, float weight)
	{
		mat.SetFloat("_BlendWeightIBL", weight);
	}

	public static void SetUniformOcclusion(Renderer target, float diffuse, float specular)
	{
		if (target != null)
		{
			Vector4 one = Vector4.one;
			one.x = diffuse;
			one.y = specular;
			Material[] targetMaterials = SkyNew.getTargetMaterials(target);
			foreach (Material material in targetMaterials)
			{
				material.SetVector("_UniformOcclusion", one);
			}
		}
	}

	public void SetCustomExposure(float diffInt, float specInt, float skyInt, float camExpo)
	{
		this.SetCustomExposure(null, diffInt, specInt, skyInt, camExpo);
	}

	public void SetCustomExposure(Renderer target, float diffInt, float specInt, float skyInt, float camExpo)
	{
		Vector4 one = Vector4.one;
		this.ComputeExposureVector(ref one, diffInt, specInt, skyInt, camExpo);
		if (target == null)
		{
			Shader.SetGlobalVector(this.blendIDs[0].exposureIBL, one);
		}
		else
		{
			Material[] targetMaterials = SkyNew.getTargetMaterials(target);
			foreach (Material material in targetMaterials)
			{
				material.SetVector(this.blendIDs[0].exposureIBL, one);
			}
		}
	}

	public void ToggleChildLights(bool enable)
	{
		Light[] componentsInChildren = base.GetComponentsInChildren<Light>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = enable;
		}
	}

	private void UpdateSkySize()
	{
		if (this.HasDimensions)
		{
			this.skyMin = this.Dimensions.center - this.Dimensions.extents;
			this.skyMax = this.Dimensions.center + this.Dimensions.extents;
			Vector3 localScale = base.transform.localScale;
			this.skyMin.x = this.skyMin.x * localScale.x;
			this.skyMin.y = this.skyMin.y * localScale.y;
			this.skyMin.z = this.skyMin.z * localScale.z;
			this.skyMax.x = this.skyMax.x * localScale.x;
			this.skyMax.y = this.skyMax.y * localScale.y;
			this.skyMax.z = this.skyMax.z * localScale.z;
		}
		else
		{
			this.skyMax = Vector4.one * 100000f;
			this.skyMin = Vector4.one * -100000f;
		}
	}

	private void UpdateSkyTransform()
	{
		this.skyMatrix.SetTRS(base.transform.position, base.transform.rotation, Vector3.one);
		this.invMatrix = this.skyMatrix.inverse;
	}

	private void ComputeExposureVector(ref Vector4 result, float diffInt, float specInt, float skyInt, float camExpo)
	{
		result.x = this.masterIntensity * diffInt;
		result.y = this.masterIntensity * specInt;
		result.z = this.masterIntensity * skyInt * camExpo;
		result.w = camExpo;
		float num = 6f;
		if (this.linearSpace)
		{
			num = Mathf.Pow(num, 2.2f);
		}
		if (!this.hdrSpec)
		{
			result.y /= num;
		}
		if (!this.hdrSky)
		{
			result.z /= num;
		}
	}

	private void UpdateExposures()
	{
		this.ComputeExposureVector(ref this.exposures, this.diffIntensity, this.specIntensity, this.skyIntensity, this.camExposure);
		this.exposuresLM.x = this.diffIntensityLM;
		this.exposuresLM.y = this.specIntensityLM;
	}

	private void UpdatePropertyIDs()
	{
		this.blendIDs[0].Link();
		this.blendIDs[1].Link("1");
	}

	public void Awake()
	{
		this.UpdatePropertyIDs();
	}

	private void Reset()
	{
		this.skyMatrix = (this.invMatrix = Matrix4x4.identity);
		this.exposures = Vector4.one;
		this.exposuresLM = Vector4.one;
		this.specularCube = (this.skyboxCube = null);
		this.masterIntensity = (this.skyIntensity = (this.specIntensity = (this.diffIntensity = 1f)));
		this.hdrSky = (this.hdrSpec = false);
	}

	private void OnEnable()
	{
		if (this.SH == null)
		{
			this.SH = new SHEncoding();
		}
		if (this.CustomSH != null)
		{
			this.SH.copyFrom(this.CustomSH.SH);
		}
		this.SH.copyToBuffer();
	}

	private void OnLevelWasLoadedNew(Scene scene, LoadSceneMode mode)
	{
		this.UpdateExposures();
		this.UpdateSkyTransform();
		this.UpdateSkySize();
	}

	private void Start()
	{
		this.UpdateExposures();
		this.UpdateSkyTransform();
		this.UpdateSkySize();
		SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.OnLevelWasLoadedNew);
	}

	private void Update()
	{
		if (base.transform.hasChanged)
		{
			this.Dirty = true;
			this.UpdateSkyTransform();
			this.UpdateSkySize();
			base.transform.hasChanged = false;
		}
		this.UpdateExposures();
	}

	private void OnDestroy()
	{
		this.SH = null;
		this._blackCube = null;
		this.specularCube = null;
		this.skyboxCube = null;
		SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(this.OnLevelWasLoadedNew);
	}

	public void DrawProjectionCube(Vector3 center, Vector3 radius)
	{
		if (this.projMaterial == null)
		{
			this.projMaterial = Resources.Load<Material>("projectionMat");
			if (!this.projMaterial)
			{
				Debug.LogError("Failed to find projectionMat material in Resources folder!");
			}
		}
		Vector4 vector = Vector4.one;
		vector.z = this.CamExposure;
		vector *= this.masterIntensity;
		ShaderIDs shaderIDs = this.blendIDs[0];
		this.projMaterial.color = new Color(0.7f, 0.7f, 0.7f, 1f);
		this.projMaterial.SetVector(shaderIDs.skyMin, -this.Dimensions.extents);
		this.projMaterial.SetVector(shaderIDs.skyMax, this.Dimensions.extents);
		this.projMaterial.SetVector(shaderIDs.exposureIBL, vector);
		this.projMaterial.SetTexture(shaderIDs.skyCubeIBL, this.specularCube);
		this.projMaterial.SetMatrix(shaderIDs.skyMatrix, this.skyMatrix);
		this.projMaterial.SetMatrix(shaderIDs.invSkyMatrix, this.invMatrix);
		this.projMaterial.SetPass(0);
		GL.PushMatrix();
		GL.MultMatrix(base.transform.localToWorldMatrix);
		GLUtil.DrawCube(center, -radius);
		GL.End();
		GL.PopMatrix();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<Renderer>())
		{
			this.Apply(other.GetComponent<Renderer>(), 0);
		}
	}

	private void OnPostRender()
	{
	}

	[SerializeField]
	private Texture specularCube;

	[SerializeField]
	private Texture skyboxCube;

	public bool IsProbe;

	[SerializeField]
	private Bounds dimensions = new Bounds(Vector3.zero, Vector3.one);

	private bool _dirty;

	[SerializeField]
	private float masterIntensity = 1f;

	[SerializeField]
	private float skyIntensity = 1f;

	[SerializeField]
	private float specIntensity = 1f;

	[SerializeField]
	private float diffIntensity = 1f;

	[SerializeField]
	private float camExposure = 1f;

	[SerializeField]
	private float specIntensityLM = 1f;

	[SerializeField]
	private float diffIntensityLM = 1f;

	[SerializeField]
	private bool hdrSky = true;

	[SerializeField]
	private bool hdrSpec = true;

	[SerializeField]
	private bool linearSpace = true;

	[SerializeField]
	private bool autoDetectColorSpace = true;

	[SerializeField]
	private bool hasDimensions;

	public SHEncoding SH = new SHEncoding();

	public SHEncodingFile CustomSH;

	private Matrix4x4 skyMatrix = Matrix4x4.identity;

	private Matrix4x4 invMatrix = Matrix4x4.identity;

	private Vector4 exposures = Vector4.one;

	private Vector4 exposuresLM = Vector4.one;

	private Vector4 skyMin = -Vector4.one;

	private Vector4 skyMax = Vector4.one;

	private ShaderIDs[] blendIDs = new ShaderIDs[]
	{
		new ShaderIDs(),
		new ShaderIDs()
	};

	[SerializeField]
	private Cubemap _blackCube;

	[SerializeField]
	private Material _SkyboxMaterial;

	private static bool internalProjectionSupport;

	private static bool internalBlendingSupport;

	private Material projMaterial;
}
