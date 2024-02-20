using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Interfaces;
using Photon.Interfaces.Game;
using System;

namespace Gura.Patchs;

[HarmonyPatch(typeof(PhotonServerConnection))]
public class PhotonServerConnectionPatch
{
    [HarmonyPatch(nameof(PhotonServerConnection.SendGameAction), [typeof(GameActionCode), typeof(Hashtable)])]
    [HarmonyPrefix]
    public static bool SendGameAction(GameActionCode actionCode, Hashtable actionData)
    {
        if (Settings.IsDebug)
            switch (actionCode)
            {
                case GameActionCode.Resume:
                case GameActionCode.Pause:

                case GameActionCode.Board:
                case GameActionCode.Unboard:
                case GameActionCode.TravelByBoat:
                case GameActionCode.RestoreBoatPosition:

                case GameActionCode.Walk:
                case GameActionCode.Throw:
                case GameActionCode.Water:
                case GameActionCode.Move:
                case GameActionCode.FinishMove:

                case GameActionCode.ConfirmBite:
                case GameActionCode.CatchFish:
                case GameActionCode.TakeFish:

                case GameActionCode.Reset:
                    break;

                default:
                    Plugin.Log.LogInfo($"{nameof(PhotonServerConnection)} {nameof(SendGameAction)} {actionCode} {Extention.PrintKeys(actionData)}");
                    break;
            }

        return true;
    }

    [HarmonyPatch(nameof(PhotonServerConnection.SaveTelemetryInfo))]
    [HarmonyPrefix]
    public static bool SaveTelemetryInfo(TelemetryCode code, string message)
    {
        if (Settings.IsDebug)
            switch (code)
            {
                case TelemetryCode.Cheat:
                    return false;

                default:
                    Plugin.Log.LogInfo($"{nameof(PhotonServerConnection)} {nameof(SaveTelemetryInfo)} {code} {message}");
                    break;
            }

        return true;
    }
}