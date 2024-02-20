using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TPM;
using UnityEngine;

public class ComplexAnimatorPlayer : Player3dBehaviourController<RodAnimatorDebug>
{
	protected override void Awake()
	{
		base.Awake();
		this._equipment = base.transform.Find("equipment");
		GameObject gameObject = Object.Instantiate<GameObject>(this._gripPrefab);
		this._grip = gameObject.GetComponent<GripSettings>();
		this._grip.SetPlayerVisibility(true);
	}

	public void OnAddPlayer(Dictionary<CustomizedParts, TPMModelLayerSettings> parts)
	{
		int count = this._rods.Count;
		Transform transform = parts[MeshBakersController.SKELETON_SRC_PART].transform;
		Transform parent = transform.parent;
		base.RegisterNewAnimator(new ObjWithAnimator(new AnimatedObject
		{
			obj = parent.gameObject,
			isAllTimeActiveObject = false,
			isLogEnabled = true,
			layersCount = 2
		}));
		this.rodBones.Init(transform);
		GameObject rightHand = this.rodBones.RightHand;
		GameObject leftHand = this.rodBones.LeftHand;
		this._rods.Add(new Dictionary<bool, RodAnimatorDebug>
		{
			{
				true,
				new RodAnimatorDebug(this.CloneEquipmentModel("rod_cast"), this.CloneEquipmentModel("reel_cast"), rightHand, leftHand, true)
			},
			{
				false,
				new RodAnimatorDebug(this.CloneEquipmentModel("rod"), this.CloneEquipmentModel("reel"), rightHand, leftHand, false)
			}
		});
		this._animators.Add(this._rods[count][false]);
		this._animators.Add(this._rods[count][true]);
		this.StopSimulation();
		this.Activate(true);
		this.ChangeHand(false);
	}

	protected override void CatchFishAction(bool flag)
	{
		this._grip.SetGameVisibility(flag);
	}

	private GameObject CloneEquipmentModel(string name)
	{
		return Object.Instantiate<GameObject>(this._equipment.Find(name).gameObject);
	}

	protected override bool IsValidAnimator(AnimatedObject obj)
	{
		return base.IsValidAnimator(obj) && obj.obj.activeInHierarchy;
	}

	protected void SetRod(bool isBaitcasting)
	{
		for (int i = 0; i < this._rods.Count; i++)
		{
			this._rods[i][!isBaitcasting].Activate(false);
			this._rods[i][isBaitcasting].Activate(true);
			base.UpdateAllParameters(this._rods[i][isBaitcasting]);
			this._curRodType = isBaitcasting;
		}
	}

	protected override void ChangeHand(bool isLeft)
	{
		this._grip.ChangeRoot((!isLeft) ? this.rodBones.LeftHand.transform : this.rodBones.RightHand.transform);
		for (int i = 0; i < this._rods.Count; i++)
		{
			this._rods[i][this._curRodType].ChangeHand(isLeft);
		}
	}

	protected override void Activate(bool flag)
	{
		base.Activate(flag);
		if (flag)
		{
			this.SetRod(this._boolParameters[1]);
		}
	}

	private void StopSimulation()
	{
		this.Activate(false);
		this.UpdateParameter(TPMMecanimIParameter.ThrowType, 0, true);
		this.UpdateParameter(TPMMecanimIParameter.CatchedFishType, 0, true);
		base.UpdateParameter(TPMMecanimBParameter.ASimpleThrow, false, true);
		base.UpdateParameter(TPMMecanimBParameter.IsThrowFinished, false, true);
		base.UpdateParameter(TPMMecanimBParameter.IsRollActive, false, true);
		base.UpdateParameter(TPMMecanimBParameter.IsInGame, false, true);
	}

	protected override void Update()
	{
		base.Update();
		if (this._animators.Count == 0)
		{
			return;
		}
		if (Input.GetKeyDown(103))
		{
			if (this._boolParameters[2])
			{
				this.StopSimulation();
			}
			else
			{
				this.Activate(!this._boolParameters[2]);
			}
		}
		if (Input.GetKeyDown(112))
		{
			base.UpdateParameter(TPMMecanimBParameter.IsPitching, !this._boolParameters[0], true);
		}
		if (Input.GetKeyDown(98))
		{
			bool flag = !this._boolParameters[1];
			this.StopSimulation();
			base.UpdateParameter(TPMMecanimBParameter.IsBaitcasting, flag, true);
			this.Activate(true);
		}
		if (Input.GetKeyDown(323))
		{
			base.UpdateParameter(TPMMecanimBParameter.IsRollActive, true, true);
			this.UpdateParameter(TPMMecanimFParameter.RollSpeed, this._floatParameters[2], true);
		}
		if (Input.GetKeyUp(323))
		{
			base.UpdateParameter(TPMMecanimBParameter.IsRollActive, false, true);
		}
		if (Input.GetKeyDown(114))
		{
			base.UpdateParameter(TPMMecanimBParameter.IsRollActive, !this._boolParameters[4], true);
			this.UpdateParameter(TPMMecanimFParameter.RollSpeed, this._floatParameters[2], true);
		}
		if (Input.GetKeyDown(115) && this._boolParameters[0] && !this._boolParameters[5])
		{
			this.UpdateParameter(TPMMecanimIParameter.ThrowType, 5, true);
			base.UpdateParameter(TPMMecanimBParameter.ASimpleThrow, true, true);
			base.StartCoroutine(this.FinishSimpleThrow());
		}
		if (Input.GetKeyDown(48))
		{
			base.UpdateParameter(TPMMecanimBParameter.IsThrowFinished, false, true);
			base.UpdateParameter(TPMMecanimBParameter.IsRollActive, false, true);
			this.UpdateParameter(TPMMecanimIParameter.ThrowType, 0, true);
		}
		if (Input.GetKeyDown(49))
		{
			this.UpdateParameter(TPMMecanimIParameter.ThrowType, 1, true);
			base.StartCoroutine(this.FinishThrow());
		}
		if (Input.GetKeyDown(50))
		{
			this.UpdateParameter(TPMMecanimIParameter.ThrowType, 2, true);
			base.UpdateParameter(TPMMecanimBParameter.ASimpleThrow, true, true);
			base.StartCoroutine(this.FinishSimpleThrow());
		}
		if (Input.GetKeyDown(51))
		{
			this.UpdateParameter(TPMMecanimIParameter.ThrowType, 3, true);
			base.StartCoroutine(this.FinishThrow());
		}
		if (Input.GetKeyDown(52))
		{
			this.UpdateParameter(TPMMecanimIParameter.ThrowType, 4, true);
			base.StartCoroutine(this.FinishThrow());
		}
		if (Input.GetKeyDown(53))
		{
			this.UpdateParameter(TPMMecanimIParameter.ThrowType, 5, true);
			base.StartCoroutine(this.FinishThrow());
		}
		if (Input.GetKeyDown(282))
		{
			this.UpdateParameter(TPMMecanimIParameter.CatchedFishType, 1, true);
		}
		if (Input.GetKeyDown(283))
		{
			this.UpdateParameter(TPMMecanimIParameter.CatchedFishType, 2, true);
		}
		if (Input.GetKeyDown(27))
		{
			this.UpdateParameter(TPMMecanimIParameter.ThrowType, 0, true);
			base.UpdateParameter(TPMMecanimBParameter.IsThrowFinished, false, true);
			base.UpdateParameter(TPMMecanimBParameter.IsRollActive, false, true);
			this.UpdateParameter(TPMMecanimIParameter.CatchedFishType, 0, true);
		}
		float num = Mathf.Abs(this.sideSliderValue);
		this.UpdateParameter(TPMMecanimFParameter.CenterWeight, 1f - num, false);
		this.UpdateParameter(TPMMecanimFParameter.LeftWeight, (this.sideSliderValue >= 0f) ? 0f : num, false);
		this.UpdateParameter(TPMMecanimFParameter.RightWeight, (this.sideSliderValue <= 0f) ? 0f : num, false);
		this.UpdateParameter(TPMMecanimFParameter.PullValue, this.pullSliderValue, false);
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(700f, 10f, 50f, 30f), "Side");
		this.sideSliderValue = GUI.HorizontalSlider(new Rect(750f, 15f, 100f, 30f), this.sideSliderValue, -1f, 1f);
		GUI.Label(new Rect(900f, 10f, 50f, 30f), "Pull");
		this.pullSliderValue = GUI.HorizontalSlider(new Rect(950f, 15f, 100f, 30f), this.pullSliderValue, 0f, 1f);
		if (this.isHUDVisible)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < this._animators.Count; i++)
			{
				if (this._animators[i].IsLogEnabled && this._animators[i].LayersClips.Length > 0)
				{
					stringBuilder.AppendFormat("{0} Mecanim layers:\n", this._animators[i].GameObject);
					byte b = 0;
					while ((int)b < this._animators[i].LayersClips.Length)
					{
						stringBuilder.AppendFormat("{0}: {1}\n", b, this._animators[i].LayersClips[(int)b]);
						b += 1;
					}
				}
			}
			GUI.TextField(new Rect(10f, 10f, (float)this.debugHUDWidth, (float)this.debugHUDHeight), stringBuilder.ToString());
		}
	}

	private IEnumerator FinishThrow()
	{
		yield return new WaitForSeconds(2f);
		base.UpdateParameter(TPMMecanimBParameter.IsThrowFinished, true, true);
		yield break;
	}

	private IEnumerator FinishSimpleThrow()
	{
		yield return new WaitForSeconds(0.7f);
		base.UpdateParameter(TPMMecanimBParameter.ASimpleThrow, false, true);
		base.StartCoroutine(this.FinishThrow());
		yield break;
	}

	public bool isHUDVisible;

	public int debugHUDWidth = 350;

	public int debugHUDHeight = 40;

	private List<Dictionary<bool, RodAnimatorDebug>> _rods = new List<Dictionary<bool, RodAnimatorDebug>>();

	private bool _curRodType;

	private Transform _equipment;

	private GripSettings _grip;

	private int sideIndex = 1;

	private float sideSliderValue;

	private float pullSliderValue;
}
