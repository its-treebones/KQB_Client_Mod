using HarmonyLib;
using Nakama;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SteamClientMod
{
	//TODO this isn't working, maybe move up a level instead of patching iterator is the fastest fix.
	[HarmonyPatch(typeof(NewsTicker), "FetchNews")]
	public class Patches
	{
		class SimpleEnumerator : IEnumerable
		{
			public IEnumerator enumerator;
			public Action prefixAction, postfixAction;
			public Action<object> preItemAction, postItemAction;
			public Func<object, object> itemAction;
			IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
			public IEnumerator GetEnumerator()
			{
				prefixAction();
				while (enumerator.MoveNext())
				{
					var item = enumerator.Current;
					preItemAction(item);
					yield return itemAction(item);
					postItemAction(item);
				}
				postfixAction();
			}
		}

		static void Postfix(ref IEnumerator __result)
		{
			Action prefixAction = () => { 
				Console.WriteLine("--> beginning");
			};
			Action postfixAction = () => { Console.WriteLine("--> ending"); };
			Action<object> preItemAction = (item) => { Console.WriteLine($"--> before {item}"); };
			Action<object> postItemAction = (item) => { Console.WriteLine($"--> after {item}"); };
			Func<object, object> itemAction = (item) =>
			{
				Debug.Log("+++++++++++Starting Fetch News");
				bool fetchedNews = (bool)typeof(NewsTicker).GetField("fetchedNews", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
				if (!fetchedNews)
				{
					Debug.Log("+++++++++++News not fetched yet");

					try
					{
						return new NakamaUtils().GetFromStorage("newsticker", "NEWS").ContinueWith(t =>
						{
							Debug.Log("+++++++++++Got news from nakama");
							IApiStorageObjects newsResults = t.GetAwaiter().GetResult();
							Debug.Log("But we never return");
							Debug.Log(Nakama.TinyJson.JsonParser.FromJson<Dictionary<string, List<string>>>(newsResults.Objects.GetEnumerator().Current.Value)["news"][1]);
							foreach (IApiStorageObject so in newsResults.Objects)
							{
								Dictionary<string, List<string>> items = Nakama.TinyJson.JsonParser.FromJson<Dictionary<string, List<string>>>(so.Value);
								typeof(NewsTicker).GetType().GetField("newsFromServer", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, items["news"]);
								typeof(NewsTicker).GetType().GetField("fetchedNews", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, true);
							}
						});
					}
					catch (Exception e)
					{
						Debug.Log("=========EXCEPTION GETTING REAL NEWS=========");
						Debug.Log(e);
					}
				}
				return null;
			};
			var myEnumerator = new SimpleEnumerator()
			{
				enumerator = __result,
				prefixAction = prefixAction,
				postfixAction = postfixAction,
				preItemAction = preItemAction,
				postItemAction = postItemAction,
				itemAction = itemAction
			};
			__result = myEnumerator.GetEnumerator();
		}
	}

}

