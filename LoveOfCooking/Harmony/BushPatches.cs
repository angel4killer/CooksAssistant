﻿using Harmony;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace LoveOfCooking.Core.HarmonyPatches
{
	public static class BushPatches
	{
		public static void Patch(HarmonyInstance harmony)
		{
			System.Type type = typeof(Bush);
			var prefixes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>(nameof(InBloom_Prefix), nameof(Bush.inBloom)),
				new KeyValuePair<string, string>(nameof(IsDestroyable_Prefix), nameof(Bush.isDestroyable)),
				new KeyValuePair<string, string>(nameof(EffectiveSize_Prefix), "getEffectiveSize"),
				new KeyValuePair<string, string>(nameof(Shake_Prefix), "shake"),
			};

			foreach (KeyValuePair<string, string> pair in prefixes)
			{
				string prefix = pair.Key;
				string original = pair.Value;
				Log.D($"Applying prefix: {type.Name}.{original}",
					ModEntry.Config.DebugMode);
				harmony.Patch(
					original: AccessTools.Method(type, original),
					prefix: new HarmonyMethod(typeof(BushPatches), prefix));
			}
		}
		
		public static bool InBloom_Prefix(Bush __instance, ref bool __result, string season, int dayOfMonth)
		{
			if (!(__instance is CustomBush bush))
				return true;
			__result = CustomBush.InBloomBehaviour(bush, season, dayOfMonth);
			return false;
		}

		public static bool IsDestroyable_Prefix(Bush __instance, ref bool __result)
		{
			if (!(__instance is CustomBush bush))
				return true;
			__result = CustomBush.IsDestroyableBehaviour(bush);
			return false;
		}

		public static bool EffectiveSize_Prefix(Bush __instance, ref int __result)
		{
			if (!(__instance is CustomBush bush))
				return true;
			__result = CustomBush.GetEffectiveSizeBehaviour(bush);
			return false;
		}

		public static bool Shake_Prefix(Bush __instance, Vector2 tileLocation)
		{
			if (!(__instance is CustomBush bush))
				return true;
			CustomBush.ShakeBehaviour(bush, tileLocation);
			return true;
		}
	}
}
