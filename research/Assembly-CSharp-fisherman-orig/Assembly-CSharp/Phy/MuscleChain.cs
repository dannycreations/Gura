using System;
using UnityEngine;

namespace Phy
{
	public class MuscleChain
	{
		public MuscleChain(Mass[] masses, float force, float contractSpeed, float relaxSpeed)
		{
			this.masses = masses;
			this.PeristalsisFactor = 0.5f;
			this.defaultContractSpeed = contractSpeed;
			this.defaultRelaxSpeed = relaxSpeed;
			this.defaultForce = force;
			this.muscles = new Muscle[masses.Length - 1];
			for (int i = 0; i < this.muscles.Length; i++)
			{
				Muscle muscle = new Muscle(masses[i], masses[i + 1]);
				muscle.ContractSpeed = contractSpeed;
				muscle.RelaxSpeed = relaxSpeed;
				muscle.MaxForce = force;
				this.muscles[i] = muscle;
			}
		}

		public MuscleChain(Muscle[] muscles)
		{
			this.muscles = muscles;
			this.masses = new Mass[muscles.Length + 1];
			for (int i = 0; i < muscles.Length; i++)
			{
				this.masses[i] = muscles[i].Mass1;
			}
			this.masses[muscles.Length] = muscles[muscles.Length - 1].Mass2;
			this.defaultContractSpeed = muscles[0].ContractSpeed;
			this.defaultRelaxSpeed = muscles[0].RelaxSpeed;
			this.defaultForce = muscles[0].MaxForce;
		}

		public Muscle[] Muscles
		{
			get
			{
				return this.muscles;
			}
		}

		public void SetSpeedMultiplier(float sm)
		{
			for (int i = 0; i < this.muscles.Length; i++)
			{
				this.muscles[i].ContractSpeed = this.defaultContractSpeed * sm;
				this.muscles[i].RelaxSpeed = this.defaultRelaxSpeed * sm;
			}
		}

		public void SetForceMultiplier(float fm)
		{
			for (int i = 0; i < this.muscles.Length; i++)
			{
				this.muscles[i].MaxForce = this.defaultForce * fm;
			}
		}

		public void Stimulate()
		{
			this.muscles[(this.PeristalsisFactor < 0f) ? (this.muscles.Length - 1) : 0].Strain = 1f;
		}

		public void SetStrain(float strain)
		{
			for (int i = 0; i < this.muscles.Length; i++)
			{
				this.muscles[i].CurrentStrain = strain;
			}
		}

		public void AddStrain(float strain)
		{
			for (int i = 0; i < this.muscles.Length; i++)
			{
				this.muscles[i].CurrentStrain = Mathf.Max(strain, this.muscles[i].CurrentStrain);
			}
		}

		public void SetStrain(float strain, int iFirst, int iLast)
		{
			for (int i = iFirst; i <= iLast; i++)
			{
				this.muscles[i].CurrentStrain = strain;
			}
		}

		public void SetOscillatingStrain(float amp, float freq, float phase, float phaseDelta)
		{
			for (int i = 0; i < this.muscles.Length; i++)
			{
				this.muscles[i].CurrentStrain = amp * Mathf.Sin(freq * Time.time + phase + phaseDelta * (float)i);
			}
		}

		public void Update()
		{
			int num;
			int num2;
			int num3;
			if (this.PeristalsisFactor >= 0f)
			{
				num = 0;
				num2 = this.muscles.Length;
				num3 = 1;
			}
			else
			{
				num = this.muscles.Length - 1;
				num2 = -1;
				num3 = -1;
			}
			for (int num4 = num; num4 != num2; num4 += num3)
			{
				if (this.muscles[num4].ControlState == 0 && Mathf.Approximately(this.muscles[num4].Strain, 1f))
				{
					this.muscles[num4].Strain = 0f;
				}
				if (num4 + num3 != num2 && this.muscles[num4].ControlState != 0)
				{
					float num5 = (float)this.muscles[num4].ControlState * (this.muscles[num4].Strain - 1f) + 1f;
					if (num5 >= Mathf.Abs(this.PeristalsisFactor) && Mathf.Approximately(this.muscles[num4 + num3].Strain, 0f))
					{
						this.muscles[num4 + num3].Strain = 1f;
					}
				}
			}
		}

		public float PeristalsisFactor;

		private Mass[] masses;

		private Muscle[] muscles;

		private float defaultContractSpeed;

		private float defaultRelaxSpeed;

		private float defaultForce;
	}
}
