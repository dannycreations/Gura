using System;

public enum RenderEventType
{
	BeginFrame,
	EndFrame,
	InitRenderThread = 0,
	Pause,
	Resume,
	LeftEyeEndFrame,
	RightEyeEndFrame,
	TimeWarp,
	PlatformUI,
	PlatformUIConfirmQuit
}
