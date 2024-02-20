using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject(1)]
	public class HintMessage
	{
		public Dictionary<byte, object> Data
		{
			get
			{
				return this.data;
			}
			set
			{
				this.data = value;
			}
		}

		public Dictionary<string, string> Strings
		{
			get
			{
				return this.strings;
			}
			set
			{
				this.strings = value;
			}
		}

		[JsonProperty]
		public string MessageId
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.MessageId);
			}
			set
			{
				this.data[1] = value;
			}
		}

		public string FullMessageId
		{
			get
			{
				return string.Format("Mission: {0}, Task: {1}, Message: {2}", this.MissionId, this.TaskId, this.MessageId);
			}
		}

		[JsonProperty]
		public bool IsCompact
		{
			get
			{
				return this.SafeGetValueInt(HintMessageParameterType.IsCompact) != 0;
			}
			set
			{
				this.data[83] = ((!value) ? 0 : 1);
			}
		}

		[JsonProperty]
		public int MissionId
		{
			get
			{
				return this.SafeGetValueInt(HintMessageParameterType.MissionId);
			}
			set
			{
				this.data[2] = value;
			}
		}

		[JsonProperty]
		public string MissionName
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.MissionName);
			}
			set
			{
				this.data[3] = value;
			}
		}

		[JsonProperty]
		public int TaskId
		{
			get
			{
				return this.SafeGetValueInt(HintMessageParameterType.TaskId);
			}
			set
			{
				this.data[4] = value;
			}
		}

		[JsonProperty]
		public string TaskName
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.TaskName);
			}
			set
			{
				this.data[5] = value;
			}
		}

		[JsonProperty]
		public MissionEventType EventType
		{
			get
			{
				return (MissionEventType)this.SafeGetValueInt(HintMessageParameterType.EventType);
			}
			set
			{
				this.data[6] = (int)value;
			}
		}

		[JsonProperty]
		public string EventName
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.EventName);
			}
			set
			{
				this.data[7] = value;
			}
		}

		[JsonProperty]
		public HintMessageLevel Level
		{
			get
			{
				return (HintMessageLevel)this.SafeGetValueInt(HintMessageParameterType.Level);
			}
			set
			{
				this.data[8] = (int)value;
			}
		}

		[JsonProperty]
		public string Code
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.Code);
			}
			set
			{
				this.data[9] = value;
			}
		}

		[JsonProperty]
		public int OrderIndex
		{
			get
			{
				return this.SafeGetValueInt(HintMessageParameterType.OrderIndex);
			}
			set
			{
				this.data[59] = value;
			}
		}

		[JsonProperty]
		public string Title
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.Title);
			}
			set
			{
				this.data[10] = value;
				this.titleParsedFormatInfo = null;
			}
		}

		public string TitleFormatted
		{
			get
			{
				string text = this.SafeGetValueString(HintMessageParameterType.Title);
				if (this.titleParsedFormatInfo == null)
				{
					this.titleParsedFormatInfo = HintMessage.Parse(text);
				}
				return this.StringFormat(text, this.titleParsedFormatInfo);
			}
		}

		[JsonProperty]
		public string OriginalTitle
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.OriginalTitle);
			}
			set
			{
				this.data[73] = value;
			}
		}

		public string OriginalTitleFormatted
		{
			get
			{
				string text = this.SafeGetValueString(HintMessageParameterType.OriginalTitle);
				if (this.originalTitleParsedFormatInfo == null)
				{
					this.originalTitleParsedFormatInfo = HintMessage.Parse(text);
				}
				return this.StringFormat(text, this.originalTitleParsedFormatInfo);
			}
		}

		[JsonProperty]
		public string Description
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.Description);
			}
			set
			{
				this.data[12] = value;
				this.descriptionParsedFormatInfo = null;
			}
		}

		public string DescriptionFormatted
		{
			get
			{
				string text = this.SafeGetValueString(HintMessageParameterType.Description);
				if (this.descriptionParsedFormatInfo == null)
				{
					this.descriptionParsedFormatInfo = HintMessage.Parse(text);
				}
				return this.StringFormat(text, this.descriptionParsedFormatInfo);
			}
		}

		[JsonProperty]
		public string OriginalDescription
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.OriginalDescription);
			}
			set
			{
				this.data[74] = value;
			}
		}

		public string OriginalDescriptionFormatted
		{
			get
			{
				string text = this.SafeGetValueString(HintMessageParameterType.OriginalDescription);
				if (this.originalDescriptionParsedFormatInfo == null)
				{
					this.originalDescriptionParsedFormatInfo = HintMessage.Parse(text);
				}
				return this.StringFormat(text, this.originalDescriptionParsedFormatInfo);
			}
		}

		[JsonProperty]
		public string Tooltip
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.Tooltip);
			}
			set
			{
				this.data[69] = value;
				this.tooltipParsedFormatInfo = null;
			}
		}

		public string TooltipFormatted
		{
			get
			{
				string text = this.SafeGetValueString(HintMessageParameterType.Tooltip);
				if (this.tooltipParsedFormatInfo == null)
				{
					this.tooltipParsedFormatInfo = HintMessage.Parse(text);
				}
				return this.StringFormat(text, this.tooltipParsedFormatInfo);
			}
		}

		[JsonProperty]
		public string OriginalTooltip
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.OriginalTooltip);
			}
			set
			{
				this.data[75] = value;
			}
		}

		public string OriginalTooltipFormatted
		{
			get
			{
				string text = this.SafeGetValueString(HintMessageParameterType.Tooltip);
				if (this.originalTooltipParsedFormatInfo == null)
				{
					this.originalTooltipParsedFormatInfo = HintMessage.Parse(text);
				}
				return this.StringFormat(text, this.originalTooltipParsedFormatInfo);
			}
		}

		[JsonProperty]
		public string Image
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.Image);
			}
			set
			{
				this.data[13] = value;
			}
		}

		[JsonProperty]
		public int? ImageId
		{
			get
			{
				return this.SafeGetValueIntNullable(HintMessageParameterType.ImageId);
			}
			set
			{
				if (value != null)
				{
					this.data[46] = value;
				}
				else
				{
					this.data.Remove(46);
				}
			}
		}

		[JsonProperty]
		public HintItemClass ItemClass
		{
			get
			{
				return (HintItemClass)this.SafeGetValueInt(HintMessageParameterType.ItemClass);
			}
			set
			{
				HintItemClass hintItemClass = (HintItemClass)this.SafeGetValueInt(HintMessageParameterType.ItemClass);
				this.data[56] = value;
				if (hintItemClass != value)
				{
					this.shouldTranslateItemName = true;
				}
			}
		}

		[JsonProperty]
		public int ItemId
		{
			get
			{
				return this.SafeGetValueInt(HintMessageParameterType.ItemId);
			}
			set
			{
				int num = this.SafeGetValueInt(HintMessageParameterType.ItemId);
				this.data[14] = value;
				if (num != value)
				{
					this.shouldTranslateItemName = true;
				}
			}
		}

		[JsonProperty]
		public string ItemName
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.ItemName);
			}
			set
			{
				this.data[15] = value;
			}
		}

		[JsonProperty]
		public ItemTypes ItemType
		{
			get
			{
				return (ItemTypes)this.SafeGetValueInt(HintMessageParameterType.ItemType);
			}
			set
			{
				this.data[57] = (int)value;
			}
		}

		[JsonProperty]
		public ItemSubTypes ItemSubType
		{
			get
			{
				return (ItemSubTypes)this.SafeGetValueInt(HintMessageParameterType.ItemSubType);
			}
			set
			{
				this.data[58] = (int)value;
			}
		}

		[JsonProperty]
		public StoragePlaces Storage
		{
			get
			{
				return (StoragePlaces)this.SafeGetValueInt(HintMessageParameterType.Storage);
			}
			set
			{
				this.data[68] = (int)value;
			}
		}

		[JsonProperty]
		public StoragePlaces SourceStorage
		{
			get
			{
				return (StoragePlaces)this.SafeGetValueInt(HintMessageParameterType.SourceStorage);
			}
			set
			{
				this.data[82] = (int)value;
			}
		}

		[JsonProperty]
		public string InstanceId
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.InstanceId);
			}
			set
			{
				this.data[72] = value;
			}
		}

		[JsonProperty]
		public int RodId
		{
			get
			{
				return this.SafeGetValueInt(HintMessageParameterType.RodId);
			}
			set
			{
				this.data[63] = value;
			}
		}

		[JsonProperty]
		public int Slot
		{
			get
			{
				return this.SafeGetValueInt(HintMessageParameterType.Slot);
			}
			set
			{
				this.data[64] = value;
			}
		}

		[JsonProperty]
		public int CategoryId
		{
			get
			{
				return this.SafeGetValueInt(HintMessageParameterType.CategoryId);
			}
			set
			{
				int num = this.SafeGetValueInt(HintMessageParameterType.CategoryId);
				this.data[76] = value;
				if (num != value)
				{
					this.shouldTranslateCategoryName = true;
				}
			}
		}

		[JsonProperty]
		public string CategoryName
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.CategoryName);
			}
			set
			{
				this.data[77] = value;
			}
		}

		[JsonProperty]
		public int RootCategoryId
		{
			get
			{
				return this.SafeGetValueInt(HintMessageParameterType.RootCategoryId);
			}
			set
			{
				int num = this.SafeGetValueInt(HintMessageParameterType.RootCategoryId);
				this.data[85] = value;
				if (num != value)
				{
					this.shouldTranslateRootCategoryName = true;
				}
			}
		}

		[JsonProperty]
		public string RootCategoryName
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.RootCategoryName);
			}
			set
			{
				this.data[86] = value;
			}
		}

		[JsonProperty]
		public float MinLoad
		{
			get
			{
				return this.SafeGetValueFloat(HintMessageParameterType.MinLoad);
			}
			set
			{
				this.data[78] = value;
			}
		}

		[JsonProperty]
		public float MaxLoad
		{
			get
			{
				return this.SafeGetValueFloat(HintMessageParameterType.MaxLoad);
			}
			set
			{
				this.data[79] = value;
			}
		}

		[JsonProperty]
		public float MinThickness
		{
			get
			{
				return this.SafeGetValueFloat(HintMessageParameterType.MinThickness);
			}
			set
			{
				this.data[80] = value;
			}
		}

		[JsonProperty]
		public float MaxThickness
		{
			get
			{
				return this.SafeGetValueFloat(HintMessageParameterType.MaxThickness);
			}
			set
			{
				this.data[81] = value;
			}
		}

		[JsonProperty]
		public string ElementId
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.ElementId);
			}
			set
			{
				this.data[16] = value;
			}
		}

		[JsonProperty]
		public Point2 ScreenPosition
		{
			get
			{
				return (Point2)this.SafeGetValue(HintMessageParameterType.ScreenPosition);
			}
			set
			{
				this.data[17] = value;
			}
		}

		[JsonProperty]
		public Point3 ScenePosition
		{
			get
			{
				return (Point3)this.SafeGetValue(HintMessageParameterType.ScenePosition);
			}
			set
			{
				this.data[18] = value;
			}
		}

		[JsonProperty]
		public Point3 ScenePositionOffset
		{
			get
			{
				return (Point3)this.SafeGetValue(HintMessageParameterType.ScenePositionOffset);
			}
			set
			{
				this.data[19] = value;
			}
		}

		[JsonProperty]
		public Point3 Rotation
		{
			get
			{
				return (Point3)this.SafeGetValue(HintMessageParameterType.Rotation);
			}
			set
			{
				this.data[20] = value;
			}
		}

		[JsonProperty]
		public Point3 Scale
		{
			get
			{
				return (Point3)this.SafeGetValue(HintMessageParameterType.Scale);
			}
			set
			{
				this.data[21] = value;
			}
		}

		[JsonProperty]
		public IMissionGeometry Geometry
		{
			get
			{
				return (IMissionGeometry)this.SafeGetValue(HintMessageParameterType.Geometry);
			}
			set
			{
				this.data[35] = value;
			}
		}

		[JsonProperty]
		public SpatialGeometry Spatial
		{
			get
			{
				return (SpatialGeometry)this.SafeGetValue(HintMessageParameterType.Spatial);
			}
			set
			{
				this.data[66] = value;
			}
		}

		[JsonProperty]
		public GameMarkerState MarkerState
		{
			get
			{
				return (GameMarkerState)this.SafeGetValueInt(HintMessageParameterType.MarkerState);
			}
			set
			{
				this.data[67] = (int)value;
			}
		}

		[JsonProperty]
		public MissionInteractiveObject InteractiveObject
		{
			get
			{
				return (MissionInteractiveObject)this.SafeGetValue(HintMessageParameterType.InteractiveObject);
			}
			set
			{
				this.data[36] = value;
			}
		}

		[JsonProperty]
		public string InteractiveObjectId
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.InteractiveObjectId);
			}
			set
			{
				this.data[37] = value;
			}
		}

		[JsonProperty]
		public string AllowInteraction
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.AllowInteraction);
			}
			set
			{
				this.data[11] = value;
			}
		}

		[JsonProperty]
		public string ActiveState
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.ActiveState);
			}
			set
			{
				this.data[45] = value;
			}
		}

		[JsonProperty]
		public int InteractiveVersion
		{
			get
			{
				return this.SafeGetValueInt(HintMessageParameterType.InteractiveVersion);
			}
			set
			{
				this.data[52] = value;
			}
		}

		[JsonProperty]
		public DateTime? ExecutionNextTime
		{
			get
			{
				return this.SafeGetDateTimeNullable(HintMessageParameterType.ExecutionNextTime);
			}
			set
			{
				if (value == null)
				{
					this.data.Remove(87);
				}
				else
				{
					this.data[87] = value;
				}
			}
		}

		[JsonProperty]
		public bool IsDisabled
		{
			get
			{
				return this.SafeGetValueInt(HintMessageParameterType.IsDisabled) != 0;
			}
			set
			{
				this.data[88] = ((!value) ? 0 : 1);
			}
		}

		[JsonProperty]
		public DateTime? StartTime
		{
			get
			{
				return this.SafeGetDateTimeNullable(HintMessageParameterType.StartTime);
			}
			set
			{
				if (value == null)
				{
					this.data.Remove(89);
				}
				else
				{
					this.data[89] = value;
				}
			}
		}

		[JsonProperty]
		public int? TimeToComplete
		{
			get
			{
				return this.SafeGetValueIntNullable(HintMessageParameterType.TimeToComplete);
			}
			set
			{
				if (value != null)
				{
					this.data[90] = value;
				}
				else
				{
					this.data.Remove(90);
				}
			}
		}

		[JsonProperty]
		public string ShaderName
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.ShaderName);
			}
			set
			{
				this.data[22] = value;
			}
		}

		[JsonProperty]
		public HintArrowType ArrowType
		{
			get
			{
				return (HintArrowType)this.SafeGetValueInt(HintMessageParameterType.ArrowType);
			}
			set
			{
				this.data[23] = (int)value;
			}
		}

		[JsonProperty]
		public HintArrowType3D ArrowType3D
		{
			get
			{
				return (HintArrowType3D)this.SafeGetValueInt(HintMessageParameterType.ArrowType3D);
			}
			set
			{
				this.data[55] = (int)value;
			}
		}

		[JsonProperty]
		public HintSide Side
		{
			get
			{
				return (HintSide)this.SafeGetValueInt(HintMessageParameterType.Side);
			}
			set
			{
				this.data[65] = (int)value;
			}
		}

		[JsonProperty]
		public HintHighlightType HighlightType
		{
			get
			{
				return (HintHighlightType)this.SafeGetValueInt(HintMessageParameterType.HighlightType);
			}
			set
			{
				this.data[24] = (int)value;
			}
		}

		[JsonProperty]
		public HintKeyType KeyType
		{
			get
			{
				return (HintKeyType)this.SafeGetValueInt(HintMessageParameterType.KeyType);
			}
			set
			{
				this.data[38] = (int)value;
			}
		}

		[JsonProperty]
		public HintGizmoType GizmoType
		{
			get
			{
				return (HintGizmoType)this.SafeGetValueInt(HintMessageParameterType.GizmoType);
			}
			set
			{
				this.data[25] = (int)value;
			}
		}

		[JsonProperty]
		public GameDashType DashType
		{
			get
			{
				return (GameDashType)this.SafeGetValueInt(HintMessageParameterType.DashType);
			}
			set
			{
				this.data[62] = (int)value;
			}
		}

		[JsonProperty]
		public GameScreenType ScreenType
		{
			get
			{
				return (GameScreenType)this.SafeGetValueInt(HintMessageParameterType.ScreenType);
			}
			set
			{
				this.data[26] = (int)value;
			}
		}

		[JsonProperty]
		public GameScreenTabType ScreenTab
		{
			get
			{
				return (GameScreenTabType)this.SafeGetValueInt(HintMessageParameterType.ScreenTab);
			}
			set
			{
				this.data[44] = (int)value;
			}
		}

		[JsonProperty]
		public StoragePlaces DisplayStorage
		{
			get
			{
				return (StoragePlaces)this.SafeGetValueInt(HintMessageParameterType.DisplayStorage);
			}
			set
			{
				this.data[70] = (int)value;
			}
		}

		[JsonProperty]
		public bool IsAutoDestroy
		{
			get
			{
				return this.SafeGetValueInt(HintMessageParameterType.IsAutoDestroy) != 0;
			}
			set
			{
				this.data[27] = ((!value) ? 0 : 1);
			}
		}

		[JsonProperty]
		public int ShowAfterMs
		{
			get
			{
				return this.SafeGetValueInt(HintMessageParameterType.ShowAfterMs);
			}
			set
			{
				this.data[28] = value;
			}
		}

		[JsonProperty]
		public int ShowDuringMs
		{
			get
			{
				return this.SafeGetValueInt(HintMessageParameterType.ShowDuringMs);
			}
			set
			{
				this.data[29] = value;
			}
		}

		[JsonProperty]
		public bool IsAutomatic
		{
			get
			{
				return this.SafeGetValueInt(HintMessageParameterType.IsAutomatic) != 0;
			}
			set
			{
				this.data[71] = ((!value) ? 0 : 1);
			}
		}

		[JsonProperty]
		public string Color
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.Color);
			}
			set
			{
				this.data[30] = value;
			}
		}

		[JsonProperty]
		public HintType HintType
		{
			get
			{
				return (HintType)this.SafeGetValueInt(HintMessageParameterType.HintType);
			}
			set
			{
				this.data[31] = (int)value;
			}
		}

		[JsonProperty]
		public string BackgroundColor
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.BackgroundColor);
			}
			set
			{
				this.data[39] = value;
			}
		}

		[JsonProperty]
		public HintBackgroundType BackgroundType
		{
			get
			{
				return (HintBackgroundType)this.SafeGetValueInt(HintMessageParameterType.BackgroundType);
			}
			set
			{
				this.data[40] = (int)value;
			}
		}

		[JsonProperty]
		public string Alignment
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.Alignment);
			}
			set
			{
				this.data[41] = value;
			}
		}

		[JsonProperty]
		public string Padding
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.Padding);
			}
			set
			{
				this.data[42] = value;
			}
		}

		public MissionCommandType CommandType
		{
			get
			{
				return (MissionCommandType)this.SafeGetValueInt(HintMessageParameterType.CommandType);
			}
			set
			{
				this.data[32] = (int)value;
			}
		}

		public string CommandName
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.CommandName);
			}
			set
			{
				this.data[43] = value;
			}
		}

		public string CommandArgument
		{
			get
			{
				return this.SafeGetValueString(HintMessageParameterType.CommandArgument);
			}
			set
			{
				this.data[33] = value;
			}
		}

		[JsonProperty]
		public int Count
		{
			get
			{
				return this.SafeGetValueInt(HintMessageParameterType.Count);
			}
			set
			{
				this.data[34] = value;
			}
		}

		[JsonProperty]
		public int Value
		{
			get
			{
				return this.SafeGetValueInt(HintMessageParameterType.Value);
			}
			set
			{
				this.data[47] = value;
			}
		}

		[JsonProperty]
		public float Length
		{
			get
			{
				return this.SafeGetValueFloat(HintMessageParameterType.Length);
			}
			set
			{
				this.data[48] = value;
			}
		}

		[JsonProperty]
		public float Weight
		{
			get
			{
				return this.SafeGetValueFloat(HintMessageParameterType.Weight);
			}
			set
			{
				this.data[49] = value;
			}
		}

		[JsonProperty]
		public float Temparature
		{
			get
			{
				return this.SafeGetValueFloat(HintMessageParameterType.Temparature);
			}
			set
			{
				this.data[50] = value;
			}
		}

		[JsonProperty]
		public float Speed
		{
			get
			{
				return this.SafeGetValueFloat(HintMessageParameterType.Speed);
			}
			set
			{
				this.data[51] = value;
			}
		}

		[JsonProperty]
		public float Min
		{
			get
			{
				return this.SafeGetValueFloat(HintMessageParameterType.Min);
			}
			set
			{
				this.data[60] = value;
			}
		}

		[JsonProperty]
		public float Max
		{
			get
			{
				return this.SafeGetValueFloat(HintMessageParameterType.Max);
			}
			set
			{
				this.data[61] = value;
			}
		}

		public override string ToString()
		{
			return string.Join(", ", this.data.Keys.OrderBy(delegate(byte k)
			{
				switch (k)
				{
				case 7:
					return 0;
				default:
					if (k != 1)
					{
						return 5;
					}
					return 9;
				case 10:
					return 1;
				}
			}).Select(delegate(byte k)
			{
				object obj;
				switch (k)
				{
				case 73:
					obj = this.OriginalTitleFormatted;
					break;
				case 74:
					obj = this.OriginalDescriptionFormatted;
					break;
				case 75:
					obj = this.OriginalTooltipFormatted;
					break;
				default:
					switch (k)
					{
					case 10:
						obj = this.TitleFormatted;
						break;
					default:
						if (k != 23)
						{
							if (k != 24)
							{
								if (k != 55)
								{
									if (k != 69)
									{
										obj = this.data[k];
									}
									else
									{
										obj = this.TooltipFormatted;
									}
								}
								else
								{
									obj = this.ArrowType3D.ToString();
								}
							}
							else
							{
								obj = this.HighlightType.ToString();
							}
						}
						else
						{
							obj = this.ArrowType.ToString();
						}
						break;
					case 12:
						obj = this.DescriptionFormatted;
						break;
					}
					break;
				}
				return string.Format("{0}: {1}", (HintMessageParameterType)k, obj);
			}).ToArray<string>());
		}

		public bool AreSame(HintMessage message)
		{
			return (string.IsNullOrEmpty(this.MessageId) && string.IsNullOrEmpty(message.MessageId) && object.ReferenceEquals(this, message)) || (!string.IsNullOrEmpty(this.MessageId) && this.MessageId == message.MessageId);
		}

		private object SafeGetValue(HintMessageParameterType parameter)
		{
			if (this.data.ContainsKey((byte)parameter))
			{
				return this.data[(byte)parameter];
			}
			return null;
		}

		private int SafeGetValueInt(HintMessageParameterType parameter)
		{
			if (this.data.ContainsKey((byte)parameter))
			{
				return Convert.ToInt32(this.data[(byte)parameter]);
			}
			return 0;
		}

		private float SafeGetValueFloat(HintMessageParameterType parameter)
		{
			if (this.data.ContainsKey((byte)parameter))
			{
				return Convert.ToSingle(this.data[(byte)parameter]);
			}
			return 0f;
		}

		private int? SafeGetValueIntNullable(HintMessageParameterType parameter)
		{
			if (this.data.ContainsKey((byte)parameter))
			{
				return new int?(Convert.ToInt32(this.data[(byte)parameter]));
			}
			return null;
		}

		private string SafeGetValueString(HintMessageParameterType parameter)
		{
			if (this.data.ContainsKey((byte)parameter))
			{
				return (string)this.data[(byte)parameter];
			}
			return null;
		}

		private DateTime? SafeGetDateTimeNullable(HintMessageParameterType parameter)
		{
			if (this.data.ContainsKey((byte)parameter))
			{
				return new DateTime?(Convert.ToDateTime(this.data[(byte)parameter], CultureInfo.InvariantCulture));
			}
			return null;
		}

		public static HintMessage.FormatValueHandler FormatValue { get; set; }

		private string PrepareFormattedValue(object value, HintMessageParameterFormat format)
		{
			if (HintMessage.FormatValue == null || format == HintMessageParameterFormat.None)
			{
				return (value == null) ? string.Empty : value.ToString();
			}
			string text = HintMessage.FormatValue(value, format);
			if (text == null)
			{
				return (value == null) ? string.Empty : value.ToString();
			}
			return text;
		}

		private static List<HintMessage.ParsedFormatInfo> Parse(string formatString)
		{
			List<HintMessage.ParsedFormatInfo> list = new List<HintMessage.ParsedFormatInfo>();
			if (string.IsNullOrEmpty(formatString))
			{
				return list;
			}
			IEnumerator enumerator = HintMessage.formatRegex.Matches(formatString).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Match match = (Match)obj;
					HintMessage.ParsedFormatInfo parsedFormatInfo = new HintMessage.ParsedFormatInfo
					{
						Pattern = match.Value
					};
					string text = match.Value.Trim(new char[] { '{', '}' });
					HintMessageParameterFormat hintMessageParameterFormat = HintMessageParameterFormat.None;
					int num = text.IndexOf('#');
					if (num > 0)
					{
						hintMessageParameterFormat = (HintMessageParameterFormat)Enum.Parse(typeof(HintMessageParameterFormat), text.Substring(num + 1));
						text = text.Substring(0, num);
					}
					parsedFormatInfo.Key = text;
					try
					{
						HintMessageParameterType hintMessageParameterType = (HintMessageParameterType)Enum.Parse(typeof(HintMessageParameterType), text);
						if (hintMessageParameterFormat == HintMessageParameterFormat.None)
						{
							switch (hintMessageParameterType)
							{
							case HintMessageParameterType.Length:
								hintMessageParameterFormat = HintMessageParameterFormat.Length;
								break;
							case HintMessageParameterType.Weight:
								hintMessageParameterFormat = HintMessageParameterFormat.Weight;
								break;
							case HintMessageParameterType.Temparature:
								hintMessageParameterFormat = HintMessageParameterFormat.Temperature;
								break;
							case HintMessageParameterType.Speed:
								hintMessageParameterFormat = HintMessageParameterFormat.Speed;
								break;
							}
						}
						parsedFormatInfo.Parameter = (byte)hintMessageParameterType;
					}
					catch
					{
					}
					parsedFormatInfo.Format = hintMessageParameterFormat;
					list.Add(parsedFormatInfo);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
			return list;
		}

		private string StringFormat(string formatString, List<HintMessage.ParsedFormatInfo> list)
		{
			if (string.IsNullOrEmpty(formatString))
			{
				return formatString;
			}
			foreach (HintMessage.ParsedFormatInfo parsedFormatInfo in list)
			{
				object obj;
				string text2;
				string text = ((parsedFormatInfo.Parameter == 0 || !this.data.TryGetValue(parsedFormatInfo.Parameter, out obj)) ? ((!this.strings.TryGetValue(parsedFormatInfo.Key, out text2)) ? parsedFormatInfo.Pattern : this.PrepareFormattedValue(text2, parsedFormatInfo.Format)) : this.PrepareFormattedValue(obj, parsedFormatInfo.Format));
				formatString = formatString.Replace(parsedFormatInfo.Pattern, text);
			}
			return formatString;
		}

		public bool IsStartMissionEvent()
		{
			MissionEventType eventType = this.EventType;
			return eventType == MissionEventType.MissionStarted || eventType == MissionEventType.MissionRestored;
		}

		public bool IsCancelMissionEvent()
		{
			switch (this.EventType)
			{
			case MissionEventType.MissionArchived:
			case MissionEventType.MissionCompleted:
			case MissionEventType.MissionCancelled:
			case MissionEventType.MissionDisabled:
			case MissionEventType.MissionFailed:
				return true;
			}
			return false;
		}

		public MissionOnClient Mission
		{
			get
			{
				return this.mission;
			}
		}

		public MissionTaskTrackedOnClient Task
		{
			get
			{
				return this.task;
			}
		}

		public HintMessage ApplyInit(Action<HintMessage> init)
		{
			if (init != null)
			{
				init(this);
			}
			return this;
		}

		public HintMessage SetMissionAndTask(MissionOnClient mission, MissionTaskTrackedOnClient task)
		{
			this.mission = mission;
			this.task = task;
			this.MissionId = mission.MissionId;
			this.MissionName = mission.Name;
			if (task != null)
			{
				this.TaskId = task.TaskId;
				this.TaskName = task.Code;
			}
			else
			{
				this.TaskId = 0;
			}
			return this;
		}

		public HintMessage SetMessageIdAsCode(string hash = null)
		{
			this.MessageId = string.Format("{0}|{1}|", this.Code, this.mission.LastUpdatedHash) + hash;
			return this;
		}

		public HintMessage SetMessageIdAsCodeItemId(string hash = null, string code = null)
		{
			if (code != null)
			{
				this.Code = code;
			}
			this.MessageId = string.Format("{0}: {1}|{2}|{3}|", new object[]
			{
				this.Code,
				this.ItemId,
				this.InstanceId,
				this.mission.LastUpdatedHash
			}) + hash;
			return this;
		}

		public HintMessage SetItemId(MissionsContext context, int itemId, HintItemClass itemClass, string instanceId = null, StoragePlaces? sourceStorage = null)
		{
			this.ItemId = itemId;
			this.ItemClass = itemClass;
			this.InstanceId = instanceId;
			if (sourceStorage != null)
			{
				this.SourceStorage = sourceStorage.Value;
			}
			return this;
		}

		public HintMessage SetOrderIndex(int orderIndex)
		{
			this.OrderIndex = orderIndex;
			return this;
		}

		public HintMessage SetAutomatic()
		{
			this.IsAutomatic = true;
			return this;
		}

		public HintMessage Translate(string translationCode = null)
		{
			this.task.TranslateMessage(this, translationCode);
			return this;
		}

		internal void TranslateBeforeSend(MissionsContext context)
		{
			if (this.shouldTranslateItemName)
			{
				switch (this.ItemClass)
				{
				case HintItemClass.InventoryItem:
				{
					InventoryItem inventoryItem = context.GetGameCacheAdapter().ResolveItemGloablly(this.ItemId);
					if (inventoryItem != null)
					{
						this.ItemName = inventoryItem.Name;
						this.ItemType = inventoryItem.ItemType;
						this.ItemSubType = inventoryItem.ItemSubType;
						this.CategoryId = (int)this.ItemSubType;
						this.RootCategoryId = (int)this.ItemType;
					}
					break;
				}
				case HintItemClass.InventoryCategory:
				{
					InventoryCategory inventoryCategory = context.GetGameCacheAdapter().ResolveInventoryCategory(this.ItemId);
					if (inventoryCategory != null)
					{
						this.ItemName = inventoryCategory.Name;
						if (inventoryCategory.ParentCategoryId == null)
						{
							this.ItemType = (ItemTypes)this.ItemId;
							this.ItemSubType = (ItemSubTypes)this.ItemId;
						}
						else
						{
							this.ItemType = (ItemTypes)inventoryCategory.ParentCategoryId.Value;
							this.ItemSubType = (ItemSubTypes)this.ItemId;
						}
						this.CategoryId = (int)this.ItemSubType;
						this.RootCategoryId = (int)this.ItemType;
					}
					break;
				}
				case HintItemClass.Pond:
					this.ItemName = context.GetGameCacheAdapter().ResolvePondName(this.ItemId);
					break;
				case HintItemClass.License:
					this.ItemName = context.GetGameCacheAdapter().ResolveLicenseName(this.ItemId);
					break;
				case HintItemClass.Fish:
					this.ItemName = context.GetGameCacheAdapter().ResolveFishName(this.ItemId);
					break;
				case HintItemClass.Achievement:
					this.ItemName = context.GetGameCacheAdapter().ResolveAchievementName(this.ItemId);
					break;
				}
				this.shouldTranslateItemName = false;
			}
			if (this.shouldTranslateCategoryName)
			{
				InventoryCategory inventoryCategory2 = context.GetGameCacheAdapter().ResolveInventoryCategory(this.CategoryId);
				if (inventoryCategory2 != null)
				{
					this.CategoryName = inventoryCategory2.Name;
				}
				this.shouldTranslateCategoryName = false;
			}
			if (this.shouldTranslateRootCategoryName)
			{
				InventoryCategory inventoryCategory3 = context.GetGameCacheAdapter().ResolveInventoryCategory(this.RootCategoryId);
				if (inventoryCategory3 != null)
				{
					this.RootCategoryName = inventoryCategory3.Name;
				}
				this.shouldTranslateRootCategoryName = false;
			}
		}

		public HintMessage CloneForClient()
		{
			HintMessage hintMessage = new HintMessage();
			hintMessage.data = this.data.ToDictionary((KeyValuePair<byte, object> p) => p.Key, (KeyValuePair<byte, object> p) => p.Value);
			hintMessage.strings = this.strings.ToDictionary((KeyValuePair<string, string> p) => p.Key, (KeyValuePair<string, string> p) => p.Value);
			HintMessage hintMessage2 = hintMessage;
			hintMessage2.OriginalTitle = null;
			hintMessage2.OriginalDescription = null;
			hintMessage2.OriginalTooltip = null;
			MissionInteractiveObject interactiveObject = hintMessage2.InteractiveObject;
			if (interactiveObject != null)
			{
				hintMessage2.InteractiveObjectId = interactiveObject.ResourceKey;
				hintMessage2.InteractiveVersion = interactiveObject.Version;
				hintMessage2.InteractiveObject = null;
			}
			if (hintMessage2.Spatial != null)
			{
				hintMessage2.Spatial = null;
			}
			foreach (KeyValuePair<byte, object> keyValuePair in hintMessage2.data.ToList<KeyValuePair<byte, object>>())
			{
				if (keyValuePair.Value == null)
				{
					hintMessage2.data.Remove(keyValuePair.Key);
				}
			}
			return hintMessage2;
		}

		private Dictionary<byte, object> data = new Dictionary<byte, object>();

		private Dictionary<string, string> strings = new Dictionary<string, string>();

		private List<HintMessage.ParsedFormatInfo> titleParsedFormatInfo;

		private List<HintMessage.ParsedFormatInfo> originalTitleParsedFormatInfo;

		private List<HintMessage.ParsedFormatInfo> descriptionParsedFormatInfo;

		private List<HintMessage.ParsedFormatInfo> originalDescriptionParsedFormatInfo;

		private List<HintMessage.ParsedFormatInfo> tooltipParsedFormatInfo;

		private List<HintMessage.ParsedFormatInfo> originalTooltipParsedFormatInfo;

		private bool shouldTranslateItemName;

		private bool shouldTranslateCategoryName;

		private bool shouldTranslateRootCategoryName;

		private static readonly Regex formatRegex = new Regex("\\{[\\w\\d_#]+\\}");

		private MissionOnClient mission;

		private MissionTaskTrackedOnClient task;

		public delegate string FormatValueHandler(object value, HintMessageParameterFormat format);

		private class ParsedFormatInfo
		{
			public string Pattern { get; set; }

			public string Key { get; set; }

			public byte Parameter { get; set; }

			public HintMessageParameterFormat Format { get; set; }
		}
	}
}
