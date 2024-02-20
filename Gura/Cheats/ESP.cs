using Gura.Utils;
using System;
using UnityEngine;

namespace Gura.Cheats;

public class ESP : CheatBase
{
    public override void OnDisplay()
    {
        if (!Settings.ESP)
            return;

        FishESP();
    }

    private void FishESP()
    {
        if (!Safe.IsResult(() => StateUtil.Fish) ||
            StateUtil.Fish.Count == 0 ||
            CommonUtil.IsWindowBusy
        ) return;

        foreach (var fish in StateUtil.Fish.Values)
        {
            var fishPos = fish.Owner.transform.position;
            var worldToScreen = Safe.Result<Vector3?>(() => Camera.main.WorldToScreenPoint(fishPos));
            if (worldToScreen == null) break;

            var fishName = fish.ToFormatName();
            var fishWeight = fish.ToFormatWeight();
            var fishLength = fish.ToFormatLength();

            var guiStyle = new GUIStyle(GUI.skin.box) { fontSize = 12, normal = { textColor = Color.white } };
            GUI.Box(new Rect(worldToScreen.Value.x - 75f, Screen.height - worldToScreen.Value.y, 180f, 70f), string.Concat([
                $"<b>{fishName}</b>\n",
                $"<b>[{fish.Behavior}] Ai {fish.FishAIBehaviour}</b>\n",
                $"<b>[Weight]</b> <color=#FF8C00>{fishWeight}</color>\n",
                $"<b>[Length]</b> <color=#008FFF>{fishLength}</color>",
            ]), guiStyle);
        }
    }
}