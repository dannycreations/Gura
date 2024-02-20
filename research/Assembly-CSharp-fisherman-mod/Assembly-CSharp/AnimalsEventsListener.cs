using System;
using System.Collections;
using System.Collections.Generic;
using SWS;
using UnityEngine;

[RequireComponent(typeof(splineMove))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AnimalsMoveAnimator))]
public class AnimalsEventsListener : MonoBehaviour
{
	public void PlayActionAtSplineEnd(int actionIndex)
	{
		PathSelector selector = this.GetSelector();
		if (selector != null)
		{
			this._animator.SetInteger(this._actionHash, actionIndex);
			this._animator.SetTrigger(this._actionTriggerHash);
			base.StartCoroutine(this.SelectSplineCoroutine(selector.switchDelay));
		}
	}

	private void Start()
	{
		this.moverAnimator = base.GetComponent<AnimalsMoveAnimator>();
		this._movers = base.GetComponents<splineMove>();
		for (int i = 0; i < this._movers.Length; i++)
		{
			this._movers[i].enabled = false;
		}
		splineMove mover = this.GetMover(this.curPathName);
		if (mover != null)
		{
			mover.enabled = true;
			mover.StartMove();
			this.moverAnimator.SetNewMover(mover);
		}
		else
		{
			this.curPathName = string.Empty;
		}
		this._animator = base.GetComponent<Animator>();
		this._actionHash = Animator.StringToHash("Action");
		this._actionTriggerHash = Animator.StringToHash("ActionTrigger");
	}

	private splineMove GetMover(string pathName)
	{
		for (int i = 0; i < this._movers.Length; i++)
		{
			if (this._movers[i].pathContainer.gameObject.name == pathName)
			{
				return this._movers[i];
			}
		}
		return null;
	}

	private PathSelector GetSelector()
	{
		for (int i = 0; i < this.pathSelectors.Count; i++)
		{
			if (this.pathSelectors[i].fromPath == this.curPathName)
			{
				return this.pathSelectors[i];
			}
		}
		return null;
	}

	private IEnumerator SelectSplineCoroutine(float delay)
	{
		yield return new WaitForSeconds(delay);
		this.SelectSpline();
		yield break;
	}

	private void SelectSpline()
	{
		this._animator.SetInteger(this._actionHash, 0);
		PathSelector selector = this.GetSelector();
		if (selector != null)
		{
			int num = Random.Range(0, selector.pathes.Count);
			this.ActivateMoverByName(selector.pathes[num]);
		}
	}

	private void ActivateMoverByName(string newPathName)
	{
		this.GetMover(this.curPathName).enabled = false;
		splineMove mover = this.GetMover(newPathName);
		if (mover != null)
		{
			this.curPathName = newPathName;
			mover.moveToPath = true;
			mover.enabled = true;
			mover.StartMove();
			this.moverAnimator.SetNewMover(mover);
		}
	}

	public string curPathName;

	public List<PathSelector> pathSelectors = new List<PathSelector>();

	private splineMove[] _movers;

	private Animator _animator;

	private AnimalsMoveAnimator moverAnimator;

	private int _actionHash;

	private int _actionTriggerHash;
}
