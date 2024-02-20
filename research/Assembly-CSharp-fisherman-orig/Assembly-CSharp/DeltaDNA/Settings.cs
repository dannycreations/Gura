using System;

namespace DeltaDNA
{
	public class Settings
	{
		internal Settings()
		{
			this.DebugMode = false;
			this.OnFirstRunSendNewPlayerEvent = true;
			this.OnInitSendClientDeviceEvent = true;
			this.OnInitSendGameStartedEvent = true;
			this.HttpRequestRetryDelaySeconds = 2f;
			this.HttpRequestMaxRetries = 0;
			this.HttpRequestCollectTimeoutSeconds = 30;
			this.HttpRequestEngageTimeoutSeconds = 5;
			this.BackgroundEventUpload = true;
			this.BackgroundEventUploadStartDelaySeconds = 0;
			this.BackgroundEventUploadRepeatRateSeconds = 60;
			this.UseEventStore = true;
			this.SessionTimeoutSeconds = 300;
		}

		public bool OnFirstRunSendNewPlayerEvent { get; set; }

		public bool OnInitSendClientDeviceEvent { get; set; }

		public bool OnInitSendGameStartedEvent { get; set; }

		public bool DebugMode
		{
			get
			{
				return this._debugMode;
			}
			set
			{
				Logger.SetLogLevel((!value) ? Logger.Level.WARNING : Logger.Level.DEBUG);
				this._debugMode = value;
			}
		}

		public float HttpRequestRetryDelaySeconds { get; set; }

		public int HttpRequestMaxRetries { get; set; }

		public int HttpRequestCollectTimeoutSeconds { get; set; }

		public int HttpRequestEngageTimeoutSeconds { get; set; }

		public bool BackgroundEventUpload { get; set; }

		public int BackgroundEventUploadStartDelaySeconds { get; set; }

		public int BackgroundEventUploadRepeatRateSeconds { get; set; }

		public bool UseEventStore { get; set; }

		public int SessionTimeoutSeconds { get; set; }

		internal static readonly string SDK_VERSION = "Unity SDK v4.1.4";

		internal static readonly string ENGAGE_API_VERSION = "4";

		internal static readonly string EVENT_STORAGE_PATH = "{persistent_path}/ddsdk/events/";

		internal static readonly string ENGAGE_STORAGE_PATH = "{persistent_path}/ddsdk/engage/";

		internal static readonly string LEGACY_SETTINGS_STORAGE_PATH = "{persistent_path}/GASettings.ini";

		internal static readonly string EVENT_TIMESTAMP_FORMAT = "yyyy-MM-dd HH:mm:ss.fff";

		internal static readonly string USERID_URL_PATTERN = "{host}/uuid";

		internal static readonly string COLLECT_URL_PATTERN = "{host}/{env_key}/bulk";

		internal static readonly string COLLECT_HASH_URL_PATTERN = "{host}/{env_key}/bulk/hash/{hash}";

		internal static readonly string ENGAGE_URL_PATTERN = "{host}/{env_key}";

		internal static readonly string ENGAGE_HASH_URL_PATTERN = "{host}/{env_key}/hash/{hash}";

		private bool _debugMode;
	}
}
