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

namespace SteamClientMod
{
    [HarmonyPatch(typeof(LiquidBit.KillerQueenX.SteamClient))]
    [HarmonyPatch("SteamAuth")]
    public static class SteamClient_Patch
    {

        static bool Prefix(LiquidBit.KillerQueenX.SteamClient __instance, Action onSuccess, Action onFailure) 
        {
            byte[] array = new byte[1024];
            uint num;
            SteamUser.GetAuthSessionTicket(array, 1024, out num);
            string steamAuthTicket = "";
            int num2 = 0;
            while ((long)num2 < (long)((ulong)num))
            {
                steamAuthTicket += string.Format("{0:X2}", array[num2]);
                num2++;
            }
            __instance.GetType().GetField("steamAuthTicket", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, steamAuthTicket);
            __instance.GetType().GetMethod("UpdateAuthStatus", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[] { AuthStatus.WaitingForAuth });

            GameManager.GMInstance.StartCoroutine(SteamClient_Patch.GetAvatarAndLogin(__instance, onSuccess, onFailure));
            return false;
        }

        public static IEnumerator GetAvatarAndLogin(LiquidBit.KillerQueenX.SteamClient __instance, Action successCallback, Action errorCallack)
        {
            string steamId = SteamUser.GetSteamID().ToString();
            UnityWebRequestAsyncOperation async = new UnityWebRequest("https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=1D04E36F7E09998864E2966531960BD1&steamids=" + steamId)
            {
                method = "GET",
                downloadHandler = ((DownloadHandler)new DownloadHandlerBuffer())
            }.SendWebRequest();
            yield return (object)async;
            UnityWebRequest webRequest = async.webRequest;
            GSRequestData data = new GSRequestData();
            data.AddString("playerId", steamId);
            if (!webRequest.isNetworkError && !webRequest.isHttpError)
            {
                SteamClient_Patch.GetPlayerSummaries getPlayerSummaries = JsonConvert.DeserializeObject<SteamClient_Patch.GetPlayerSummaries>(webRequest.downloadHandler.text);
                if (getPlayerSummaries.response.players.Count > 0)
                {
                    SteamPlayerSummary summary = getPlayerSummaries.response.players[0];
                    data.AddString("avatarUrl", summary.avatarfull);
                    data.AddString("liquidId", Guid.NewGuid().ToString());
                    data.AddString("displayName", summary.personaname);
                    GSRequestData resData = new GSRequestData();
                    resData.Add("profile", data);
                    GameLogic.Profile profile = JsonConvert.DeserializeObject<GameLogic.Profile>(resData.GetGSData("profile").JSON);
                    GameManager.GMInstance.platformClient.AssignProfile(profile);
                    INetworkManager networkManager = GameManager.GMInstance.networkManager;
                    if (networkManager != null)
                    {
                        networkManager.SetNickname(summary.personaname);
                    }
                    LiquidBit.KillerQueenX.SteamClient client = (LiquidBit.KillerQueenX.SteamClient)GameManager.GMInstance.platformClient;
                    client.CallOnUserIdUpdated(steamId);
                    __instance.GetType().GetMethod("UpdateAuthStatus", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[] { AuthStatus.NotAuthed });

                    successCallback();
                }
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
}