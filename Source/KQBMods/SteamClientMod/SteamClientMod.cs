using HarmonyLib;
using Steamworks;
using System;
using System.Collections;
using UnityEngine.Networking;
using BepInEx.Logging;
using GameSparks.Core;
using Newtonsoft.Json;
using LiquidBit.KillerQueenX;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Nakama;
using LiquidBit.KillerQueenX.Utility;
using System.Threading.Tasks;
using GameSparks.Platforms;

namespace SteamClientMod
{
    [HarmonyPatch(typeof(LiquidBit.KillerQueenX.SteamClient))]
    [HarmonyPatch("SteamAuth")]
    public static class SteamClient_Patch
    {

        static bool Prefix(LiquidBit.KillerQueenX.SteamClient __instance, Action onSuccess, Action onFailure) 
        {
            __instance.GetType().GetMethod("UpdateAuthStatus", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { AuthStatus.WaitingForAuth });
            __instance.GetType().GetField("appId", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, SteamUtils.GetAppID().ToString());
            DiscordUtility discordUtility = (DiscordUtility)__instance.GetType().GetField("discordUtility", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            discordUtility.RegisterSteam(SteamUtils.GetAppID().ToString());
            string steamAuthTicket = (string)__instance.GetType().GetField("steamAuthTicket", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            if (steamAuthTicket == null)
            {
                byte[] array = new byte[1024];
                uint num;
                SteamUser.GetAuthSessionTicket(array, 1024, out num);
                steamAuthTicket = "";
                int num2 = 0;
                while ((long)num2 < (long)((ulong)num))
                {
                    steamAuthTicket += string.Format("{0:X2}", array[num2]);
                    num2++;
                }
                __instance.GetType().GetField("steamAuthTicket", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, steamAuthTicket);
            }
            Debug.Log(string.Concat(new object[]
            {
                "---- steamAuthTicket: ",
                steamAuthTicket,
                " length: ",
                steamAuthTicket.Length
            }));
            GameManager.GMInstance.StartCoroutine(SteamClient_Patch.GetAvatarAndLogin(__instance, onSuccess, onFailure));
            return false;
        }

        public static IEnumerator GetAvatarAndLogin(LiquidBit.KillerQueenX.SteamClient __instance, Action onSuccess, Action onFailure)
        {
            Client nakamaClient = new Nakama.Client("https", "kqb-nakama.fly.dev", 7350, "XXXXXXXXXXXXXXXX");
            var deviceId = PlayerPrefs.GetString("deviceId", SystemInfo.deviceUniqueIdentifier);
            string steamId = SteamUser.GetSteamID().ToString();
            PlayerPrefs.SetString("deviceId", deviceId);
            PlayerPrefs.SetString("steamId", steamId);

            AuthenticateWithDevice(__instance, nakamaClient, onSuccess, onFailure);
            yield break;
        }

        // nakama stuff
        public static async void AuthenticateWithDevice(LiquidBit.KillerQueenX.SteamClient __instance, Client client, Action onSuccess, Action onFailure)
        {

            // Authenticate with the Nakama server using Device Authentication.
            try
            {
                Session session = (Session) await client.AuthenticateDeviceAsync(PlayerPrefs.GetString("deviceId"), PlayerPrefs.GetString("steamId"), true);

                PlayerPrefs.SetString("nakama.refreshToken", session.RefreshToken);
                PlayerPrefs.SetString("nakama.authToken", session.AuthToken);

                Debug.Log("Authenticated with Device ID");

                var account = await client.GetAccountAsync(session);
                Debug.Log("Got user account: " + account.User.Username);
                GSRequestData data = new GSRequestData();
                data.AddString("liquidId", account.User.Id);
                data.AddString("avatarUrl", account.User.AvatarUrl);
                data.AddString("displayName", account.User.DisplayName);
                data.AddString("playerId", account.User.Username);

                GSRequestData resData = new GSRequestData();
                resData.Add("profile", data);
                GameLogic.Profile profile = JsonConvert.DeserializeObject<GameLogic.Profile>(resData.GetGSData("profile").JSON);
                __instance.GetType().GetMethod("SuccessfulLogin", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { session.AuthToken, profile });
                __instance.CallOnUserIdUpdated(PlayerPrefs.GetString("steamId"));
                onSuccess();
            }
            catch (ApiResponseException ex)
            {
                string json = ex.Message;
                Debug.LogWarning("Nakama: SteamConnectRequest failed - steamTicket : " + json);
                __instance.GetType().GetField("steamAuthTicket", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, null);
                __instance.GetType().GetMethod("UpdateAuthStatus", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { AuthStatus.NotAuthed });
                Debug.LogFormat("Error authenticating with Device ID: {0}", ex.Message);
                onFailure();
            }
        }

        class GetPlayerSummaries
        {
            public SteamClient_Patch.GetPlayerSummariesResponse response;
        }

        public class GetPlayerSummariesResponse
        {
            public List<SteamClient_Patch.SteamPlayerSummary> players;
        }

        public class SteamPlayerSummary
        {
            public string steamid;
            public int communityvisibilitystate;
            public int profilestate;
            public string personaname;
            public string profileurl;
            public string avatar;
            public string avatarmedium;
            public string avatarfull;
            public string avatarhash;
            public int personastate;
        }

    }

    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch("InitPlayerPrefs")]
    public static class PlayerPrefs_Patch
    {
        public static ManualLogSource logger;

        public static void Prefix()
        {
            MockClient c = (MockClient)GameManager.GMInstance.platformClient;
            FieldInfo smp = c.GetType().GetField("steamManagerPrefab", BindingFlags.Instance | BindingFlags.NonPublic);
            GameObject go = (GameObject)smp.GetValue(c);
            LiquidBit.KillerQueenX.SteamClient client = new LiquidBit.KillerQueenX.SteamClient(go);
            client.Init();
            GameManager.GMInstance.platformClient = client;
        }
    }


    [HarmonyPatch(typeof(LB_AchievementManager))]
    [HarmonyPatch("PullAchievementsGameSparks")]
    public static class PullAchievementsGameSparks_Patch {

        public static bool Prefix(Action onSuccess = null, Action onFailure = null) 
        {

            if (onSuccess != null)
                onSuccess();
            return false;
        }
    }

    [HarmonyPatch(typeof(UserBlocker))]
    [HarmonyPatch("RefreshBlockedProfiles")]
    public static class RefreshBlockedProfiles_Patch 
    {

        public static bool Prefix(UserBlocker __instance) 
        {
            Client nakamaClient = new Nakama.Client("https", "kqb-nakama.fly.dev", 7350, "abetterserverkey");
            ISession session = Session.Restore(PlayerPrefs.GetString("nakama.authToken", null), PlayerPrefs.GetString("nakama.refreshToken", null));
            IApiFriendList friendslistSync = nakamaClient.ListFriendsAsync(session, 3, 100, null).GetAwaiter().GetResult();
            List<GameLogic.Profile> AllBlockedUsers = new List<GameLogic.Profile>();
            foreach (IApiFriend friend in friendslistSync.Friends)
            {
                AllBlockedUsers.Add(ConvertFriendToProfile(friend));
            }
            __instance.GetType().GetField("BlockedUsers", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, AllBlockedUsers);
            return false;
        }

        public static GameLogic.Profile ConvertFriendToProfile(IApiFriend friend) {
            GSRequestData data = new GSRequestData();
            data.AddString("liquidId", friend.User.Id);
            data.AddString("avatarUrl", friend.User.AvatarUrl);
            data.AddString("displayName", friend.User.DisplayName);
            data.AddString("playerId", friend.User.Username);
            GSRequestData resData = new GSRequestData();
            resData.Add("profile", data);
            return JsonConvert.DeserializeObject<GameLogic.Profile>(resData.GetGSData("profile").JSON);
        }
    }

    [HarmonyPatch(typeof(DefaultPlatform))]
    [HarmonyPatch("Update")]
    public static class HandleUserIDValidated_Patch
    {

        public static bool Prefix()
        {
            return false;
        }
    }
}