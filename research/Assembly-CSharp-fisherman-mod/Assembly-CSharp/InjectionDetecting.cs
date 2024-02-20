using System;
using CodeStage.AntiCheat.Detectors;
using UnityEngine;

public class InjectionDetecting : MonoBehaviour
{
	private void Start()
	{
		InjectionDetecting.Instance = this;
		SpeedHackDetector.StartDetection(delegate
		{
			this._cheaterSpeedHack = true;
		});
		WallHackDetector.StartDetection(delegate
		{
			this._cheaterWallHack = true;
		});
	}

	internal void Update()
	{
		if (PhotonConnectionFactory.Instance == null || !PhotonConnectionFactory.Instance.IsAuthenticated || !PhotonConnectionFactory.Instance.IsConnectedToGameServer)
		{
			return;
		}
		if (this._cheaterDLL && !this._sentInfo)
		{
			this._sentInfo = true;
		}
		if (this._cheaterSpeedHack && !this._sentInfo)
		{
			this._sentInfo = true;
			PhotonConnectionFactory.Instance.SaveTelemetryInfo(2, "SpeedHack");
		}
		if (this._cheaterWallHack && !this._sentInfo)
		{
			this._sentInfo = true;
			PhotonConnectionFactory.Instance.SaveTelemetryInfo(2, "WallHack");
		}
	}

	public static InjectionDetecting Instance { get; private set; }

	public void DetectedLineLengthHack(float currentLength, float targetLength)
	{
		if (!this._cheaterLineLengthHackSent)
		{
			this._cheaterLineLengthHackSent = true;
			PhotonConnectionFactory.Instance.SaveTelemetryInfo(2, string.Format("LineLengthHack currentLength: {0}, targetLength: {1}", currentLength, targetLength));
		}
	}

	private bool _cheaterDLL;

	private bool _cheaterSpeedHack;

	private bool _cheaterWallHack;

	private bool _cheaterLineLengthHackSent;

	private bool _sentInfo;
}
