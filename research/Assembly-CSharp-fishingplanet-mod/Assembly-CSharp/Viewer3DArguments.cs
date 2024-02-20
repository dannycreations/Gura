using System;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

public class Viewer3DArguments
{
	public static bool HasArgs(ItemSubTypes itemSubType, int itemId)
	{
		return Viewer3DArguments._viewTypes.ContainsKey(itemSubType) || (Viewer3DArguments._viewTypesByIds.ContainsKey(itemSubType) && Viewer3DArguments._viewTypesByIds[itemSubType].Ids.Contains(itemId));
	}

	public static Viewer3DArguments.ViewArgs GetArgs(ItemSubTypes itemSubType, int? itemId = null)
	{
		if (itemId != null && Viewer3DArguments._viewTypesByIds.ContainsKey(itemSubType) && Viewer3DArguments._viewTypesByIds[itemSubType].Ids.Contains(itemId.Value))
		{
			return Viewer3DArguments._viewTypesByIds[itemSubType].Args;
		}
		return (!Viewer3DArguments._viewTypes.ContainsKey(itemSubType)) ? null : Viewer3DArguments._viewTypes[itemSubType];
	}

	private static readonly Dictionary<ItemSubTypes, Viewer3DArguments.ViewArgs> _viewTypes = new Dictionary<ItemSubTypes, Viewer3DArguments.ViewArgs>
	{
		{
			ItemSubTypes.CastingRod,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(-3.74f, -107f, 0f),
				rotateAroundRoot = true,
				desiredScale = 3.5f,
				setPosition = true,
				position = new Vector3(-0.1f, 0.05f, 0f)
			}
		},
		{
			ItemSubTypes.MatchRod,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(-3.74f, -107f, 0f),
				rotateAroundRoot = true,
				desiredScale = 3.5f,
				setPosition = true,
				position = new Vector3(-0.1f, 0.05f, 0f)
			}
		},
		{
			ItemSubTypes.SpinningRod,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(-3.74f, -107f, 0f),
				rotateAroundRoot = true,
				desiredScale = 3.5f,
				setPosition = true,
				position = new Vector3(-0.1f, 0.05f, 0f)
			}
		},
		{
			ItemSubTypes.TelescopicRod,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(-3.74f, -107f, 0f),
				rotateAroundRoot = true,
				desiredScale = 2.5f,
				setPosition = true,
				position = new Vector3(-0.1f, 0.05f, 0f)
			}
		},
		{
			ItemSubTypes.FeederRod,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(-3.74f, -107f, 0f),
				rotateAroundRoot = true,
				desiredScale = 3.5f,
				setPosition = true,
				position = new Vector3(-0.1f, 0.05f, 0f)
			}
		},
		{
			ItemSubTypes.BottomRod,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(-3.74f, -107f, 0f),
				rotateAroundRoot = true,
				desiredScale = 2.5f,
				setPosition = true,
				position = new Vector3(-0.1f, 0.05f, 0f)
			}
		},
		{
			ItemSubTypes.CarpRod,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(-3.74f, -107f, 0f),
				rotateAroundRoot = true,
				desiredScale = 2.5f,
				setPosition = true,
				position = new Vector3(-0.1f, 0.05f, 0f)
			}
		},
		{
			ItemSubTypes.SpodRod,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(-3.74f, -107f, 0f),
				rotateAroundRoot = true,
				desiredScale = 2.5f,
				setPosition = true,
				position = new Vector3(-0.1f, 0.05f, 0f)
			}
		},
		{
			ItemSubTypes.RodStand,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(0f, 70f, 0f),
				rotateAroundRoot = true,
				desiredScale = 1.5f,
				scaleFactorMin = 0.5f,
				scaleFactorMax = 2f
			}
		},
		{
			ItemSubTypes.CastReel,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(0f, -140f, 0f),
				rotateAroundRoot = true,
				desiredScale = 9.5f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.SpinReel,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(0f, -127f, 0f),
				rotateAroundRoot = true,
				desiredScale = 9.5f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.Bobber,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(40f, 75f, 0f),
				rotateAroundRoot = true,
				desiredScale = 9f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.Waggler,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(40f, 75f, 0f),
				rotateAroundRoot = true,
				desiredScale = 9f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.Slider,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(40f, 75f, 0f),
				rotateAroundRoot = true,
				desiredScale = 9f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.Spoon,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(300f, 60f, -25f),
				rotateAroundRoot = true,
				desiredScale = 9.5f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.Spinner,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(125f, 85f, -45f),
				rotateAroundAxis = new Vector3(0f, 0f, -1f),
				rotateAroundRoot = false,
				desiredScale = 20f,
				setPosition = true,
				position = new Vector3(0.3f, -0.5f, 0f),
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.Cranckbait,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(316f, 90f, 90f),
				rotateAroundRoot = true,
				desiredScale = 26f,
				setPosition = true,
				position = new Vector3(0.24f, 0.5f, 0f),
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.BassJig,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(-70f, 90f, 0f),
				rotateAroundRoot = true,
				desiredScale = 13f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.BarblessSpoons,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(300f, 60f, -25f),
				rotateAroundRoot = true,
				desiredScale = 9.5f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.BarblessSpinners,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(125f, 85f, -45f),
				rotateAroundAxis = new Vector3(0f, 0f, -1f),
				rotateAroundRoot = false,
				desiredScale = 20f,
				setPosition = true,
				position = new Vector3(0.3f, -0.5f, 0f),
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.Spinnerbait,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(22f, 90f, 0f),
				rotateAroundRoot = true,
				desiredScale = 10f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.Swimbait,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(22f, 90f, 90f),
				rotateAroundRoot = true,
				desiredScale = 10f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.BuzzBait,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(22f, 90f, 0f),
				rotateAroundRoot = true,
				desiredScale = 10f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.Popper,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(60f, 270f, -270f),
				desiredScale = 10f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.Walker,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(60f, 270f, -270f),
				rotateAroundRoot = true,
				desiredScale = 10f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.Frog,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(26f, 220f, 90f),
				rotateAroundRoot = true,
				desiredScale = 13f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.Worm,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(0f, 90f, 0f),
				rotateAroundRoot = true,
				desiredScale = 17f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.Grub,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(0f, 90f, 0f),
				rotateAroundRoot = true,
				desiredScale = 17f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f,
				setPosition = true,
				position = new Vector3(0.68f, 0f)
			}
		},
		{
			ItemSubTypes.Shad,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(0f, 90f, 0f),
				rotateAroundRoot = true,
				desiredScale = 17f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.Slug,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(15f, 220f, 0f),
				rotateAroundRoot = true,
				desiredScale = 17f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f,
				setPosition = true,
				position = new Vector3(-0.512f, -0.034f, -0.61f)
			}
		},
		{
			ItemSubTypes.Tube,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(-60f, 90f, 90f),
				rotateAroundRoot = true,
				desiredScale = 20f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.Craw,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(-45f, 90f, -90f),
				rotateAroundRoot = true,
				desiredScale = 16f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f,
				setPosition = true,
				position = new Vector3(0f, 0.55f, 0f)
			}
		},
		{
			ItemSubTypes.Tail,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(300f, 90f, 270f),
				rotateAroundRoot = true,
				desiredScale = 16f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.Kayak,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(-13f, 23f, 0f),
				rotateAroundRoot = true,
				desiredScale = 0.6f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.MotorBoat,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(-5f, 23f, 0f),
				rotateAroundRoot = true,
				desiredScale = 0.6f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.Zodiak,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(-5f, 23f, 0f),
				rotateAroundRoot = true,
				desiredScale = 0.6f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		},
		{
			ItemSubTypes.BassBoat,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(-5f, 23f, 0f),
				rotateAroundRoot = true,
				desiredScale = 0.5f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f,
				setPosition = true,
				position = Vector3.zero
			}
		},
		{
			ItemSubTypes.Jerkbait,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(60f, 270f, -270f),
				rotateAroundRoot = true,
				desiredScale = 26f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f,
				setPosition = true,
				position = Vector3.zero
			}
		},
		{
			ItemSubTypes.Minnow,
			new Viewer3DArguments.ViewArgs
			{
				rotationAxis = new Vector3(60f, 270f, -270f),
				rotateAroundRoot = true,
				desiredScale = 10f,
				scaleFactorMin = 0.2f,
				scaleFactorMax = 3f
			}
		}
	};

	private static readonly Dictionary<ItemSubTypes, Viewer3DArguments.ViewArgsByIds> _viewTypesByIds = new Dictionary<ItemSubTypes, Viewer3DArguments.ViewArgsByIds> { 
	{
		ItemSubTypes.Popper,
		new Viewer3DArguments.ViewArgsByIds(new Viewer3DArguments.ViewArgs
		{
			rotationAxis = new Vector3(60f, 270f, -270f),
			desiredScale = 20f,
			scaleFactorMin = 0.2f,
			scaleFactorMax = 3f
		}, new int[] { 13100, 13110, 13120, 13170, 13180, 13190, 13200, 13210, 13220 })
	} };

	public class ViewArgs
	{
		public Vector3 rotationAxis;

		public float desiredScale;

		public bool setPosition;

		public Vector3 position;

		public Vector3 rotateAroundAxis;

		public bool rotateAroundRoot = true;

		public float angleFrom;

		public float angleTo;

		public float scaleFactorMin;

		public float scaleFactorMax;
	}

	private class ViewArgsByIds
	{
		public ViewArgsByIds(Viewer3DArguments.ViewArgs args, int[] ids)
		{
			this.Args = args;
			this.Ids = ids;
		}

		public Viewer3DArguments.ViewArgs Args { get; private set; }

		public int[] Ids { get; private set; }
	}
}
