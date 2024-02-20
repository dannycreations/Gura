using System;
using UnityEngine;

public static class OVRPluginEvent
{
	public static void Issue(RenderEventType eventType)
	{
		GL.IssuePluginEvent(OVRPluginEvent.EncodeType((int)eventType));
	}

	public static void IssueWithData(RenderEventType eventType, int eventData)
	{
		GL.IssuePluginEvent(OVRPluginEvent.EncodeData((int)eventType, eventData, 0));
		GL.IssuePluginEvent(OVRPluginEvent.EncodeData((int)eventType, eventData, 1));
		GL.IssuePluginEvent(OVRPluginEvent.EncodeType((int)eventType));
	}

	private static int EncodeType(int eventType)
	{
		return eventType & int.MaxValue;
	}

	private static int EncodeData(int eventId, int eventData, int pos)
	{
		uint num = 0U;
		num |= 2147483648U;
		num |= (uint)((pos << 30) & 1073741824);
		num |= (uint)((eventId << 25) & 1040187392);
		return (int)(num | (((uint)eventData >> pos * 16) & 65535U));
	}

	private static int DecodeData(int eventData)
	{
		uint num = (uint)(eventData & 1073741824) >> 30;
		return (eventData & 65535) << (int)(16U * num);
	}

	private const uint IS_DATA_FLAG = 2147483648U;

	private const uint DATA_POS_MASK = 1073741824U;

	private const int DATA_POS_SHIFT = 30;

	private const uint EVENT_TYPE_MASK = 1040187392U;

	private const int EVENT_TYPE_SHIFT = 25;

	private const uint PAYLOAD_MASK = 65535U;

	private const int PAYLOAD_SHIFT = 16;
}
