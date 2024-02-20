using System;
using UnityEngine;

public abstract class RodAnimatorBase : ObjWithAnimator
{
	protected RodAnimatorBase(GameObject reelObj, GameObject rightRoot, GameObject leftRoot, bool isBaitcasting)
		: base(reelObj)
	{
		this._isCurHandLeft = false;
		this._isActive = true;
		this._rollSpeedHash = Animator.StringToHash(TPMMecanimFParameter.RollSpeed.ToString());
		this._rightRoot = rightRoot;
		this._leftRoot = leftRoot;
		this._isBaitcasting = isBaitcasting;
	}

	protected abstract GameObject Rod { get; }

	protected GameObject Reel
	{
		get
		{
			return base.AnimatorParent;
		}
		set
		{
			base.AnimatorParent = value;
		}
	}

	public override void Activate(bool flag)
	{
		base.Activate(flag);
		if (this.Rod != null)
		{
			this.Rod.SetActive(flag);
			if (!flag)
			{
				this.ChangeHand(false);
			}
		}
		this._isActive = flag;
	}

	public virtual void ChangeHand(bool isLeft)
	{
		if (this.Rod != null)
		{
			this._isCurHandLeft = isLeft;
			this.Rod.transform.parent = ((!isLeft) ? this._rightRoot.transform : this._leftRoot.transform);
			this.Rod.transform.localPosition = Vector3.zero;
			this.Rod.transform.localRotation = ((!this._isBaitcasting) ? Quaternion.identity : Quaternion.Euler(0f, 0f, 180f));
		}
	}

	public override void SetFloat(int keyHash, float value)
	{
		if (base.AnimatorParent != null && base.AnimatorParent.activeInHierarchy)
		{
			if (this._isBaitcasting && keyHash == this._rollSpeedHash)
			{
				value *= 0.6f;
			}
			base.Animator.SetFloat(keyHash, value);
		}
	}

	protected GameObject _rightRoot;

	protected GameObject _leftRoot;

	protected bool _isBaitcasting;

	protected readonly int _rollSpeedHash;

	protected bool _isActive;

	protected bool _isCurHandLeft;
}
