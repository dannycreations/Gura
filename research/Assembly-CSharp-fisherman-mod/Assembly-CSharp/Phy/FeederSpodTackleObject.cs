using System;
using System.Linq;
using ObjectModel;
using UnityEngine;

namespace Phy
{
	public class FeederSpodTackleObject : FeederTackleObject
	{
		public FeederSpodTackleObject(PhyObjectType type, float normalMass, float hangingMass, float buoyancy, Animator anim, ConnectedBodiesSystem sim, TackleBehaviour tackle)
			: base(type, normalMass, hangingMass, buoyancy, sim, tackle)
		{
			this.anim = anim;
			Chum chum = FeederHelper.FindPreparedChumActiveRodAll().FirstOrDefault<Chum>();
			if (chum != null && chum.Weight != null && !Mathf.Approximately((float)chum.Weight.Value, 0f))
			{
				anim.ResetTrigger("Open");
				anim.SetTrigger("Close");
			}
			else
			{
				anim.ResetTrigger("Close");
				anim.SetTrigger("Open");
			}
		}

		public override void OnSetFilled(bool isFilled)
		{
			base.OnSetFilled(isFilled);
			if (!isFilled)
			{
				this.anim.ResetTrigger("Close");
				this.anim.SetTrigger("Open");
			}
			else
			{
				this.anim.ResetTrigger("Open");
				this.anim.SetTrigger("Close");
			}
		}

		public Animator anim;
	}
}
