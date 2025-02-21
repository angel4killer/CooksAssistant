﻿using SpaceCore;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace LoveOfCooking.Objects
{
	public interface ICookingSkillAPI
	{
		public enum Profession
		{
			ImprovedOil,
			Restoration,
			GiftBoost,
			SalePrice,
			ExtraPortion,
			BuffDuration
		}

		bool IsEnabled();
		CookingSkill GetSkill();
		int GetLevel();
		Dictionary<ICookingSkillAPI.Profession, bool> GetCurrentProfessions(long playerID = -1L);
		bool HasProfession(ICookingSkillAPI.Profession profession, long playerID = -1L);
		bool AddExperienceDirectly(int experience);
		void AddCookingBuffToItem(string name, int value);
		int GetTotalCurrentExperience();
		int GetExperienceRequiredForLevel(int level);
		int GetTotalExperienceRequiredForLevel(int level);
		int GetExperienceRemainingUntilLevel(int level);
		Dictionary<int, List<string>> GetAllLevelUpRecipes();
		List<string> GetCookingRecipesForLevel(int level);
		int CalculateExperienceGainedFromCookingItem(Item item, int numIngredients, int numCooked, bool applyExperience);
		bool RollForExtraPortion();
	}

	public class CookingSkillAPI : ICookingSkillAPI
	{
		private readonly IReflectionHelper Reflection;

		public CookingSkillAPI(IReflectionHelper reflection)
		{
			this.Reflection = reflection;
		}

		/// <returns>Whether the Cooking skill is enabled in the mod config.</returns>
		public bool IsEnabled()
		{
			return ModEntry.Config.AddCookingSkillAndRecipes;
		}

		/// <returns>Instance of the Cooking skill.</returns>
		public CookingSkill GetSkill()
		{
			return SpaceCore.Skills.GetSkill(CookingSkill.InternalName) as CookingSkill;
		}

		/// <returns>Current base level.</returns>
		public int GetLevel()
		{
			return SpaceCore.Skills.GetSkillLevel(Game1.player, CookingSkill.InternalName);
		}

		/// <returns>A dictionary of all possible Cooking professions and whether each is active.</returns>
		public Dictionary<ICookingSkillAPI.Profession, bool> GetCurrentProfessions(long playerID = -1L)
		{
			var dict = new Dictionary<ICookingSkillAPI.Profession, bool>();
			int count = Enum.GetNames(typeof(ICookingSkillAPI.Profession)).Length;
			for (int i = 0; i < count; ++i)
			{
				var profession = (ICookingSkillAPI.Profession)i;
				dict.Add(profession, this.HasProfession(profession: profession, playerID: playerID));
			}
			return dict;
		}

		/// <returns>Whether the player has unlocked and chosen the given profession.</returns>
		public bool HasProfession(ICookingSkillAPI.Profession profession, long playerID = -1L)
		{
			Farmer player = Game1.getFarmer(playerID);
			return this.IsEnabled() && (player ?? Game1.player).HasCustomProfession(this.GetSkill().Professions[(int)profession]);
		}

		/// <summary>
		/// Not yet implemented
		/// </summary>
		/// <param name="name">Name of the item to receive the buff.</param>
		/// <param name="value">Added skill level amount, will act as a debuff if negative.</param>
		public void AddCookingBuffToItem(string name, int value)
		{
			if (value == 0)
				return;

			throw new NotImplementedException();
		}

		/// <summary>
		/// Add experience to the skill.
		/// Experience gains from cooking may be applied via CalculateExperienceGainedFromCookingItem. This method should not be used where CalculateExperience would be more appropriate.
		/// </summary>
		/// <returns>Whether the player gained a level from added experience.</returns>
		public bool AddExperienceDirectly(int experience)
		{
			Events.InvokeOnCookingExperienceGained(experienceGained: experience);

			int level = this.GetLevel();
			SpaceCore.Skills.AddExperience(Game1.player, CookingSkill.InternalName, experience);
			return level < this.GetLevel();
		}

		/// <returns>Total accumulated experience.</returns>
		public int GetTotalCurrentExperience()
		{
			return SpaceCore.Skills.GetExperienceFor(Game1.player, CookingSkill.InternalName);
		}

		/// <returns>Experience required to reach this level from the previous level.</returns>
		public int GetExperienceRequiredForLevel(int level)
		{
			CookingSkill skill = this.GetSkill();
			return level > 0 && level <= skill.ExperienceCurve.Length
				? level switch
				{
					0 => 0,
					1 => skill.ExperienceCurve[level - 1],
					_ => skill.ExperienceCurve[level - 1] - skill.ExperienceCurve[level - 2]
				}
				: 0;
		}

		/// <returns>Accumulated experience required to reach this level from zero.</returns>
		public int GetTotalExperienceRequiredForLevel(int level)
		{
			CookingSkill skill = this.GetSkill();
			return level > 0 && level <= skill.ExperienceCurve.Length
				? skill.ExperienceCurve[level - 1]
				: 0;
		}

		/// <returns>
		/// Difference between total accumulated experience and experience required to reach level.
		/// Negative if level is lower than current skill level.
		/// </returns>
		public int GetExperienceRemainingUntilLevel(int level)
		{
			return this.GetTotalExperienceRequiredForLevel(level) - this.GetTotalCurrentExperience();
		}

		/// <returns>Table of recipes learned through leveling Cooking.</returns>
		public Dictionary<int, List<string>> GetAllLevelUpRecipes()
		{
			return CookingSkill.CookingSkillLevelUpRecipes;
		}

		/// <returns>New recipes learned when reaching this level.</returns>
		public List<string> GetCookingRecipesForLevel(int level)
		{
			// Level undefined
			if (!CookingSkill.CookingSkillLevelUpRecipes.ContainsKey(level))
			{
				return new List<string>();
			}
			// Level used for professions, no new recipes added
			if (level % 5 == 0)
			{
				return new List<string>();
			}
			return CookingSkill.CookingSkillLevelUpRecipes[level];
		}

		/// <summary>
		/// Calculates the experience to be gained from a certain food, and applies the gained experience to the Cooking skill if needed.
		/// </summary>
		/// <param name="item">Item to be cooked, used to determine whether the recipe has been cooked before.</param>
		/// <param name="ingredientsCount">Number of ingredients used in the cooking recipe for this item.</param>
		/// <param name="apply">Whether to apply experience gains to the skill.</param>
		/// <returns>Experience gained.</returns>
		public int CalculateExperienceGainedFromCookingItem(Item item, int ingredientsCount, int numCooked, bool apply)
		{
			// Magical numbers live here

			// Reward players for cooking brand new recipes
			int newBonus = Game1.player.recipesCooked.ContainsKey(item.ParentSheetIndex) ? 0 : 34;

			// Gain more experience for the first time cooking a meal each day
			int dailyBonus = ModEntry.Instance.States.Value.FoodCookedToday.ContainsKey(item.Name) ? 0 : 12;
			if (!ModEntry.Instance.States.Value.FoodCookedToday.ContainsKey(item.Name))
				ModEntry.Instance.States.Value.FoodCookedToday[item.Name] = 0;

			// Gain less experience the more that the same food is cooked for this day
			float stackBonus // (Quantity * (Rate of decay per quantity towards roughly 50%) / Some divisor for experience rate)
				= Math.Min(numCooked, CookingSkill.MaxFoodStackPerDayForExperienceGains)
					* Math.Max(6f, 12f - ModEntry.Instance.States.Value.FoodCookedToday[item.Name]) / 8f;

			// Gain more experience for recipe complexity
			float ingredientsBonus = 1f + ingredientsCount * 0.2f;
			float experienceFromIngredients = 5 + 4f * ingredientsBonus;

			// Sum up experience
			int currentLevel = this.GetLevel();
			int nextLevel = currentLevel + 1; // TODO: COMPAT: Level Extender affects max level in API
			//int maxLevel = ModEntry.UsingLevelExtender ? 100 : 10;
			int maxLevel = 10;
			int finalExperience = 0;

			if (currentLevel >= maxLevel)
			{
				Log.D($"No experience was applied: Skill is at max level.",
					ModEntry.Config.DebugMode);
			}
			else
			{
				int remainingExperience = this.GetExperienceRemainingUntilLevel(nextLevel);
				int requiredExperience = this.GetExperienceRequiredForLevel(nextLevel);
				int summedExperience = (int)(newBonus + dailyBonus + experienceFromIngredients * stackBonus * (ModEntry.Config.DebugMode ? CookingSkill.GlobalExperienceRate : 1));
				finalExperience = maxLevel - currentLevel == 1
					? Math.Min(remainingExperience, summedExperience)
					: summedExperience;
				Log.D($"Cooked up {item.Name} with {ingredientsCount} ingredients.",
					ModEntry.Config.DebugMode);

				if (finalExperience < 1)
				{
					Log.D($"No experience was applied: None gained.",
						ModEntry.Config.DebugMode);
				}
				else if (apply)
				{
					Log.D($"\nExperience until level {nextLevel}:"
						+ $" ({requiredExperience - remainingExperience}/{requiredExperience})"
						+ $"\nTotal experience: ({this.GetTotalCurrentExperience()}/{this.GetTotalExperienceRequiredForLevel(nextLevel)})"
						+ $"\n+{finalExperience} experience!",
						ModEntry.Config.DebugMode);
					this.AddExperienceDirectly(finalExperience);
				}
				else
				{
					Log.D($"No experience was applied: Probe.",
						ModEntry.Config.DebugMode);
				}
			}

			return finalExperience;
		}

		public bool RollForExtraPortion()
		{
			return Game1.random.NextDouble() < CookingSkill.ExtraPortionChance;
		}
	}
}
