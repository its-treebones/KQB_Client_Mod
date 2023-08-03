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
using GameSparks.Core;
using System;
using GameLogic;

namespace SteamlessClientMod
{
    [HarmonyPatch(typeof(MockClient))]
    [HarmonyPatch("ValidateOwnership")]
    public static class Valid_Patch
    {
        public static bool Prefix(MockClient __instance)
        {
            __instance.GetType().GetField("steamInitialized", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, true);
            return true;
        }

    }
}
