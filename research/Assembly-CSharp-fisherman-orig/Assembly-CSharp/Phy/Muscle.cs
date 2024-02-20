using System;
using UnityEngine;

namespace Phy
{
	public class Muscle : ConnectionBase
	{
		public Muscle(Mass mass1, Mass mass2)
			: base(mass1, mass2, -1)
		{
		}

		public float Strain
		{
			get
			{
				return this.currentStrain;
			}
			set
			{
				this.destStrain = Mathf.Clamp(value, 0f, 1f);
			}
		}

		public float CurrentStrain
		{
			get
			{
				return this.currentStrain;
			}
			set
			{
				this.currentStrain = value;
			}
		}

		public int ControlState
		{
			get
			{
				if (Mathf.Approximately(this.destStrain, this.currentStrain))
				{
					return 0;
				}
				return (int)Mathf.Sign(this.destStrain - this.currentStrain);
			}
		}

		public override void Solve()
		{
			if (!Mathf.Approximately(this.currentStrain, this.destStrain))
			{
				float num = ((this.currentStrain >= this.destStrain) ? (-this.RelaxSpeed * 0.0004f) : (this.ContractSpeed * 0.0004f));
				if (Mathf.Abs(this.destStrain - this.currentStrain) > Mathf.Abs(num))
				{
					this.currentStrain += num;
				}
				else
				{
					this.currentStrain = this.destStrain;
				}
			}
		}

		public new float MaxForce;

		public float ContractSpeed;

		public float RelaxSpeed;

		private float destStrain;

		private float currentStrain;
	}
}
