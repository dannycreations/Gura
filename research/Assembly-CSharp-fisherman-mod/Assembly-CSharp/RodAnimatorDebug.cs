using System;
using UnityEngine;

public class RodAnimatorDebug : RodAnimatorBase
{
	public RodAnimatorDebug(GameObject rod, GameObject reel, GameObject rightRoot, GameObject leftRoot, bool isBaitcasting)
		: base(reel, rightRoot, leftRoot, isBaitcasting)
	{
		this._rod = rod;
		this._rod.transform.parent = this._rightRoot.transform;
		this._rod.transform.localPosition = Vector3.zero;
		this._rod.transform.localRotation = ((!this._isBaitcasting) ? Quaternion.identity : Quaternion.Euler(0f, 0f, 180f));
		reel.transform.parent = this._rod.transform;
		reel.transform.localPosition = Vector3.zero;
		reel.transform.localRotation = ((!this._isBaitcasting) ? Quaternion.identity : Quaternion.Euler(0f, 0f, 180f));
	}

	protected override GameObject Rod
	{
		get
		{
			return this._rod;
		}
	}

	private readonly GameObject _rod;
}
