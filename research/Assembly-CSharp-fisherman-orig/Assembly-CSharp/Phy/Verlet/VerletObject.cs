using System;
using System.Linq;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy.Verlet
{
	public class VerletObject : PhyObject
	{
		public VerletObject(PhyObjectType type, ConnectedBodiesSystem sim)
			: base(type, sim)
		{
		}

		public VerletObject(ConnectedBodiesSystem sim, VerletObject source)
			: base(sim, source)
		{
		}

		protected VerletMass addMass(float mass, Vector3 position, Mass.MassType type)
		{
			VerletMass verletMass = new VerletMass(this.Sim, mass, position, type);
			verletMass.Sim = this.Sim;
			verletMass.Radius = 0.05f;
			verletMass.Buoyancy = 0f;
			verletMass.StopMass();
			verletMass.IgnoreEnvironment = true;
			verletMass.Collision = Mass.CollisionType.Full;
			verletMass.StaticFrictionFactor = 0.01f;
			verletMass.SlidingFrictionFactor = 0.01f;
			base.Masses.Add(verletMass);
			return verletMass;
		}

		protected VerletSpring addSpring(VerletMass m1, VerletMass m2, float friction, float customLength = -1f, bool compressible = false)
		{
			float num = ((customLength <= 0f) ? (m1.Position4f - m2.Position4f).Magnitude() : customLength);
			VerletSpring verletSpring = new VerletSpring(m1, m2, num, friction, compressible);
			base.Connections.Add(verletSpring);
			return verletSpring;
		}

		protected int addTetrahedron(float mass, Vector3 a, Vector3 b, Vector3 c, Vector3 d, float customEdgeLength = -1f)
		{
			int count = base.Masses.Count;
			float num = mass * 0.25f;
			VerletMass verletMass = this.addMass(num, a, Mass.MassType.Fish);
			VerletMass verletMass2 = this.addMass(num, b, Mass.MassType.Fish);
			VerletMass verletMass3 = this.addMass(num, c, Mass.MassType.Fish);
			VerletMass verletMass4 = this.addMass(num, d, Mass.MassType.Fish);
			this.addSpring(verletMass, verletMass2, 0.1f, customEdgeLength, false);
			this.addSpring(verletMass, verletMass3, 0.1f, customEdgeLength, false);
			this.addSpring(verletMass, verletMass4, 0.1f, customEdgeLength, false);
			this.addSpring(verletMass2, verletMass3, 0.1f, customEdgeLength, false);
			this.addSpring(verletMass3, verletMass4, 0.1f, customEdgeLength, false);
			this.addSpring(verletMass4, verletMass2, 0.1f, customEdgeLength, false);
			return count;
		}

		protected TetrahedronBallJoint addBallJoint(int t1, int t2, float stiffness, float friction)
		{
			TetrahedronBallJoint tetrahedronBallJoint = new TetrahedronBallJoint(new Mass[]
			{
				base.Masses[t1],
				base.Masses[t1 + 1],
				base.Masses[t1 + 2],
				base.Masses[t1 + 3]
			}.Cast<VerletMass>().ToArray<VerletMass>(), new Mass[]
			{
				base.Masses[t2],
				base.Masses[t2 + 1],
				base.Masses[t2 + 2],
				base.Masses[t2 + 3]
			}.Cast<VerletMass>().ToArray<VerletMass>(), stiffness, friction);
			base.Connections.Add(tetrahedronBallJoint);
			return tetrahedronBallJoint;
		}

		public const float DefaultSpringFriction = 0.1f;

		public const float DefaultSurfaceFriction = 0.01f;

		public const float DefaultRadius = 0.05f;
	}
}
