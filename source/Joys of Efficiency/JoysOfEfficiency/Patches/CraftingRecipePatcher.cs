﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JoysOfEfficiency.Core;
using JoysOfEfficiency.Utils;
using StardewValley;
using StardewValley.Objects;

namespace JoysOfEfficiency.Patches
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal class ConsumeIngredientsPatcher
    {

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal static bool Prefix(ref CraftingRecipe __instance)
        {
            //consumeIngredients
            return ConsumeIngredients(__instance);
        }

        private static bool ConsumeIngredients(CraftingRecipe recipe)
        {
            Dictionary<int, int> recipeList = InstanceHolder.Reflection
                .GetField<Dictionary<int, int>>(recipe, "recipeList").GetValue();
            foreach (KeyValuePair<int, int> kv in recipeList)
            {
                int index = kv.Key;
                int count = kv.Value;
                int toConsume;
                foreach (Item playerItem in new List<Item>(Game1.player.Items))
                {
                    //Search for the player inventory
                    if (playerItem != null && (playerItem.ParentSheetIndex == index || playerItem.Category == index))
                    {
                        toConsume = Math.Min(playerItem.Stack, count);
                        playerItem.Stack -= toConsume;
                        count -= toConsume;
                        if (playerItem.Stack == 0)
                        {
                            Game1.player.removeItemFromInventory(playerItem);
                        }
                    }
                }

                foreach (Chest chest in Util.GetNearbyChests(!recipe.isCookingRecipe))
                {
                    //Search for the chests
                    foreach (Item chestItem in new List<Item>(chest.items))
                    {
                        if (chestItem != null && (chestItem.ParentSheetIndex == index || chestItem.Category == index))
                        {
                            toConsume = Math.Min(chestItem.Stack, count);
                            chestItem.Stack -= toConsume;
                            count -= toConsume;
                            if (chestItem.Stack == 0)
                            {
                                chest.items.Remove(chestItem);
                            }
                        }
                    }
                }

                if (count > 0 && recipe.isCookingRecipe)
                {
                    //Delegate process to the original method.
                    return true;
                }
            }

            return false;
        }
    }

}
