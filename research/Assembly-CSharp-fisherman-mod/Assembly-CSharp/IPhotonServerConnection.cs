using System;
using System.Collections.Generic;
using System.IO;
using BiteEditor;
using ExitGames.Client.Photon;
using ObjectModel;
using ObjectModel.Common;
using Photon.Interfaces;
using Photon.Interfaces.Game;
using Photon.Interfaces.LeaderBoards;
using Photon.Interfaces.Tournaments;
using TPM;
using UnityEngine;

public interface IPhotonServerConnection : IPhotonPeerListener
{
	bool IsMessageQueueRunning { get; }

	bool IsConnectedToMaster { get; }

	bool IsConnectedToGameServer { get; }

	bool IsAuthenticated { get; }

	bool IsSilentMode { get; set; }

	bool IsChatMessagingOn { get; set; }

	Profile Profile { get; }

	bool IsPondStayFinished { get; }

	void StopPhotonMessageQueue();

	void StartPhotonMessageQueue();

	event Action OnDisconnect;

	event Action OnConnectionFailed;

	DateTime ServerUtcNow { get; }

	LoadbalancingPeer Peer { get; }

	string MasterServerAddress { get; set; }

	ConnectionProtocol Protocol { get; set; }

	string AppName { get; set; }

	string Environment { get; }

	string Email { get; set; }

	string Password { get; set; }

	string SteamAuthTicket { get; set; }

	void BackupSteamAuthTicket();

	string UserName { get; }

	StatusCode ConnectionState { get; }

	string LastError { get; }

	string UserId { get; }

	bool? IsTempUser { get; }

	RoomDesc Room { get; }

	void ConnectToMaster();

	event Action OnConnectedToMaster;

	void ConnectToMasterNoAuth();

	void Authenticate();

	event Action OnAuthenticated;

	event OnFailure OnAuthenticationFailed;

	event OnFailure OnRefreshSecurityTokenFailed;

	void Disconnect();

	void Reset();

	void ReuseCaches();

	void GracefulDisconnect();

	event Action OnPreviewQuitComplete;

	event OnFailure OnPreviewQuitFailed;

	void RefreshSecurityToken();

	OperationResponse LastResponse { get; }

	void GetProtocolVersion();

	event Action<bool> OnGotProtocolVersion;

	event OnFailure OnOperationFailed;

	event Action OnDuplicateLogin;

	event Action<OperationResponse> OnOpCompleted;

	void RequestProfileOnError(OnGotProfile continuation = null, bool refresh = false);

	void RefreshProfile();

	void RequestProfile();

	event OnGotProfile OnGotProfile;

	void RequestOtherPlayerProfile(string userId);

	event OnGotProfile OnGotOtherPlayerProfile;

	void RequestStats();

	event OnGotStats OnGotStats;

	void CheckEmailIsUnique(string email);

	event Action<bool> OnCheckEmailIsUnique;

	void CheckUsernameIsUnique(string userName);

	event Action<bool> OnCheckUsernameIsUnique;

	void RegisterNewAccount(string name, string password, string email, int languageId = 1, string source = null, string extId = null, string promoCode = null);

	void RegisterTempAccount(string name, int languageId = 1);

	void UpdateProfile(Profile profile2Save);

	void UpdateProfileSettings(Dictionary<string, string> profileSettings);

	event Action OnProfileUpdated;

	void ChangePassword(string oldPassword, string newPassword);

	void SetLastPin(int pindId);

	event Action OnChangePassword;

	void MakeTempAccountPersistent(string name, string password, string email);

	event Action OnMakeTempAccountPersistent;

	void RequestPasswordRecoverOnEmail(string email, string name);

	void RecoverPassword(string email, string token, string newPassword);

	event Action OnPasswordRecoverRequested;

	event Action OnPasswordRecovered;

	void RequestEndOfMissionResult();

	event OnEndOfPeriodResult OnEndOfDayResult;

	event OnEndOfPeriodResult OnEndOfMissionResult;

	void ReleaseFishFromFishCage(global::ObjectModel.Fish fish);

	event Action<Guid> OnReleaseFishFromFishCage;

	event OnFailure OnReleaseFishFromFishCageFailed;

	void RequestFishCage();

	event Action OnRequestFishCage;

	event OnFailure OnRequestFishCageFailed;

	void SkipTutorial();

	event Action OnTutorialSkipped;

	event OnFailure OnSkipTutorialFailed;

	void SetModifier(float value);

	event OnGotProfile OnRegisterUser;

	int LevelCap { get; }

	event Action OnPrimarySteamAuthenticationFailed;

	void ChangeName(string newName);

	event Action OnNameChanged;

	event OnFailure OnNameChangeFailed;

	event Action<StatsCounterType, int> OnStatsUpdated;

	event Action<int, int> OnExpGained;

	event Action<ProfileFailure> OnProfileOperationFailed;

	event Action<Amount> OnMoneyUpdate;

	void ResetProfileToDefault();

	event Action OnResetProfileToDefault;

	event OnFailure OnResetProfileToDefaultFailed;

	void SetAvatarUrl(string url);

	event Action OnAvatarUrlSet;

	event OnFailure OnSettingAvatarUrlFailed;

	void GetPlayerPlacementByExternalId(string externalId);

	event Action<int?, string> OnGotPlayerPlacement;

	event OnFailure OnGettingPlayerPlacementFailed;

	void FlagSnowballHit(string targetUserId);

	event Action OnSnowballHitFlagged;

	event OnFailure OnFlagSnowballHitFailed;

	void GeneratePromoCode();

	event Action OnPromoCodeGenerated;

	event OnFailure OnGeneratePromoCodeFailed;

	void CheckInviteIsUnique(string email);

	event Action OnInviteIsUnique;

	event OnFailure OnInviteDuplicated;

	void CreateInvite(string email, string name);

	event Action OnInviteCreated;

	event OnFailure OnCreateInviteFailed;

	void CheckPromoCode(string promoCode);

	event Action OnPromoCodeValid;

	event OnFailure OnPromoCodeInvalid;

	event OnReferralReward OnReferralReward;

	int? CurrentPondId { get; }

	int? CurrentTournamentId { get; }

	bool IsInRoom { get; }

	bool IsOnBase { get; }

	bool LockConnection();

	void ReleaseConnection();

	void MoveToBase();

	void MoveToPond(int pondId, int? days, bool? hasMovedByCar = null);

	void MoveToRoom(int pondId, string roomName = null);

	void MoveToTournamentRoom(int pondId, int tournamentId);

	event OnMoved OnMoved;

	event OnFailure OnMoveFailed;

	void ProlongPondStay(int days);

	event Action<ProlongInfo> OnPondStayProlonged;

	event Action OnRoomIsFull;

	event OnFailure OnUnableToEnterRoom;

	void UnsubscribeProfileEvents();

	void ChangeGameScreen(GameScreenType gameScreen, GameScreenTabType gameScreenTab = GameScreenTabType.Undefined, int? categoryId = null, int? itemId = null, int[] childCategoryIds = null, string categoryElementId = null, string[] categoryElementsPath = null);

	event OnGameScreenChanged GameScreenChanged;

	event OnFailure ChangeGameScreenFailed;

	void ChangeSelectedElement(GameElementType type, string value, int? itemId = null);

	event OnSelectedElementChanged SelectedElementChanged;

	event OnFailure ChangeSelectedElementFailed;

	void ChangeSwitch(GameSwitchType type, bool value, int? slot = null);

	event OnSwitchChanged SwitchChanged;

	event OnFailure ChangeSwitchFailed;

	void ChangeIndicator(GameIndicatorType type, int value);

	event OnIndicatorChanged IndicatorChanged;

	event OnFailure ChangeIndicatorFailed;

	void SendGameAction(GameActionType type);

	event OnGameActionSent GameActionSent;

	event OnFailure SendGameActionFailed;

	void SendPlayerPositionAndRotation(Vector3 playerPosition, Quaternion playerRotation, Vector3? targetPoint);

	event Action PlayerPositionAndRotationSent;

	event OnFailure SendPlayerPositionAndRotationFailed;

	void RestartArchivedMission(int missionId, bool activate);

	event OnActiveMissionSet ArchivedMissionRestarted;

	event OnFailure RestartArchivedMissionFailed;

	event OnMissionHintsReceived MissionHintsReceived;

	event OnMissionProgressReceived MissionProgressReceived;

	event Action<int> ActiveMissionChanged;

	void OnMissionHintsReceived(List<HintMessage> messages);

	void GetMissionsStarted();

	void GetMissionsCompleted();

	void GetMissionsArchived();

	void GetMissionsFailed();

	event OnMissionsListReceived MissionsListReceived;

	event OnFailure GetMissionsListFailed;

	void GetActiveMission();

	event OnMissionGet ActiveMissionGet;

	event OnFailure ActiveMissionGetFailed;

	void GetMission(int missionId);

	event OnMissionGet MissionGet;

	event OnFailure MissionGetFailed;

	void SetActiveMission(int? missionId);

	event OnActiveMissionSet ActiveMissionSet;

	event OnFailure ActiveMissionSetFailed;

	void DisableMissionActivation();

	event Action MissionActivationDisabled;

	event OnFailure DisableMissionActivationFailed;

	event OnMissionReward OnMissionReward;

	event OnMissionFailed OnMissionTimedOut;

	event OnMissionTracked OnMissionTrackStarted;

	event OnMissionTracked OnMissionTrackStopped;

	void GetMissionInteractiveObject(int missionId, string resourceKey);

	event OnGotInteractiveObject OnGotInteractiveObject;

	event OnFailure GetInteractiveObjectFailed;

	void SendMissionInteraction(int missionId, string resourceKey, string interaction, object argument);

	event OnSendMissionInteraction MissionInteractionSent;

	event OnFailure SendMissionInteractionFailed;

	void CompleteMissionClientCondition(int missionId, int taskId, string resourceKey, bool completed, string progress);

	event OnCompleteMissionClientCondition MissionClientConditionCompleted;

	event OnFailure CompleteMissionClientConditionFailed;

	void ForceProcessMissions();

	event Action OnForceProcessMissions;

	event OnFailure OnForceProcessMissionsFailed;

	void GetAvailablePonds(int countryId);

	event Action<IEnumerable<PondBrief>> OnGotAvailablePonds;

	void GetAvailableLocations(int? pondId);

	event Action<IEnumerable<LocationBrief>> OnGotAvailableLocations;

	void GetPondInfo(int pondId);

	event Action<Pond> OnGotPondInfo;

	void GetFishList(int[] fishIds);

	event OnGotFishList OnGotFishList;

	event OnFailure OnErrorGettingFishList;

	void GetFishCategories();

	event OnGotFishCategories OnGotFishCategories;

	event OnFailure OnErrorGettingFishCategories;

	void GetPondWeather(int pondId, int days = 1);

	event OnGotPondWeather OnGotPondWeather;

	event OnGotPondWeather OnGotPondWeatherForecast;

	void GetAllPondsWeather();

	event OnGotAllPondWeather OnGotAllPondWeather;

	void GetLocationInfo(int locationId);

	event Action<Location> OnGotLocationInfo;

	void GetLicenses(int? stateId = null, int? pondId = null, int? regionId = null);

	void GetStates(int countryId);

	event Action<IEnumerable<State>> OnGotStates;

	event OnFailure OnGettingStatesFailed;

	void PreviewChangeResidenceState(int stateId);

	event Action<ChangeResidenceInfo> OnPreviewResidenceChange;

	void ChangeResidenceState(int stateId);

	event Action<ChangeResidenceInfo> OnResidenceChanged;

	void GetMinTravelCost();

	event Action<int> OnGotMinTravelCost;

	void FinishTutorial();

	event Action OnTutorialFinished;

	void GetPondStats(int? pondId);

	event OnGotPondStats OnGotPondStats;

	event OnFailure OnGettingPondStatsFailed;

	void GetRoomsPopulation(int pondId);

	event OnGotRoomsPopulation OnGotRoomsPopulation;

	event OnFailure OnGettingRoomsPopulationFailed;

	void GetCurrentRoomPopulation();

	event OnPlayersAction OnGotCurrentRoomPopulation;

	event OnFailure OnGettingCurrenRoomPopulationFailed;

	void GetOwnRoom();

	event Action<string> OnGotOwnRoom;

	event OnFailure OnGettingOwnRoomFailed;

	void GetInteractiveObjects(int pondId);

	event OnGotInteractiveObjects OnGotInteractiveObjects;

	event OnFailure OnGettingInteractiveObjectsFailed;

	void InteractWithObject(InteractiveObject obj);

	event OnInteractedWithObject OnInteractedWithObject;

	event OnFailure OnInteractWithObjectFailed;

	event OnGotLicenses OnGotLicenses;

	void RentBoat(int boatId, int daysCount);

	event Action OnBoatRented;

	event OnFailure OnErrorRentingBoat;

	event Action<short[]> OnBoatsRentFinished;

	void OutdateRent();

	event Action OnOutdateRent;

	event OnFailure OnErrorOutdateRent;

	event Action<PondStayFinish> OnPondStayFinish;

	event OnIncomingChatMessage OnIncomingChatMessage;

	event OnIncomingChatMessage OnIncomingChannelMessage;

	event OnIncognitoChanged OnIncognitoChanged;

	void SendChatMessage(string recepient, string message, string channel = null, string group = null, string data = null, bool isOffline = false, DateTime? expiration = null);

	event OnLocalEvent OnLocalEvent;

	void RefreshFriendsDetails();

	event Action OnFriendsDetailsRefreshed;

	event OnFailure OnFriendsDetailsRefreshFailed;

	void FindPlayersByName(string name);

	void FindPlayersByEmail(string email);

	event OnPlayersAction OnFindPlayers;

	event OnFailure OnFindPlayersFailed;

	int FriendsCount { get; }

	void RequestFriendship(Player player);

	event OnFriendshipRequest OnFriendshipRequest;

	void UnFriend(Player player, bool ignore);

	event OnFriendAction OnFriendshipAdded;

	event OnFriendAction OnFriendshipRemoved;

	void IgnorePlayer(Player player);

	void UnIgnorePlayer(Player player);

	void ReplyToFriendshipRequest(Player player, FriendshipAction reaction);

	void GetChatPlayersCount();

	event OnGotChatPlayersCount OnGotChatPlayersCount;

	event OnFailure OnGettingChatPlayersCountFailed;

	void SendChatCommand(ChatCommands command, string username);

	event OnChatCommandSucceeded OnChatCommandSucceeded;

	event OnFailure OnChatCommandFailed;

	void AddFriendsByExternalIds(string source, IEnumerable<string> newFriendsList);

	event OnPlayersAction OnFriendsAddedByExternalIds;

	void SyncFriendsByExternalIds(string source, IEnumerable<string> friendExternalIds);

	event OnPlayersAction OnFriendsSyncedByExternalIds;

	void JoinChatChannel(string channelId);

	void LeaveChatChannel(string channelId);

	void SendChatMessageToChannel(string channelId, string message);

	void ExpireChatChannel(string channelId, DateTime expiration);

	void SwapRods(Rod currentRodInHands, Rod rodToHands);

	void MoveItemOrCombine(InventoryItem item, InventoryItem parent, StoragePlaces storage, bool moveRelatedItems = true);

	void SplitItem(InventoryItem item, InventoryItem parent, int count, StoragePlaces storage);

	void SplitItem(InventoryItem item, InventoryItem parent, float amount, StoragePlaces storage);

	void CombineItems(InventoryItem item, InventoryItem targetItem);

	void SubordinateItem(InventoryItem parent, InventoryItem newParent);

	void ReplaceItem(InventoryItem item, InventoryItem replacementItem);

	void SplitItemAndReplace(InventoryItem item, InventoryItem replacementItem, int count);

	void SplitItemAndReplace(InventoryItem item, InventoryItem replacementItem, float amount);

	void SetActiveQuiverTip(FeederRod rod, int index);

	event Action OnActiveQuiverTipSet;

	event OnFailure OnSetActiveQuiverTipFailed;

	void PutRodOnStand(int slot, bool isRodOnStand);

	event Action OnPutRodOnStand;

	event OnFailure OnPutRodOnStandFailed;

	void BeginFillChum(Chum[] chums);

	event Action OnBeginFillChumDone;

	event OnFailure OnBeginFillChumFailed;

	void FinishFillChum(Chum[] chums);

	event Action<Chum[]> OnFinishFillChumDone;

	event OnFailure OnFinishFillChumFailed;

	void CancelFillChum(Chum[] chums);

	event Action<Chum[]> OnCancelFillChumDone;

	event OnFailure OnCancelFillChumFailed;

	void ThrowChum(Chum[] chums);

	event Action<Chum[]> OnThrowChumDone;

	event OnFailure OnThrowChumFailed;

	event Action OnInventoryMoved;

	event Action OnInventoryMoveFailure;

	void PutRodStand(InventoryItem item, bool put);

	event Action OnPutRodStand;

	event OnFailure OnPutRodStandFailed;

	bool ProfileWasRequested { get; }

	event Action OnInventoryUpdated;

	event Action OnInventoryUpdateCancelled;

	void RaiseInventoryUpdateCancelled();

	event OnFailure OnInventoryUpdatedFailure;

	void RaiseInventoryUpdated(bool profileRequested = false);

	void GetItemCategories(bool topLevelOnly = true);

	void GetItemsCountForCategories(int? pondId = null);

	event Action<List<CategoryItemsCount>> OnGotItemsCountForCategories;

	void GetGlobalItemsFromCategory(int[] categoryIds, bool allItems = false);

	void GetLocalItemsFromCategory(int[] categoryIds, int pondId);

	event OnGotItems OnGotItemsForCategory;

	event OnFailure OnGettingItemsForCategoryFailed;

	void GetItemsByIds(int[] ids, int subscriberId, bool useGlobalContext = false);

	event OnGotItemsSubscribed OnGotItems;

	event OnFailure OnGettingItemsFailed;

	void BuyItem(InventoryItem item, int? itemCount = null);

	event Action<InventoryItem> OnItemBought;

	void BuyItemAndSendToBase(InventoryItem item, int? itemCount = null);

	void DestroyItem(InventoryItem item);

	event Action<InventoryItem> OnItemDestroyed;

	event Action<TransactionFailure> OnTransactionFailed;

	void BuyLicense(ShopLicense license, int term);

	event Action<PlayerLicense> OnLicenseBought;

	void BuyProduct(StoreProduct product, int? count = null);

	event OnProductBought OnProductBought;

	event OnFailure OnBuyProductFailed;

	void DestroyLicense(PlayerLicense license);

	event Action<PlayerLicense> OnLicenseDestroyed;

	void RepairItem(InventoryItem item);

	void SetLeaderLength(Rod rod);

	event Action OnSetLeaderLength;

	event OnFailure OnSetLeaderLengthFailed;

	void GetBrands();

	event OnGotBrands OnGotBrands;

	void ConsumeItem(InventoryItem item, int itemCount);

	event Action OnItemConsumed;

	event OnFailure OnConsumeItemFailed;

	void MakeGift(Player receiver, InventoryItem item, int itemCount);

	event Action OnGiftMade;

	event OnFailure OnMakingGiftFailed;

	event Action<Player, IEnumerable<InventoryItem>> OnItemGifted;

	event OnGotItemCategories OnGotItemCategories;

	void SetBuoy(string name, Point3 position, CaughtFish fish);

	event Action<BuoySetting> OnBuoySet;

	event OnFailure OnBuoySettingFailed;

	void TakeBuoy(int bouyId);

	event Action<int> OnBuoyTaken;

	event OnFailure OnBuoyTakingFailed;

	void RenameBuoy(int buoyId, string name);

	event Action OnBuoyRenamed;

	event OnFailure OnBuoyRenamingFailed;

	void ShareBuoy(int buoyId, string receiver);

	event Action OnBuoyShared;

	event OnFailure OnBuoySharingFailed;

	event OnReceiveBuoyShareRequest OnReceiveBuoyShareRequest;

	void AcceptSharedBuoy(int buoyRequestId);

	event Action OnBuoyShareAccepted;

	event OnFailure OnBuoyShareAcceptingFailed;

	void DeclineSharedBuoy(int buoyRequestId);

	event Action OnBuoyShareDeclined;

	event OnFailure OnBuoyShareDecliningFailed;

	void SetShimsCount(Waggler waggler, int shimsCount);

	event Action OnShimsCountSet;

	event OnFailure OnSetShimsCountFailed;

	void RodSetupSaveNew(int slot, string name);

	event OnSaveNewRodSetup OnSaveNewRodSetup;

	event OnFailure OnSaveNewRodSetupFailure;

	void RodSetupRemove(RodSetup setup);

	event OnRemoveRodSetup OnRodSetupRemove;

	event OnFailure OnRodSetupRemoveFailure;

	void RodSetupRename(RodSetup setup, string name);

	event OnRenameRodSetup OnRodSetupRename;

	event OnFailure OnRodSetupRenameFailure;

	void RodSetupEquip(RodSetup setup, int slot);

	event OnEquipRodSetup OnRodSetupEquip;

	event OnFailure OnRodSetupEquipFailure;

	void RodSetupShare(RodSetup setup, string receiver);

	event OnShareRodSetup OnSharedRodSetup;

	event OnFailure OnShareRodSetupFailure;

	event OnReceiveNewRodSetup OnReceiveNewRodSetup;

	void RodSetupAdd(RodSetup setup);

	event OnSaveNewRodSetup OnAddRodSetup;

	event OnFailure OnAddRodSetupFailure;

	void MixChum(Chum chum);

	event Action<Chum> OnChumMix;

	event OnFailure OnChumMixFailure;

	void RenameChum(Chum chum, string name);

	event Action<Chum> OnChumRename;

	event OnFailure OnChumRenameFailure;

	void ChumRecipeSaveNew(Chum chum);

	event Action<Chum> OnChumRecipeSaveNew;

	event OnFailure OnChumRecipeSaveNewFailure;

	void ChumRecipeRename(Chum chum, string name);

	event Action<Chum> OnChumRecipeRename;

	event OnFailure OnChumRecipeRenameFailure;

	void ChumRecipeRemove(Chum chum);

	event Action<Chum> OnChumRecipeRemove;

	event OnFailure OnChumRecipeRemoveFailure;

	void ChumRecipeShare(Chum recipe, string receiver);

	event OnShareChumRecipe OnChumRecipeShare;

	event OnFailure OnChumRecipeShareFailure;

	event OnReceiveNewChumRecipe OnReceiveNewChumRecipe;

	Game Game { get; }

	Game PendingGame { get; }

	Game GetGameSlot(int index);

	int CurrentGameSlotIndex { get; }

	int PendingGameSlotIndex { get; }

	event OnGameActionResult OnGameActionResult;

	event OnGameEvent OnGameEvent;

	event OnGameHint OnGameHint;

	event OnLicensePenalty OnLicensePenalty;

	event OnLicensePenaltyWarning OnLicensePenaltyWarning;

	event OnItemBroken OnItemBroken;

	event OnItemLost OnItemLost;

	event OnItemLost OnChumLost;

	event OnItemLost OnWearedItemLostOnDeequip;

	event OnItemGained OnItemGained;

	event OnRodUnequiped OnRodUnequiped;

	event OnBaitLost OnBaitLost;

	event OnBaitReplenished OnBaitReplenished;

	event OnNeedClientReset OnNeedClientReset;

	event OnRodCantBeUsed OnRodCantBeUsed;

	event Action<LevelInfo> OnLevelGained;

	event Action<AchivementInfo> OnAchivementGained;

	event Action<BonusInfo> OnBonusGained;

	event Action<HashSet<ushort>> OnFBAZonesUpdate;

	void GetSounderFish(Point3 position);

	event Action<float[]> OnGotSounderFish;

	event OnFailure OnGettingSounderFishFailed;

	void GetServerTime(int callerId);

	event OnGotServerTime OnGotServerTime;

	void GetTime();

	event OnGotTime OnGotTime;

	void MoveTimeForward(int? hours);

	event OnGotTime OnMovedTimeForward;

	event OnFailure OnMovingTimeForwardFailed;

	DateTime? PreviewMoveTimeForward(bool hasPremium, int? hours);

	int RemoveTimeForwardCooldownCost { get; }

	float PremiumTimeForwardCooldownDiff { get; }

	void GetTimeForwardCooldown();

	event OnGotTimeForwardCooldown OnGotTimeForwardCooldown;

	event OnFailure OnGettingTimeForwardCooldownFailed;

	void RemoveTimeForwardCooldown();

	event Action OnRemovedTimeForwardCooldown;

	event OnFailure OnRemovingTimeForwardCooldownFailed;

	void DebugFishing(string message);

	void DebugInventory(string message);

	void DebugSecurity(string message);

	void DebugClient(byte flagIndex, string message);

	void PinFps(float fps);

	void PinError(string message, string stackTrace);

	void PinError(Exception ex);

	void PinSysInfo(SysInfo info);

	void SaveTelemetryInfo(TelemetryCode code, string message);

	void SaveBinaryData(byte[] data);

	void DebugBiteSystemEvents(bool enabled);

	event Action OnDebugBiteSystemEvents;

	event OnFailure OnDebugBiteSystemEventsFailed;

	event Action<ChumPiece> OnChumPieceUpdated;

	void GetTopPlayers(TopScope scope, TopPlayerOrder order);

	void GetTopFish(TopScope scope, TopFishOrder order, int? pondId, int? fishCategoryId, int[] fishId = null);

	void GetTopTournamentPlayers(TopScope scope, TopTournamentKind tournamentKind);

	event OnGotLeaderboards OnGotLeaderboards;

	event OnFailure OnGettingLeaderboardsFailed;

	void GetTournamentSeries();

	event OnGotTournamentSeries OnGotTournamentSeries;

	event OnFailure OnGettingTournamentSeriesFailed;

	void GetTournamentSerieInstances();

	event OnGotTournamentSerieInstances OnGotTournamentSerieInstances;

	event OnFailure OnGettingTournamentSerieInstancesFailed;

	void GetTournamentGrid();

	event OnGotTournamentGrid OnGotTournamentGrid;

	event OnFailure OnGettingTournamentGridFailed;

	void GetTournaments(TournamentKinds? kind, int? pondId, DateTime? start = null, DateTime? end = null);

	event OnGotTournamentsByKind OnGotTournaments;

	event OnFailure OnGettingTournamentsFailed;

	void GetTournament(int tournamentId);

	event OnGotTournament OnGotTournament;

	event OnFailure OnGettingTournamentFailed;

	void GetTournamentWeather(int tournamentId);

	event OnGotPondWeather OnGotTournamentWeather;

	event OnFailure OnGettingTournamentWeatherFailed;

	void GetTournamentRewards(int tournamentId);

	event OnGotTournamentRewards OnGotTournamentRewards;

	event OnFailure OnGettingTournamentRewardsFailed;

	void GetOpenTournaments(int? kindId, int? pondId);

	event OnGotTournaments OnGotOpenTournaments;

	event OnFailure OnGettingOpenTournamentsFailed;

	void GetMyTournaments(int? pondId);

	event OnGotTournaments OnGotMyTournaments;

	event OnFailure OnGettingMyTournamentsFailed;

	void GetPlayerTournaments(int? pondId);

	event OnGotTournaments OnGotPlayerTournaments;

	event OnFailure OnGettingPlayerTournamentsFailed;

	void GetTournamentParticipationHistory(int? tournamentTemplateId);

	event OnGotTournamentResults OnGotTournamentParticipationHistory;

	event OnFailure OnGettingTournamentParticipationHistoryFailed;

	void RegisterForTournament(int tournamentId, bool removeOtherRegistrations = false);

	event Action OnRegisteredForTournament;

	event OnFailure OnRegisterForTournamentFailed;

	void UnregisterFromTournament(int tournamentId);

	event Action OnUnregisteredFromTournament;

	event OnFailure OnUnregisterFromTournamentFailed;

	void GetLatestEula();

	event OnGotLatestEula OnGotLatestEula;

	event OnFailure OnGettingLatestEulaFailed;

	void SignEula(int version);

	event Action OnEulaSigned;

	event OnFailure OnEulaSignFailed;

	void RegisterForTournamentSerie(int serieInstanceId, bool removeOtherRegistrations = false);

	event Action OnRegisteredForTournamentSerie;

	event OnFailure OnRegisterForTournamentSerieFailed;

	TimeSpan? TournamentRemainingTime { get; }

	DateTime? TournamentRemainingTimeReceived { get; }

	void PreviewTournamentStart();

	event Action OnTournamentStartPreview;

	event OnFailure OnPreviewTournamentStartFailed;

	void GetIntermediateTournamentResult(int? tournamentId = null);

	event OnGotTournamentResults OnGotTournamentIntermediateResult;

	void GetFinalTournamentResult(int tournamentId);

	event OnGotTournamentResults OnGotTournamentFinalResult;

	void GetIntermediateTournamentTeamResult(int? tournamentId = null);

	event OnGotTournamentTeamResults OnGotIntermediateTournamentTeamResult;

	void GetFinalTournamentTeamResult(int tournamentId);

	event OnGotTournamentTeamResults OnGotFinalTournamentTeamResult;

	void GetSecondaryTournamentResult(int tournamentId);

	event OnGotTournamentSecondaryResult OnGotTournamentSecondaryResult;

	void EndTournament(bool prematurely);

	event Action OnTournamentEnded;

	event OnFailure OnTournamentEndFailed;

	void GetTournamentSerieStats();

	event OnGotTournamentSerieStats OnGotTournamentSerieStats;

	event OnFailure OnGettingTournamentSerieStatsFailed;

	event Action OnTournamentScoringStarted;

	event OnFailure OnStartTournamentScoringFailed;

	event OnTournamentStarted OnTournamentStarted;

	event OnTournamentCancelled OnTournamentCancelled;

	event Action<ErrorCode> OnTournamentWrongTackle;

	event OnTournamentUpdate OnTournamentUpdate;

	event OnTournamentTimeEnded OnTournamentTimeEnded;

	event OnTournamentResult OnTournamentResult;

	event OnLicensesGained OnLicensesGained;

	event Action OnTournamentLogoRequested;

	event OnFishSold OnFishSold;

	void GetMetadataForCompetitionSearch();

	event OnGetMetadataForCompetitionSearch OnGetMetadataForCompetitionSearch;

	event OnFailureUserCompetition OnFailureGetMetadataForCompetitionSearch;

	void GetMetadataForCompetition();

	event OnGetMetadataForCompetition OnGetMetadataForCompetition;

	event OnFailureUserCompetition OnFailureGetMetadataForCompetition;

	void GetMetadataForCompetitionPond(int pondId);

	event OnGetMetadataForCompetitionPond OnGetMetadataForCompetitionPond;

	event OnFailureUserCompetition OnFailureGetMetadataForCompetitionPond;

	void GetMetadataForCompetitionPondWeather(int pondId, int? templateId = null);

	event OnGetMetadataForCompetitionPondWeather OnGetMetadataForCompetitionPondWeather;

	event OnFailureUserCompetition OnFailureGetMetadataForCompetitionPondWeather;

	void GetMetadataForPredefinedCompetitionTemplate(int templateId);

	event OnGetMetadataForPredefinedCompetitionTemplate OnGetMetadataForPredefinedCompetitionTemplate;

	event OnFailureUserCompetition OnFailureGetMetadataForPredefinedCompetitionTemplate;

	void TryCreateCompetition(bool removeOtherRegistrations = false);

	event OnTryCreateCompetition OnTryCreateCompetition;

	event OnFailureUserCompetition OnFailureTryCreateCompetition;

	void SaveCompetition(UserCompetition competition, bool removeOtherRegistrations = false);

	event OnSaveCompetition OnSaveCompetition;

	event OnFailureUserCompetition OnFailureSaveCompetition;

	void UpdateCompetitionLastActivityTime(int tournamentId, bool isOnline);

	event OnUpdateCompetitionLastActivityTime OnUpdateCompetitionLastActivityTime;

	event OnFailureUserCompetition OnFailureUpdateCompetitionLastActivityTime;

	void LoadCompetition(int competitionId);

	event OnLoadCompetition OnLoadCompetition;

	event OnFailureUserCompetition OnFailureLoadCompetition;

	void RemoveCompetition(int competitionId, bool removePublished = false);

	event OnRemoveCompetition OnRemoveCompetition;

	event OnFailureUserCompetition OnFailureRemoveCompetition;

	void SendToReviewCompetition(int competitionId, string comment = null);

	event OnSendToReviewCompetition OnSendToReviewCompetition;

	event OnFailureUserCompetition OnFailureSendToReviewCompetition;

	void PublishCompetition(int competitionId, bool isPublished, bool removeOtherRegistrations = false);

	event OnPublishCompetition OnPublishCompetition;

	event OnFailureUserCompetition OnFailurePublishCompetition;

	void GetUserCompetitions(FilterForUserCompetitions filter);

	event OnGetUserCompetitions OnGetUserCompetitions;

	event OnFailureUserCompetition OnFailureGetUserCompetitions;

	void GetUserCompetition(int competitionId);

	event OnGetUserCompetition OnGetUserCompetition;

	event OnFailureUserCompetition OnFailureGetUserCompetition;

	void GetUserCompetitionReward(int competitionId);

	event OnGetUserCompetitionReward OnGetUserCompetitionReward;

	event OnFailureUserCompetition OnFailureGetUserCompetitionReward;

	void InvitePlayerToCompetition(int competitionId, Guid playerId, string team);

	event OnInvitePlayerToCompetition OnInvitePlayerToCompetition;

	event OnFailureUserCompetition OnFailureInvitePlayerToCompetition;

	void AcceptInvitationToCompetition(int competitionId, string team, bool removeOtherRegistrations = false);

	event OnAcceptInvitationToCompetition OnAcceptInvitationToCompetition;

	event OnFailureUserCompetition OnFailureAcceptInvitationToCompetition;

	void RegisterInCompetition(int competitionId, string team = null, string password = null, bool removeOtherRegistrations = false);

	event OnRegisterInCompetition OnRegisterInCompetition;

	event OnFailureUserCompetition OnFailureRegisterInCompetition;

	void UnregisterFromCompetition(int competitionId);

	event OnUnregisterFromCompetition OnUnregisterFromCompetition;

	event OnFailureUserCompetition OnFailureUnregisterFromCompetition;

	void ApproveParticipationInCompetition(int competitionId, bool isApproved);

	event OnApproveParticipationInCompetition OnApproveParticipationInCompetition;

	event OnFailureUserCompetition OnFailureApproveParticipationInCompetition;

	void MovePlayerToCompetitionTeam(int competitionId, Guid playerId, string team);

	event OnMovePlayerToCompetitionTeam OnMovePlayerToCompetitionTeam;

	event OnFailureUserCompetition OnFailureMovePlayerToCompetitionTeam;

	void LockPlayerInCompetitionTeam(int competitionId, Guid playerId, bool isLocked);

	event OnLockPlayerInCompetitionTeam OnLockPlayerInCompetitionTeam;

	event OnFailureUserCompetition OnFailureLockPlayerInCompetitionTeam;

	void RemovePlayerFromCompetition(int competitionId, Guid playerId);

	event OnRemovePlayerFromCompetition OnRemovePlayerFromCompetition;

	event OnFailureUserCompetition OnFailureRemovePlayerFromCompetition;

	void AutoArrangePlayersAndTeams(int competitionId);

	event OnAutoArrangePlayersAndTeams OnAutoArrangePlayersAndTeams;

	event OnFailureUserCompetition OnFailureAutoArrangePlayersAndTeams;

	void MoveToCompetitionTeam(int competitionId, string team);

	event OnMoveToCompetitionTeam OnMoveToCompetitionTeam;

	event OnFailureUserCompetition OnFailureMoveToCompetitionTeam;

	void ProposeCompetitionTeamExchange(int competitionId, Guid playerId);

	event OnProposeCompetitionTeamExchange OnProposeCompetitionTeamExchange;

	event OnFailureUserCompetition OnFailureProposeCompetitionTeamExchange;

	void AcceptCompetitionTeamExchange(int competitionId, string team);

	event OnAcceptCompetitionTeamExchange OnAcceptCompetitionTeamExchange;

	event OnFailureUserCompetition OnFailureAcceptCompetitionTeamExchange;

	void RequestStartCompetition(int competitionId, bool isStartRequested);

	event OnRequestStartCompetition OnRequestStartCompetition;

	event OnFailureUserCompetition OnFailureRequestStartCompetition;

	void StartCompetition(int competitionId);

	event OnStartCompetition OnStartCompetition;

	event OnFailureUserCompetition OnFailureStartCompetition;

	event OnUserCompetitionReviewed OnUserCompetitionApprovedOnReview;

	event OnUserCompetitionReviewed OnUserCompetitionDeclinedOnReview;

	event OnUserCompetitionReviewed OnUserCompetitionRemovedOnReview;

	event OnUserCompetitionCancelled OnUserCompetitionCancelled;

	event OnUserCompetitionReverted OnUserCompetitionReverted;

	event OnUserCompetitionPromotion OnUserCompetitionPromotion;

	event OnUserCompetitionInvitation OnUserCompetitionInvitation;

	event OnUserCompetitionInvitationResponse OnUserCompetitionInvitationResponse;

	event OnUserCompetitionTeamExchange OnUserCompetitionTeamExchange;

	event OnUserCompetitionTeamExchangeResponse OnUserCompetitionTeamExchangeResponse;

	event OnUserCompetitionStarted OnUserCompetitionStarted;

	void CheckForProductSales();

	event OnCheckedForProductSales OnCheckedForProductSales;

	event OnFailure OnCheckForProductSalesFailed;

	void GetProduct(int productId);

	event Action<StoreProduct> OnGotProduct;

	void GetProducts(int? typeId = null);

	void GetProducts(int[] typeIds);

	event OnGotProducts OnGotProducts;

	void GetProductCategories();

	event OnGotProductCategories OnGotProductCategories;

	void GetOffers();

	event OnGotOffers OnGotOffers;

	event OnFailure OnGetOffersFailed;

	void CompleteOffer(int offerId);

	event OnCompleteOffer OnCompleteOffer;

	event OnFailure OnCompleteOfferFailed;

	string BuyMoneyUrl(int productId);

	string BuySubscriptionUrl(int productId);

	void StartSteamTransaction(int productId, string ipAddress);

	event OnSteamTransactionStarted OnSteamTransactionStarted;

	event OnFailure OnStartSteamTransactionFailed;

	void FinalizeSteamTransaction(string orderId);

	event OnFailure OnFinalizeSteamTransactionFailed;

	void GivePsProduct(int productId, string foreignProductId, int count, DateTime expireDate);

	event OnProductDelivered OnProductDelivered;

	event Action<Subscription[]> OnSubscriptionRemoved;

	event OnFailure OnErrorGivingProduct;

	void DeleteAbsentSubscriptions(string[] entitlmentIds);

	event Action OnAbsentSubscriptionsDeleted;

	event OnFailure OnErrorDeletingAbsentSubscriptions;

	void GiveXboxProduct(string foreignProductId, int count);

	event Action<string> OnXboxProductTranComplete;

	event Action OnSubscriptionEnded;

	void GetAllItemBriefs();

	event Action<InventoryItemBrief[]> OnGotItemBriefs;

	event OnFailure OnErrorGettingItemBriefs;

	void ManageRetailOwnership(Dictionary<string, int> ownedProducts);

	void SetAddProps(int languageId, string country = null, bool requestProfile = false);

	event Action OnSetAddPropsComplete;

	event OnFailure OnSetAddPropsFailed;

	void GetWhatsNewList();

	event OnGotWhatsNewList OnGotWhatsNewList;

	event OnFailure OnGettingWhatsNewListFailed;

	void WhatsNewShown(List<WhatsNewItem> items);

	event Action OnWhatsNewShown;

	event OnFailure OnWhatsNewShownFailed;

	void WhatsNewClicked(WhatsNewItem item);

	event Action OnWhatsNewClicked;

	event OnFailure OnWhatsNewClickedFailed;

	void SetTutorialStep(string stepId);

	void GetCurrentEvent();

	event OnGotCurrentEvent OnGotCurrentEvent;

	event OnFailure OnGettingCurrentEventFailed;

	void CaptureActionInStats(string action, string id = null, string categoryId = null, string subCategoryId = null);

	event OnFarmRebootTriggered OnFarmRebootTriggered;

	event Action OnFarmRebootCanceled;

	bool CreateSupportTicket(string title, string description, Stream attachment);

	event Action OnSupportTicketCreated;

	event OnFailure OnCreateSupportTicketFailed;

	void GetAbTestSelection(int[] testIds);

	event Action<Dictionary<int, bool>> OnGotAbTestSelection;

	event OnFailure OnGettingAbTestSelectionFailed;

	void SpawnFish(int slot, FishName fishName, FishForm fishForm, float? fishWeight = null, float? biteTime = null, bool? hookResult = null);

	event Action<XboxGoldAccountNeedReason> OnXboxGoldAccountNeeded;

	event ServerCachesRefreshedHandler ServerCachesRefreshed;

	void GetAllPondInfos(int countryId);

	event OnGotAllPondInfos OnGotAllPondInfos;

	event OnFailure OnGettingAllPondInfosFailed;

	void GetAllPondLevels(int countryId);

	event OnGotAllPondLevels OnGotAllPondLevels;

	event OnFailure OnGettingAllPondLevelsFailed;

	IDictionary<int, Pond> PondInfos { get; }

	event Action OnPondLevelInvalidated;

	void GetInventoryItemAssets();

	event OnGotInventoryItemAssets OnGotInventoryItemAssets;

	event OnFailure OnGettingInventoryItemAssetsFailed;

	void GetFishAssets();

	event OnGotFishAssets OnGotFishAssets;

	event OnFailure OnGettingFishAssetsFailed;

	void GetFishBrief();

	event OnGotFishBrief OnGotFishBrief;

	event OnFailure OnGettingFishBriefFailed;

	void GetGlobalVariables();

	event OnGotGlobalVariables OnGotGlobalVariables;

	event OnFailure OnGettingGlobalVariablesFailed;

	void GetBoats();

	event OnGotBoats OnGotBoats;

	event OnFailure OnGettingBoatsFailed;

	float MaxBonusExp { get; }

	float MaxPenaltyExp { get; }

	int ChangeNameCost { get; }

	int PondDayStart { get; }

	bool IsCert { get; }

	bool HasSilverMultiplayerPriv { get; }

	bool IsItalianOn { get; }

	bool IsPredatorFishOn { get; }

	bool IsTest { get; }

	bool IsUgcOn { get; }

	bool IsFreeRoamingOn { get; }

	bool IsSavePlayerStateOn { get; }

	bool UgcSilverAccCanJoin { get; }

	bool IsFPSShowOn { get; }

	event OnAdsTriggered OnAdsTriggered;

	event OnLoyatyBonusTriggered OnLoyatyBonusTriggered;

	int PreviewCurrencyExchange(int amount);

	void ExchangeCurrency(int amount);

	event OnCurrencyExchanged OnCurrencyExchanged;

	event OnFailure OnCurrencyExchangeFailed;

	void GetPondUnlocks();

	event OnGotPondUnlocks OnGotPondUnlocks;

	event OnFailure OnGettingPondUnlocksFailed;

	void UnlockPond(int pondId);

	event OnPondUnlocked OnPondUnlocked;

	event OnFailure OnPondUnlockFailed;

	void GetReferralReward();

	event OnGotReferralReward OnGotReferralReward;

	event OnFailure OnGetingReferralRewardFailed;

	void RaiseUpdateCharacter(Package package);

	event OnCharacterEvent OnCharacterCreated;

	event OnCharacterEvent OnCharacterDestroyed;

	event OnCharacterUpdateEvent OnCharacterUpdated;

	void SendGameAction(GameActionCode actionCode, Hashtable actionData = null);

	void DispatchChatMessages();

	void DispatchTravelActions();

	void DispatchTimeActions();
}
