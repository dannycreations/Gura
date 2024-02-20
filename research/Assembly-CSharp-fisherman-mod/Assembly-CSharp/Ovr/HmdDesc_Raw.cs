using System;
using System.Runtime.InteropServices;

namespace Ovr
{
	internal struct HmdDesc_Raw
	{
		public IntPtr Handle;

		public uint Type;

		public IntPtr ProductName;

		public IntPtr Manufacturer;

		public short VendorId;

		public short ProductId;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)]
		public string SerialNumber;

		public short FirmwareMajor;

		public short FirmwareMinor;

		public float CameraFrustumHFovInRadians;

		public float CameraFrustumVFovInRadians;

		public float CameraFrustumNearZInMeters;

		public float CameraFrustumFarZInMeters;

		public uint HmdCaps;

		public uint TrackingCaps;

		public uint DistortionCaps;

		public FovPort DefaultEyeFov_0;

		public FovPort DefaultEyeFov_1;

		public FovPort MaxEyeFov_0;

		public FovPort MaxEyeFov_1;

		public Eye EyeRenderOrder_0;

		public Eye EyeRenderOrder_1;

		public Sizei Resolution;

		public Vector2i WindowsPos;

		public IntPtr DisplayDeviceName;

		public int DisplayId;
	}
}
