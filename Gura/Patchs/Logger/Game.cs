using HarmonyLib;

namespace Gura.Patchs;

[HarmonyPatch(typeof(Game))]
public class GamePatch
{
    //[HarmonyPatch(nameof(Game.FinishAttack))]
    //[HarmonyPrefix]
    //public static bool FinishAttack(Game __instance, ObscuredVector3 playerPosition, ObscuredVector3 terminalTacklePosition, ObscuredBool isStriking, ObscuredBool hasWrongStriking, ObscuredBool pulled, ObscuredFloat distanceToTackle)
    //{
    //    if (Settings.IsDebug)
    //    {
    //        var hashtable = new Dictionary<string, dynamic>();
    //        hashtable["sN"] = __instance.RodSlot;
    //        if (__instance.Rod is Rod1stBehaviour && StateUtil.Player.IsPullTriggered)
    //            hashtable["iPt"] = true;
    //        hashtable["pP"] = playerPosition.ToPoint3().ToString();
    //        hashtable["tP"] = terminalTacklePosition.ToPoint3().ToString();
    //        if (isStriking)
    //            hashtable["iS"] = true;
    //        if (hasWrongStriking)
    //            hashtable["wS"] = true;
    //        if (pulled)
    //            hashtable["iP"] = true;
    //        if (distanceToTackle != 0f)
    //            hashtable["d2t"] = ObscuredFloat.Decrypt(distanceToTackle.GetEncrypted());
    //        hashtable["lTf"] = __instance.minTerminalTackleForceForPeriod;
    //        hashtable["hTf"] = __instance.maxTerminalTackleForceForPeriod;
    //        hashtable["hRdF"] = __instance.maxRodForceForPeriod;
    //        hashtable["hRlF"] = __instance.maxReelForceForPeriod;
    //        Plugin.Log.LogInfo($"{nameof(Game)} {nameof(Game.FinishAttack)} {hashtable.Stringify()}");
    //    }

    //    return true;
    //}

    //[HarmonyPatch(nameof(Game.Move))]
    //[HarmonyPrefix]
    //public static bool Move(Game __instance, ObscuredVector3 playerPosition, ObscuredVector3 terminalTacklePosition, Il2CppSystem.Nullable<ObscuredVector3> fishPosition, DragStyle style, ObscuredBool isBobberIdle, ObscuredBool isLyingOnBottom, ObscuredBool isFeederLyingOnBottom, ObscuredBool isMoving, ObscuredFloat terminalTackleForce, ObscuredFloat rodForce, ObscuredFloat reelForce, ObscuredFloat lineLength, ObscuredInt speed, ObscuredBool isReeling, ObscuredBool hasLineSlack, ObscuredBool isAnchorDown, ObscuredBool isRowing, ObscuredFloat boatStamina, bool forceSend)
    //{
    //    if (Settings.IsDebug)
    //    {
    //        __instance.UpdatePeriodData(isBobberIdle, isLyingOnBottom, isFeederLyingOnBottom, isMoving);
    //        __instance.UpdateForces(terminalTackleForce, rodForce, reelForce, 0f, true, hasLineSlack, false, speed, isReeling, false, false);
    //        if (!forceSend && !__instance.CheckUpdateTimeout())
    //            return false;

    //        var hashtable = __instance.CreateActionDataHashtable();
    //        hashtable["rS"] = __instance.maxSpeed;
    //        hashtable["iR"] = __instance.wasReeling;
    //        hashtable["lL"] = ObscuredFloat.Decrypt(lineLength.GetEncrypted());
    //        if (__instance.hadLineSlack)
    //            hashtable["l"] = __instance.hadLineSlack;

    //        if (__instance.maxSpeed < 4 || !__instance.wasReeling)
    //        {
    //            var point = terminalTacklePosition.ToPoint3();
    //            if (fishPosition != null)
    //            {
    //                var point2 = fishPosition.Value.ToPoint3();
    //                hashtable["aFd"] = point.Distance(point2);
    //            }
    //            hashtable["pP"] = playerPosition.ToPoint3();
    //            hashtable["tP"] = point;
    //            if (__instance.wasBobberIdleForPeriod)
    //            {
    //                hashtable["iBi"] = true;
    //            }
    //            if (__instance.wasLyingOnBottom)
    //            {
    //                hashtable["tPs"] = true;
    //            }
    //            if (__instance.wasFeederLyingOnBottom)
    //            {
    //                hashtable["fPs"] = true;
    //            }
    //            if (__instance.wasMoving)
    //            {
    //                hashtable["tMs"] = true;
    //            }
    //            hashtable["d"] = (byte)style;
    //        }

    //        PhotonConnectionFactory.Instance.SendGameAction(GameActionCode.Move, hashtable);
    //        __instance.ResetPeriod();
    //        __instance.ResetBoatTravelingFlags();
    //        return false;
    //    }

    //    return true;
    //}
}