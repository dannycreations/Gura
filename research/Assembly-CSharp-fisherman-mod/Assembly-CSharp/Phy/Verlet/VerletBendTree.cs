using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Phy.Verlet
{
	public class VerletBendTree : VerletObject
	{
		public VerletBendTree(ConnectedBodiesSystem sim, Transform root, float mass, float friction, float rotationalStiffness, float displacementStiffness, float normalStiffness)
			: base(PhyObjectType.Undefined, sim)
		{
			this.rootMass = base.addMass(1f, root.position, Mass.MassType.Unknown);
			this.rootMass.Rotation = root.rotation;
			this.mass = mass;
			this.friction = friction;
			this.rotationalStiffness = rotationalStiffness;
			this.displacementStiffness = displacementStiffness;
			this.normalStiffness = normalStiffness;
			this.NodesList = new List<Transform>();
			this.NodesList.Add(root);
			this.buildChildren(root, this.rootMass);
			for (int i = 0; i < base.Masses.Count; i++)
			{
				base.Masses[i].MassValue = this.mass / (float)base.Masses.Count;
			}
			this.UpdateSim();
		}

		private void buildChildren(Transform t, VerletMass m)
		{
			VerletMass[] array = new VerletMass[t.childCount];
			int num = 0;
			IEnumerator enumerator = t.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					array[num] = base.addMass(1f, transform.position, Mass.MassType.Unknown);
					array[num].IgnoreEnvForces = false;
					array[num].Rotation = transform.rotation;
					VerletBend verletBend = this.addVerletBend(m, array[num], this.friction, this.rotationalStiffness, this.displacementStiffness, this.normalStiffness);
					this.NodesList.Add(transform);
					num++;
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
			num = 0;
			IEnumerator enumerator2 = t.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					object obj2 = enumerator2.Current;
					Transform transform2 = (Transform)obj2;
					this.buildChildren(transform2, array[num]);
					num++;
				}
			}
			finally
			{
				IDisposable disposable2;
				if ((disposable2 = enumerator2 as IDisposable) != null)
				{
					disposable2.Dispose();
				}
			}
		}

		protected VerletBend addVerletBend(VerletMass m1, VerletMass m2, float f, float rs, float ds, float ns)
		{
			VerletBend verletBend = new VerletBend(m1, m2, f, rs, ds, ns);
			base.Connections.Add(verletBend);
			return verletBend;
		}

		public List<Transform> NodesList;

		protected VerletMass rootMass;

		protected float mass;

		protected float friction;

		protected float rotationalStiffness;

		protected float displacementStiffness;

		protected float normalStiffness;
	}
}
