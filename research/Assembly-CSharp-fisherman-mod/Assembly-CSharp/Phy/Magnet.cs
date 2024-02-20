using System;
using UnityEngine;

namespace Phy
{
	public sealed class Magnet : ConnectionBase
	{
		public Magnet(Mass mass1, Mass mass2, float repulsionConstant, float repulsionDistance, float attractionConstant, float attractionDistance)
			: base(mass1, mass2, -1)
		{
			this.RepulsionConstant = repulsionConstant;
			this.RepulsionDistance = repulsionDistance;
			this.AttractionConstant = attractionConstant;
			this.AttractionDistance = attractionDistance;
		}

		public Magnet(Simulation sim, Magnet source)
			: base(sim.DictMasses[source.Mass1.UID], sim.DictMasses[source.Mass2.UID], source.UID)
		{
			this.RepulsionConstant = source.RepulsionConstant;
			this.RepulsionDistance = source.RepulsionDistance;
			this.AttractionConstant = source.AttractionConstant;
			this.AttractionDistance = source.AttractionDistance;
			this.Sync(source);
		}

		public bool IsAttracting
		{
			get
			{
				return this._isAttracting;
			}
			set
			{
				this._isAttracting = value;
				base.ConnectionNeedSyncMark();
			}
		}

		public override void Sync(ConnectionBase source)
		{
			base.Sync(source);
			Magnet magnet = source as Magnet;
			this._isAttracting = magnet.IsAttracting;
		}

		public override void Solve()
		{
			if (this._isAttracting)
			{
				Vector3 vector = base.Mass1.Position - base.Mass2.Position;
				float magnitude = vector.magnitude;
				if (magnitude > this.AttractionDistance)
				{
					return;
				}
				float num = (this.AttractionDistance - magnitude) / this.AttractionDistance;
				Vector3 vector2;
				if (num < 0.9f)
				{
					vector2 = vector.normalized * this.AttractionConstant * num;
				}
				else
				{
					vector2 = Vector3.zero;
				}
				base.Mass2.ApplyForce(vector2, false);
				base.Mass1.ApplyForce(-vector2, false);
			}
			else
			{
				Vector3 vector3 = base.Mass2.Position - base.Mass1.Position;
				float magnitude2 = vector3.magnitude;
				if (magnitude2 > this.RepulsionDistance)
				{
					return;
				}
				Vector3 vector2 = vector3.normalized * this.RepulsionConstant * (this.RepulsionDistance - magnitude2);
				base.Mass2.ApplyForce(vector2, false);
				base.Mass1.ApplyForce(-vector2, false);
				if (this.referenceMass != null)
				{
					this.ApplyForceToReferenceMass(vector2);
				}
			}
		}

		public void AddReferenceMass(Mass referenceMass, float sensitivity)
		{
			this.referenceMass = referenceMass;
			this.sensitivity = sensitivity;
		}

		public void ClearReferenceMass()
		{
			this.referenceMass = null;
		}

		private void ApplyForceToReferenceMass(Vector3 force)
		{
			this.referenceMass.ApplyForce(force * this.sensitivity * 2f, false);
		}

		public override string ToString()
		{
			return string.Format("  Magnet {0}: Repultion: {1}, Len:{2}, Mode: {3}", new object[]
			{
				base.ToString(),
				this.RepulsionConstant,
				this.RepulsionDistance,
				(!this.IsAttracting) ? "R" : "A"
			});
		}

		public float RepulsionConstant;

		public float RepulsionDistance;

		public float AttractionConstant;

		public float AttractionDistance;

		private bool _isAttracting;

		private Mass referenceMass;

		private float sensitivity;
	}
}
