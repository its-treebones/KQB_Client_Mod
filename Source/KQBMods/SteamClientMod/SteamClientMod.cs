using HarmonyLib;
using Steamworks;
using System;
using System.Collections;
using LiquidBit.KillerQueenX;
using System.Reflection;
using UnityEngine;
using Nakama;
using LiquidBit.KillerQueenX.Utility;

namespace SteamClientMod
{

    //Switch the SteamClient from using GS auth to Nakama auth
    [HarmonyPatch(typeof(LiquidBit.KillerQueenX.SteamClient))]
    [HarmonyPatch("SteamAuth")]
    public class SteamClient_Patch
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
            GameManager.GMInstance.StartCoroutine(GetAvatarAndLogin(__instance, steamAuthTicket, onSuccess, onFailure));
            return false;
        }

        public static IEnumerator GetAvatarAndLogin(LiquidBit.KillerQueenX.SteamClient __instance, String steamAuthTicket, Action onSuccess, Action onFailure)
        {
            var deviceId = PlayerPrefs.GetString("deviceId", SystemInfo.deviceUniqueIdentifier);
            string steamId = SteamUser.GetSteamID().ToString();
            PlayerPrefs.SetString("deviceId", deviceId);
            PlayerPrefs.SetString("steamId", steamId);

            AuthenticateWithSteam(__instance, steamAuthTicket, onSuccess, onFailure);
            yield break;
        }

        // nakama stuff
        public static async void AuthenticateWithSteam(LiquidBit.KillerQueenX.SteamClient __instance, string steamAuthTicket, Action onSuccess, Action onFailure)
        {

            // Authenticate with the Nakama server using Device Authentication.
            try
            {
                Client client = NakamaUtils.GetClient();
                Session session = (Session)await client.AuthenticateSteamAsync(steamAuthTicket);

                PlayerPrefs.SetString("nakama.refreshToken", session.RefreshToken);
                PlayerPrefs.SetString("nakama.authToken", session.AuthToken);

                Debug.Log("Authenticated with Device ID");

                IApiAccount account = await client.GetAccountAsync(session);
                Debug.Log("Got user account: " + account.User.Username);
                NakamaUtils nku = new NakamaUtils();
                GameLogic.Profile profile = nku.ConvertUserToProfile(account.User, session);

                IApiStorageObjects objects = await client.ReadStorageObjectsAsync(session);

                nku.AddPreferencesToProfile(profile, objects);

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

    }
}