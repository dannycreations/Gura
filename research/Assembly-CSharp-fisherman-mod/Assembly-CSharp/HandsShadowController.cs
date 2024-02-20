using System;
using UnityEngine;

public class HandsShadowController : MonoBehaviour
{
	private void Awake()
	{
		this._modelsRoot = base.transform.GetChild(0);
		this._speed = this._prcInOneSecond / 100f;
		this._animator = base.GetComponent<Animator>();
		this._forwardHash = Animator.StringToHash("Forward");
		this._sidewaysHash = Animator.StringToHash("Sideways");
		Vector3[] vertices = this._legs.sharedMesh.vertices;
		float num = 2.1474836E+09f;
		float num2 = -2.1474836E+09f;
		for (int i = 0; i < vertices.Length; i++)
		{
			if (vertices[i].y < num)
			{
				num = vertices[i].y;
			}
			if (vertices[i].y > num2)
			{
				num2 = vertices[i].y;
			}
		}
		this._nominalLength = num2 - num;
	}

	private void Update()
	{
		if (GameFactory.Player != null && GameFactory.Player.CurrentBoat != null && GameFactory.Player.CurrentBoat.ShadowPivot != null)
		{
			Transform shadowPivot = GameFactory.Player.CurrentBoat.ShadowPivot;
			this.UpdateContactPoint(new Vector3(this._root.position.x, shadowPivot.position.y, this._root.position.z));
			this.AdjustAxis(0f, this._sidewaysHash);
			this.AdjustAxis(0f, this._forwardHash);
		}
		else
		{
			Vector3? maskedRayContactPoint = Math3d.GetMaskedRayContactPoint(this._root.position, this._root.position + Vector3.down * 5f, GlobalConsts.GroundObstacleMask);
			if (maskedRayContactPoint != null)
			{
				this.UpdateContactPoint(maskedRayContactPoint.Value);
			}
			this.AdjustAxis(ControlsController.ControlsActions.Move.X, this._sidewaysHash);
			this.AdjustAxis(ControlsController.ControlsActions.Move.Y, this._forwardHash);
		}
	}

	private void UpdateContactPoint(Vector3 contactPos)
	{
		Vector3 vector = contactPos + this._root.TransformDirection(this._displacement);
		Vector3 vector2 = this._backAnchor.position - vector;
		Vector3 vector3 = Vector3.Cross(base.transform.right, vector2);
		base.transform.position = vector;
		base.transform.localScale = new Vector3(this._width, vector2.magnitude / this._nominalLength, 1f);
		this._modelsRoot.rotation = Quaternion.LookRotation(vector3, vector2);
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

	public void SetActive(bool flag)
	{
		base.gameObject.SetActive(flag);
		this._bodyRenderer.enabled = flag;
	}

	private const float RAY_DIST = 5f;

	private const float PRECISION = 0.1f;

	[SerializeField]
	private Transform _root;

	[SerializeField]
	private Transform _backAnchor;

	[SerializeField]
	private SkinnedMeshRenderer _legs;

	[SerializeField]
	private Vector3 _displacement = new Vector3(0f, 0f, -0.15f);

	[SerializeField]
	private float _width = 0.8f;

	[SerializeField]
	private float _prcInOneSecond = 300f;

	[SerializeField]
	private SkinnedMeshRenderer _bodyRenderer;

	private float _nominalLength;

	private Animator _animator;

	private int _forwardHash;

	private int _sidewaysHash;

	private float _speed;

	private Transform _modelsRoot;
}
