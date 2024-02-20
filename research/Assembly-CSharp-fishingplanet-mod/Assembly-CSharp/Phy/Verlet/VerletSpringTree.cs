using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Phy.Verlet
{
	public class VerletSpringTree : VerletObject
	{
		public VerletSpringTree(ConnectedBodiesSystem sim, Transform root, float mass, float friction)
			: base(PhyObjectType.Undefined, sim)
		{
			this.rootMass = base.addMass(1f, root.position, Mass.MassType.Unknown);
			this.rootMass.Rotation = root.rotation;
			this.mass = mass;
			this.friction = friction;
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
					VerletSpring verletSpring = base.addSpring(m, array[num], this.friction, -1f, false);
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

		public List<Transform> NodesList;

		protected VerletMass rootMass;

		protected float mass;

		protected float friction;
	}
}
