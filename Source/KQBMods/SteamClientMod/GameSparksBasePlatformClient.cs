using HarmonyLib;
using LiquidBit.KillerQueenX;
using Nakama;
using System;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using LiquidBit.KillerQueenX.Utility;
using System.Threading.Tasks;

namespace SteamClientMod
{
    [HarmonyPatch(typeof(GameSparksBasePlatformClient), "SetStatus", new Type[] { typeof(GameLogic.Profile.Status), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(Action<bool>) })]
    [HarmonyPatch("SetStatus")]
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
            if (account == null) {
                Debug.Log("WE AINT GOT NO SOUL");
                return true;
            }
            if (account.status != status || account.allowFriendsToJoinParty != allowFriendsToJoinParty || account.allowFriendsOfFriendsToJoinParty != allowFriendsOfFriendsToJoinParty || account.allowFriendsToJoinCustomMatch != allowFriendsToJoinCustomMatch || account.allowSpectateCustomMatch != allowSpectateCustomMatch || (OnlinePrivilegeStatus)account.currentNetworkingPreferences != __instance.GetPlayerNetworkingPreferences())
            {
                Client client = NakamaUtils.GetClient();
                ISession session = NakamaUtils.RestoreSession();
                try
                {
                    var partyItems = "{" +
                                       "\"allowFriendsToJoinParty\": \"" + allowFriendsToJoinParty + "\"," +
                                       "\"allowFriendsToJoinCustomMatch\": \"" + allowFriendsToJoinCustomMatch + "\"," +
                                       "\"allowSpectateCustomMatch\": \"" + allowSpectateCustomMatch + "\"," +
                                       "\"allowFriendsOfFriendsToJoinParty\": \"" + allowFriendsOfFriendsToJoinParty + "\"," +
                                       "\"currentNetworkingPreferences\": " + (int)__instance.GetPlayerNetworkingPreferences() + "," +
                                       " \"status\": " + (int)status + 
                                     "}";
                    Debug.Log("party Items: ");
                    Debug.Log(partyItems);
                    var saveGame = "{ \"progress\": " + (int)status +" }";
                    var writeObjects2 = new[] {
                        new WriteStorageObject
                        {
                            Collection = "prefs",
                            Key = "party_prefs",
                            Value = partyItems
                        }
                    };
                    Task<IApiStorageObjectAcks> task = client.WriteStorageObjectsAsync(session, writeObjects2);
                    task.ContinueWith(t =>{
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
                catch (Exception e) {
                    Debug.Log(e.Message);
                    if (callback != null)
                        callback(false);
                }
            }
            if(callback != null)
               callback(true);
            return false;
        }
    }
}
