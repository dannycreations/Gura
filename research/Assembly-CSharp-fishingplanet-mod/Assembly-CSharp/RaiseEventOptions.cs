﻿using System;

public class RaiseEventOptions
{
	public static readonly RaiseEventOptions Default = new RaiseEventOptions();

	public EventCaching CachingOption;

	public byte InterestGroup;

	public int[] TargetActors;

	public ReceiverGroup Receivers;

	public byte SequenceChannel;

	public bool ForwardToWebhook;

	public int CacheSliceIndex;

	public bool Encrypt;
}
