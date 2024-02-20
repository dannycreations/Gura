using System;
using System.Collections.Generic;
using Photon.Interfaces;
using Photon.Interfaces.Game;
using Photon.Interfaces.Monetization;
using Photon.Interfaces.Profile;
using Photon.Interfaces.Sys;
using Photon.Interfaces.Tournaments;

public static class OpTimer
{
	public static float AvgPing { get; private set; }

	public static float MaxPing { get; private set; }

	public static float GameAvgPing { get; private set; }

	public static float GameMaxPing { get; private set; }

	public static int OperationsInProgress { get; private set; }

	public static void Reset()
	{
		OpTimer.OperationsInProgress = 0;
	}

	public static void Start(OperationCode opCode)
	{
		OpTimer.Start(Enum.GetName(typeof(OperationCode), opCode), true);
	}

	public static void Start(OperationCode opCode, InventoryOperationCode subCode)
	{
		OpTimer.StartFormat("{0}.{1}", new object[]
		{
			Enum.GetName(typeof(OperationCode), opCode),
			Enum.GetName(typeof(InventoryOperationCode), subCode)
		});
	}

	public static void Start(OperationCode opCode, ClientCacheOperationCode subCode)
	{
		OpTimer.StartFormat("{0}.{1}", new object[]
		{
			Enum.GetName(typeof(OperationCode), opCode),
			Enum.GetName(typeof(ClientCacheOperationCode), subCode)
		});
	}

	public static void Start(OperationCode opCode, ProfileSubOperationCode subCode)
	{
		OpTimer.StartFormat("{0}.{1}", new object[]
		{
			Enum.GetName(typeof(OperationCode), opCode),
			Enum.GetName(typeof(ProfileSubOperationCode), subCode)
		});
	}

	public static void Start(OperationCode opCode, GameActionCode subCode, bool display)
	{
		OpTimer.StartFormatGame(display, "{0}.{1}", new object[]
		{
			Enum.GetName(typeof(OperationCode), opCode),
			Enum.GetName(typeof(GameActionCode), subCode)
		});
	}

	public static void Start(OperationCode opCode, TournamentSubOperationCode subCode)
	{
		OpTimer.StartFormat("{0}.{1}", new object[]
		{
			Enum.GetName(typeof(OperationCode), opCode),
			Enum.GetName(typeof(TournamentSubOperationCode), subCode)
		});
	}

	public static void Start(OperationCode opCode, UserCompetitionSubOperationCode subCode)
	{
		OpTimer.StartFormat("{0}.{1}", new object[]
		{
			Enum.GetName(typeof(OperationCode), opCode),
			Enum.GetName(typeof(UserCompetitionSubOperationCode), subCode)
		});
	}

	public static void Start(OperationCode opCode, SysSubOperationCode subCode)
	{
		OpTimer.StartFormat("{0}.{1}", new object[]
		{
			Enum.GetName(typeof(OperationCode), opCode),
			Enum.GetName(typeof(SysSubOperationCode), subCode)
		});
	}

	public static void Start(OperationCode opCode, MissionOperationCode subCode)
	{
		OpTimer.StartFormat("{0}.{1}", new object[]
		{
			Enum.GetName(typeof(OperationCode), opCode),
			Enum.GetName(typeof(MissionOperationCode), subCode)
		});
	}

	public static void Start(OperationCode opCode, PremiumShopSubOperationCode subCode)
	{
		OpTimer.StartFormat("{0}.{1}", new object[]
		{
			Enum.GetName(typeof(OperationCode), opCode),
			Enum.GetName(typeof(PremiumShopSubOperationCode), subCode)
		});
	}

	public static void End(OperationCode opCode)
	{
		OpTimer.End(Enum.GetName(typeof(OperationCode), opCode), true, false);
	}

	public static void End(OperationCode opCode, InventoryOperationCode subCode)
	{
		OpTimer.EndFormat("{0}.{1}", new object[]
		{
			Enum.GetName(typeof(OperationCode), opCode),
			Enum.GetName(typeof(InventoryOperationCode), subCode)
		});
	}

	public static void End(OperationCode opCode, ClientCacheOperationCode subCode)
	{
		OpTimer.EndFormat("{0}.{1}", new object[]
		{
			Enum.GetName(typeof(OperationCode), opCode),
			Enum.GetName(typeof(ClientCacheOperationCode), subCode)
		});
	}

	public static void End(OperationCode opCode, ProfileSubOperationCode subCode)
	{
		OpTimer.EndFormat("{0}.{1}", new object[]
		{
			Enum.GetName(typeof(OperationCode), opCode),
			Enum.GetName(typeof(ProfileSubOperationCode), subCode)
		});
	}

	public static void End(OperationCode opCode, GameActionCode subCode, bool display)
	{
		OpTimer.EndFormatGame(display, "{0}.{1}", new object[]
		{
			Enum.GetName(typeof(OperationCode), opCode),
			Enum.GetName(typeof(GameActionCode), subCode)
		});
	}

	public static void End(OperationCode opCode, TournamentSubOperationCode subCode)
	{
		OpTimer.EndFormat("{0}.{1}", new object[]
		{
			Enum.GetName(typeof(OperationCode), opCode),
			Enum.GetName(typeof(TournamentSubOperationCode), subCode)
		});
	}

	public static void End(OperationCode opCode, UserCompetitionSubOperationCode subCode)
	{
		OpTimer.EndFormat("{0}.{1}", new object[]
		{
			Enum.GetName(typeof(OperationCode), opCode),
			Enum.GetName(typeof(UserCompetitionSubOperationCode), subCode)
		});
	}

	public static void End(OperationCode opCode, SysSubOperationCode subCode)
	{
		OpTimer.EndFormat("{0}.{1}", new object[]
		{
			Enum.GetName(typeof(OperationCode), opCode),
			Enum.GetName(typeof(SysSubOperationCode), subCode)
		});
	}

	public static void End(OperationCode opCode, MissionOperationCode subCode)
	{
		OpTimer.EndFormat("{0}.{1}", new object[]
		{
			Enum.GetName(typeof(OperationCode), opCode),
			Enum.GetName(typeof(MissionOperationCode), subCode)
		});
	}

	public static void End(OperationCode opCode, PremiumShopSubOperationCode subCode)
	{
		OpTimer.EndFormat("{0}.{1}", new object[]
		{
			Enum.GetName(typeof(OperationCode), opCode),
			Enum.GetName(typeof(PremiumShopSubOperationCode), subCode)
		});
	}

	private static void StartFormat(string format, params object[] args)
	{
		OpTimer.Start(string.Format(format, args), true);
	}

	private static void StartFormatGame(bool display, string format, params object[] args)
	{
		OpTimer.Start(string.Format(format, args), display);
	}

	private static void EndFormatGame(bool display, string format, params object[] args)
	{
		OpTimer.End(string.Format(format, args), display, true);
	}

	private static void EndFormat(string format, params object[] args)
	{
		OpTimer.End(string.Format(format, args), true, false);
	}

	private static void Start(string opName, bool display = true)
	{
		OpTimer.OperationsInProgress++;
	}

	private static void End(string opName, bool display = true, bool gameOp = false)
	{
		OpTimer.OperationsInProgress--;
		OpTimer.TryReportOpTimeouts();
	}

	private static void TryReportOpTimeouts()
	{
		foreach (KeyValuePair<string, OpTimer.OpInfoData> keyValuePair in OpTimer.startInfoData)
		{
			if (!keyValuePair.Value.Reported && TimeHelper.UtcTime().Subtract(keyValuePair.Value.StartTime).TotalSeconds > 30.0)
			{
				keyValuePair.Value.Reported = true;
			}
		}
	}

	private const int MaxOperationTimeout = 30;

	private static Dictionary<string, OpTimer.OpInfoData> startInfoData = new Dictionary<string, OpTimer.OpInfoData>();

	private static readonly Averager AppPingAverager = new Averager(30);

	private static readonly Averager GamePingAverager = new Averager(30);

	private class OpInfoData
	{
		public OpInfoData(DateTime startTime)
		{
			this.StartTime = startTime;
			this.Count = 1;
		}

		public DateTime StartTime;

		public int Count;

		public bool Reported;
	}
}
