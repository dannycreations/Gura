using System;
using ObjectModel;
using Phy;
using UnityEngine;

namespace Boats
{
	public interface IBoatController : IBoatData
	{
		ItemSubTypes Category { get; }

		Vector3 TPMPlayerPosition { get; }

		Vector3 DriverCameraLocalPosition { get; }

		Transform DriverPivot { get; }

		Transform AnglerPivot { get; }

		Transform ShadowPivot { get; }

		float Width { get; }

		float Length { get; }

		Transform Transform { get; }

		bool IsOarPresent { get; }

		bool IsEnginePresent { get; }

		bool IsTrollingPossible { get; }

		bool IsAnchored { get; set; }

		bool IsInTransitionState { get; }

		float BoatVelocity { get; }

		Vector3 BoatCollisionImpulse { get; }

		float Stamina { get; }

		bool IsRowing { get; }

		bool IsEngineForceActive { get; }

		BoatState State { get; }

		Animation BoatAnimation { get; }

		RodPodController RodSlots { get; }

		ConnectedBodiesSystem Sim { get; }

		SimulationThread SimThread { get; }

		bool IsTrolling { get; }

		event Action<bool> FishingModeSwitched;

		bool IsReadyForRod { get; }

		bool IsOpenMap { get; }

		sbyte SavedFishingSlot { get; }

		void Teleport(Vector3 position, Quaternion rotation);

		void Rent();

		void OnRentExpired();

		void TakeControll(PlayerController driver);

		bool IsBeingLookedAt(Collider collider);

		void CameraSetActive(bool flag);

		void FreezeCamera(bool flag);

		void SetFishPhotoMode(bool flag);

		void InitTransitionToZeroXRotation();

		void TransitToZeroXRotation();

		void FinalizeTransitionToZeroXRotation();

		void Update();

		void LateUpdate();

		bool HiddenLeave();

		void SetBoatInExitZone(bool flag);

		void Destroy();

		void OnTimeChanged();

		void OnCloseMap();

		void SetVisibility(bool flag);

		void DampenRodForce();

		bool CantOpenInventory { get; }

		void SetExternalGlobalRotation(Quaternion rotation);

		void OnPlayerExternalControllReleased();

		void DisableSound(bool flag);

		void OnPlayerEnabled();

		void OnTakeRodFromPod();

		bool IsActiveMovement { get; }
	}
}
