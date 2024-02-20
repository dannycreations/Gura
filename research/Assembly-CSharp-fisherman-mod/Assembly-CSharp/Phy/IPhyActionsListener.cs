using System;
using UnityEngine;

namespace Phy
{
	public interface IPhyActionsListener
	{
		void MassCreated(Mass m);

		void MassDestroyed(Mass m);

		void ConnectionCreated(ConnectionBase c);

		void ConnectionDestroyed(ConnectionBase c);

		void ObjectCreated(PhyObject b);

		void ObjectDestroyed(PhyObject b);

		void StructureCleared();

		void MassChanged(int UID, float newMass);

		void PositionChanged(int UID, Vector3 newPosition);

		void LocalPositionChanged(int UID, Vector3 newLocalPosition);

		void RotationChanged(int UID, Quaternion newRotation);

		void VelocityChanged(int UID, Vector3 newVelocity);

		void ForceApplied(int UID, Vector3 force);

		void MotorChanged(int UID, Vector3 motor);

		void WaterMotorChanged(int UID, Vector3 watermotor);

		void IsKinematicChanged(int UID, bool isKinematic);

		void IsRefChanged(int UID, bool isRef);

		void VisualPositionOffsetChanged(int UID, Vector3 visualPositionOffset);

		void StopMassCalled(int UID);

		void ResetCalled(int UID);

		void KinematicTranslateCalled(int UID, Vector3 translate);

		void AirDragConstantChanged(int UID, float airDrag);

		void WaterDragConstantChanged(int UID, float waterDrag);

		void GravityOverrideChanged(int UID, float gravity);

		void BuoyancyChanged(int UID, float buoyancy);

		void BuoyancySpeedMultiplierChanged(int UID, float mult);

		void VelocityLimitChanged(int UID, float limit);

		void HorizontalVelocityLimitChanged(int UID, float limit);

		void TopWaterStrikeSignal(int UID);

		void MassTypeChanged(int UID, Mass.MassType newMassType);

		void BoatInertiaChanged(int UID);

		void DisableSimualtionChanged(int UID, bool disabled);

		void CollisionTypeChanged(int UID, Mass.CollisionType newCollisionType);

		void ConnectionNeedSyncMark(int UID);

		void ObjectNeedSyncMark(int UID);

		void FishIsHookedChanged(int UID, int hookState);

		void VelocityLimitChanged(float limit);

		void GlobalReset();

		void VisualPositionOffsetGlobalChanged(Vector3 vpo);
	}
}
