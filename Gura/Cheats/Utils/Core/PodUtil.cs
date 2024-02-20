using Il2CppSystem;

namespace Gura.Utils;

public static class PodUtil
{
    public static bool GetFreeRodSlot(out int slot)
    {
        if (HasRodPods)
            for (int i = 0; i < StateUtil.RodPods.Count; i++)
            {
                var pod = StateUtil.RodPods[i];
                if (pod._occupiedSlots.Count < pod.RodsCount)
                {
                    slot = pod._occupiedSlots.Count + 1;
                    return true;
                }

                for (var j = 0; j < pod._occupiedSlots.Count; j++)
                {
                    var key = pod._occupiedSlots.Keys[j];
                    var rod = pod._occupiedSlots[key];
                    if (rod != null) continue;

                    slot = key + 1;
                    return true;
                }
            }

        slot = 0;
        return false;
    }

    public static bool FindRodSlotByRodId(Nullable<Guid> instanceId, out int slot)
    {
        if (HasRodPods)
            for (int i = 0; i < StateUtil.RodPods.Count; i++)
            {
                var pod = StateUtil.RodPods[i];
                for (var j = 0; j < pod._occupiedSlots.Count; j++)
                {
                    var key = pod._occupiedSlots.Keys[j];
                    var rod = pod._occupiedSlots[key];
                    if (rod == null) continue;

                    var rodId = rod.Rod.Behaviour.RodAssembly.Rod.InstanceId.ToString();
                    if (rodId != instanceId.ToString()) continue;

                    slot = key + 1;
                    return true;
                }
            }

        slot = 0;
        return false;
    }

    public static bool HasRodPods =>
        StateUtil.Player.RodPods.Count > 0;
}