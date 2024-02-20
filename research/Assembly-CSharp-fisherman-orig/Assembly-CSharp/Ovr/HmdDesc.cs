using System;
using System.Runtime.InteropServices;

namespace Ovr
{
	public struct HmdDesc
	{
		internal HmdDesc(HmdDesc_Raw raw)
		{
			this.Handle = raw.Handle;
			this.Type = (HmdType)raw.Type;
			this.ProductName = Marshal.PtrToStringAnsi(raw.ProductName);
			this.Manufacturer = Marshal.PtrToStringAnsi(raw.Manufacturer);
			this.VendorId = raw.VendorId;
			this.ProductId = raw.ProductId;
			this.SerialNumber = raw.SerialNumber;
			this.FirmwareMajor = raw.FirmwareMajor;
			this.FirmwareMinor = raw.FirmwareMinor;
			this.CameraFrustumHFovInRadians = raw.CameraFrustumHFovInRadians;
			this.CameraFrustumVFovInRadians = raw.CameraFrustumVFovInRadians;
			this.CameraFrustumNearZInMeters = raw.CameraFrustumNearZInMeters;
			this.CameraFrustumFarZInMeters = raw.CameraFrustumFarZInMeters;
			this.HmdCaps = raw.HmdCaps;
			this.TrackingCaps = raw.TrackingCaps;
			this.DistortionCaps = raw.DistortionCaps;
			this.Resolution = raw.Resolution;
			this.WindowsPos = raw.WindowsPos;
			this.DefaultEyeFov = new FovPort[] { raw.DefaultEyeFov_0, raw.DefaultEyeFov_1 };
			this.MaxEyeFov = new FovPort[] { raw.MaxEyeFov_0, raw.MaxEyeFov_1 };
			this.EyeRenderOrder = new Eye[]
			{
				Eye.Left,
				Eye.Right
			};
			this.DisplayDeviceName = Marshal.PtrToStringAnsi(raw.DisplayDeviceName);
			this.DisplayId = raw.DisplayId;
		}

		public IntPtr Handle;

		public HmdType Type;

		public string ProductName;

		public string Manufacturer;

		public short VendorId;

		public short ProductId;

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

		public FovPort[] DefaultEyeFov;

		public FovPort[] MaxEyeFov;

		public Eye[] EyeRenderOrder;

		public Sizei Resolution;

		public Vector2i WindowsPos;

		public string DisplayDeviceName;

		public int DisplayId;
	}
}
