using HarmonyLib;
using LiquidBit.KillerQueenX;
using System.Reflection;
using UnityEngine;

namespace SteamClientMod
{
    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch("InitPlayerPrefs")]
    public class PlayerPrefs_Patch
    {
        //This Patch is necessary to replace the MockClient the direct-connect version of the game uses with a SteamClient
        public static void Prefix()
        {
            MockClient mockClient = (MockClient)GameManager.GMInstance.platformClient;
            GameObject steamManagerPrefab = (GameObject)mockClient.GetType().GetField("steamManagerPrefab", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(mockClient);
            LiquidBit.KillerQueenX.SteamClient client = new LiquidBit.KillerQueenX.SteamClient(steamManagerPrefab);
            client.Init();
            GameManager.GMInstance.platformClient = client;
        }
    }
}

