﻿using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile;
using xTile.Tiles;

namespace LoveOfCooking.Core.HarmonyPatches
{
	public static class CommunityCentrePatches
	{
		public static void Patch(HarmonyInstance harmony)
		{
			// TODO: TEST: Community centre cooking bundle completion and all bundle completion
			// TODO: TEST: Big problem in JunimoNoteMenu.setUpMenu():
			// >> if (!Game1.player.hasOrWillReceiveMail("hasSeenAbandonedJunimoNote") && whichArea == 6)
			// TODO: POLISH: Move as many routines out of harmony as possible

			Type type = (Type) null;

			{
				type = typeof(Farmer);
				string original = "hasCompletedCommunityCenter";
				Log.D($"Applying postfix: {type.Name}.{original}",
					ModEntry.Config.DebugMode);
				harmony.Patch(
					original: AccessTools.Method(type, original),
					postfix: new HarmonyMethod(typeof(CommunityCentrePatches), nameof(HasCompletedCommunityCentre_Postfix)));
			}

			type = typeof(CommunityCenter);
			var prefixes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>(nameof(AreaNameFromNumber_Prefix), "getAreaNameFromNumber"),
				new KeyValuePair<string, string>(nameof(AreaNumberFromName_Prefix), "getAreaNumberFromName"),
				new KeyValuePair<string, string>(nameof(AreaNumberFromLocation_Prefix), "getAreaNumberFromLocation"),
				new KeyValuePair<string, string>(nameof(AreaEnglishDisplayNameFromNumber_Prefix), "getAreaEnglishDisplayNameFromNumber"),
				new KeyValuePair<string, string>(nameof(AreaDisplayNameFromNumber_Prefix), "getAreaDisplayNameFromNumber"),
				new KeyValuePair<string, string>(nameof(LoadArea_Prefix), "loadArea"),
				new KeyValuePair<string, string>(nameof(ShouldNoteAppearInArea_Prefix), "shouldNoteAppearInArea"),
				new KeyValuePair<string, string>(nameof(IsJunimoNoteAtArea_Prefix), "isJunimoNoteAtArea"),
				new KeyValuePair<string, string>(nameof(AddJunimoNote_Prefix), "addJunimoNote"),
				new KeyValuePair<string, string>(nameof(UpdateWhenCurrentLocation_Prefix), "UpdateWhenCurrentLocation"),
				
				// these ones are probably alright

				new KeyValuePair<string, string>(nameof(SetViewportToNextJunimoNoteTarget_Prefix), "setViewportToNextJunimoNoteTarget"),
				new KeyValuePair<string, string>(nameof(JunimoGoodbyeDance_Prefix), "junimoGoodbyeDance"),
				new KeyValuePair<string, string>(nameof(StartGoodbyeDance_Prefix), "startGoodbyeDance"),
				new KeyValuePair<string, string>(nameof(MessageForAreaCompletion_Prefix), "getMessageForAreaCompletion"),
			};

			foreach (KeyValuePair<string, string> pair in prefixes)
			{
				string prefix = pair.Key;
				string original = pair.Value;
				Log.D($"Applying prefix: {type.Name}.{original}",
					ModEntry.Config.DebugMode);
				harmony.Patch(
					original: AccessTools.Method(type, original),
					prefix: new HarmonyMethod(typeof(CommunityCentrePatches), prefix));
			}
		}

		public static void HasCompletedCommunityCentre_Postfix(Farmer __instance, ref bool __result)
		{
			__result &= __instance.mailReceived.Contains("cc" + Bundles.CommunityCentreAreaName);
		}

		/// <summary>
		/// Basic implementation of new CommunityCenter area.
		/// </summary>
		public static bool AreaNameFromNumber_Prefix(ref string __result, int areaNumber)
		{
			try
			{
				if (areaNumber != Bundles.CommunityCentreAreaNumber || Bundles.IsCommunityCentreComplete() || Bundles.IsAbandonedJojaMartBundleAvailable())
					return true;
				__result = Bundles.CommunityCentreAreaName;
				return false;
			}
			catch (Exception e)
			{
				Log.E($"Error in {nameof(AreaNameFromNumber_Prefix)}:\n{e}");
			}

			return true;
		}

		/// <summary>
		/// Basic implementation of new CommunityCenter area.
		/// </summary>
		public static bool AreaNumberFromName_Prefix(ref int __result, string name)
		{
			try
			{
				if (name != Bundles.CommunityCentreAreaName)
					return true;
				__result = Bundles.CommunityCentreAreaNumber;
				return false;
			}
			catch (Exception e)
			{
				Log.E($"Error in {nameof(AreaNumberFromName_Prefix)}:\n{e}");
			}

			return true;
		}

		/// <summary>
		/// Basic implementation of new CommunityCenter area.
		/// </summary>
		public static bool AreaNumberFromLocation_Prefix(ref int __result, Vector2 tileLocation)
		{
			try
			{
				Log.T($"CC_AreaNumberFromLocation_Prefix(tileLocation={tileLocation.ToString()})");
				if (!new Rectangle(0, 0, 11, 11).Contains(Utility.Vector2ToPoint(tileLocation)) || Bundles.IsCommunityCentreComplete() || Bundles.IsAbandonedJojaMartBundleAvailable())
					return true;
				__result = Bundles.CommunityCentreAreaNumber;
				return false;
			}
			catch (Exception e)
			{
				Log.E($"Error in {nameof(AreaNumberFromLocation_Prefix)}:\n{e}");
			}

			return true;
		}

		/// <summary>
		/// Basic implementation of new CommunityCenter area.
		/// </summary>
		public static bool AreaEnglishDisplayNameFromNumber_Prefix(ref string __result, int areaNumber)
		{
			try
			{
				if (areaNumber != Bundles.CommunityCentreAreaNumber || Bundles.IsCommunityCentreComplete() || Bundles.IsAbandonedJojaMartBundleAvailable())
					return true;
				__result = ModEntry.Instance.Helper.Translation.Get("world.community_centre.kitchen");
				return false;
			}
			catch (Exception e)
			{
				Log.E($"Error in {nameof(AreaEnglishDisplayNameFromNumber_Prefix)}:\n{e}");
			}

			return true;
		}

		/// <summary>
		/// Basic implementation of new CommunityCenter area.
		/// </summary>
		public static bool AreaDisplayNameFromNumber_Prefix(ref string __result, int areaNumber)
		{
			try
			{
				if (areaNumber != Bundles.CommunityCentreAreaNumber || Bundles.IsCommunityCentreComplete() || Bundles.IsAbandonedJojaMartBundleAvailable())
					return true;
				__result = ModEntry.Instance.Helper.Translation.Get("world.community_centre.kitchen");
				return false;
			}
			catch (Exception e)
			{
				Log.E($"Error in {nameof(AreaDisplayNameFromNumber_Prefix)}:\n{e}");
			}

			return true;
		}

		/// <summary>
		/// GetAreaBounds() throws FatalEngineExecutionError when patched.
		/// Mimics LoadArea() using a static areaToRefurbish value in place of GetAreaBounds().
		/// </summary>
		public static bool LoadArea_Prefix(CommunityCenter __instance, int area, bool showEffects)
		{
			try
			{
				Log.T($"CC_LoadArea_Prefix(area={area})");
				if (area != Bundles.CommunityCentreAreaNumber || Bundles.IsCommunityCentreComplete() || Bundles.IsAbandonedJojaMartBundleAvailable())
					return true;

				Rectangle areaToRefurbish = area != Bundles.CommunityCentreAreaNumber
					? ModEntry.Instance.Helper.Reflection.GetMethod(__instance, "getAreaBounds").Invoke<Rectangle>(area)
					: Bundles.CommunityCentreArea;
				Map refurbishedMap = Game1.game1.xTileContent.Load<Map>("Maps\\CommunityCenter_Refurbished");

				//PyTK.Extensions.PyMaps.mergeInto(__instance.Map, refurbishedMap, Vector2.Zero, ModEntry.CommunityCentreArea);
				//__instance.addLightGlows();
				//return false;

				StardewModdingAPI.IReflectedMethod adjustMapLightPropertiesForLamp = ModEntry.Instance.Helper.Reflection
					.GetMethod(__instance, "adjustMapLightPropertiesForLamp");

				for (int x = areaToRefurbish.X; x < areaToRefurbish.Right; ++x)
				{
					for (int y = areaToRefurbish.Y; y < areaToRefurbish.Bottom; ++y)
					{
						if (refurbishedMap.GetLayer("Back").Tiles[x, y] != null)
						{
							__instance.map.GetLayer("Back").Tiles[x, y].TileIndex
								= refurbishedMap.GetLayer("Back").Tiles[x, y].TileIndex;
						}
						if (refurbishedMap.GetLayer("Buildings").Tiles[x, y] != null)
						{
							__instance.map.GetLayer("Buildings").Tiles[x, y] = new StaticTile(
								__instance.map.GetLayer("Buildings"), __instance.map.TileSheets[0],
								BlendMode.Alpha, refurbishedMap.GetLayer("Buildings").Tiles[x, y].TileIndex);
							adjustMapLightPropertiesForLamp.Invoke(
								refurbishedMap.GetLayer("Buildings").Tiles[x, y].TileIndex, x, y, "Buildings");
							if (Game1.player.getTileX() == x && Game1.player.getTileY() == y)
							{
								Game1.player.Position = new Vector2(2080f, 576f);
							}
							if (refurbishedMap.GetLayer("Buildings").Tiles[x, y].TileIndex == Bundles.FridgeTileIndexes[Bundles.FridgeTilesToUse][1])
							{
								Bundles.FridgeTilePosition = new Vector2(x, y);
							}
						}
						else
						{
							__instance.map.GetLayer("Buildings").Tiles[x, y] = null;
						}
						if (refurbishedMap.GetLayer("Front").Tiles[x, y] != null)
						{
							__instance.map.GetLayer("Front").Tiles[x, y] = new StaticTile(
								__instance.map.GetLayer("Front"), __instance.map.TileSheets[0],
								BlendMode.Alpha, refurbishedMap.GetLayer("Front").Tiles[x, y].TileIndex);
							adjustMapLightPropertiesForLamp.Invoke(
								refurbishedMap.GetLayer("Front").Tiles[x, y].TileIndex, x, y, "Front");
						}
						else
						{
							__instance.map.GetLayer("Front").Tiles[x, y] = null;
						}
						if (refurbishedMap.GetLayer("Paths").Tiles[x, y] != null
							&& refurbishedMap.GetLayer("Paths").Tiles[x, y].TileIndex == 8)
						{
							Game1.currentLightSources.Add(new LightSource(
								4, new Vector2(x * 64, y * 64), 2f));
						}
						if (showEffects && Game1.random.NextDouble() < 0.58
										&& refurbishedMap.GetLayer("Buildings").Tiles[x, y] == null)
						{
							__instance.temporarySprites.Add(new TemporaryAnimatedSprite(
								6, new Vector2(x * 64, y * 64), Color.White)
							{
								layerDepth = 1f,
								interval = 50f,
								motion = new Vector2(Game1.random.Next(17) / 10f, 0f),
								acceleration = new Vector2(-0.005f, 0f),
								delayBeforeAnimationStart = Game1.random.Next(500)
							});
						}
					}
				}
				Log.D("End of LoadAreaPrefix",
					ModEntry.Config.DebugMode);
				return false;
			}
			catch (Exception e)
			{
				Log.E($"Error in {nameof(LoadArea_Prefix)}:\n{e}");
			}

			return true;
		}

		/// <summary>
		/// GetNotePosition() throws FatalEngineExecutionError when patched.
		/// Mimics IsJunimoNoteAtArea() using a static p value in place of GetNotePosition().
		/// </summary>
		public static bool IsJunimoNoteAtArea_Prefix(CommunityCenter __instance, ref bool __result, int area)
		{
			try
			{
				Log.T($"CC_IsJunimoNoteAtArea_Prefix(area={area})");
				if (area != Bundles.CommunityCentreAreaNumber
					|| Bundles.IsCommunityCentreComplete()
					|| Bundles.IsAbandonedJojaMartBundleAvailable())
					return true;

				Point p = Bundles.CommunityCentreNotePosition;
				__result = __instance.map.GetLayer("Buildings").Tiles[p.X, p.Y] != null;
				return false;
			}
			catch (Exception e)
			{
				Log.E($"Error in {nameof(IsJunimoNoteAtArea_Prefix)}:\n{e}");
			}

			return true;
		}

		/// <summary>
		/// GetNotePosition() throws FatalEngineExecutionError when patched.
		/// Mimics ShouldNoteAppearInArea() using a static position in place of GetNotePosition().
		/// </summary>
		public static bool ShouldNoteAppearInArea_Prefix(CommunityCenter __instance, ref bool __result, int area)
		{
			try
			{
				if (Bundles.IsCommunityCentreComplete()
					&& Bundles.IsAbandonedJojaMartBundleAvailable()
					&& (Game1.netWorldState.Value.BundleData.Keys.Any(key => key.StartsWith(Bundles.CommunityCentreAreaName))))
				{
					Log.D($"ShouldNoteAppearInArea removing custom bundle data.",
						ModEntry.Config.DebugMode);
					Bundles.SaveAndUnloadBundleData();
				}

				if (area != Bundles.CommunityCentreAreaNumber)
					return true;
				__result = !Bundles.IsCommunityCentreKitchenComplete()
					&& __instance.numberOfCompleteBundles() > (ModEntry.Config.DebugMode ? 0 : 2);
				return false;
			}
			catch (ArgumentOutOfRangeException e)
			{
				Log.D($"Error in {nameof(ShouldNoteAppearInArea_Prefix)}, may be non-critical:\n{e}",
					ModEntry.Config.DebugMode);
				return false;
			}
			catch (Exception e)
			{
				Log.E($"Error in {nameof(ShouldNoteAppearInArea_Prefix)}:\n{e}");
			}

			return true;
		}

		/// <summary>
		/// GetNotePosition() throws FatalEngineExecutionError when patched.
		/// Mimics AddJunimoNote() using a constant position value in place of GetNotePosition().
		/// </summary>
		public static bool AddJunimoNote_Prefix(CommunityCenter __instance, int area)
		{
			try
			{
				Log.T($"CC_AddJunimoNote_Prefix(area={area})");

				if (area != Bundles.CommunityCentreAreaNumber
					|| Bundles.IsCommunityCentreComplete()
					|| Bundles.IsAbandonedJojaMartBundleAvailable())
					return true;

				Point p = Bundles.CommunityCentreNotePosition;

				StaticTile[] tileFrames = CommunityCenter.getJunimoNoteTileFrames(area, __instance.Map);
				const string layer = "Buildings";
				__instance.Map.GetLayer(layer).Tiles[p.X, p.Y]
					= new AnimatedTile(__instance.Map.GetLayer(layer), tileFrames, 70L);
				Game1.currentLightSources.Add(new LightSource(
					4, new Vector2(p.X * 64, p.Y * 64), 1f));
				__instance.temporarySprites.Add(new TemporaryAnimatedSprite(
					6, new Vector2(p.X * 64, p.Y * 64), Color.White)
				{
					layerDepth = 1f,
					interval = 50f,
					motion = new Vector2(1f, 0f),
					acceleration = new Vector2(-0.005f, 0f)
				});
				__instance.temporarySprites.Add(new TemporaryAnimatedSprite(
					6, new Vector2(p.X * 64 - 12, p.Y * 64 - 12), Color.White)
				{
					scale = 0.75f,
					layerDepth = 1f,
					interval = 50f,
					motion = new Vector2(1f, 0f),
					acceleration = new Vector2(-0.005f, 0f),
					delayBeforeAnimationStart = 50
				});
				__instance.temporarySprites.Add(new TemporaryAnimatedSprite(
					6, new Vector2(p.X * 64 - 12, p.Y * 64 + 12), Color.White)
				{
					layerDepth = 1f,
					interval = 50f,
					motion = new Vector2(1f, 0f),
					acceleration = new Vector2(-0.005f, 0f),
					delayBeforeAnimationStart = 100
				});
				__instance.temporarySprites.Add(new TemporaryAnimatedSprite(
					6, new Vector2(p.X * 64, p.Y * 64), Color.White)
				{
					layerDepth = 1f,
					scale = 0.75f,
					interval = 50f,
					motion = new Vector2(1f, 0f),
					acceleration = new Vector2(-0.005f, 0f),
					delayBeforeAnimationStart = 150
				});

				return false;
			}
			catch (Exception e)
			{
				Log.E($"Error in {nameof(AddJunimoNote_Prefix)}:\n{e}");
			}

			return true;
		}

		/// <summary>
		/// GetNotePosition() throws FatalEngineExecutionError when patched.
		/// Mimics SetViewportToNextJunimoNoteTarget() using a static p value in place of GetNotePosition().
		/// </summary>
		public static bool SetViewportToNextJunimoNoteTarget_Prefix(CommunityCenter __instance)
		{
			try
			{
				List<int> viewportTargets = ModEntry.Instance.Helper.Reflection.GetField
					<List<int>>
					(__instance, "junimoNotesViewportTargets")
					.GetValue();
				if (viewportTargets.Count < 1
					|| viewportTargets[0] != Bundles.CommunityCentreAreaNumber
					|| !Bundles.IsCommunityCentreKitchenEnabledByHost())
					return true;

				StardewModdingAPI.IReflectedMethod reachedTarget = ModEntry.Instance.Helper.Reflection
					.GetMethod(__instance, "afterViewportGetsToJunimoNotePosition");
				StardewModdingAPI.IReflectedMethod endFunction = ModEntry.Instance.Helper.Reflection
					.GetMethod(__instance, "setViewportToNextJunimoNoteTarget");

				Point p = Bundles.CommunityCentreNotePosition;
				Game1.moveViewportTo(new Vector2(p.X, p.Y) * Game1.tileSize, 5f, 2000, () => reachedTarget.Invoke(), () => endFunction.Invoke());
				return false;
			}
			catch (ArgumentException e)
			{
				Log.D($"Error in {nameof(SetViewportToNextJunimoNoteTarget_Prefix)}, may be non-critical:\n{e}",
					ModEntry.Config.DebugMode);
				return false;
			}
			catch (Exception e)
			{
				Log.E($"Error in {nameof(SetViewportToNextJunimoNoteTarget_Prefix)}:\n{e}");
			}

			return true;
		}

		/// <summary>
		/// Adds an extra Junimo to the goodbye dance, as the number of Junimos added is otherwise hardcoded.
		/// </summary>
		public static bool StartGoodbyeDance_Prefix(CommunityCenter __instance)
		{
			try
			{
				if (!Bundles.IsCommunityCentreKitchenEnabledByHost())
					return true;

				Bundles.DrawStarInCommunityCentre(__instance);

				Junimo junimo = __instance.getJunimoForArea(Bundles.CommunityCentreAreaNumber);
				junimo.Position = new Vector2(22f, 12f) * Game1.tileSize;
				junimo.stayStill();
				junimo.faceDirection(1);
				junimo.fadeBack();
				junimo.IsInvisible = false;
				junimo.setAlpha(1f);
				junimo.sayGoodbye();
			}
			catch (Exception e)
			{
				Log.E($"Error in {nameof(StartGoodbyeDance_Prefix)}: {e}");
			}

			return true;
		}

		/// <summary>
		/// Adds an extra Junimo to the goodbye dance, as the number of Junimos added is otherwise hardcoded.
		/// </summary>
		public static bool JunimoGoodbyeDance_Prefix(CommunityCenter __instance)
		{
			try
			{
				if (!Bundles.IsCommunityCentreKitchenEnabledByHost())
					return true;

				__instance.getJunimoForArea(Bundles.CommunityCentreAreaNumber).Position = new Vector2(22f, 12f) * Game1.tileSize;
			}
			catch (Exception e)
			{
				Log.E($"Error in {nameof(JunimoGoodbyeDance_Prefix)}:\n{e}");
			}
			return true;
		}

		public static bool MessageForAreaCompletion_Prefix(CommunityCenter __instance, ref string __result)
		{
			try
			{
				if (!Bundles.IsAbandonedJojaMartBundleAvailable())
				{
					__result = Game1.content.LoadString("Strings\\Locations:CommunityCenter_AreaCompletion" + __instance.areasComplete.Count(_ => _), Game1.player.Name);
				}
				return string.IsNullOrEmpty(__result);
			}
			catch (Exception e)
			{
				Log.E($"Error in {nameof(MessageForAreaCompletion_Prefix)}:\n{e}");
			}
			return true;
		}

		public static bool UpdateWhenCurrentLocation_Prefix(CommunityCenter __instance, GameTime time)
		{
			try
			{
				var timerField = ModEntry.Instance.Helper.Reflection.GetField<int>(__instance, "restoreAreaTimer");
				var phaseField = ModEntry.Instance.Helper.Reflection.GetField<int>(__instance, "restoreAreaPhase");
				int timer = timerField.GetValue();
				int phase = phaseField.GetValue();
				int index = ModEntry.Instance.Helper.Reflection.GetField<int>(__instance, "restoreAreaIndex").GetValue();

				if (timer == 0 || !Bundles.IsCommunityCentreKitchenEnabledByHost())
				{
					return true;
				}

				if (index != Bundles.CommunityCentreAreaNumber)
				{
					// Reload the incomplete custom bundle area if the Pantry was just completed, as it overrides the area
					if (phase == 3 && timer == 2000 && Game1.IsMasterGame && Game1.currentLocation is CommunityCenter
						&& index == 0
						&& __instance.areasComplete.Count > Bundles.CommunityCentreAreaNumber
						&& !__instance.areasComplete[Bundles.CommunityCentreAreaNumber])
					{
						Bundles.CheckAndTryToUnrenovateKitchen();
					}
					return true;
				}

				var messageAlphaField = ModEntry.Instance.Helper.Reflection.GetField<float>(__instance, "messageAlpha");
				float messageAlpha = messageAlphaField.GetValue();
				ICue buildUpSound = ModEntry.Instance.Helper.Reflection.GetField<ICue>(__instance, "buildUpSound").GetValue();
				List<int> junimoNotesViewportTargets = ModEntry.Instance.Helper.Reflection.GetField<List<int>>(__instance, "junimoNotesViewportTargets").GetValue();

				__instance.missedRewardsChest.Value.updateWhenCurrentLocation(time, __instance);

				if (timer > 0)
				{
					var old = timer;
					timer -= time.ElapsedGameTime.Milliseconds;
					switch (phase)
					{
						case 0:
							if (timer <= 0)
							{
								timer = 3000;
								phase = 1;
								if (Game1.player.currentLocation == __instance)
								{
									Game1.player.faceDirection(2);
									Game1.player.jump();
									Game1.player.jitterStrength = 1f;
									Game1.player.showFrame(94);

									// THIS IS AN IMPORTANT BIT:
									// Add some mail to flag this bundle as having been completed
									Log.D($"Sending mail for Cooking bundle completion ({ModEntry.MailKitchenCompleted})",
										ModEntry.Config.DebugMode);
									Game1.player.mailForTomorrow.Add(ModEntry.MailKitchenCompleted + "%&NL&%");
								}
							}
							break;
						case 1:
							if (Game1.IsMasterGame && Game1.random.NextDouble() < 0.4)
							{
								Rectangle area = new Rectangle(Bundles.CommunityCentreArea.X, Bundles.CommunityCentreArea.Y,
									Bundles.CommunityCentreArea.Width + 12, Bundles.CommunityCentreArea.Height + 6);
								Vector2 v = Utility.getRandomPositionInThisRectangle(area, Game1.random);
								Junimo i = new Junimo(v * Game1.tileSize, index, temporary: true);
								if (!__instance.isCollidingPosition(i.GetBoundingBox(), Game1.viewport, i))
								{
									__instance.characters.Add(i);
									ModEntry.Instance.Helper.Reflection.GetField<Multiplayer>(
										typeof(Game1), "multiplayer").GetValue().broadcastSprites(__instance,
										new TemporaryAnimatedSprite((Game1.random.NextDouble() < 0.5) ? 5 : 46,
										v * Game1.tileSize + new Vector2(16f, 16f), Color.White)
										{
											layerDepth = 1f
										});
									__instance.localSound("tinyWhip");
								}
							}
							if (timer <= 0)
							{
								timer = 999999;
								phase = 2;
								if (Game1.player.currentLocation != __instance)
								{
									break;
								}
								Game1.screenGlowOnce(Color.White, hold: true, 0.005f, 1f);
								if (Game1.soundBank != null)
								{
									buildUpSound = Game1.soundBank.GetCue("wind");
									buildUpSound.SetVariable("Volume", 0f);
									buildUpSound.SetVariable("Frequency", 0f);
									buildUpSound.Play();
								}
								Game1.player.jitterStrength = 2f;
								Game1.player.stopShowingFrame();
							}
							Game1.drawLighting = false;
							break;
						case 2:
							if (buildUpSound != null)
							{
								buildUpSound.SetVariable("Volume", Game1.screenGlowAlpha * 150f);
								buildUpSound.SetVariable("Frequency", Game1.screenGlowAlpha * 150f);
							}
							if (Game1.screenGlowAlpha >= Game1.screenGlowMax)
							{
								messageAlpha += 0.008f;
								messageAlpha = Math.Min(messageAlpha, 1f);
							}
							if ((Game1.screenGlowAlpha == Game1.screenGlowMax
								|| Game1.currentLocation != __instance) && timer > 5200)
							{
								timer = 5200;
							}
							if (timer < 5200 && Game1.random.NextDouble() < (5200 - timer) / 10000f)
							{
								__instance.localSound((Game1.random.NextDouble() < 0.5) ? "dustMeep" : "junimoMeep1");
							}
							if (timer > 0)
							{
								break;
							}
							timer = 2000;
							messageAlpha = 0f;
							phase = 3;
							if (Game1.IsMasterGame)
							{
								for (var j = __instance.characters.Count - 1; j >= 0; j--)
								{
									if (__instance.characters[j] is Junimo
										&& (__instance.characters[j] as Junimo).temporaryJunimo.Value)
									{
										__instance.characters.RemoveAt(j);
									}
								}
							}
							if (Game1.player.currentLocation == __instance)
							{
								Game1.screenGlowHold = false;
								__instance.loadArea(index);
								if (buildUpSound != null)
								{
									buildUpSound.Stop(AudioStopOptions.Immediate);
								}
								__instance.localSound("wand");
								Game1.changeMusicTrack("junimoStarSong");
								__instance.localSound("woodyHit");
								Game1.flashAlpha = 1f;
								Game1.player.stopJittering();
								Game1.drawLighting = true;
							}
							break;
						case 3:
							if (old > 1000 && timer <= 1000)
							{
								Junimo junimo = __instance.getJunimoForArea(index);
								if (junimo != null && Game1.IsMasterGame)
								{
									if (!junimo.holdingBundle.Value)
									{
										junimo.Position = Utility.getRandomAdjacentOpenTile(Utility.PointToVector2(Bundles.CommunityCentreNotePosition), __instance) * 64f;
										var iter = 0;
										while (__instance.isCollidingPosition(junimo.GetBoundingBox(), Game1.viewport, junimo) && iter < 20)
										{
											junimo.Position = Utility.getRandomPositionInThisRectangle(Bundles.CommunityCentreArea, Game1.random) * Game1.tileSize;
											iter++;
										}
										if (iter < 20)
										{
											junimo.fadeBack();
										}
									}
									junimo.returnToJunimoHutToFetchStar(__instance);
								}
							}
							if (timer <= 0 && !ModEntry.Instance.Helper.Reflection.GetField<bool>(
								__instance, "_isWatchingJunimoGoodbye").GetValue())
							{
								Game1.freezeControls = false;
							}
							break;
					}
				}
				else if (Game1.activeClickableMenu == null
					&& junimoNotesViewportTargets?.Count > 0
					&& !Game1.isViewportOnCustomPath())
				{
					ModEntry.Instance.Helper.Reflection.GetMethod(__instance, "setViewportToNextJunimoNoteTarget").Invoke();
				}

				timerField.SetValue(timer);
				phaseField.SetValue(phase);
				messageAlphaField.SetValue(messageAlpha);

				return false;
			}
			catch (Exception e)
			{
				Log.E($"Error in {nameof(UpdateWhenCurrentLocation_Prefix)}:\n{e}");
			}
			return true;
		}
	}
}
