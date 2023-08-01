using HarmonyLib;
using Steamworks;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using BepInEx.Logging;
using LiquidBit.KillerQueenX;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json;

namespace SteamlessClientMod
{
    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch("InitPlayerPrefs")]
    public static class PlayerPrefs_Patch
    {
        public static void Prefix()
        {
            LiquidBit.KillerQueenX.TestClient client = new LiquidBit.KillerQueenX.TestClient();
            string path;
            if (File.Exists("TestData.json"))
            {
                path = "TestData.json";
            }
            else
            {
                path = "TestData.json.sample";
            }
            TestData testData = JsonConvert.DeserializeObject<TestData>(File.ReadAllText(path));
            GameManager.GMInstance.cvars.testAccountName = testData.accounts[0].username;
            client.Init();
            GameManager.GMInstance.platformClient = client;
        }
    }

    [HarmonyPatch(typeof(TestClient))]
    [HarmonyPatch("DoLoginFlow")]
    public static class Login_Patch
    {
        public static void Postfix(TestClient __instance)
        {
            string path;
            if (File.Exists("TestData.json"))
            {
                path = "TestData.json";
            }
            else
            {
                path = "TestData.json.sample";
            }
            TestData.TestAccount testAccount = JsonConvert.DeserializeObject<TestData>(File.ReadAllText(path)).accounts[0];

            GameLogic.Profile profile = new GameLogic.Profile();
            profile.displayName = testAccount.displayName;
            profile.avatarUrl = testAccount.avatarUrl;
            profile.playerId = "00000-" + testAccount.displayName;
            __instance.AssignProfile(profile);
            __instance.SetAccount(testAccount);
        }
    }
}
