using System;
using ExitGames.Client.Photon;
using Newtonsoft.Json;
using ObjectModel;
using UnityEngine;

public static class PhotonConnectionFactory
{
	static PhotonConnectionFactory()
	{
		LoadbalancingPeer.GetPlayerSessionInfo = () => JsonConvert.SerializeObject(new PlayerSessionInfo
		{
			MacAddress = SysInfoHelper.GetSysInfoCached().Mac
		});
		JsonConvert.SerializeObject(new PlayerSessionInfo());
	}

	public static bool HasPhotonServerConnection
	{
		get
		{
			return PhotonConnectionFactory.connection != null;
		}
	}

	public static IPhotonServerConnection Instance
	{
		get
		{
			if (PhotonConnectionFactory.connection == null)
			{
				PhotonConnectionFactory.connection = PhotonConnectionFactory.CreateConnection();
				ClientMissionsManager.ChannelGetProfile = () => PhotonConnectionFactory.connection.Profile;
				ClientMissionsManager.ChannelConfigurationRequest = new ClientMissionsManager.InteractiveObjectConfigurationRequest(PhotonConnectionFactory.connection.GetMissionInteractiveObject);
				ClientMissionsManager.ChannelHintsReceived = new ClientMissionsManager.HintsReceivedHandler(PhotonConnectionFactory.connection.OnMissionHintsReceived);
				ClientMissionsManager.ChannelInteract = new ClientMissionsManager.InteractHandler(PhotonConnectionFactory.connection.SendMissionInteraction);
				ClientMissionsManager.ChannelCompleteCondition = new ClientMissionsManager.CompleteConditionHandler(PhotonConnectionFactory.connection.CompleteMissionClientCondition);
				HintMessage.FormatValue = delegate(object value, HintMessageParameterFormat format)
				{
					switch (format)
					{
					case HintMessageParameterFormat.Length:
						return string.Format("{0}{1}", Math.Ceiling((double)MeasuringSystemManager.LineLength(Convert.ToSingle(value))), MeasuringSystemManager.LineLengthSufix());
					case HintMessageParameterFormat.LengthCm:
						return string.Format("{0}{1}", Math.Ceiling((double)MeasuringSystemManager.LineLeashLength(Convert.ToSingle(value))), MeasuringSystemManager.LineLeashLengthSufix());
					case HintMessageParameterFormat.Weight:
						return string.Format("{0}{1}", MeasuringSystemManager.FishWeight(Convert.ToSingle(value)), MeasuringSystemManager.FishWeightSufix());
					case HintMessageParameterFormat.Temperature:
						return string.Format("{0}{1}", MeasuringSystemManager.Temperature(Convert.ToSingle(value)), MeasuringSystemManager.TemperatureSufix());
					case HintMessageParameterFormat.Speed:
						return string.Format("{0}{1}", MeasuringSystemManager.WindSpeed(Convert.ToSingle(value)), MeasuringSystemManager.WindSpeedSufix());
					case HintMessageParameterFormat.SpeedKm:
						return string.Format("{0}{1}", MeasuringSystemManager.Speed(Convert.ToSingle(value)), MeasuringSystemManager.SpeedSufix());
					case HintMessageParameterFormat.Hour:
						return MeasuringSystemManager.TimeString(new DateTime(2000, 1, 1, Convert.ToInt32(value), 0, 0));
					default:
						return null;
					}
				};
			}
			return PhotonConnectionFactory.connection;
		}
	}

	public static void Clear()
	{
		PhotonConnectionFactory.connection.UnsubscribeProfileEvents();
		PhotonConnectionFactory.connection = null;
		ClientMissionsManager.ChannelGetProfile = null;
		ClientMissionsManager.ChannelConfigurationRequest = null;
		ClientMissionsManager.ChannelHintsReceived = null;
		ClientMissionsManager.ChannelInteract = null;
		ClientMissionsManager.ChannelCompleteCondition = null;
		ClientMissionsManager.Clear();
	}

	private static IPhotonServerConnection CreateConnection()
	{
		Application.runInBackground = true;
		PhotonPeer.RegisterType(typeof(Point3), 86, new SerializeMethod(VectorSerializationHelper.SerializePoint3), new DeserializeMethod(VectorSerializationHelper.DeserializePoint3));
		ClientObjectModelBinder.Init();
		SysInfoHelper.Init();
		GameObject gameObject = new GameObject();
		gameObject.AddComponent<PhotonDispatcher>();
		gameObject.name = "PhotonMono";
		gameObject.hideFlags = 1;
		return new PhotonServerConnection();
	}

	private static IPhotonServerConnection connection;
}
