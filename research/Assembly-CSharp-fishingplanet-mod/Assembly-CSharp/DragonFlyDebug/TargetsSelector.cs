using System;
using System.Collections.Generic;
using UnityEngine;

namespace DragonFlyDebug
{
	public class TargetsSelector : MonoBehaviour
	{
		private void Start()
		{
			this.ChangeTarget(Random.Range(0, this.targets.Count));
		}

		private void Update()
		{
			if (Input.GetKeyUp(101))
			{
				this._roamer.EscapeAction();
				this._curTargetIndex = -1;
			}
			else if (Input.GetKeyUp(116))
			{
				int num;
				do
				{
					num = Random.Range(0, this.targets.Count);
				}
				while (num == this._curTargetIndex);
				this.ChangeTarget(num);
			}
			else if (Input.GetKeyUp(49))
			{
				this.ChangeTarget(0);
			}
			else if (Input.GetKeyUp(50))
			{
				this.ChangeTarget(1);
			}
			else if (Input.GetKeyUp(51))
			{
				this.ChangeTarget(2);
			}
		}

		private void ChangeTarget(int newTargetIndex)
		{
			if (this._curTargetIndex != newTargetIndex)
			{
				this._curTargetIndex = newTargetIndex;
				TargetsSelector.Target target = this.targets[this._curTargetIndex];
				this._roamer.SetTarget(target.Transform, target.LocalPos);
			}
		}

		public List<TargetsSelector.Target> targets;

		public DragonFlyController _roamer;

		private int _curTargetIndex = -1;

		[Serializable]
		public class Target
		{
			public Transform Transform;

			public Vector3 LocalPos;
		}
	}
}
