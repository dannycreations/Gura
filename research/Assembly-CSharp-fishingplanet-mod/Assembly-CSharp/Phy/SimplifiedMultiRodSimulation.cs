using System;
using UnityEngine;

namespace Phy
{
	public class SimplifiedMultiRodSimulation : ConnectedBodiesSystem
	{
		public SimplifiedMultiRodSimulation(string name)
			: base(name)
		{
		}

		public RodAndTackleOnPodObject AddRod(BendingSegment segment, RodBehaviour rod, TackleBehaviour tackle, GameFactory.RodSlot slot, RodOnPodBehaviour.TransitionData transitionData)
		{
			return new RodAndTackleOnPodObject(segment, rod, tackle, slot, this, transitionData);
		}

		public SimplifiedFishObject AddFish(RodAndTackleOnPodObject rod, Vector3 position, float mass, float force, float speed, float length)
		{
			rod.fishObject = new SimplifiedFishObject(this, mass, position, length, force, speed);
			rod.fishForce = force;
			return rod.fishObject;
		}

		public void HookFish(RodAndTackleOnPodObject rod)
		{
			rod.fishToHookSpring = new Spring(rod.fishObject.Mouth, rod.hookMass, 0f, (rod.fishObject.Mouth.Position - rod.hookMass.Position).magnitude, 0.002f);
			rod.Connections.Add(rod.fishToHookSpring);
			base.Connections.Add(rod.fishToHookSpring);
			this.RefreshObjectArrays(true);
		}

		public void EscapeFish(RodAndTackleOnPodObject rod)
		{
			if (rod.fishToHookSpring != null)
			{
				rod.Connections.Remove(rod.fishToHookSpring);
				base.RemoveConnection(rod.fishToHookSpring);
				this.RefreshObjectArrays(true);
			}
		}

		public void RemoveFish(RodAndTackleOnPodObject rod)
		{
			if (rod.fishObject != null)
			{
				rod.fishObject.Remove();
				rod.fishObject = null;
			}
		}

		public Magnet AddMagnet(Mass hookMass, PhyObject fishObject)
		{
			Mass mass = fishObject.Masses[0];
			Magnet magnet = new Magnet(mass, hookMass, 25f, 0.03f, 25f, 0.06f);
			base.Connections.Add(magnet);
			this.RefreshObjectArrays(true);
			return magnet;
		}

		public void DestroyMagnet(Magnet magnet)
		{
			magnet.ClearReferenceMass();
			base.RemoveConnection(magnet);
			this.RefreshObjectArrays(true);
		}
	}
}
