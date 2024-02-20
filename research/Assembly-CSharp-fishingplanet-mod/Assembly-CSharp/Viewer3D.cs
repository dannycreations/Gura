using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.UI._2D.Helpers;
using Boats;
using InControl;
using Phy;
using UnityEngine;
using UnityEngine.UI;

public class Viewer3D : MonoBehaviour
{
	private void Awake()
	{
		this.Info.resizeTextMinSize = 17;
	}

	public void SetModel(GameObject model, Bounds modelBounds, Viewer3DArguments.ViewArgs viewArgs, ModelInfo info, Vector3 meshOffset = default(Vector3))
	{
		this.SetModel(model, modelBounds, viewArgs.desiredScale, viewArgs.rotationAxis, info, false, meshOffset);
		if (viewArgs.setPosition)
		{
			model.transform.localPosition = viewArgs.position;
		}
		this._viewArgs = viewArgs;
	}

	public void SetModel(GameObject model, Bounds modelBounds, float desiredScale, Vector3 angles, ModelInfo info, bool bendFishMesh = false, Vector3 meshOffset = default(Vector3))
	{
		for (int i = 0; i < this.UI.Length; i++)
		{
			this.UI[i].SetActive(true);
		}
		foreach (Transform transform in model.GetComponentsInChildren<Transform>(true))
		{
			transform.gameObject.layer = 13;
		}
		this.parent.localRotation = Quaternion.identity;
		if (this._loadedModel != null)
		{
			Object.Destroy(this._loadedModel);
		}
		this._loadedModel = model;
		model.transform.SetParent(this.parent);
		this._viewRoot = model.transform;
		this.SetScaleAndPositon(model, modelBounds, desiredScale, angles, meshOffset);
		this.SetMaterials(model, info.Materials);
		this.SetInfo(info);
		this._viewArgs = new Viewer3DArguments.ViewArgs
		{
			scaleFactorMin = 0.2f,
			scaleFactorMax = 6f,
			rotateAroundRoot = true
		};
		this.modelMeshRenderer = model.GetComponentInChildren<SkinnedMeshRenderer>();
		this.BendFishMesh = bendFishMesh;
		if (this.BendFishMesh)
		{
			this.InitFishBend();
		}
	}

	private void SetMaterials(GameObject model, string[] materials)
	{
		TPMBoatSettings component = model.GetComponent<TPMBoatSettings>();
		if (materials != null && materials.Length == component.Renderers.Length)
		{
			for (int i = 0; i < component.Renderers.Length; i++)
			{
				Material material = Resources.Load<Material>(materials[i]);
				component.Renderers[i].material = material;
			}
			component.Oar.transform.localPosition += new Vector3(0f, 0.6f, 0f);
		}
	}

	private void SetInfo(ModelInfo info)
	{
		if (info == null)
		{
			return;
		}
		this.Title.text = info.Title;
		this.Info.text = info.Info;
		List<ListOfCompatibility.ConstraintType> list = null;
		if (ListOfCompatibility.ItemsConstraints.TryGetValue(info.ItemSubType, out list))
		{
			for (int i = 0; i < this.Constraints.Length; i++)
			{
				this.Constraints[i].gameObject.SetActive(true);
				if (list.Contains((ListOfCompatibility.ConstraintType)i))
				{
					this.Constraints[i].color = this.greenColor;
					this.Constraints[i].transform.GetComponentInChildren<Text>().color = this.textEnabledColor;
				}
				else
				{
					this.Constraints[i].color = this.grayColor;
					this.Constraints[i].transform.GetComponentInChildren<Text>().color = this.textDisabledColor;
				}
			}
		}
		if (info.Technologies != null)
		{
			for (int j = 0; j < info.Technologies.Length; j++)
			{
				Transform transform = this.TechologiesPanel.transform.Find("TechnologyImage" + j);
				if (transform != null)
				{
					if (this.images.Count <= j)
					{
						this.images.Add(new ResourcesHelpers.AsyncLoadableImage());
					}
					this.images[j].Image = transform.GetComponent<Image>();
					this.images[j].Load(string.Format("Textures/Inventory/{0}", info.Technologies[j].LogoBID));
				}
			}
		}
		if (info.Brand != null)
		{
			this.brandImg.Image = this.BrandImage;
			this.brandImg.Load(string.Format("Textures/Inventory/{0}", info.Brand.LogoBID));
		}
	}

	private void SetScaleAndPositon(GameObject model, Bounds modelBounds, float desiredScale, Vector3 angles, Vector3 meshOffset)
	{
		Vector3 vector = modelBounds.min;
		Vector3 vector2 = modelBounds.max;
		vector = Quaternion.Euler(angles) * vector;
		vector2 = Quaternion.Euler(angles) * vector2;
		Vector3 vector3 = Vector3.up * meshOffset.y + (vector + vector2) * 0.5f;
		Vector3 vector4 = vector2 - vector;
		Vector3 vector5;
		vector5..ctor(Mathf.Abs(model.transform.localScale.x * this.viewBox.localScale.x / vector4.x), Mathf.Abs(model.transform.localScale.y * this.viewBox.localScale.y / vector4.y), Mathf.Abs(model.transform.localScale.z * this.viewBox.localScale.z / vector4.z));
		float num = ((vector5.x >= vector5.y) ? ((vector5.y >= vector5.z) ? vector5.z : vector5.y) : ((vector5.x >= vector5.z) ? vector5.z : vector5.x));
		if (desiredScale != 0f)
		{
			num = desiredScale;
		}
		model.transform.localPosition = -vector3 * num;
		model.transform.localScale = new Vector3(num, num, num);
		model.transform.Rotate(angles);
	}

	private void Update()
	{
		if (this._viewArgs == null)
		{
			return;
		}
		if (Input.GetMouseButtonDown(1))
		{
			this._prevCursor = Input.mousePosition;
		}
		else if (Input.GetMouseButton(1))
		{
			Vector3 vector = Input.mousePosition - this._prevCursor;
			Transform transform = ((!this._viewArgs.rotateAroundRoot) ? this._viewRoot : this._viewRoot.parent);
			transform.Rotate((!(this._viewArgs.rotateAroundAxis == Vector3.zero)) ? this._viewArgs.rotateAroundAxis : Vector3.up, this._rotationSpeed * -vector.x * Time.deltaTime);
			this.ClampAngle();
		}
		this._prevCursor = Input.mousePosition;
		float axis = Input.GetAxis("Mouse ScrollWheel");
		if (axis != 0f && this._viewArgs.scaleFactorMax != 0f)
		{
			float num = Mathf.Clamp(6f * axis + this._currentScale, this._viewArgs.scaleFactorMin, this._viewArgs.scaleFactorMax);
			Camera.main.transform.position += (num - this._currentScale) * Camera.main.transform.forward;
			this._currentScale = num;
		}
		if (InputManager.ActiveDevice.GetControl(InputControlType.LeftStickLeft).Value > 0f)
		{
			Transform transform2 = ((!this._viewArgs.rotateAroundRoot) ? this._viewRoot : this._viewRoot.parent);
			transform2.Rotate((!(this._viewArgs.rotateAroundAxis == Vector3.zero)) ? this._viewArgs.rotateAroundAxis : Vector3.up, this._rotationSpeed * Time.deltaTime * InputManager.ActiveDevice.GetControl(InputControlType.LeftStickLeft).Value * 5f);
			this.ClampAngle();
		}
		if (InputManager.ActiveDevice.GetControl(InputControlType.LeftStickRight).Value > 0f)
		{
			Transform transform3 = ((!this._viewArgs.rotateAroundRoot) ? this._viewRoot : this._viewRoot.parent);
			transform3.Rotate((!(this._viewArgs.rotateAroundAxis == Vector3.zero)) ? this._viewArgs.rotateAroundAxis : Vector3.up, -this._rotationSpeed * Time.deltaTime * InputManager.ActiveDevice.GetControl(InputControlType.LeftStickRight).Value * 5f);
			this.ClampAngle();
		}
	}

	private void LateUpdate()
	{
		if (this.BendFishMesh)
		{
			this.UpdateFishBend();
		}
	}

	private void ClampAngle()
	{
		if (this._viewArgs != null && this._viewArgs.angleFrom != this._viewArgs.angleTo)
		{
			float num = this._viewRoot.parent.eulerAngles.y % 360f;
			if (num < 0f)
			{
				num += 360f;
			}
			if (num < this._viewArgs.angleFrom || num > this._viewArgs.angleTo)
			{
				num = ((Mathf.Abs(this._viewArgs.angleFrom % 360f - num) >= Mathf.Abs(this._viewArgs.angleTo % 360f - num)) ? this._viewArgs.angleTo : this._viewArgs.angleFrom);
			}
			this._viewRoot.parent.eulerAngles = new Vector3(this._viewRoot.parent.eulerAngles.x, num, this._viewRoot.parent.eulerAngles.z);
		}
	}

	public void ClosePreview()
	{
		Object.Destroy(this._loadedModel);
		base.StartCoroutine(this.CloseDelayed());
	}

	private IEnumerator CloseDelayed()
	{
		yield return new WaitForEndOfFrame();
		Shader.DisableKeyword("FISH_FUR_PROCEDURAL_BEND_BYPASS");
		if (this.CloseAction != null)
		{
			this.CloseAction();
		}
		yield break;
	}

	private void InitFishBend()
	{
		if (this.modelMeshRenderer != null)
		{
			this.furDisplacementPropertyID = Shader.PropertyToID("Displacement");
			for (int i = 0; i < this.modelMeshRenderer.sharedMaterials.Length; i++)
			{
				if (this.modelMeshRenderer.sharedMaterials[i].HasProperty(this.furDisplacementPropertyID))
				{
					this.furMaterial = this.modelMeshRenderer.sharedMaterials[i];
					break;
				}
			}
			Vector3[] vertices = this.modelMeshRenderer.sharedMesh.vertices;
			this.fishMinZ = vertices[0].z;
			this.fishMaxZ = vertices[0].z;
			for (int j = 0; j < vertices.Length; j++)
			{
				if (this.fishMinZ > vertices[j].z)
				{
					this.fishMinZ = vertices[j].z;
				}
				if (this.fishMaxZ < vertices[j].z)
				{
					this.fishMaxZ = vertices[j].z;
				}
			}
			float num = 0f;
			float num2 = 1f / (float)this.bezierCurve.Order;
			for (int k = 0; k <= this.bezierCurve.Order; k++)
			{
				this.bezierCurve.AnchorPoints[k] = Vector3.forward * (this.fishMinZ + (this.fishMaxZ - this.fishMinZ) * num);
				num += num2;
			}
			Vector3 localScale = this._loadedModel.transform.localScale;
			this.bezierCurve.LateralScale = new Vector2(localScale.x / localScale.z, localScale.y / localScale.z);
			for (int l = 0; l < this.bezierCurve.RightAxis.Length; l++)
			{
				this.bezierCurve.RightAxis[l] = Vector3.right;
			}
			this.fishChord = new List<Transform>();
			Transform transform = this._viewRoot.Find("root");
			while (transform != null)
			{
				this.fishChord.Add(transform);
				transform = transform.Find("bone" + this.fishChord.Count);
			}
			transform = this.fishChord[this.fishChord.Count - 1].Find("tail_mid");
			if (transform != null)
			{
				this.fishChord.Add(transform);
				transform = this.fishChord[this.fishChord.Count - 1].Find("tail");
				if (transform != null)
				{
					this.fishChord.Add(transform);
				}
			}
			this.fishBaseChordPos = new Vector3[this.fishChord.Count];
			this.fishBaseChordRot = new Quaternion[this.fishChord.Count];
			for (int m = 0; m < this.fishChord.Count; m++)
			{
				this.fishBaseChordPos[m] = this._viewRoot.InverseTransformPoint(this.fishChord[m].position);
				this.fishBaseChordRot[m] = Quaternion.Inverse(this._viewRoot.rotation) * this.fishChord[m].rotation;
			}
		}
	}

	private void UpdateFishBend()
	{
		if (this.modelMeshRenderer != null)
		{
			if (this.furMaterial != null)
			{
				this.furMaterial.SetVector(this.furDisplacementPropertyID, new Vector3(Mathf.PerlinNoise(0f, Time.time) - 0.5f, Mathf.PerlinNoise(Time.time, 0f) - 0.5f, Mathf.PerlinNoise(Time.time, Time.time) - 0.5f) * 2f);
			}
			this.bezierCurve.AnchorPoints[0] = Vector3.right * 0.02f * 0.2f * Mathf.Sin(-Time.time * 3f) + Vector3.forward * (this.fishMinZ + (this.fishMaxZ - this.fishMinZ) * 0f);
			this.bezierCurve.AnchorPoints[1] = Vector3.right * 0.02f * 0.6f * Mathf.Sin(-Time.time * 3f + 1.2566371f) + Vector3.forward * (this.fishMinZ + (this.fishMaxZ - this.fishMinZ) * 0.2f);
			this.bezierCurve.AnchorPoints[2] = Vector3.right * 0.02f * 0.8f * Mathf.Sin(-Time.time * 3f + 2.5132742f) + Vector3.forward * (this.fishMinZ + (this.fishMaxZ - this.fishMinZ) * 0.4f);
			this.bezierCurve.AnchorPoints[3] = Vector3.right * 0.02f * 0.9f * Mathf.Sin(-Time.time * 3f + 3.7699115f) + Vector3.forward * (this.fishMinZ + (this.fishMaxZ - this.fishMinZ) * 0.6f);
			this.bezierCurve.AnchorPoints[4] = Vector3.right * 0.02f * 1f * Mathf.Sin(-Time.time * 3f + 5.0265484f) + Vector3.forward * (this.fishMinZ + (this.fishMaxZ - this.fishMinZ) * 0.8f);
			this.bezierCurve.AnchorPoints[5] = Vector3.right * 0.02f * 1f * Mathf.Sin(-Time.time * 3f + 6.2831855f) + Vector3.forward * (this.fishMinZ + (this.fishMaxZ - this.fishMinZ) * 1f);
			for (int i = 0; i < this.fishChord.Count; i++)
			{
				this.bezierCurve.SetT((this.fishBaseChordPos[i].z - this.fishMinZ) / (this.fishMaxZ - this.fishMinZ));
				this.fishChord[i].position = this._viewRoot.TransformPoint(this.bezierCurve.CurvedCylinderTransform(this.fishBaseChordPos[i]));
				this.fishChord[i].rotation = this.bezierCurve.CurvedCylinderTransformRotation(this.fishBaseChordRot[i]) * this._viewRoot.rotation;
			}
		}
	}

	public void SetLogoRetail()
	{
		for (int i = 0; i < this.UI.Length; i++)
		{
			if (this.UI[i].name == "Canvas")
			{
				Transform transform = this.UI[i].transform.Find("logo");
				if (transform != null)
				{
					Image component = transform.GetComponent<Image>();
					if (component != null)
					{
						component.overrideSprite = MessageBoxList.Instance.LogoRetail;
					}
				}
				break;
			}
		}
	}

	public Transform parent;

	public Transform viewBox;

	public Action CloseAction;

	public GameObject[] UI;

	public Text Title;

	public Text Info;

	public Image[] Constraints;

	public Image BrandImage;

	public GameObject TechologiesPanel;

	private Transform _viewRoot;

	private Vector3 _prevCursor;

	private Viewer3DArguments.ViewArgs _viewArgs;

	private float _rotationSpeed = 30f;

	private GameObject _loadedModel;

	private float _currentScale = 1f;

	public const float FishBendAmp = 0.02f;

	public const float FishBendFreq = 3f;

	private bool BendFishMesh;

	private SkinnedMeshRenderer modelMeshRenderer;

	public const bool SHOW_ITEMS_PREVIEW = true;

	public const bool SHOW_FISH_PREVIEW = true;

	private Color grayColor = new Color(0.68235296f, 0.68235296f, 0.68235296f, 1f);

	private Color greenColor = new Color(0.41568628f, 0.6117647f, 0.42745098f, 1f);

	private Color textEnabledColor = new Color(0.96862745f, 0.96862745f, 0.96862745f, 1f);

	private Color textDisabledColor = new Color(0.8627451f, 0.8627451f, 0.8627451f, 1f);

	private const int InfoResizeTextMinSize = 17;

	private List<ResourcesHelpers.AsyncLoadableImage> images = new List<ResourcesHelpers.AsyncLoadableImage>();

	private ResourcesHelpers.AsyncLoadableImage brandImg = new ResourcesHelpers.AsyncLoadableImage();

	private Vector3[] fishBaseChordPos;

	private Quaternion[] fishBaseChordRot;

	private List<Transform> fishChord;

	private BezierCurveWithTorsion bezierCurve = new BezierCurveWithTorsion(5);

	private float fishMinZ;

	private float fishMaxZ;

	private int furDisplacementPropertyID;

	private Material furMaterial;
}
