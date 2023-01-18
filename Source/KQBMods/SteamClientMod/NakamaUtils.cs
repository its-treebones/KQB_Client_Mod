using GameSparks.Core;
using Nakama;
using Newtonsoft.Json;
using UnityEngine;

namespace SteamClientMod
{
    class NakamaUtils
    {
        public static string serverkey = "abetterserverkey";
        public GameLogic.Profile ConvertUserToProfile(IApiUser user, ISession session)
        {
            GSRequestData data = new GSRequestData();
            data.AddString("liquidId", user.Username);
            data.AddString("avatarUrl", user.AvatarUrl);
            data.AddString("displayName", user.DisplayName);
            data.AddString("playerId", user.Id);
            GSRequestData resData = new GSRequestData();
            resData.Add("profile", data);
            return JsonConvert.DeserializeObject<GameLogic.Profile>(resData.GetGSData("profile").JSON);
        }

        public static Client GetClient()
        {
            return new Client("https", "kqb-nakama.fly.dev", 7350, serverkey);
        }

        public static ISession RestoreSession()
        { 
            return Session.Restore(PlayerPrefs.GetString("nakama.authToken", null), PlayerPrefs.GetString("nakama.refreshToken", null));
        }

        public GameLogic.Profile AddPreferencesToProfile(GameLogic.Profile profile, IApiStorageObjects objects) {
            //TODO get party prefs for each user
            return profile;
        }
    }
}
