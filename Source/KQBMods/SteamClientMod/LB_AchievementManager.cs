using System;
using HarmonyLib;
namespace SteamClientMod
{
    // We don't get acheivements from GS anymore, just skip it
    //TODO nakama achievements? could be a thing.
    [HarmonyPatch(typeof(LB_AchievementManager))]
    [HarmonyPatch("PullAchievementsGameSparks")]
    public class PullAchievementsGameSparks_Patch
    {

        public static bool Prefix(Action onSuccess = null, Action onFailure = null)
        {

            if (onSuccess != null)
                onSuccess();
            return false;
        }
    }
}
