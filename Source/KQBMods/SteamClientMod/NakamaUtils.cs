using GameSparks.Core;
using Nakama;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SteamClientMod
{
    class NakamaUtils
    {
        private static string serverkey = "";
        private static string nakamaUrl = "";
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
            return new Client("https", nakamaUrl, 7350, serverkey);
        }

        public static ISession RestoreSession()
        { 
            return Session.Restore(PlayerPrefs.GetString("nakama.authToken", null), PlayerPrefs.GetString("nakama.refreshToken", null));
        }

        public async Task<IApiStorageObjects> GetFromStorage(string collection, string key)
        {
            ISession session = RestoreSession();
            return await GetClient().ReadStorageObjectsAsync(session, 
                new[] {
                  new StorageObjectId {
                   Collection = collection,
                   Key = key,
                   UserId = session.UserId
                  }
                });
        }

        public GameLogic.Profile AddPreferencesToProfile(GameLogic.Profile profile, IApiStorageObjects objects) {
            //TODO get party prefs for each user
            foreach(IApiStorageObject so in objects.Objects)
            {
                Dictionary<string, string> prefs = Nakama.TinyJson.JsonParser.FromJson<Dictionary<string,string>>(so.Value);
                Debug.Log("I think it worked?");
                profile.allowFriendsToJoinParty = Convert.ToBoolean(prefs["allowFriendsToJoinParty"]);
                profile.status = (GameLogic.Profile.Status)Enum.Parse(typeof(GameLogic.Profile.Status), prefs["status"]);
                profile.allowSpectateCustomMatch = Convert.ToBoolean(prefs["allowSpectateCustomMatch"]);
                profile.currentNetworkingPreferences = int.Parse(prefs["currentNetworkingPreferences"]);
                profile.allowFriendsToJoinCustomMatch = Convert.ToBoolean(prefs["allowFriendsToJoinCustomMatch"]);
                profile.allowFriendsOfFriendsToJoinParty = Convert.ToBoolean(prefs["allowFriendsOfFriendsToJoinParty"]);
            }
            return profile;
        }
    }
}
