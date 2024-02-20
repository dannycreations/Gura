using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CastingHandsHanlders))]
public class ShadowController : MonoBehaviour
{
	private void Awake()
	{
		this._rod = Object.Instantiate<GameObject>(this._rodPrefab).transform;
		this._castingRod = Object.Instantiate<GameObject>(this._castingRodPrefab).transform;
		this._rightBone = TransformHelper.FindDeepChild(base.transform, "RightHand");
		this._leftBone = TransformHelper.FindDeepChild(base.transform, "LeftHand");
		this._rod.gameObject.SetActive(false);
		this._castingRod.gameObject.SetActive(false);
		this._speed = this._prcInOneSecond / 100f;
		this._forwardHash = Animator.StringToHash("Forward");
		this._sidewaysHash = Animator.StringToHash("Sideways");
		this._animator = base.GetComponent<Animator>();
		this._animatorIHashes = new int[4];
		for (int i = 0; i < this._animatorIHashes.Length; i++)
		{
			int[] animatorIHashes = this._animatorIHashes;
			int num = i;
			TPMMecanimIParameter tpmmecanimIParameter = (TPMMecanimIParameter)i;
			animatorIHashes[num] = Animator.StringToHash(tpmmecanimIParameter.ToString());
		}
		this._animatorFHashes = new int[5];
		for (int j = 0; j < this._animatorFHashes.Length; j++)
		{
			int[] animatorFHashes = this._animatorFHashes;
			int num2 = j;
			TPMMecanimFParameter tpmmecanimFParameter = (TPMMecanimFParameter)j;
			animatorFHashes[num2] = Animator.StringToHash(tpmmecanimFParameter.ToString());
		}
		this._animatorBHashes = new int[14];
		for (int k = 0; k < this._animatorBHashes.Length; k++)
		{
			int[] animatorBHashes = this._animatorBHashes;
			int num3 = k;
			TPMMecanimBParameter tpmmecanimBParameter = (TPMMecanimBParameter)k;
			animatorBHashes[num3] = Animator.StringToHash(tpmmecanimBParameter.ToString());
		}
		this._renderers = RenderersHelper.GetAllRenderersForObject<Renderer>(base.transform);
		this._renderers.Add(this._rod.GetComponent<MeshRenderer>());
		this._renderers.Add(this._castingRod.GetComponent<MeshRenderer>());
	}

	private void Start()
	{
		this._handsHandler = base.GetComponent<CastingHandsHanlders>();
		this._handsHandler.OnSetHand += this.HandsHandlerOnSetHand;
		this.HandsHandlerOnSetHand(false);
	}

	private void HandsHandlerOnSetHand(bool isLeft)
	{
		this._rod.parent = ((!isLeft) ? this._rightBone : this._leftBone);
		this._rod.localPosition = Vector3.zero;
		this._castingRod.parent = ((!isLeft) ? this._rightBone : this._leftBone);
		this._castingRod.localPosition = Vector3.zero;
	}

	public void Update3dCharMecanimParameter(TPMMecanimIParameter name, byte value)
	{
		this._animator.SetInteger(this._animatorIHashes[(int)((byte)name)], (int)value);
	}

	public void Update3dCharMecanimParameter(TPMMecanimFParameter name, float value)
	{
		this._animator.SetFloat(this._animatorFHashes[(int)((byte)name)], value);
	}

	public void Update3dCharMecanimParameter(TPMMecanimBParameter name, bool value)
	{
		this._animator.SetBool(this._animatorBHashes[(int)((byte)name)], value);
		if (name == TPMMecanimBParameter.IsInGame)
		{
			if (this._isCastingRod)
			{
				this._castingRod.gameObject.SetActive(value);
			}
			else
			{
				this._rod.gameObject.SetActive(value);
			}
		}
	}

	public void ChangeRod(bool isCasting)
	{
		this._isCastingRod = isCasting;
		if (this._animator.GetBool(this._animatorBHashes[2]))
		{
			this._rod.gameObject.SetActive(!isCasting);
			this._castingRod.gameObject.SetActive(isCasting);
		}
	}

	public void SetVisibility(bool flag)
	{
		for (int i = 0; i < this._renderers.Count; i++)
		{
			this._renderers[i].enabled = flag;
		}
	}

	private void Update()
	{
		this.AdjustAxis(ControlsController.ControlsActions.Move.X, this._sidewaysHash);
		this.AdjustAxis(ControlsController.ControlsActions.Move.Y, this._forwardHash);
	}

	private void AdjustAxis(float input, int propertyHash)
	{
		float @float = this._animator.GetFloat(propertyHash);
		if (Mathf.Abs(input) < 0.1f)
		{
			if (Mathf.Abs(@float) > 0.1f)
			{
				float num = @float - Mathf.Sign(@float) * this._speed * Time.deltaTime;
				if (Mathf.Sign(num) != Mathf.Sign(@float))
				{
					num = 0f;
				}
				this._animator.SetFloat(propertyHash, num);
			}
		}
		else
		{
			this._animator.SetFloat(propertyHash, (input <= 0f) ? Mathf.Max(@float - this._speed * Time.deltaTime, -1f) : Mathf.Min(@float + this._speed * Time.deltaTime, 1f));
		}
	}

	private void OnDestroy()
	{
		if (this._handsHandler != null)
		{
			this._handsHandler.OnSetHand -= this.HandsHandlerOnSetHand;
		}
	}

	[SerializeField]
	private GameObject _rodPrefab;

	[SerializeField]
	private GameObject _castingRodPrefab;

	[SerializeField]
	private float _prcInOneSecond = 300f;

	private const float PRECISION = 0.1f;

	private Animator _animator;

	private int[] _animatorIHashes;

	private int[] _animatorFHashes;

	private int[] _animatorBHashes;

	private int _forwardHash;

	private int _sidewaysHash;

	private float _speed;

	private Transform _rod;

	private Transform _castingRod;

	private Transform _rightBone;

	private Transform _leftBone;

	private CastingHandsHanlders _handsHandler;

	private bool _isCastingRod;

	private List<Renderer> _renderers;
}
