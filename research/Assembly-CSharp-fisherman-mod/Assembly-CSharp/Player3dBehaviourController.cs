using System;
using System.Collections.Generic;
using UnityEngine;

public class Player3dBehaviourController<T> : MonoBehaviour where T : RodAnimatorBase
{
	protected virtual string PlayerName
	{
		get
		{
			return string.Empty;
		}
	}

	protected virtual void Awake()
	{
		this._tagHashToName = new Dictionary<int, string> { 
		{
			Animator.StringToHash("EmptyState"),
			"EmptyState"
		} };
		this._byteParameters = new byte[4];
		this._floatParameters = new float[7];
		this._floatParameters[2] = 1f;
		this._boolParameters = new bool[17];
		this._animatorIHashes = new int[4];
		for (int i = 0; i < this._byteParameters.Length; i++)
		{
			int[] animatorIHashes = this._animatorIHashes;
			int num = i;
			TPMMecanimIParameter tpmmecanimIParameter = (TPMMecanimIParameter)i;
			animatorIHashes[num] = Animator.StringToHash(tpmmecanimIParameter.ToString());
		}
		this._animatorFHashes = new int[7];
		for (int j = 0; j < this._floatParameters.Length; j++)
		{
			int[] animatorFHashes = this._animatorFHashes;
			int num2 = j;
			TPMMecanimFParameter tpmmecanimFParameter = (TPMMecanimFParameter)j;
			animatorFHashes[num2] = Animator.StringToHash(tpmmecanimFParameter.ToString());
		}
		this._animatorBHashes = new int[17];
		for (int k = 0; k < this._boolParameters.Length; k++)
		{
			int[] animatorBHashes = this._animatorBHashes;
			int num3 = k;
			TPMMecanimBParameter tpmmecanimBParameter = (TPMMecanimBParameter)k;
			animatorBHashes[num3] = Animator.StringToHash(tpmmecanimBParameter.ToString());
		}
	}

	protected virtual bool IsValidAnimator(AnimatedObject obj)
	{
		return obj.obj != null;
	}

	protected void RegisterNewAnimator(ObjWithAnimator animator)
	{
		this._animators.Add(animator);
		this.SubscribeAnimatorEvents(animator);
		this.UpdateAllParameters(animator);
	}

	protected virtual void SubscribeAnimatorEvents(ObjWithAnimator animator)
	{
		if (this._handsHandler == null)
		{
			this._handsHandler = animator.GameObject.GetComponent<CastingHandsHanlders>();
			if (this._handsHandler != null)
			{
				this._handsHandler.OnSetHand += this.ChangeHand;
			}
		}
		if (this._fishHandler == null)
		{
			this._fishHandler = animator.GameObject.GetComponent<FishCatchingHandler>();
			if (this._fishHandler != null)
			{
				this._fishHandler.OnCatch += this.CatchFishAction;
			}
		}
	}

	protected virtual void ChangeHand(bool isLeft)
	{
		if (this._curRod != null)
		{
			this._curRod.ChangeHand(isLeft);
		}
	}

	protected virtual void CatchFishAction(bool flag)
	{
	}

	protected virtual void Activate(bool flag)
	{
		this.UpdateParameter(TPMMecanimBParameter.IsInGame, flag, true);
		for (int i = 0; i < this._animators.Count; i++)
		{
			this._animators[i].Activate(flag);
			if (flag)
			{
				this.UpdateAllParameters(this._animators[i]);
			}
		}
	}

	protected void SetAnimation(int clipHash)
	{
		for (int i = 0; i < this._animators.Count; i++)
		{
			if (this._animators[i].Animator != null)
			{
				this._animators[i].Animator.CrossFade(clipHash, 0f, this._animators[i].SyncLayerIndex);
			}
		}
	}

	protected void UpdateAllParameters(ObjWithAnimator animator)
	{
		for (int i = 0; i < this._byteParameters.Length; i++)
		{
			animator.SetInteger(this._animatorIHashes[i], (int)this._byteParameters[i]);
		}
		for (int j = 0; j < this._floatParameters.Length; j++)
		{
			if (j != 5)
			{
				animator.SetFloat(this._animatorFHashes[j], this._floatParameters[j]);
			}
		}
		for (int k = 0; k < this._boolParameters.Length; k++)
		{
			if (k != 14)
			{
				animator.SetBool(this._animatorBHashes[k], this._boolParameters[k], (TPMMecanimBParameter)k);
			}
		}
	}

	protected virtual void UpdateParameter(TPMMecanimIParameter name, byte value, bool logEnabled = true)
	{
		if (!this._wasRestarted && this._byteParameters[(int)((byte)name)] == value)
		{
			return;
		}
		if (logEnabled)
		{
		}
		this._byteParameters[(int)((byte)name)] = value;
		for (int i = 0; i < this._animators.Count; i++)
		{
			this._animators[i].SetInteger(this._animatorIHashes[(int)((byte)name)], (int)value);
		}
	}

	public void UpdateParameter(TPMMecanimBParameter name, bool value, bool logEnabled = true)
	{
		byte b = (byte)name;
		if (!this._wasRestarted && this._boolParameters[(int)b] == value)
		{
			return;
		}
		if (logEnabled)
		{
		}
		this._boolParameters[(int)b] = value;
		for (int i = 0; i < this._animators.Count; i++)
		{
			this._animators[i].SetBool(this._animatorBHashes[(int)b], value, name);
		}
	}

	protected virtual void UpdateParameter(TPMMecanimFParameter name, float value, bool logEnabled = true)
	{
		byte b = (byte)name;
		if (!this._wasRestarted && (double)Math.Abs(this._floatParameters[(int)b] - value) < 0.001)
		{
			return;
		}
		if (logEnabled)
		{
		}
		this._floatParameters[(int)b] = value;
		for (int i = 0; i < this._animators.Count; i++)
		{
			this._animators[i].SetFloat(this._animatorFHashes[(int)((byte)name)], value);
		}
	}

	protected virtual void OnNewStateAnimation(byte layerIndex, string clipName)
	{
	}

	protected virtual void OnNewStateTag(byte layerIndex, string tag)
	{
	}

	protected virtual void Update()
	{
		this.CheckAnimatorState();
	}

	protected virtual void LateUpdate()
	{
	}

	private void CheckAnimatorState()
	{
		for (int i = 0; i < this._animators.Count; i++)
		{
			ObjWithAnimator objWithAnimator = this._animators[i];
			byte b = 0;
			while ((int)b < objWithAnimator.LayersStateHash.Length)
			{
				AnimatorStateInfo currentAnimatorStateInfo = objWithAnimator.GetCurrentAnimatorStateInfo((int)b);
				if (currentAnimatorStateInfo.fullPathHash != objWithAnimator.LayersStateHash[(int)b])
				{
					string text2;
					if (this._tagHashToName.ContainsKey(currentAnimatorStateInfo.tagHash))
					{
						if (objWithAnimator.IsLogEnabled)
						{
						}
						string text = this._tagHashToName[currentAnimatorStateInfo.tagHash];
						this.OnNewStateTag(b, text);
						text2 = string.Format("Tag <{0}>", text);
					}
					else if (objWithAnimator.GetCurrentAnimatorClipInfo((int)b).Length > 0)
					{
						if (objWithAnimator.IsLogEnabled)
						{
						}
						text2 = objWithAnimator.GetCurrentAnimatorClipInfo((int)b)[0].clip.name;
						this.OnNewStateAnimation(b, text2);
					}
					else
					{
						if (objWithAnimator.IsLogEnabled)
						{
						}
						text2 = string.Format("Unknown tag with hash {0}", currentAnimatorStateInfo.tagHash);
					}
					objWithAnimator.UpdateLayerInfo((int)b, currentAnimatorStateInfo.fullPathHash, text2);
				}
				b += 1;
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (this._handsHandler != null)
		{
			this._handsHandler.OnSetHand -= this.ChangeHand;
		}
		if (this._fishHandler != null)
		{
			this._fishHandler.OnCatch -= this.CatchFishAction;
		}
	}

	[SerializeField]
	protected GameObject _gripPrefab;

	[SerializeField]
	protected ChumBall _pChumBall;

	public RodBones rodBones;

	protected Dictionary<int, string> _tagHashToName;

	protected int[] _animatorIHashes;

	protected int[] _animatorFHashes;

	protected int[] _animatorBHashes;

	protected byte[] _byteParameters;

	protected float[] _floatParameters;

	protected bool[] _boolParameters;

	protected List<ObjWithAnimator> _animators = new List<ObjWithAnimator>();

	protected T _curRod;

	protected bool _wasRestarted;

	private CastingHandsHanlders _handsHandler;

	private FishCatchingHandler _fishHandler;
}
