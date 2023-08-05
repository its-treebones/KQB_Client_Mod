using HarmonyLib;
using LiquidBit.KillerQueenX;
using System.Reflection;

namespace LinuxServer
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

    [HarmonyPatch(typeof(UIManager))]
    [HarmonyPatch("DirectConnectToServer")]
    public static class ShowError_Patch
    {
        public static bool Prefix(UIManager __instance, string ipAddress, ushort port, bool loopback)
        {
            return false;
        }

    }
}
