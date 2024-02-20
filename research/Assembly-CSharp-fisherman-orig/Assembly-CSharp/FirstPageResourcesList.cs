using System;
using System.Collections.Generic;

public static class FirstPageResourcesList
{
	private const string path = "Textures/Local_Shops/Sliced/";

	public static readonly Dictionary<int, FirstPageResource> Values = new Dictionary<int, FirstPageResource>
	{
		{
			111,
			new FirstPageResource
			{
				PondId = 111,
				HeaderImage = "Textures/Local_Shops/Sliced/Emerald/Shop_Main_V1_Emerald_top",
				FooterImage = "Textures/Local_Shops/Sliced/Emerald/Shop_Main_V1_Emerald_bottom",
				MainImage = "Textures/Local_Shops/Sliced/Emerald/Shop_Main_V1_Emerald_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "EmeraldLeftCaption",
					Ids = new List<int> { 1171, 261, 606, 605, 724, 621, 622, 663, 668, 569 }
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Ids = new List<int> { 192, 747, 1156, 217, 216, 1471, 1142, 1192, 1452, 710 }
				}
			}
		},
		{
			2,
			new FirstPageResource
			{
				PondId = 2,
				HeaderImage = "Textures/Local_Shops/Sliced/LoneStar/Shop_Main_V1_LoneStar_top",
				FooterImage = "Textures/Local_Shops/Sliced/LoneStar/Shop_Main_V1_LoneStar_bottom",
				MainImage = "Textures/Local_Shops/Sliced/LoneStar/Shop_Main_V1_LoneStar_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "HomewaterLeftCaption",
					Ids = new List<int> { 294, 262, 210, 212, 209, 269, 172, 745, 434, 315 }
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "HomewaterRightCaption",
					Ids = new List<int> { 1001, 1134, 256, 246, 281, 282, 526, 529 }
				}
			}
		},
		{
			113,
			new FirstPageResource
			{
				PondId = 113,
				HeaderImage = "Textures/Local_Shops/Sliced/Everglades/Shop_Main_V1_Everglades_top",
				FooterImage = "Textures/Local_Shops/Sliced/Everglades/Shop_Main_V1_Everglades_bottom",
				MainImage = "Textures/Local_Shops/Sliced/Everglades/Shop_Main_V1_Everglades_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "EvergladesLeftCaption",
					Ids = new List<int>
					{
						597, 1256, 1257, 1259, 1260, 1261, 1262, 1263, 1177, 1024,
						385
					}
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Ids = new List<int>
					{
						524, 743, 1249, 1251, 1476, 1164, 1478, 784, 1148, 1304,
						1306, 682, 683, 684, 685
					}
				}
			}
		},
		{
			109,
			new FirstPageResource
			{
				PondId = 109,
				HeaderImage = "Textures/Local_Shops/Sliced/Falcon/Shop_Main_V1_Falcon_top",
				FooterImage = "Textures/Local_Shops/Sliced/Falcon/Shop_Main_V1_Falcon_bottom",
				MainImage = "Textures/Local_Shops/Sliced/Falcon/Shop_Main_V1_Falcon_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "FalconLeftCaption",
					Ids = new List<int>
					{
						277, 581, 617, 618, 619, 620, 273, 944, 945, 946,
						375
					}
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Ids = new List<int>
					{
						1066, 1067, 1068, 1069, 995, 996, 997, 1472, 1162, 1146,
						1453, 1455, 841, 1065
					}
				}
			}
		},
		{
			102,
			new FirstPageResource
			{
				PondId = 102,
				HeaderImage = "Textures/Local_Shops/Sliced/Mudwater/Shop_Main_V1_Mudwater_top",
				FooterImage = "Textures/Local_Shops/Sliced/Mudwater/Shop_Main_V1_Mudwater_bottom",
				MainImage = "Textures/Local_Shops/Sliced/Mudwater/Shop_Main_V1_Mudwater_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "MudwaterLeftCaption",
					Ids = new List<int>
					{
						283, 284, 572, 360, 342, 722, 723, 270, 1136, 1003,
						453, 501
					}
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Ids = new List<int> { 275, 1021, 1207, 451, 513, 1373 }
				}
			}
		},
		{
			106,
			new FirstPageResource
			{
				PondId = 106,
				HeaderImage = "Textures/Local_Shops/Sliced/Neherrin/Shop_Main_V1_Neherrin_top",
				FooterImage = "Textures/Local_Shops/Sliced/Neherrin/Shop_Main_V1_Neherrin_bottom",
				MainImage = "Textures/Local_Shops/Sliced/Neherrin/Shop_Main_V1_Neherrin_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "NeherrinLeftCaption",
					Ids = new List<int>
					{
						969, 1321, 1322, 276, 674, 676, 695, 600, 725, 728,
						782, 382, 1291, 1292, 1343
					}
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Ids = new List<int> { 642, 643, 644, 1178, 337, 518, 1160, 1468, 1200 }
				}
			}
		},
		{
			100,
			new FirstPageResource
			{
				PondId = 100,
				HeaderImage = "Textures/Local_Shops/Sliced/Rocky/Shop_Main_V1_Rocky_top",
				FooterImage = "Textures/Local_Shops/Sliced/Rocky/Shop_Main_V1_Rocky_bottom",
				MainImage = "Textures/Local_Shops/Sliced/Rocky/Shop_Main_V1_Rocky_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "RockyLeftCaption",
					Ids = new List<int>
					{
						968, 1184, 1183, 953, 361, 950, 951, 952, 623, 624,
						626, 627, 1217, 568, 358
					}
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Ids = new List<int>
					{
						1065, 1064, 1063, 1066, 1076, 1077, 1096, 1099, 1100, 1124,
						1125, 1126, 1117, 1118, 498, 669, 1451, 710
					}
				}
			}
		},
		{
			114,
			new FirstPageResource
			{
				PondId = 114,
				HeaderImage = "Textures/Local_Shops/Sliced/SanJoaquin/Shop_Main_V1_SanJoaquin_top",
				FooterImage = "Textures/Local_Shops/Sliced/SanJoaquin/Shop_Main_V1_SanJoaquin_bottom",
				MainImage = "Textures/Local_Shops/Sliced/SanJoaquin/Shop_Main_V1_SanJoaquin_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "SanJoaquinLeftCaption",
					Ids = new List<int> { 1051, 469, 1056, 1167, 217, 229, 1151, 1057, 1058, 699 }
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Ids = new List<int> { 1365, 1046, 1048, 1154, 1474, 1470 }
				}
			}
		},
		{
			115,
			new FirstPageResource
			{
				PondId = 115,
				HeaderImage = "Textures/Local_Shops/Sliced/Saint-Croix/Shop_Main_V1_Saint-Croix_top",
				FooterImage = "Textures/Local_Shops/Sliced/Saint-Croix/Shop_Main_V1_Saint-Croix_bottom",
				MainImage = "Textures/Local_Shops/Sliced/Saint-Croix/Shop_Main_V1_Saint-Croix_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "LakeRuaLeftCaption",
					Ids = new List<int> { 1179, 835, 793, 794, 819, 820, 821, 771, 772, 402 }
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Ids = new List<int> { 1035, 1036, 1182, 1324, 1326, 1170, 1212, 1208, 810, 811 }
				}
			}
		},
		{
			118,
			new FirstPageResource
			{
				PondId = 118,
				HeaderImage = "Textures/Local_Shops/Sliced/WhiteMoose/Shop_Main_V1_WhiteMoose_top",
				FooterImage = "Textures/Local_Shops/Sliced/WhiteMoose/Shop_Main_V1_WhiteMoose_bottom",
				MainImage = "Textures/Local_Shops/Sliced/WhiteMoose/Shop_Main_V1_WhiteMoose_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "WhiteMooseLeftCaption",
					Ids = new List<int>
					{
						192, 747, 712, 827, 844, 1163, 228, 229, 713, 1009,
						858
					}
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Ids = new List<int> { 1180, 1039, 1041, 1043, 922, 923, 1166, 1473, 1469, 1150 }
				}
			}
		},
		{
			119,
			new FirstPageResource
			{
				PondId = 119,
				HeaderImage = "Textures/Local_Shops/Sliced/LoneStar/Shop_Main_V1_LoneStar_top",
				FooterImage = "Textures/Local_Shops/Sliced/LoneStar/Shop_Main_V1_LoneStar_bottom",
				MainImage = "Textures/Local_Shops/Sliced/LoneStar/Shop_Main_V1_LoneStar_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "HomewaterLeftCaption",
					Ids = new List<int> { 294, 262, 210, 212, 209, 269, 172, 745, 434, 315 }
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "HomewaterRightCaption",
					Ids = new List<int> { 1001, 1134, 256, 246, 281, 282, 526, 529 }
				}
			}
		},
		{
			121,
			new FirstPageResource
			{
				PondId = 121,
				HeaderImage = "Textures/Local_Shops/Sliced/KaniqCreek/Shop_Main_V1_Alaska_top",
				FooterImage = "Textures/Local_Shops/Sliced/KaniqCreek/Shop_Main_V1_Alaska_bottom",
				MainImage = "Textures/Local_Shops/Sliced/KaniqCreek/Shop_Main_V1_Alaska_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "KaniqCreekLeftCaption",
					Ids = new List<int>
					{
						1358, 1359, 1360, 1361, 1354, 1355, 1356, 1357, 1368, 1333,
						1330, 1348, 1349, 1351, 1353
					}
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Ids = new List<int> { 1477, 1479, 1369, 1035, 1036, 1346, 1347, 1350, 1352 }
				}
			}
		},
		{
			123,
			new FirstPageResource
			{
				PondId = 123,
				HeaderImage = "Textures/Local_Shops/Sliced/Quanchkin/Shop_Main_V1_Quanchkin_top",
				FooterImage = "Textures/Local_Shops/Sliced/Quanchkin/Shop_Main_V1_Quanchkin_bottom",
				MainImage = "Textures/Local_Shops/Sliced/Quanchkin/Shop_Main_V1_Quanchkin_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "QuanchkinLeftCaption",
					Ids = new List<int>
					{
						1274, 652, 653, 656, 388, 913, 914, 423, 415, 1296,
						1300
					}
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Ids = new List<int>
					{
						1057, 1058, 1059, 1060, 1194, 1454, 1202, 1168, 229, 867,
						230, 1456, 1152
					}
				}
			}
		},
		{
			124,
			new FirstPageResource
			{
				PondId = 124,
				HeaderImage = "Textures/Local_Shops/Sliced/WWLake/Shop_Main_V1_WWLake_top",
				FooterImage = "Textures/Local_Shops/Sliced/WWLake/Shop_Main_V1_WWLake_bottom",
				MainImage = "Textures/Local_Shops/Sliced/WWLake/Shop_Main_V1_WWLake_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "WWLeftCaption",
					Ids = new List<int>
					{
						10460, 6670, 6680, 10490, 6270, 6280, 7280, 7290, 10310, 7590,
						10320, 6800, 6810, 6820, 7670, 7680, 9310, 9320, 10210, 10220,
						9330, 9340, 10230, 10240, 8200, 9560, 9570, 9580, 9590, 9600,
						9610, 9620, 9630, 9640, 9650, 9660, 9670
					}
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "WWRightCaption",
					Ids = new List<int>
					{
						6710, 6720, 6730, 6340, 6350, 6360, 7420, 7390, 7400, 7410,
						10370, 10380, 7730, 9380, 9390, 9170, 9180, 9350, 10160, 9360,
						10170, 8930, 8950, 9370, 10180, 10110, 10130, 9940, 9960
					}
				}
			}
		},
		{
			130,
			new FirstPageResource
			{
				PondId = 130,
				HeaderImage = "Textures/Local_Shops/Sliced/Mississippi/Shop_Main_V1_Mississippi_River_top",
				FooterImage = "Textures/Local_Shops/Sliced/Mississippi/Shop_Main_V1_Mississippi_River_bottom",
				MainImage = "Textures/Local_Shops/Sliced/Mississippi/Shop_Main_V1_Mississippi_River_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "MississippiLocalShop2Caption",
					Ids = new List<int>
					{
						13410, 13470, 13290, 13440, 13540, 4850, 747, 193, 1058, 227,
						229, 230, 228, 867, 14200, 14210, 14220, 14230, 14240, 14250,
						14260, 14270
					}
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "MississippiLocalShop1Caption",
					Ids = new List<int>
					{
						13370, 13380, 13390, 13310, 13320, 13330, 13900, 13940, 13980, 13100,
						13110, 13120, 13170, 13180, 13190, 13200, 13210, 13220
					}
				}
			}
		},
		{
			140,
			new FirstPageResource
			{
				PondId = 140,
				HeaderImage = "Textures/Local_Shops/Sliced/AhtubaRiver/Shop_Main_V1_AHTUBA_RIVER_MARKET_top",
				FooterImage = "Textures/Local_Shops/Sliced/AhtubaRiver/Shop_Main_V1_AHTUBA_RIVER_MARKET_bottom",
				MainImage = "Textures/Local_Shops/Sliced/AhtubaRiver/Shop_Main_V1_AHTUBA_RIVER_MARKET_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "AhtubaLeftCaption",
					Ids = new List<int>
					{
						4550, 4560, 4370, 4390, 4840, 4820, 4680, 229, 230, 3710,
						3700, 715, 1456, 716, 721, 858, 1452, 713
					}
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "AhtubaRightCaption",
					Ids = new List<int> { 1330, 1332, 1363, 918, 920, 1421, 1459, 1458 }
				}
			}
		},
		{
			150,
			new FirstPageResource
			{
				PondId = 150,
				HeaderImage = "Textures/Local_Shops/Sliced/LesniVila/Shop_Main_V1_LesniVila_top",
				FooterImage = "Textures/Local_Shops/Sliced/LesniVila/Shop_Main_V1_LesniVila_bottom",
				MainImage = "Textures/Local_Shops/Sliced/LesniVila/Shop_Main_V1_LesniVila_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "LesnaVilaLeftCaption",
					Ids = new List<int>
					{
						1663, 1678, 5270, 271, 1014, 4140, 209, 210, 212, 5000,
						3800, 4020, 434, 443
					}
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "LesnaVilaRightCaption",
					Ids = new List<int> { 246, 360, 278, 283, 284, 526, 529 }
				}
			}
		},
		{
			160,
			new FirstPageResource
			{
				PondId = 160,
				HeaderImage = "Textures/Local_Shops/Sliced/GhentTerneuzenCanal/Shop_Main_V1_GHENT_TERNEUZEN_CANAL_top",
				FooterImage = "Textures/Local_Shops/Sliced/GhentTerneuzenCanal/Shop_Main_V1_GHENT_TERNEUZEN_CANAL_bottom",
				MainImage = "Textures/Local_Shops/Sliced/GhentTerneuzenCanal/Shop_Main_V1_GHENT_TERNEUZEN_CANAL_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "ZeekanalLeftCaption",
					Ids = new List<int>
					{
						1661, 1662, 1675, 1676, 271, 1142, 4150, 4160, 3940, 3920,
						209, 249, 250, 3790, 5050, 4040, 4210, 5420, 706, 704,
						709
					}
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "ZeekanalRightCaption",
					Ids = new List<int> { 601, 383, 1018, 1409, 1410, 619, 620, 607, 608 }
				}
			}
		},
		{
			170,
			new FirstPageResource
			{
				PondId = 170,
				HeaderImage = "Textures/Local_Shops/Sliced/TiberRiver/Shop_Main_V1_TiberRiver_top",
				FooterImage = "Textures/Local_Shops/Sliced/TiberRiver/Shop_Main_V1_TiberRiver_bottom",
				MainImage = "Textures/Local_Shops/Sliced/TiberRiver/Shop_Main_V1_TiberRiver_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "TiberLeftCaption",
					Ids = new List<int>
					{
						5220, 1667, 5300, 1681, 414, 1025, 4640, 4200, 209, 210,
						212, 214, 215, 3660, 3670, 3740, 3850, 4040, 4030, 4000,
						4010, 706, 40, 434, 709
					}
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "TiberRightCaption",
					Ids = new List<int>
					{
						1029, 377, 1018, 192, 747, 212, 214, 215, 714, 717,
						511, 710
					}
				}
			}
		},
		{
			220,
			new FirstPageResource
			{
				PondId = 220,
				HeaderImage = "Textures/Local_Shops/Sliced/MakuMakuLake/Shop_Main_V1_Maku-Maku-Lake_top",
				FooterImage = "Textures/Local_Shops/Sliced/MakuMakuLake/Shop_Main_V1_Maku-Maku-Lake_bottom",
				MainImage = "Textures/Local_Shops/Sliced/MakuMakuLake/Shop_Main_V1_Maku-Maku-Lake_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "MakuMakuLeftCaption",
					Ids = new List<int>
					{
						13410, 13470, 13290, 13440, 13540, 4850, 747, 193, 1058, 214,
						215, 216, 217, 15550, 15560, 15570, 15590, 15530, 718, 711,
						14195, 14435, 14445, 14205
					}
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "MakuMakuRightCaption",
					Ids = new List<int>
					{
						15170, 15180, 15190, 15600, 15200, 15210, 15220, 15610, 13900, 13940,
						13980, 15410, 15420, 15430, 15440, 15450, 15460, 15470, 15480, 15490,
						15500, 15510, 15520, 14590, 14610, 14600, 14620, 14650, 14660, 14630,
						14640
					}
				}
			}
		},
		{
			180,
			new FirstPageResource
			{
				PondId = 180,
				HeaderImage = "Textures/Local_Shops/Sliced/Quanchkin/Shop_Main_V1_Quanchkin_top",
				FooterImage = "Textures/Local_Shops/Sliced/Quanchkin/Shop_Main_V1_Quanchkin_bottom",
				MainImage = "Textures/Local_Shops/Sliced/Quanchkin/Shop_Main_V1_Quanchkin_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "QuanchkinLeftCaption",
					Ids = new List<int>
					{
						1274, 652, 653, 656, 388, 913, 914, 423, 415, 1296,
						1300
					}
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Ids = new List<int>
					{
						1057, 1058, 1059, 1060, 1194, 1454, 1202, 1168, 229, 867,
						230, 1456, 1152
					}
				}
			}
		},
		{
			190,
			new FirstPageResource
			{
				PondId = 190,
				HeaderImage = "Textures/Local_Shops/Sliced/LaCreuse/Shop_Main_V1_Pecheur_top",
				FooterImage = "Textures/Local_Shops/Sliced/LaCreuse/Shop_Main_V1_Pecheur_bottom",
				MainImage = "Textures/Local_Shops/Sliced/LaCreuse/Shop_Main_V1_Pecheur_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "CreuseLeftCaptionRetail",
					Ids = new List<int>
					{
						1672, 1668, 1686, 5300, 1628, 414, 4640, 4650, 3770, 3790,
						4220, 214, 215, 216, 435, 434, 718, 1440
					}
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "CreuseRightCaption",
					Ids = new List<int>
					{
						1273, 1274, 387, 415, 1010, 786, 782, 783, 784, 1045,
						1047, 798, 799, 800, 801, 802, 803, 804, 805
					}
				}
			}
		},
		{
			200,
			new FirstPageResource
			{
				PondId = 200,
				HeaderImage = "Textures/Local_Shops/Sliced/SanderBaggerseeLake/Shop_Main_V1_SANDER_BAGGERSEE_LAKE_top",
				FooterImage = "Textures/Local_Shops/Sliced/SanderBaggerseeLake/Shop_Main_V1_SANDER_BAGGERSEE_LAKE_bottom",
				MainImage = "Textures/Local_Shops/Sliced/SanderBaggerseeLake/Shop_Main_V1_SANDER_BAGGERSEE_LAKE_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "GermanyLeftCaption",
					Ids = new List<int>
					{
						4540, 4370, 1368, 4680, 229, 230, 3690, 3700, 708, 713,
						716, 721
					}
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "GermanyRightCaption",
					Ids = new List<int> { 5260, 5330, 4750, 1353, 1351, 1352 }
				}
			}
		},
		{
			210,
			new FirstPageResource
			{
				PondId = 210,
				HeaderImage = "Textures/Local_Shops/Sliced/Quanchkin/Shop_Main_V1_Quanchkin_top",
				FooterImage = "Textures/Local_Shops/Sliced/Quanchkin/Shop_Main_V1_Quanchkin_bottom",
				MainImage = "Textures/Local_Shops/Sliced/Quanchkin/Shop_Main_V1_Quanchkin_center",
				LeftPanelSet = new FirstPageResource.RequestSet
				{
					Caption = "QuanchkinLeftCaption",
					Ids = new List<int>
					{
						1274, 652, 653, 656, 388, 913, 914, 423, 415, 1296,
						1300
					}
				},
				RightPanelSet = new FirstPageResource.RequestSet
				{
					Ids = new List<int>
					{
						1057, 1058, 1059, 1060, 1194, 1454, 1202, 1168, 229, 867,
						230, 1456, 1152
					}
				}
			}
		}
	};
}
