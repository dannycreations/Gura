using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using I2.Loc;
using ObjectModel;
using Photon.Interfaces.Tournaments;

namespace Assets.Scripts.UI._2D.Tournaments.UGC
{
	public class UserCompetitionFailureHandler
	{
		public static void Fire(UserCompetitionFailure failure, Action actionCalled = null, bool hideWaiting = false)
		{
			if (hideWaiting)
			{
				UIHelper.Waiting(false, null);
			}
			if (UserCompetitionFailureHandler.UgcErrorCodesWindowUi.Contains(failure.UserCompetitionErrorCode))
			{
				string text = ScriptLocalization.Get(string.Format("{0}{1}", "UGC_", failure.UserCompetitionErrorCode.ToString()));
				if (string.IsNullOrEmpty(text))
				{
					text = failure.UserCompetitionErrorCode.ToString();
				}
				if (failure.UserCompetitionErrorCode == 30)
				{
					string yellowTan = UgcConsts.GetYellowTan(UserCompetitionHelper.GetDefaultName(failure.UserCompetition));
					text = string.Format(text, yellowTan);
				}
				UIHelper.ShowCanceledMsg(ScriptLocalization.Get("MessageCaption"), text, TournamentCanceledInit.MessageTypes.Error, actionCalled, false);
			}
		}

		public static string GetSportEventName(UserCompetitionFailure failure)
		{
			if (failure.UserCompetitionErrorCode == 64 || failure.UserCompetitionErrorCode == 42 || failure.UserCompetitionErrorCode == 1)
			{
				return UgcConsts.GetYellowTan(UserCompetitionHelper.GetDefaultName(failure.UserCompetition));
			}
			if (failure.UserCompetitionErrorCode == 61 || failure.UserCompetitionErrorCode == 62)
			{
				return UgcConsts.GetYellowTan(string.Join(",", failure.Tournaments.Select((TournamentBrief p) => p.Name).ToArray<string>()));
			}
			return null;
		}

		public static DateTime GetSportEventEndDate(UserCompetitionFailure failure)
		{
			if (failure.UserCompetitionErrorCode == 64)
			{
				return failure.UserCompetition.EndDate;
			}
			if (failure.UserCompetitionErrorCode == 61)
			{
				return failure.Tournaments.Max((TournamentBrief p) => p.EndDate);
			}
			return default(DateTime);
		}

		public static void ShowMsgSportEventWait(UserCompetitionFailure failure)
		{
			UserCompetitionFailureHandler.ShowMsgSportEventWait(UserCompetitionFailureHandler.GetSportEventName(failure), UserCompetitionFailureHandler.GetSportEventEndDate(failure));
		}

		public static void ShowMsgSportEventWait(string sportEventName, DateTime endDate)
		{
			UIHelper.ShowCanceledMsg(ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("UGC_AlreadySignedToSportEventWait"), sportEventName), TournamentCanceledInit.MessageTypes.Warning, endDate, null, false);
		}

		public static void PrintLog(UserCompetitionFailure failure)
		{
			LogHelper.Error("UGC:Failure ErrorCode:{0} Op:{1} Full:{2}", new object[] { failure.UserCompetitionErrorCode, failure.SubOperation, failure.FullErrorInfo });
		}

		public const string UgcPrefix = "UGC_";

		private static readonly IList<UserCompetitionErrorCode> UgcErrorCodesWindowUi = new ReadOnlyCollection<UserCompetitionErrorCode>(new List<UserCompetitionErrorCode>
		{
			16, 15, 24, 26, 30, 31, 33, 34, 36, 37,
			40, 49, 50, 42, 1, 51, 53, 47, 29, 58,
			54, 55, 5, 2, 3, 4, 6, 7, 8, 9,
			10, 12, 13, 14
		});
	}
}
