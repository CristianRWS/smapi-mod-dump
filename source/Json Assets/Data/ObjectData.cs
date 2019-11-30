﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using StardewValley;
using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace JsonAssets.Data
{
    public class ObjectData : DataNeedsId
    {
        [JsonIgnore]
        public Texture2D texture;
        [JsonIgnore]
        public Texture2D textureColor;

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Category_
        {
            // SDV Patcher made these static readonly, so I can't use them in the enum
            Vegetable = -75, //SObject.VegetableCategory,
            Fruit = -79, //SObject.FruitsCategory,
            Flower = -80, //SObject.flowersCategory,
            Gem = -2, //SObject.GemCategory,
            Fish = -4, //SObject.FishCategory,
            Egg = -5, //SObject.EggCategory,
            Milk = -6, //SObject.MilkCategory,
            Cooking = -7, //SObject.CookingCategory,
            Crafting = -8, //SObject.CraftingCategory,
            Mineral = -12, //SObject.mineralsCategory,
            Meat = -14, //SObject.meatCategory,
            Metal = -15, //SObject.metalResources,
            Junk = -20, //SObject.junkCategory,
            Syrup = -27, //SObject.syrupCategory,
            MonsterLoot = -28, //SObject.monsterLootCategory,
            ArtisanGoods = -26, //SObject.artisanGoodsCategory,
            Seeds = -74, //SObject.SeedsCategory,
            Ring = -96, //SObject.ringCategory,
            AnimalGoods = -18, //SObject.sellAtPierresAndMarnies
        }

        public class Recipe_
        {
            public class Ingredient
            {
                public object Object { get; set; }
                public int Count { get; set; }
            }
            // Possibly friendship option (letters, like vanilla) and/or skill levels (on levelup?)
            public int ResultCount { get; set; } = 1;
            public IList<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

            public bool IsDefault { get; set; } = false;
            public bool CanPurchase { get; set; } = false;
            public int PurchasePrice { get; set; }
            public string PurchaseFrom { get; set; } = "Gus";
            public IList<string> PurchaseRequirements { get; set; } = new List<string>();

            internal string GetRecipeString( ObjectData parent )
            {
                var str = "";
                foreach (var ingredient in Ingredients)
                    str += Mod.instance.ResolveObjectId(ingredient.Object) + " " + ingredient.Count + " ";
                str = str.Substring(0, str.Length - 1);
                str += $"/what is this for?/{parent.id}/";
                if (parent.Category != Category_.Cooking)
                    str += "false/";
                str += "/null"; // TODO: Requirement
                return str;
            }

            internal string GetPurchaseRequirementString()
            {
                var str = $"1234567890";
                foreach (var cond in PurchaseRequirements)
                    str += $"/{cond}";
                return str;
            }
        }

        public class FoodBuffs_
        {
            public int Farming { get; set; } = 0;
            public int Fishing { get; set; } = 0;
            public int Mining { get; set; } = 0;
            public int Luck { get; set; } = 0;
            public int Foraging { get; set; } = 0;
            public int MaxStamina { get; set; } = 0;
            public int MagnetRadius { get; set; } = 0;
            public int Speed { get; set; } = 0;
            public int Defense { get; set; } = 0;
            public int Attack { get; set; } = 0;
            public int Duration { get; set; } = 0;
        }

        public string Description { get; set; }
        public Category_ Category { get; set; }
        public string CategoryTextOverride { get; set; }
        public Color CategoryColorOverride { get; set; } = new Color(0, 0, 0, 0);
        public bool IsColored { get; set; } = false;

        public int Price { get; set; }

        public Recipe_ Recipe { get; set; }

        public int Edibility { get; set; } = SObject.inedible;
        public bool EdibleIsDrink { get; set; } = false;
        public FoodBuffs_ EdibleBuffs = new FoodBuffs_();

        public bool CanPurchase { get; set; } = false;
        public int PurchasePrice { get; set; }
        public string PurchaseFrom { get; set; } = "Pierre";
        public IList<string> PurchaseRequirements { get; set; } = new List<string>();

        public class GiftTastes_
        {
            public IList<string> Love = new List<string>();
            public IList<string> Like = new List<string>();
            public IList<string> Neutral = new List<string>();
            public IList<string> Dislike = new List<string>();
            public IList<string> Hate = new List<string>();
        }
        public GiftTastes_ GiftTastes;

        public Dictionary<string, string> NameLocalization = new Dictionary<string, string>();
        public Dictionary<string, string> DescriptionLocalization = new Dictionary<string, string>();

        public List<string> ContextTags = new List<string>();

        public string LocalizedName()
        {
            var currLang = LocalizedContentManager.CurrentLanguageCode;
            if (currLang == LocalizedContentManager.LanguageCode.en)
                return Name;
            if (NameLocalization == null || !NameLocalization.ContainsKey(currLang.ToString()))
                return Name;
            return NameLocalization[currLang.ToString()];
        }

        public string LocalizedDescription()
        {
            var currLang = LocalizedContentManager.CurrentLanguageCode;
            if (currLang == LocalizedContentManager.LanguageCode.en)
                return Description;
            if (DescriptionLocalization == null || !DescriptionLocalization.ContainsKey(currLang.ToString()))
                return Description;
            return DescriptionLocalization[currLang.ToString()];
        }

        public int GetObjectId() { return id; }

        internal string GetObjectInformation()
        {
            if (Edibility != SObject.inedible)
            {
                var itype = (int)Category;
                var str = $"{Name}/{Price}/{Edibility}/{Category} {itype}/{LocalizedName()}/{LocalizedDescription()}/";
                str += (EdibleIsDrink ? "drink" : "food") + "/";
                if (EdibleBuffs == null)
                    EdibleBuffs = new FoodBuffs_();
                str += $"{EdibleBuffs.Farming} {EdibleBuffs.Fishing} {EdibleBuffs.Mining} 0 {EdibleBuffs.Luck} {EdibleBuffs.Foraging} 0 {EdibleBuffs.MaxStamina} {EdibleBuffs.MagnetRadius} {EdibleBuffs.Speed} {EdibleBuffs.Defense} {EdibleBuffs.Attack}/{EdibleBuffs.Duration}";
                return str;
            }
            else
            {
                var itype = (int)Category;
                return $"{Name}/{Price}/{Edibility}/Basic {itype}/{LocalizedName()}/{LocalizedDescription()}";
            }
        }

        internal string GetPurchaseRequirementString()
        {
            var str = $"1234567890";
            foreach (var cond in PurchaseRequirements)
                str += $"/{cond}";
            return str;
        }
    }
}
