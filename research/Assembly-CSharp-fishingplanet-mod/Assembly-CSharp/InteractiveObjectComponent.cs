using System;
using ObjectModel;
using UnityEngine;

public class InteractiveObjectComponent : MonoBehaviour
{
	private void Awake()
	{
		this._animator = base.GetComponent<Animator>();
	}

	public void OnCreate(bool ready, bool isNew)
	{
		if (this._animator != null)
		{
			this._animator.SetBool("IsNew", isNew);
			if (ready)
			{
				this._animator.SetTrigger("Ready");
			}
			else
			{
				this._animator.SetTrigger("Complete");
			}
		}
	}

	public void OnSelectedInteractionReady()
	{
		if (this._animator != null)
		{
			this._animator.SetTrigger("Ready");
		}
	}

	public void OnSelectedBeforeInteractionStart()
	{
		if (this._animator != null)
		{
			this._animator.SetTrigger("Before");
		}
	}

	public void OnSelectedInteractionComplete()
	{
		if (this._animator != null)
		{
			this._animator.SetTrigger("Complete");
		}
	}

	public void OnSelectedAfterInteractionFinish()
	{
		if (this._animator != null)
		{
			this._animator.SetTrigger("After");
		}
	}

	public void OnUnselected()
	{
		if (this._animator != null)
		{
			this._animator.SetTrigger("Unselected");
		}
	}

	public InteractiveObject obj;

	private Animator _animator;
}
