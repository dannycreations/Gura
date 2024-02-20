using System;
using Ovr;
using UnityEngine;

public class OVRTracker
{
	public bool isPresent
	{
		get
		{
			return (OVRManager.capiHmd.GetTrackingState(0.0).StatusFlags & 32U) != 0U;
		}
	}

	public bool isPositionTracked
	{
		get
		{
			return (OVRManager.capiHmd.GetTrackingState(0.0).StatusFlags & 2U) != 0U;
		}
	}

	public bool isEnabled
	{
		get
		{
			uint trackingCaps = OVRManager.capiHmd.GetDesc().TrackingCaps;
			return (trackingCaps & 64U) != 0U;
		}
		set
		{
			uint num = 48U;
			if (value)
			{
				num |= 64U;
			}
			OVRManager.capiHmd.ConfigureTracking(num, 0U);
		}
	}

	public OVRTracker.Frustum frustum
	{
		get
		{
			HmdDesc desc = OVRManager.capiHmd.GetDesc();
			return new OVRTracker.Frustum
			{
				nearZ = desc.CameraFrustumNearZInMeters,
				farZ = desc.CameraFrustumFarZInMeters,
				fov = 57.29578f * new Vector2(desc.CameraFrustumHFovInRadians, desc.CameraFrustumVFovInRadians)
			};
		}
	}

	public OVRPose GetPose(double predictionTime = 0.0)
	{
		double num = Hmd.GetTimeInSeconds() + predictionTime;
		return OVRManager.capiHmd.GetTrackingState(num).CameraPose.ToPose(true);
	}

	public struct Frustum
	{
		public float nearZ;

		public float farZ;

		public Vector2 fov;
	}
}
