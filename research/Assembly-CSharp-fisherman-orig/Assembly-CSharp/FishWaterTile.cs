using System;
using UnityEngine;

[ExecuteInEditMode]
public class FishWaterTile : MonoBehaviour
{
	private void Awake()
	{
		for (int i = 1; i <= 5; i++)
		{
			this._disturbsInput[i - 1] = "_Input" + i;
		}
	}

	private void MoveDynWater(RenderTexture[] field, RenderTexture[] fieldLod, RenderTexture[] outFlow, RenderTexture[] outFlowLOD)
	{
		if (!this.waterMoveInits)
		{
			this.waterMoveInits = true;
		}
		if (this.waterBase != null && this.waterBase.sharedMaterial != null && this.waterInits)
		{
			Vector4 vector = this.waterBase.sharedMaterial.GetVector("_FlowOffsets");
			Vector4 vector2 = this.waterBase.sharedMaterial.GetVector("_DynWaterOffsets");
			this.m_copyField.SetTexture("_FieldLOD", fieldLod[0]);
			Graphics.Blit(fieldLod[0], fieldLod[1], this.m_copyFieldLOD);
			RTUtility.Swap(fieldLod);
			this.m_copyField.SetTexture("_FieldLOD", fieldLod[0]);
			Graphics.Blit(outFlowLOD[0], outFlowLOD[1], this.m_copyFieldLOD);
			RTUtility.Swap(outFlowLOD);
			this.m_copyField.SetTexture("_FieldLOD", fieldLod[0]);
			Graphics.Blit(field[0], field[1], this.m_copyField);
			RTUtility.Swap(field);
			this.m_copyField.SetTexture("_FieldLOD", outFlowLOD[0]);
			Graphics.Blit(outFlow[0], outFlow[1], this.m_copyField);
			RTUtility.Swap(outFlow);
			this.waterBase.sharedMaterial.SetTexture("_WaterField", field[0]);
			this.waterBase.sharedMaterial.SetTexture("_WaterFieldLOD", fieldLod[0]);
		}
	}

	private void OutFlow(RenderTexture[] field, RenderTexture[] fieldLod, RenderTexture[] outFlow, RenderTexture[] outFlowLOD, float damping)
	{
		this.m_outFlowMat.SetFloat("_TexSize", 1024f);
		this.m_outFlowMat.SetFloat("T", 0.1f);
		this.m_outFlowMat.SetFloat("L", 1f);
		this.m_outFlowMat.SetFloat("A", 1f);
		this.m_outFlowMat.SetFloat("G", 9.81f);
		this.m_outFlowMat.SetFloat("_Damping", 1f - damping);
		this.m_outFlowMat.SetTexture("_Field", field[0]);
		Graphics.Blit(outFlow[0], outFlow[1], this.m_outFlowMat);
		RTUtility.Swap(outFlow);
		this.m_fieldUpdateMat.SetFloat("_TexSize", 1024f);
		this.m_fieldUpdateMat.SetFloat("T", 0.1f);
		this.m_fieldUpdateMat.SetFloat("L", 1f);
		this.m_fieldUpdateMat.SetTexture("_OutFlowField", outFlow[0]);
		this.m_fieldUpdateMat.SetTexture("_FieldLOD", fieldLod[0]);
		Graphics.Blit(field[0], field[1], this.m_fieldUpdateMat);
		RTUtility.Swap(field);
		if (FishWaterTile.frameCount == 0)
		{
			this.m_outFlowMatLOD.SetFloat("_TexSize", 1024f);
			this.m_outFlowMatLOD.SetFloat("T", 0.1f);
			this.m_outFlowMatLOD.SetFloat("L", 1f);
			this.m_outFlowMatLOD.SetFloat("A", 1f);
			this.m_outFlowMatLOD.SetFloat("G", 9.81f);
			this.m_outFlowMatLOD.SetFloat("_Damping", 1f - damping);
			this.m_outFlowMatLOD.SetTexture("_Field", fieldLod[0]);
			Graphics.Blit(outFlowLOD[0], outFlowLOD[1], this.m_outFlowMatLOD);
			RTUtility.Swap(outFlowLOD);
			this.m_fieldUpdateMatLOD.SetFloat("_TexSize", 1024f);
			this.m_fieldUpdateMatLOD.SetFloat("T", 0.1f);
			this.m_fieldUpdateMatLOD.SetFloat("L", 1f);
			this.m_fieldUpdateMatLOD.SetTexture("_OutFlowField", outFlowLOD[0]);
			this.m_fieldUpdateMatLOD.SetTexture("_FieldLOD", field[0]);
			Graphics.Blit(fieldLod[0], fieldLod[1], this.m_fieldUpdateMatLOD);
			RTUtility.Swap(fieldLod);
		}
	}

	private void AcquireComponents()
	{
		if (!this.reflection)
		{
			if (base.transform.parent)
			{
				this.reflection = base.transform.parent.GetComponent<FishPlanarReflection>();
			}
			else
			{
				this.reflection = base.transform.GetComponent<FishPlanarReflection>();
			}
		}
		if (!this.waterBase)
		{
			if (base.transform.parent)
			{
				this.waterBase = base.transform.parent.GetComponent<FishWaterBase>();
			}
			else
			{
				this.waterBase = base.transform.GetComponent<FishWaterBase>();
			}
		}
		if (this.waterBase)
		{
			this.sharedMaterial = this.waterBase.sharedMaterial;
		}
	}

	public void WaterUpdate()
	{
		if (this.m_waterField != null)
		{
			RTUtility.SetToPoint(this.m_waterField);
			RTUtility.SetToPoint(this.m_waterField_lod);
		}
		FishWaterTile.frameCount++;
		if (FishWaterTile.frameCount >= 3)
		{
			FishWaterTile.frameCount = 0;
		}
		this.WaterInput();
		if (this.m_outFlowMat != null)
		{
			this.OutFlow(this.m_waterField, this.m_waterField_lod, this.m_waterOutFlow, this.m_waterOutFlow_lod, this.m_waterDamping);
		}
		if (this.m_waterField != null)
		{
			RTUtility.SetToTrilinear(this.m_waterField);
			RTUtility.SetToTrilinear(this.m_waterField_lod);
		}
		if (this.waterBase != null && this.waterBase.sharedMaterial != null)
		{
			this.waterBase.sharedMaterial.SetTexture("_WaterField", this.m_waterField[0]);
			this.waterBase.sharedMaterial.SetTexture("_WaterFieldLOD", this.m_waterField_lod[0]);
		}
		else
		{
			Debug.Log("No water material");
		}
	}

	public void UpdateDynWaterPos()
	{
		Vector4 dynWaterOffsetsLocal = this.DynWaterOffsetsLocal;
		this.DynWaterOffsetsLocal = this.DynWaterOffsets;
		this.DynWaterOffsetsLocalLOD = this.DynWaterOffsets;
		this.DynWaterOffsetsLocal.x = 0.5f - (this.DynWaterOffsetsLocal.x * this.FlowOffsets.w + this.FlowOffsets.x) * this.DynWaterOffsets.w;
		this.DynWaterOffsetsLocal.z = 0.5f - (this.DynWaterOffsetsLocal.z * this.FlowOffsets.w + this.FlowOffsets.z) * this.DynWaterOffsets.w;
		this.DynWaterOffsetsLocal.y = 0f;
		this.DynWaterOffsetsLocalLOD = this.DynWaterOffsetsLocal;
		Vector4 vector = this.DynWaterOffsetsLocal - dynWaterOffsetsLocal;
		if (vector.x != 0f || vector.z != 0f)
		{
			if (this.m_waterField != null && this.waterInits)
			{
				if (this.m_dynWaterMoveMat)
				{
					this.m_dynWaterMoveMat.SetVector("_DynWaterOffsetsOld", dynWaterOffsetsLocal);
					this.m_dynWaterMoveMat.SetVector("_DynWaterOffsets", this.DynWaterOffsetsLocal);
				}
				if (this.m_copyField)
				{
					this.m_copyField.SetVector("_DynWaterOffsetsOld", dynWaterOffsetsLocal);
					this.m_copyField.SetVector("_DynWaterOffsets", this.DynWaterOffsetsLocal);
				}
				if (this.m_copyFieldLOD)
				{
					this.m_copyFieldLOD.SetVector("_DynWaterOffsetsOld", dynWaterOffsetsLocal);
					this.m_copyFieldLOD.SetVector("_DynWaterOffsets", this.DynWaterOffsetsLocal);
				}
			}
			this.UpdateDynWater = true;
		}
	}

	public void UpdateMaterials()
	{
		if (this.waterBase != null && this.waterBase.sharedMaterial != null)
		{
			this.waterBase.sharedMaterial.SetVector("_FlowOffsets", this.FlowOffsets);
			this.waterBase.sharedMaterial.SetVector("_DynWaterOffsets", this.DynWaterOffsetsLocal);
		}
		if (this.m_outFlowMat)
		{
			this.m_outFlowMat.SetVector("_DynWaterOffsets", this.DynWaterOffsetsLocal);
		}
		if (this.m_outFlowMatLOD)
		{
			this.m_outFlowMat.SetVector("_DynWaterOffsets", this.DynWaterOffsetsLocalLOD);
		}
		if (this.m_outFlowMatLOD)
		{
			this.m_outFlowMatLOD.SetVector("_DynWaterOffsets", this.DynWaterOffsetsLocalLOD);
		}
		if (this.m_fieldUpdateMat)
		{
			this.m_fieldUpdateMat.SetVector("_DynWaterOffsets", this.DynWaterOffsetsLocal);
		}
		if (this.m_fieldUpdateMatLOD)
		{
			this.m_fieldUpdateMatLOD.SetVector("_DynWaterOffsets", this.DynWaterOffsetsLocalLOD);
		}
		if (this.m_copyField)
		{
		}
		if (this.m_copyFieldLOD)
		{
		}
	}

	public void Update()
	{
		this.AcquireComponents();
		if (!this.waterInits)
		{
			Shader.SetGlobalFloat("_TexSize", 1024f);
			if (this.m_frameCount == 1)
			{
				this.InitMaps();
			}
			if (this.m_frameCount == 2)
			{
				this.InitMaps();
				this.waterInits = true;
			}
		}
		this.m_frameCount++;
		if (this.m_frameCount > 1000000)
		{
			this.m_frameCount = 5;
		}
		if (this.NeedToDisableFakeRefl)
		{
			Shader.DisableKeyword("FAKEREFLECTION");
			if (this.waterBase && this.waterBase.sharedMaterial)
			{
				this.waterBase.sharedMaterial.DisableKeyword("FAKEREFLECTION");
			}
			this.NeedToDisableFakeRefl = false;
		}
		if (this.NeedToEnableFakeRefl)
		{
			Shader.EnableKeyword("FAKEREFLECTION");
			if (this.waterBase && this.waterBase.sharedMaterial)
			{
				this.waterBase.sharedMaterial.EnableKeyword("FAKEREFLECTION");
			}
			this.NeedToEnableFakeRefl = false;
		}
	}

	public void FixedUpdate()
	{
		if (this.waterInits && this.waterBase != null && this.waterBase.dynWaterQuality != FishDynWaterQuality.None)
		{
			if (this.m_waterField != null && this.UpdateDynWater)
			{
				this.UpdateDynWater = false;
				this.MoveDynWater(this.m_waterField, this.m_waterField_lod, this.m_waterOutFlow, this.m_waterOutFlow_lod);
				this.UpdateMaterials();
			}
			this.WaterUpdate();
		}
	}

	public void SetMaterialsDontSave()
	{
		HideFlags hideFlags = 0;
		if (this.waterBase != null && this.waterBase.sharedMaterial != null)
		{
			this.waterBase.sharedMaterial.hideFlags = hideFlags;
		}
		if (this.m_outFlowMat)
		{
			this.m_outFlowMat.hideFlags = hideFlags;
		}
		if (this.m_outFlowMatLOD)
		{
			this.m_outFlowMat.hideFlags = hideFlags;
		}
		if (this.m_outFlowMatLOD)
		{
			this.m_outFlowMatLOD.hideFlags = hideFlags;
		}
		if (this.m_fieldUpdateMat)
		{
			this.m_fieldUpdateMat.hideFlags = hideFlags;
		}
		if (this.m_fieldUpdateMatLOD)
		{
			this.m_fieldUpdateMatLOD.hideFlags = hideFlags;
		}
		if (this.m_copyField)
		{
			this.m_copyField.hideFlags = hideFlags;
		}
		if (this.m_copyFieldLOD)
		{
			this.m_copyFieldLOD.hideFlags = hideFlags;
		}
		if (this.m_dynWaterMoveMat)
		{
			this.m_dynWaterMoveMat.hideFlags = hideFlags;
		}
	}

	private void OnDestroy()
	{
		this.waterInits = false;
		if (this.m_waterOutFlow != null && this.m_waterOutFlow[0] != null)
		{
			this.m_waterOutFlow[0].Release();
			this.m_waterOutFlow[0] = null;
		}
		if (this.m_waterOutFlow != null && this.m_waterOutFlow[1] != null)
		{
			this.m_waterOutFlow[1].Release();
			this.m_waterOutFlow[1] = null;
		}
		if (this.m_waterOutFlow_lod != null && this.m_waterOutFlow_lod[0] != null)
		{
			this.m_waterOutFlow_lod[0].Release();
			this.m_waterOutFlow_lod[0] = null;
		}
		if (this.m_waterOutFlow_lod != null && this.m_waterOutFlow_lod[1] != null)
		{
			this.m_waterOutFlow_lod[1].Release();
			this.m_waterOutFlow_lod[1] = null;
		}
		if (this.m_waterField != null && this.m_waterField[0] != null)
		{
			this.m_waterField[0].Release();
			this.m_waterField[0] = null;
		}
		if (this.m_waterField != null && this.m_waterField[1] != null)
		{
			this.m_waterField[1].Release();
			this.m_waterField[1] = null;
		}
		if (this.m_waterField_lod != null && this.m_waterField_lod[0] != null)
		{
			this.m_waterField_lod[0].Release();
			this.m_waterField_lod[0] = null;
		}
		if (this.m_waterField_lod != null && this.m_waterField_lod[1] != null)
		{
			this.m_waterField_lod[1].Release();
			this.m_waterField_lod[1] = null;
		}
	}

	public void Start()
	{
		this.AcquireComponents();
		this.SetMaterialsDontSave();
		this.m_waterDamping = Mathf.Clamp01(this.m_waterDamping);
		float num = 0.0009765625f;
		this.m_rectLeft = new Rect(0f, 0f, num, 1f);
		this.m_rectRight = new Rect(1f - num, 0f, num, 1f);
		this.m_rectBottom = new Rect(0f, 0f, 1f, num);
		this.m_rectTop = new Rect(0f, 1f - num, 1f, num);
		this.m_waterOutFlow = new RenderTexture[2];
		this.m_waterOutFlow_lod = new RenderTexture[2];
		this.m_waterField = new RenderTexture[2];
		this.m_waterField_lod = new RenderTexture[2];
		this.m_waterOutFlow[0] = new RenderTexture(1024, 1024, 0, 2, 1);
		this.m_waterOutFlow[0].wrapMode = 1;
		this.m_waterOutFlow[0].filterMode = 0;
		this.m_waterOutFlow[0].Create();
		this.m_waterOutFlow[1] = new RenderTexture(1024, 1024, 0, 2, 1);
		this.m_waterOutFlow[1].wrapMode = 1;
		this.m_waterOutFlow[1].filterMode = 0;
		this.m_waterOutFlow[1].Create();
		this.m_waterOutFlow_lod[0] = new RenderTexture(1024, 1024, 0, 2, 1);
		this.m_waterOutFlow_lod[0].wrapMode = 1;
		this.m_waterOutFlow_lod[0].filterMode = 0;
		this.m_waterOutFlow_lod[0].Create();
		this.m_waterOutFlow_lod[1] = new RenderTexture(1024, 1024, 0, 2, 1);
		this.m_waterOutFlow_lod[1].wrapMode = 1;
		this.m_waterOutFlow_lod[1].filterMode = 0;
		this.m_waterOutFlow_lod[1].Create();
		this.m_waterField[0] = new RenderTexture(1024, 1024, 0, 15, 1);
		this.m_waterField[0].wrapMode = 1;
		this.m_waterField[0].filterMode = 0;
		this.m_waterField[0].Create();
		this.m_waterField[1] = new RenderTexture(1024, 1024, 0, 15, 1);
		this.m_waterField[1].wrapMode = 1;
		this.m_waterField[1].filterMode = 0;
		this.m_waterField[1].Create();
		this.m_waterField_lod[0] = new RenderTexture(1024, 1024, 0, 15, 1);
		this.m_waterField_lod[0].wrapMode = 1;
		this.m_waterField_lod[0].filterMode = 0;
		this.m_waterField_lod[0].Create();
		this.m_waterField_lod[1] = new RenderTexture(1024, 1024, 0, 15, 1);
		this.m_waterField_lod[1].wrapMode = 1;
		this.m_waterField_lod[1].filterMode = 0;
		this.m_waterField_lod[1].Create();
	}

	private void WaterInput()
	{
		if (GameFactory.Water == null)
		{
			return;
		}
		bool flag = false;
		if (this.m_waterInputMat != null && this.m_waterField != null && this.waterInits)
		{
			if (flag)
			{
				GameFactory.Water.AddWaterDisturb(new Vector3(this.m_waterInputPoint.x, 0f, this.m_waterInputPoint.y), this.m_waterInputRadius, WaterDisturbForce.Small);
			}
			if (this.m_waterInputAmount > 0f)
			{
				Vector4 vector = this.waterBase.sharedMaterial.GetVector("_FlowOffsets");
				Vector4 vector2 = this.waterBase.sharedMaterial.GetVector("_DynWaterOffsets");
				while (!GameFactory.Water.WaterStates.IsEmpty)
				{
					WaterDisturb waterDisturb = GameFactory.Water.WaterStates.Dequeue();
					Vector2 vector3;
					vector3.x = waterDisturb.GlobalPosition.x;
					vector3.y = waterDisturb.GlobalPosition.z;
					vector3.x = (vector3.x * vector.w + vector.x) * vector2.w + vector2.x;
					vector3.y = (vector3.y * vector.w + vector.z) * vector2.w + vector2.z;
					float num = waterDisturb.Radius * 0.024414062f;
					if (num < 0.001f)
					{
						num = 0.001f;
					}
					float num2 = waterDisturb.Force / 10f;
					this.m_waterInputMat.SetVector("_Point", vector3);
					this.m_waterInputMat.SetFloat("_Radius", num);
					this.m_waterInputMat.SetFloat("_Amount", num2);
					this._curDisturbsCount = 0;
					Vector2 vector4;
					for (int i = 0; i < 5; i++)
					{
						if (!GameFactory.Water.WaterStates.IsEmpty)
						{
							waterDisturb = GameFactory.Water.WaterStates.Dequeue();
							this._disturbs[this._curDisturbsCount++] = waterDisturb;
							vector4.x = waterDisturb.GlobalPosition.x;
							vector4.y = waterDisturb.GlobalPosition.z;
							vector4.x = (vector4.x * vector.w + vector.x) * vector2.w + vector2.x;
							vector4.y = (vector4.y * vector.w + vector.z) * vector2.w + vector2.z;
							float num3 = waterDisturb.Radius * 0.024414062f;
							if (num3 < 0.001f)
							{
								num3 = 0.001f;
							}
							float num4 = waterDisturb.Force / 10f;
							Vector4 vector5;
							vector5..ctor(vector4.x, vector4.y, num3, num4);
							this.m_waterInputMat.SetVector(this._disturbsInput[i], vector5);
						}
						else
						{
							this.m_waterInputMat.SetVector(this._disturbsInput[i], Vector4.zero);
						}
					}
					Graphics.Blit(this.m_waterField[0], this.m_waterField[1], this.m_waterInputMat);
					RTUtility.Swap(this.m_waterField);
					for (int j = 0; j < 5; j++)
					{
						if (this._curDisturbsCount > 0)
						{
							this._curDisturbsCount--;
							waterDisturb = this._disturbs[j];
							this._disturbs[j] = null;
							vector4.x = waterDisturb.GlobalPosition.x;
							vector4.y = waterDisturb.GlobalPosition.z;
							vector4.x = (vector4.x * vector.w + vector.x) * vector2.w + vector2.x;
							vector4.y = (vector4.y * vector.w + vector.z) * vector2.w + vector2.z;
							vector4.x = (vector4.x - 0.5f) / 3f + 0.5f;
							vector4.y = (vector4.y - 0.5f) / 3f + 0.5f;
							float num3 = waterDisturb.Radius * 0.024414062f;
							if (num3 < 0.001f)
							{
								num3 = 0.001f;
							}
							float num4 = waterDisturb.Force / 10f;
							Vector4 vector6;
							vector6..ctor(vector4.x, vector4.y, num3 / 3f, num4 / 3f);
							this.m_waterInputMat.SetVector(this._disturbsInput[j], vector6);
						}
						else
						{
							this.m_waterInputMat.SetVector(this._disturbsInput[j], Vector4.zero);
						}
					}
					Vector2 vector7;
					vector7.x = (vector3.x - 0.5f) / 3f + 0.5f;
					vector7.y = (vector3.y - 0.5f) / 3f + 0.5f;
					this.m_waterInputMat.SetVector("_Point", vector7);
					this.m_waterInputMat.SetFloat("_Radius", num / 3f);
					this.m_waterInputMat.SetFloat("_Amount", num2 / 3f);
					Graphics.Blit(this.m_waterField_lod[0], this.m_waterField_lod[1], this.m_waterInputMat);
					RTUtility.Swap(this.m_waterField_lod);
				}
			}
		}
	}

	private void SetDynWaterLocation(Vector2 xy)
	{
		if (this.waterBase != null && this.waterBase.sharedMaterial != null)
		{
			Vector4 vector = this.waterBase.sharedMaterial.GetVector("_FlowOffsets");
			Vector4 vector2 = this.waterBase.sharedMaterial.GetVector("_DynWaterOffsets");
			this.m_copyField.SetVector("_Shift", xy);
			this.m_copyField.SetTexture("_FieldLOD", this.m_waterField_lod[0]);
			Graphics.Blit(this.m_waterField[0], this.m_waterField[1], this.m_copyField);
			RTUtility.Swap(this.m_waterField);
			this.m_copyFieldLOD.SetVector("_Shift", xy);
			Graphics.Blit(this.m_waterField_lod[0], this.m_waterField_lod[1], this.m_copyFieldLOD);
			RTUtility.Swap(this.m_waterField_lod);
		}
	}

	private void WaterCopy()
	{
		RTUtility.SetToTrilinear(this.m_waterField);
		this.m_copyFieldLOD.SetTexture("_FieldLOD", this.m_waterField[0]);
		Graphics.Blit(this.m_waterField_lod[0], this.m_waterField_lod[1], this.m_copyFieldLOD);
		RTUtility.Swap(this.m_waterField_lod);
		RTUtility.SetToPoint(this.m_waterField);
	}

	public void InitMaps()
	{
		GL.Flush();
		RTUtility.ClearColor(this.m_waterField_lod);
		RTUtility.ClearColor(this.m_waterOutFlow_lod);
		RTUtility.ClearColor(this.m_waterField);
		RTUtility.ClearColor(this.m_waterOutFlow);
		GL.Flush();
	}

	public float CalcRotation(float r1, float r2)
	{
		float num = r2 - r1;
		if (num < 0f)
		{
			num = (float)(360.0 + (double)num);
		}
		if ((double)num > 360.0)
		{
			num = (float)((double)num - 360.0);
		}
		if ((double)num > 180.0)
		{
			num = (float)((360.0 - (double)num) * -1.0);
		}
		return num;
	}

	public void OnWillRenderObject()
	{
		if (FishWaterTile.DoingRenderWater && this.reflection)
		{
			this.reflection.WaterTileBeingRendered(base.transform, Camera.current);
			this.OldCameraDir = Camera.current.transform.forward;
			this.NewCameraDir = Camera.current.transform.eulerAngles;
			this.OldCameraFov = Camera.current.fieldOfView;
			Shader.SetGlobalVector("CameraShift", Vector4.zero);
			this.NeedToDisableFakeRefl = true;
		}
		else
		{
			this.RoationAngles = Quaternion.FromToRotation(this.OldCameraDir, Camera.current.transform.forward);
			float num = Camera.current.aspect;
			if (num == 0f)
			{
				num = 1f;
			}
			this.CamVector.x = this.CalcRotation(this.NewCameraDir.x, Camera.current.transform.eulerAngles.x) * 0.01f;
			this.CamVector.y = this.CalcRotation(this.NewCameraDir.y, Camera.current.transform.eulerAngles.y) * 0.01f;
			this.CamVector.z = num;
			this.CamVector.w = (float)((double)(this.OldCameraFov - Camera.current.fieldOfView) * 0.1 + 1.0);
			Shader.SetGlobalVector("CameraShift", this.CamVector);
			this.NeedToEnableFakeRefl = true;
		}
		if (this.waterBase)
		{
			this.waterBase.WaterTileBeingRendered(base.transform, Camera.current);
		}
	}

	public static bool DoingRenderWater = true;

	public FishPlanarReflection reflection;

	public FishWaterBase waterBase;

	public Vector4 FlowOffsets = new Vector4(0.5f, 0f, 0.5f, 0.001f);

	public Vector4 DynWaterOffsets = new Vector4(-1.5f, 0f, -1.5f, 4f);

	private Vector4 DynWaterOffsetsLocal = new Vector4(-1.5f, 0f, -1.5f, 4f);

	private Vector4 DynWaterOffsetsLocalLOD = new Vector4(-1.5f, 0f, -1.5f, 4f);

	public Material m_waterInputMat;

	public Material m_outFlowMat;

	public Material m_fieldUpdateMat;

	public Material m_fieldUpdateMatLOD;

	public Material m_outFlowMatLOD;

	public Material m_copyFieldLOD;

	public Material m_copyField;

	public Material m_dynWaterMoveMat;

	private Vector3 OldCameraDir = new Vector3(0f, 0f, 0f);

	private Vector4 NewCameraDir = new Vector3(0f, 0f, 0f);

	private Quaternion RoationAngles = Quaternion.identity;

	public float m_waterInputSpeed = 0.01f;

	public Vector2 m_waterInputPoint = new Vector2(0.5f, 0.5f);

	public float m_waterInputAmount = 2f;

	public float m_waterInputRadius = 0.0008f;

	public float m_waterDamping;

	private static int frameCount;

	private Material sharedMaterial;

	private bool waterInits;

	private bool waterMoveInits;

	private Vector4 CamVector = Vector4.zero;

	private bool NeedToDisableFakeRefl;

	private bool NeedToEnableFakeRefl;

	private float OldCameraFov = 65f;

	public bool UpdateDynWater;

	private int m_frameCount;

	private const int TEX_SIZE = 1024;

	private const int TOTAL_GRID_SIZE = 1024;

	private const float TIME_STEP = 0.1f;

	private const int GRID_SIZE = 128;

	private const float PIPE_LENGTH = 1f;

	private const float CELL_LENGTH = 1f;

	private const float CELL_AREA = 1f;

	private const float GRAVITY = 9.81f;

	private const int READ = 0;

	private const int WRITE = 1;

	private Rect m_rectLeft;

	private Rect m_rectRight;

	private Rect m_rectTop;

	private Rect m_rectBottom;

	private RenderTexture[] m_waterField;

	private RenderTexture[] m_waterOutFlow;

	private RenderTexture[] m_waterField_lod;

	private RenderTexture[] m_waterOutFlow_lod;

	private const int DISTURBS_IN_BATCH = 5;

	private WaterDisturb[] _disturbs = new WaterDisturb[5];

	private string[] _disturbsInput = new string[5];

	private int _curDisturbsCount;
}
