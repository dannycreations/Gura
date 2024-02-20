using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(FishWaterBase))]
public class FishPlanarReflection : MonoBehaviour
{
	public void Awake()
	{
		this.waterBase = (FishWaterBase)base.gameObject.GetComponent(typeof(FishWaterBase));
		if (this.waterBase != null)
		{
			this.sharedMaterial = this.waterBase.sharedMaterial;
		}
	}

	public void Start()
	{
		this.waterBase = (FishWaterBase)base.gameObject.GetComponent(typeof(FishWaterBase));
		if (this.waterBase != null)
		{
			this.sharedMaterial = this.waterBase.sharedMaterial;
		}
	}

	private Camera CreateReflectionCameraFor(Camera cam)
	{
		string text = base.gameObject.name + "Reflection" + cam.name;
		GameObject gameObject = GameObject.Find(text);
		if (!gameObject)
		{
			gameObject = new GameObject(text, new Type[] { typeof(Camera) });
		}
		if (!gameObject.GetComponent(typeof(Camera)))
		{
			gameObject.AddComponent(typeof(Camera));
		}
		Camera component = gameObject.GetComponent<Camera>();
		component.backgroundColor = this.clearColor;
		component.clearFlags = ((!this.reflectSkybox) ? 2 : 1);
		this.SetStandardCameraParameter(component, this.reflectionMask);
		if (!component.targetTexture)
		{
			component.targetTexture = this.CreateTextureFor(cam);
		}
		return component;
	}

	private void SetStandardCameraParameter(Camera cam, LayerMask mask)
	{
		cam.cullingMask = mask & ~(1 << LayerMask.NameToLayer("Water")) & ~(1 << LayerMask.NameToLayer("Unimportant")) & ~(1 << LayerMask.NameToLayer("SimplifiedTerrain")) & ~(1 << LayerMask.NameToLayer("Unimportant")) & ~(1 << LayerMask.NameToLayer("RangedVisible"));
		cam.backgroundColor = Color.black;
		cam.depthTextureMode = 1;
		cam.farClipPlane = 500f;
		cam.nearClipPlane = 0.3f;
		FishFog fishFog = cam.GetComponent<FishFog>();
		if (fishFog == null)
		{
			fishFog = cam.gameObject.AddComponent<FishFog>();
		}
		fishFog.heightFog = true;
		fishFog.heightDensity = 5f;
		fishFog.height = 10f;
		fishFog.enabled = true;
		cam.enabled = false;
		cam.useOcclusionCulling = false;
	}

	private RenderTexture CreateTextureFor(Camera cam)
	{
		RenderTexture renderTexture;
		if (this.waterBase != null && this.waterBase.dynWaterQuality == FishDynWaterQuality.High)
		{
			renderTexture = new RenderTexture(Mathf.FloorToInt((float)cam.pixelWidth), Mathf.FloorToInt((float)cam.pixelHeight), 16);
		}
		else
		{
			renderTexture = new RenderTexture(Mathf.FloorToInt((float)cam.pixelWidth * 0.5f), Mathf.FloorToInt((float)cam.pixelHeight * 0.5f), 16);
		}
		renderTexture.hideFlags = 52;
		renderTexture.filterMode = 0;
		renderTexture.wrapMode = 1;
		renderTexture.useMipMap = false;
		return renderTexture;
	}

	public void RenderHelpCameras(Camera currentCam)
	{
		if (this.helperCameras == null)
		{
			this.helperCameras = new Dictionary<Camera, bool>();
		}
		if (!this.helperCameras.ContainsKey(currentCam))
		{
			this.helperCameras.Add(currentCam, false);
		}
		if (this.helperCameras[currentCam])
		{
			return;
		}
		if (!this.reflectionCamera)
		{
			this.reflectionCamera = this.CreateReflectionCameraFor(currentCam);
		}
		this.RenderReflectionFor(currentCam, this.reflectionCamera);
		this.helperCameras[currentCam] = true;
	}

	public void LateUpdate()
	{
		if (this.helperCameras != null)
		{
			this.helperCameras.Clear();
		}
	}

	public void WaterTileBeingRendered(Transform tr, Camera currentCam)
	{
		if (currentCam.gameObject.tag != "MainCamera")
		{
			return;
		}
		this.RenderHelpCameras(currentCam);
		if (this.reflectionCamera && this.sharedMaterial)
		{
			this.sharedMaterial.SetTexture(this.reflectionSampler, this.reflectionCamera.targetTexture);
		}
	}

	public void OnEnable()
	{
		Shader.EnableKeyword("WATER_REFLECTIVE");
		Shader.DisableKeyword("WATER_SIMPLE");
	}

	public void OnDisable()
	{
		Shader.EnableKeyword("WATER_SIMPLE");
		Shader.DisableKeyword("WATER_REFLECTIVE");
	}

	private void RenderReflectionFor(Camera cam, Camera reflectCamera)
	{
		if (!reflectCamera)
		{
			return;
		}
		if (this.sharedMaterial && !this.sharedMaterial.HasProperty(this.reflectionSampler))
		{
			return;
		}
		reflectCamera.cullingMask = this.reflectionMask & ~((1 << LayerMask.NameToLayer("Water")) | (1 << LayerMask.NameToLayer("Unimportant")) | (1 << LayerMask.NameToLayer("RangedVisible")) | (1 << LayerMask.NameToLayer("SimplifiedTerrain")));
		reflectCamera.fieldOfView = cam.fieldOfView;
		this.SaneCameraSettings(reflectCamera);
		reflectCamera.backgroundColor = this.clearColor;
		reflectCamera.clearFlags = ((!this.reflectSkybox) ? 2 : 1);
		if (this.reflectSkybox && cam.gameObject.GetComponent(typeof(Skybox)))
		{
			Skybox skybox = (Skybox)reflectCamera.gameObject.GetComponent(typeof(Skybox));
			if (!skybox)
			{
				skybox = (Skybox)reflectCamera.gameObject.AddComponent(typeof(Skybox));
			}
			skybox.material = ((Skybox)cam.GetComponent(typeof(Skybox))).material;
		}
		GL.SetRevertBackfacing(true);
		Transform transform = base.transform;
		Vector3 eulerAngles = cam.transform.eulerAngles;
		reflectCamera.transform.eulerAngles = new Vector3(-eulerAngles.x, eulerAngles.y, eulerAngles.z);
		reflectCamera.transform.position = cam.transform.position;
		Vector3 position = transform.transform.position;
		position.y = transform.position.y;
		Vector3 up = transform.transform.up;
		float num = -Vector3.Dot(up, position) - this.clipPlaneOffset;
		Vector4 vector;
		vector..ctor(up.x, up.y, up.z, num);
		Matrix4x4 matrix4x = Matrix4x4.zero;
		matrix4x = FishPlanarReflection.CalculateReflectionMatrix(matrix4x, vector);
		this.oldpos = cam.transform.position;
		Vector3 vector2 = matrix4x.MultiplyPoint(this.oldpos);
		reflectCamera.worldToCameraMatrix = cam.worldToCameraMatrix * matrix4x;
		Vector4 vector3 = this.CameraSpacePlane(reflectCamera, position, up, 1f);
		Matrix4x4 matrix4x2 = cam.projectionMatrix;
		matrix4x2 = FishPlanarReflection.CalculateObliqueMatrix(matrix4x2, vector3);
		reflectCamera.projectionMatrix = matrix4x2;
		reflectCamera.transform.position = vector2;
		Vector3 eulerAngles2 = cam.transform.eulerAngles;
		reflectCamera.transform.eulerAngles = new Vector3(-eulerAngles2.x, eulerAngles2.y, eulerAngles2.z);
		LightShadows lightShadows = 0;
		float num2 = 0f;
		float shadowDistance = QualitySettings.shadowDistance;
		QualitySettings.shadowDistance = 0f;
		if (this.lightSource != null)
		{
			lightShadows = this.lightSource.shadows;
			this.lightSource.shadows = 0;
			num2 = QualitySettings.lodBias;
			if (this.waterBase != null && this.waterBase.dynWaterQuality == FishDynWaterQuality.High)
			{
				QualitySettings.lodBias /= 2f;
			}
			else
			{
				QualitySettings.lodBias /= 2f;
			}
		}
		reflectCamera.layerCullDistances = cam.layerCullDistances;
		reflectCamera.layerCullSpherical = cam.layerCullSpherical;
		Shader.SetGlobalFloat("_ReflectionDamper", 1f);
		Shader.SetGlobalMatrix("_Camera2World", reflectCamera.cameraToWorldMatrix);
		reflectCamera.Render();
		Shader.SetGlobalMatrix("_Camera2World", GlobalConsts.Camera2World);
		Shader.SetGlobalFloat("_ReflectionDamper", 0f);
		if (this.lightSource != null)
		{
			this.lightSource.shadows = lightShadows;
			QualitySettings.lodBias = num2;
		}
		QualitySettings.shadowDistance = shadowDistance;
		GL.SetRevertBackfacing(false);
	}

	private void SaneCameraSettings(Camera helperCam)
	{
		helperCam.depthTextureMode = 1;
		helperCam.backgroundColor = Color.black;
		helperCam.clearFlags = 2;
		helperCam.useOcclusionCulling = false;
		helperCam.farClipPlane = 500f;
		helperCam.nearClipPlane = 0.3f;
		FishFog fishFog = helperCam.GetComponent<FishFog>();
		if (fishFog == null)
		{
			fishFog = helperCam.gameObject.AddComponent<FishFog>();
		}
		fishFog.heightFog = true;
		fishFog.heightDensity = 5f;
		fishFog.height = 10f;
		fishFog.enabled = true;
	}

	private static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane)
	{
		Vector4 vector = projection.inverse * new Vector4(FishPlanarReflection.sgn(clipPlane.x), FishPlanarReflection.sgn(clipPlane.y), 1f, 1f);
		Vector4 vector2 = clipPlane * (2f / Vector4.Dot(clipPlane, vector));
		projection[2] = vector2.x - projection[3];
		projection[6] = vector2.y - projection[7];
		projection[10] = vector2.z - projection[11];
		projection[14] = vector2.w - projection[15];
		return projection;
	}

	private static Matrix4x4 CalculateReflectionMatrix(Matrix4x4 reflectionMat, Vector4 plane)
	{
		reflectionMat.m00 = 1f - 2f * plane[0] * plane[0];
		reflectionMat.m01 = -2f * plane[0] * plane[1];
		reflectionMat.m02 = -2f * plane[0] * plane[2];
		reflectionMat.m03 = -2f * plane[3] * plane[0];
		reflectionMat.m10 = -2f * plane[1] * plane[0];
		reflectionMat.m11 = 1f - 2f * plane[1] * plane[1];
		reflectionMat.m12 = -2f * plane[1] * plane[2];
		reflectionMat.m13 = -2f * plane[3] * plane[1];
		reflectionMat.m20 = -2f * plane[2] * plane[0];
		reflectionMat.m21 = -2f * plane[2] * plane[1];
		reflectionMat.m22 = 1f - 2f * plane[2] * plane[2];
		reflectionMat.m23 = -2f * plane[3] * plane[2];
		reflectionMat.m30 = 0f;
		reflectionMat.m31 = 0f;
		reflectionMat.m32 = 0f;
		reflectionMat.m33 = 1f;
		return reflectionMat;
	}

	private static float sgn(float a)
	{
		if (a > 0f)
		{
			return 1f;
		}
		if (a < 0f)
		{
			return -1f;
		}
		return 0f;
	}

	private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Vector3 vector = pos + normal * this.clipPlaneOffset;
		Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
		Vector3 vector2 = worldToCameraMatrix.MultiplyPoint(vector);
		Vector3 vector3 = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
		return new Vector4(vector3.x, vector3.y, vector3.z, -Vector3.Dot(vector2, vector3));
	}

	public LayerMask reflectionMask;

	public bool reflectSkybox;

	public Color clearColor = Color.grey;

	public string reflectionSampler = "_ReflectionTex";

	public Light lightSource;

	public float clipPlaneOffset = 0.07f;

	private Vector3 oldpos = Vector3.zero;

	private Camera reflectionCamera;

	private Material sharedMaterial;

	private Dictionary<Camera, bool> helperCameras;

	private bool oddFrameMask = true;

	private FishWaterBase waterBase;
}
