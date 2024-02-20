using System;
using UnityEngine;

public class ObjWithAnimator
{
	public ObjWithAnimator(AnimatedObject aObj)
	{
		this.AnimatorParent = aObj.obj;
		this.CreateLayersData(aObj.layersCount);
		this._isAllTimeActiveObject = aObj.isAllTimeActiveObject;
		this._syncLayerIndex = aObj.syncLayerIndex;
		this._isLogEnabled = aObj.isLogEnabled;
	}

	public ObjWithAnimator(GameObject obj)
	{
		this.AnimatorParent = obj;
		this.CreateLayersData(0);
		this._isAllTimeActiveObject = false;
		this._syncLayerIndex = 0;
	}

	public GameObject GameObject
	{
		get
		{
			return this._animatorParent;
		}
	}

	protected GameObject AnimatorParent
	{
		get
		{
			return this._animatorParent;
		}
		set
		{
			this._animatorParent = value;
			this._animator = ((!(value != null)) ? null : value.GetComponent<Animator>());
		}
	}

	public Animator Animator
	{
		get
		{
			return this._animator;
		}
	}

	public int[] LayersStateHash
	{
		get
		{
			return this._layersStateHash;
		}
	}

	public string[] LayersClips
	{
		get
		{
			return this._layersClips;
		}
	}

	public bool IsAllTimeActiveObject
	{
		get
		{
			return this._isAllTimeActiveObject;
		}
	}

	public int SyncLayerIndex
	{
		get
		{
			return this._syncLayerIndex;
		}
	}

	public bool IsLogEnabled
	{
		get
		{
			return this._isLogEnabled;
		}
	}

	private void CreateLayersData(int layersCount)
	{
		this._layersStateHash = new int[layersCount];
		this._layersClips = new string[layersCount];
		this._layersStateHashForRestore = new int[layersCount];
	}

	public void UpdateLayerInfo(int layerIndex, int stateHash, string clipName)
	{
		this._layersStateHash[layerIndex] = stateHash;
		this._layersClips[layerIndex] = clipName;
	}

	public virtual void Activate(bool flag)
	{
		if (!this._isAllTimeActiveObject && this._animatorParent != null)
		{
			this._animatorParent.SetActive(flag);
		}
	}

	public virtual void SetBool(int keyHash, bool value, TPMMecanimBParameter keyType)
	{
		if (this._animatorParent != null && this._animatorParent.activeInHierarchy)
		{
			this._animator.SetBool(keyHash, value);
		}
	}

	public virtual void SetInteger(int keyHash, int value)
	{
		if (this._animatorParent != null && this._animatorParent.activeInHierarchy)
		{
			this._animator.SetInteger(keyHash, value);
		}
	}

	public virtual void SetFloat(int keyHash, float value)
	{
		if (this._animatorParent != null && this._animatorParent.activeInHierarchy)
		{
			this._animator.SetFloat(keyHash, value);
		}
	}

	public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layerIndex)
	{
		return this._animator.GetCurrentAnimatorStateInfo(layerIndex);
	}

	public AnimatorClipInfo[] GetCurrentAnimatorClipInfo(int layerIndex)
	{
		return this._animator.GetCurrentAnimatorClipInfo(layerIndex);
	}

	public void SaveAnimatorStates()
	{
		for (int i = 0; i < this._layersStateHash.Length; i++)
		{
			this._layersStateHashForRestore[i] = this._layersStateHash[i];
		}
	}

	public void RestoreAnimatorStates()
	{
		for (int i = 0; i < this._layersStateHashForRestore.Length; i++)
		{
			if (this._animator != null)
			{
				this._animator.CrossFade(this._layersStateHashForRestore[i], 0.2f, i);
			}
		}
	}

	public virtual void OnDestroy()
	{
	}

	private GameObject _animatorParent;

	protected Animator _animator;

	private int[] _layersStateHash;

	private int[] _layersStateHashForRestore;

	private string[] _layersClips;

	private bool _isAllTimeActiveObject;

	private int _syncLayerIndex;

	private bool _isLogEnabled;
}
