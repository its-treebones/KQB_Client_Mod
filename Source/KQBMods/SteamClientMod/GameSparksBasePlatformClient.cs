using HarmonyLib;
using LiquidBit.KillerQueenX;
using Nakama;
using System;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using LiquidBit.KillerQueenX.Utility;
using System.Threading.Tasks;
using GameLogic;

namespace SteamClientMod
{
    [HarmonyPatch(typeof(GameSparksBasePlatformClient), "SetStatus", new Type[] { typeof(GameLogic.Profile.Status), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(Action<bool>) })]
    public class SetStatus_Patch
    {
        //This Patch is necessary to replace the MockClient the direct-connect version of the game uses with a SteamClient
        public static bool Prefix(GameSparksBasePlatformClient __instance,
                                  GameLogic.Profile.Status status,
                                  bool allowFriendsToJoinParty,
                                  bool allowFriendsOfFriendsToJoinParty,
                                  bool allowFriendsToJoinCustomMatch,
                                  bool allowSpectateCustomMatch,
                                  Action<bool> callback = null)
        {
            GameLogic.Profile account = (GameLogic.Profile)__instance.GetType().GetField("account", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
            if (account == null)
            {
                return false;
            }
            if (account.status != status || account.allowFriendsToJoinParty != allowFriendsToJoinParty || account.allowFriendsOfFriendsToJoinParty != allowFriendsOfFriendsToJoinParty || account.allowFriendsToJoinCustomMatch != allowFriendsToJoinCustomMatch || account.allowSpectateCustomMatch != allowSpectateCustomMatch || (OnlinePrivilegeStatus)account.currentNetworkingPreferences != __instance.GetPlayerNetworkingPreferences())
            {
                Client client = NakamaUtils.GetClient();
                ISession session = NakamaUtils.RestoreSession();
                try
                {   //TODO build objects and serialize them instead of these messy strings
                    var partyItems = "{" +
                                       "\"allowFriendsToJoinParty\": \"" + allowFriendsToJoinParty + "\"," +
                                       "\"allowFriendsToJoinCustomMatch\": \"" + allowFriendsToJoinCustomMatch + "\"," +
                                       "\"allowSpectateCustomMatch\": \"" + allowSpectateCustomMatch + "\"," +
                                       "\"allowFriendsOfFriendsToJoinParty\": \"" + allowFriendsOfFriendsToJoinParty + "\"," +
                                       "\"currentNetworkingPreferences\": " + (int)__instance.GetPlayerNetworkingPreferences() + "," +
                                       " \"status\": " + (int)status +
                                     "}";

                    var writeObjects2 = new[] {
                        new WriteStorageObject
                        {
                            Collection = "prefs",
                            Key = "party_prefs",
                            Value = partyItems
                        }
                    };
                    Task<IApiStorageObjectAcks> task = client.WriteStorageObjectsAsync(session, writeObjects2);
                    task.ContinueWith(t =>
                    {
                        Debug.Log((object)("---- Updated Profile Status To " + (object)status + "  ----"));

                        account.status = status;
                        account.allowFriendsToJoinParty = allowFriendsToJoinParty;
                        account.allowFriendsOfFriendsToJoinParty = allowFriendsOfFriendsToJoinParty;
                        account.allowFriendsToJoinCustomMatch = allowFriendsToJoinCustomMatch;
                        account.allowSpectateCustomMatch = allowSpectateCustomMatch;
                        account.currentNetworkingPreferences = (int)__instance.GetPlayerNetworkingPreferences();
                        __instance.GetType().GetField("account", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, account);
                        FieldInfo discInfo = __instance.GetType().GetField("discordUtility", BindingFlags.Instance | BindingFlags.NonPublic);
                        if (discInfo != null)
                        {
                            DiscordUtility d = (DiscordUtility)discInfo.GetValue(__instance);
                            d.UpdateActivity(status, (IPlatformClient)__instance);
                        }
                        if (callback != null)
                            callback(true);
                        __instance.UpdatePlatformStatus(status);
                    });

                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    if (callback != null)
                        callback(false);
                }
            }
            if (callback != null)
                callback(true);
            return false;
        }
    }

    [HarmonyPatch(typeof(GameSparksBasePlatformClient), "SetPartyStatus")]
    public class SetPartyStatus_Patch
    {
        public static bool Prefix(GameSparksBasePlatformClient __instance,
                                  bool inRemoteParty,
                                  bool partyLeader,
                                  int partyMemberCount,
                                  int localPlayerCount)
        {
            GameLogic.Profile account = (GameLogic.Profile)__instance.GetType().GetField("account", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
            if (account == null || __instance.authStatus != AuthStatus.Authed)
            {
                return false;
            }
            if (inRemoteParty != account.inParty || partyLeader != account.partyLeader || partyMemberCount != account.partyCount || localPlayerCount != account.localPlayerCount)
            {
                if (inRemoteParty && partyMemberCount >= 2)
                {
                    GameManager.GMInstance.achievementManager.PushAchievement(AchievementName.JoinPartyOrInviteFriend, 1);
                }
                __instance.PartyStatusUpdated();
                var partyItems = "{" +
                   "\"inRemoteParty\": \"" + inRemoteParty + "\"," +
                   "\"partyLeader\": \"" + partyLeader + "\"," +
                   "\"count\": \"" + partyMemberCount + "\"," +
                   "\"localPlayerCount\": \"" + localPlayerCount + "\"," +
                 "}";

                var writeObjects2 = new[] {
                        new WriteStorageObject
                        {
                            Collection = "PartyMember",
                            Key = "setPartyMemberCount",
                            Value = partyItems
                        }
                    };
                Client client = NakamaUtils.GetClient();
                ISession session = NakamaUtils.RestoreSession();
                try
                {
                    Task<IApiStorageObjectAcks> task = client.WriteStorageObjectsAsync(session, writeObjects2);
                    task.ContinueWith(t =>
                    {
                        account.inParty = inRemoteParty;
                        account.partyLeader = partyLeader;
                        account.partyCount = partyMemberCount;
                        account.localPlayerCount = localPlayerCount;

                        __instance.GetType().GetField("account", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, account);
                        FieldInfo discField = __instance.GetType().GetField("discordUtility", BindingFlags.Instance | BindingFlags.NonPublic);
                        if (discField != null)
                        {
                            DiscordUtility discordUtility = (DiscordUtility)discField.GetValue(__instance);
                            discordUtility.UpdateActivity(account.status, __instance);
                        }
                        Debug.Log("---- Updated Party member count to " + partyMemberCount + "  ----");
                    });
                }
                catch (Exception e)
                {
                    Debug.Log("==========Party Status Update Failed==========");
                    Debug.Log(e.Message);
                }
            }
            return false;
        }
    }
}
