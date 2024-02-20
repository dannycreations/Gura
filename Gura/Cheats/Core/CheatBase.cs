using System.Collections.Generic;

namespace Gura;

public class CheatBase
{
    public virtual void OnAwake()
    { }

    public virtual void OnUpdate()
    { }

    public virtual void OnDisplay()
    { }

    public static readonly Dictionary<string, CheatBase> Pools = [];
}