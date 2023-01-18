using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LiquidBit.KillerQueenX;
using Nakama;
using UnityEngine;

namespace SteamClientMod
{
    //replacing GS blocked users with nakama
    [HarmonyPatch(typeof(UserBlocker))]
    [HarmonyPatch("RefreshBlockedProfiles")]
    public class RefreshBlockedProfiles_Patch
    {
        public static bool Prefix(UserBlocker __instance)
        {
            Client nakamaClient = NakamaUtils.GetClient();
            ISession session = NakamaUtils.RestoreSession();
            IApiFriendList friendslistSync = nakamaClient.ListFriendsAsync(session, 3, 100, null).GetAwaiter().GetResult();
            List<GameLogic.Profile> AllBlockedUsers = new List<GameLogic.Profile>();
            foreach (IApiFriend friend in friendslistSync.Friends)
            {
                AllBlockedUsers.Add(new NakamaUtils().ConvertUserToProfile(friend.User, session));
            }
            __instance.GetType().GetField("BlockedUsers", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, AllBlockedUsers);
            return false;
        }
    }

    [HarmonyPatch(typeof(UserBlocker))]
    [HarmonyPatch("Init")]
    public class UserBlockederInit_Patch
    {
        private UserBlocker instance;

        public UserBlockederInit_Patch(UserBlocker instance)
        {
            this.instance = instance;
        }

        public static bool Prefix(UserBlocker __instance)
        {
            IPlatformClient platformClient = GameManager.GMInstance.platformClient;
            if (platformClient != null)
                platformClient.OnUserIdUpdated += new OnUserIdUpdatedHandler(new UserBlockederInit_Patch(__instance).HandleUserIDValidated);
            return false;
        }

        public async void HandleUserIDValidated(object userID)
        {
            this.instance.GetType().GetField("PlatformUserId", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this.instance, userID);
            try
            {
                Client nakamaClient = NakamaUtils.GetClient();
                ISession session = NakamaUtils.RestoreSession();
                IApiFriendList friendslistSync = await nakamaClient.ListFriendsAsync(session, 3, 100, null);
                List<GameLogic.Profile> AllBlockedUsers = new List<GameLogic.Profile>();
                foreach (IApiFriend friend in friendslistSync.Friends)
                {
                    AllBlockedUsers.Add(new NakamaUtils().ConvertUserToProfile(friend.User, session));
                }
                this.instance.GetType().GetField("BlockedUsers", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this.instance, AllBlockedUsers);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}
