﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using JoysOfEfficiency.ModCheckers;
using JoysOfEfficiency.Patches;
using JoysOfEfficiency.Utils;

using Microsoft.Xna.Framework;

using Netcode;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;

namespace JoysOfEfficiency
{
    using Player = Farmer;
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
    internal class ModEntry : Mod
    {
        public static bool IsCoGOn { get; private set; }
        public static bool IsCCOn { get; private set; }
        public static bool IsCAOn { get; private set; }

        public static Config Conf { get; private set; }

        public static IModHelper ModHelper { get; private set; }

        private bool _unableToGift;
        private string _hoverText;

        private bool _dayEnded;

        private bool _paused;

        private int _ticks;

        private double _timeoutCounter;
        private int LastTimeOfDay;

        public override void Entry(IModHelper helper)
        {
            ModHelper = helper;
            Util.Helper = helper;
            Util.Monitor = Monitor;
            Util.ModInstance = this;
            
            Conf = helper.ReadConfig<Config>();
            IModEvents Events = Helper.Events;

            Events.Input.ButtonPressed += OnButtonPressed;

            Events.GameLoop.UpdateTicked += OnGameUpdateEvent;
            
            Events.Display.RenderedHud += OnPostRenderHud;
            Events.Display.RenderedActiveMenu += OnPostRenderGui;

            Events.Display.MenuChanged += OnMenuChanged;
            
            Events.GameLoop.Saving += OnBeforeSave;

            Events.GameLoop.DayStarted += OnDayStarted;

            Conf.CpuThresholdFishing = Util.Cap(Conf.CpuThresholdFishing, 0, 0.5f);
            Conf.HealthToEatRatio = Util.Cap(Conf.HealthToEatRatio, 0.1f, 0.8f);
            Conf.StaminaToEatRatio = Util.Cap(Conf.StaminaToEatRatio, 0.1f, 0.8f);
            Conf.AutoCollectRadius = (int)Util.Cap(Conf.AutoCollectRadius, 1, 3);
            Conf.AutoHarvestRadius = (int)Util.Cap(Conf.AutoHarvestRadius, 1, 3);
            Conf.AutoPetRadius = (int)Util.Cap(Conf.AutoPetRadius, 1, 3);
            Conf.AutoWaterRadius = (int)Util.Cap(Conf.AutoWaterRadius, 1, 3);
            Conf.AutoDigRadius = (int)Util.Cap(Conf.AutoDigRadius, 1, 3);
            Conf.AutoShakeRadius = (int)Util.Cap(Conf.AutoShakeRadius, 1, 3);
            Conf.MachineRadius = (int)Util.Cap(Conf.MachineRadius, 1, 3);
            Conf.RadiusCraftingFromChests = (int) Util.Cap(Conf.RadiusCraftingFromChests, 1, 5);
            Conf.IdleTimeout = (int) Util.Cap(Conf.IdleTimeout, 1, 300);
            Conf.ScavengingRadius = (int) Util.Cap(Conf.ScavengingRadius, 1, 3);
            Conf.AnimalHarvestRadius = (int) Util.Cap(Conf.AnimalHarvestRadius, 1, 3);

            if(ModChecker.IsCoGLoaded(helper))
            {
                Monitor.Log("CasksOnGround detected.");
                IsCoGOn = true;
            }

            if (ModChecker.IsCALoaded(helper))
            {
                Monitor.Log("CasksAnywhere detected.");
                IsCAOn = true;
            }

            if (ModChecker.IsCCLoaded(helper))
            {
                Monitor.Log("Convenient Chests detected. JOE's CraftingFromChests feature will be disabled and won't patch the game.");
                Conf.CraftingFromChests = false;
                IsCCOn = true;
            }
            else
            {
                HarmonyPatcher.Init();
            }

            helper.WriteConfig(Conf);
            MineIcons.Init(helper);
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs args)
        {
            if (Conf.AutoLootTreasures && args.NewMenu is ItemGrabMenu menu)
            {
                //Opened ItemGrabMenu
                Util.LootAllAcceptableItems(menu);
            }

            if (Conf.CollectLetterAttachmentsAndQuests && args.NewMenu is LetterViewerMenu letter)
            {
                Util.CollectMailAttachmentsAndQuests(letter);
            }
        }

        private void OnGameUpdateEvent(object sender, UpdateTickedEventArgs args)
        {
            OnEveryUpdate();
            if(args.IsMultipleOf(8))
            {
                OnGameEighthUpdate();
            }
        }

        private void OnEveryUpdate()
        {
            if (!Context.IsWorldReady)
            {
                return;
            }
            if (Conf.PauseWhenIdle)
            {
                if (Util.IsPlayerIdle())
                {
                    _timeoutCounter += Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
                    if (_timeoutCounter > Conf.IdleTimeout * 1000)
                    {
                        if (!_paused)
                        {
                            Monitor.Log("Paused game");
                            _paused = true;
                        }

                        Game1.timeOfDay = LastTimeOfDay;
                    }
                }
                else
                {
                    if (_paused)
                    {
                        _paused = false;
                        Monitor.Log("Resumed game");
                    }

                    _timeoutCounter = 0;
                    LastTimeOfDay = Game1.timeOfDay;
                }
            }
            else
            {
                _paused = false;
            }

            _hoverText = null;

            Player player = Game1.player;
            if(Conf.AutoGate)
            {
                Util.TryToggleGate(player);
            }

            if (Conf.GiftInformation)
            {
                _unableToGift = false;
                if (player.CurrentItem == null || !player.CurrentItem.canBeGivenAsGift() || player.currentLocation == null || player.currentLocation.characters.Count == 0)
                {
                    return;
                }

                List<NPC> npcList = player.currentLocation.characters.Where(a => a != null && a.isVillager()).ToList();
                foreach (NPC npc in npcList)
                {
                    RectangleE npcRect = new RectangleE(
                        npc.position.X,
                        npc.position.Y - npc.Sprite.getHeight() - Game1.tileSize / 1.5f,
                        npc.Sprite.getWidth() * 3 + npc.Sprite.getWidth() / 1.5f,
                        npc.Sprite.getHeight() * 3.5f);

                    if (!npcRect.IsInternalPoint(
                        Game1.getMouseX() + Game1.viewport.X,
                        Game1.getMouseY() + Game1.viewport.Y))
                    {
                        continue;
                    }

                    //Mouse hovered on the NPC
                    StringBuilder key = new StringBuilder("taste.");
                    if (player.friendshipData.ContainsKey(npc.Name) && Game1.NPCGiftTastes.ContainsKey(npc.Name))
                    {
                        Friendship friendship = player.friendshipData[npc.Name];
                        if (friendship.GiftsThisWeek > 1)
                        {
                            //Week restriction
                            if (!npc.isMarried() || npc.getSpouse().UniqueMultiplayerID != player.UniqueMultiplayerID)
                            {
                                key.Append("gavetwogifts.");
                                _unableToGift = true;
                            }
                        }
                        if (!_unableToGift && friendship.GiftsToday > 0)
                        {
                            //Day restriction
                            key.Append("gavetoday.");
                            _unableToGift = true;
                        }
                        else if (npc.canReceiveThisItemAsGift(player.CurrentItem))
                        {
                            switch (npc.getGiftTasteForThisItem(player.CurrentItem))
                            {
                                case 0:
                                    key.Append("love.");
                                    break;
                                case 2:
                                    key.Append("like.");
                                    break;
                                case 4:
                                    key.Append("dislike.");
                                    break;
                                case 6:
                                    key.Append("hate.");
                                    break;
                                default:
                                    key.Append("neutral.");
                                    break;
                            }
                        }
                        else
                        {
                            return;
                        }

                        switch (npc.Gender)
                        {
                            case NPC.female:
                                key.Append("female");
                                break;
                            default:
                                key.Append("male");
                                break;
                        }
                        _hoverText = Helper.Translation.Get(key.ToString());
                    }
                }
            }
        }

        private void OnGameEighthUpdate()
        {
            if(Game1.currentGameTime == null)
            {
                return;
            }

            if (Conf.CloseTreasureWhenAllLooted && Game1.activeClickableMenu is ItemGrabMenu menu)
            {
                Util.TryCloseItemGrabMenu(menu);
            }

            if (!Context.IsWorldReady || !Context.IsPlayerFree)
            {
                return;
            }

            Player player = Game1.player;
            GameLocation location = Game1.currentLocation;
            IReflectionHelper reflection = Helper.Reflection;
            try
            {
                if (player.CurrentTool is FishingRod rod && Game1.activeClickableMenu == null)
                {
                    IReflectedField<int> whichFish = reflection.GetField<int>(rod, "whichFish");

                    if (rod.isNibbling && rod.isFishing && whichFish.GetValue() == -1 && !rod.isReeling && !rod.hit && !rod.isTimingCast && !rod.pullingOutOfWater && !rod.fishCaught)
                    {
                        if (Conf.AutoReelRod)
                        {
                            rod.DoFunction(player.currentLocation, 1, 1, 1, player);
                        }
                    }
                }
                if (Game1.currentLocation is MineShaft shaft)
                {
                    bool isFallingDownShaft = Helper.Reflection.GetField<bool>(shaft, "isFallingDownShaft").GetValue();
                    if (isFallingDownShaft)
                    {
                        return;
                    }
                }
                if (!Context.CanPlayerMove)
                {
                    return;
                }
                if (Conf.AutoEat)
                {
                    Util.TryToEatIfNeeded(player);
                }
                _ticks = (_ticks + 1) % 8;
                if(Conf.BalancedMode && _ticks % 8 != 0)
                {
                    return;
                }

                if (Conf.AutoPickUpTrash)
                {
                    Util.ScavengeTrashCan();
                }
                if (Conf.AutoWaterNearbyCrops)
                {
                    Util.WaterNearbyCrops();
                }
                if (Conf.AutoPetNearbyAnimals)
                {
                    int radius = Conf.AutoPetRadius * Game1.tileSize;
                    Rectangle bb = Util.Expand(player.GetBoundingBox(), radius);
                    List<FarmAnimal> animalList = Util.GetAnimalsList(player);
                    foreach (FarmAnimal animal in animalList)
                    {
                        if (bb.Contains((int)animal.Position.X, (int)animal.Position.Y) && !animal.wasPet.Value)
                        {
                            if (Game1.timeOfDay >= 1900 && !animal.isMoving())
                            {
                                continue;
                            }
                            animal.pet(player);
                        }
                    }
                }

                if (Conf.AutoShearingAndMilking)
                {
                    Util.ShearingAndMilking(player);
                }
                if(Conf.AutoPullMachineResult)
                {
                    Util.PullMachineResult();
                }
                if(Conf.AutoDepositIngredient)
                {
                    Util.DepositIngredientsToMachines();
                }
                if (Conf.AutoHarvest)
                {
                    Util.HarvestNearCrops(player);
                }
                if (Conf.AutoDestroyDeadCrops)
                {
                    Util.DestroyNearDeadCrops(player);
                }
                if (Conf.AutoRefillWateringCan)
                {
                    WateringCan can = Util.FindToolFromInventory<WateringCan>(Conf.FindCanFromInventory);
                    if (can != null && can.WaterLeft < Util.GetMaxCan(can) && Util.IsThereAnyWaterNear(player.currentLocation, player.getTileLocation()))
                    {
                        can.WaterLeft = can.waterCanMax;
                        Game1.playSound("slosh");
                        DelayedAction.playSoundAfterDelay("glug", 250);
                    }
                }
                if (Conf.AutoCollectCollectibles)
                {
                    Util.CollectNearbyCollectibles(location);
                }
                if (Conf.AutoDigArtifactSpot)
                {
                    Util.DigNearbyArtifactSpots();
                }
                if (Conf.AutoShakeFruitedPlants)
                {
                    Util.ShakeNearbyFruitedTree();
                    Util.ShakeNearbyFruitedBush();
                }
                if(Conf.AutoAnimalDoor && !_dayEnded && Game1.timeOfDay >= 1900)
                {
                    _dayEnded = true;
                    OnBeforeSave(null, null);
                }
                if(Conf.AutoPetNearbyPets)
                {
                    Util.PetNearbyPets();
                }
                if (Conf.UnifyFlowerColors)
                {
                    Util.UnifyFlowerColors();
                }
            }
            catch (Exception ex)
            {
                Monitor.Log(ex.Source);
                Monitor.Log(ex.ToString());
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs args)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }
            if (!Context.IsPlayerFree || Game1.activeClickableMenu != null)
            {
                return;
            }
            if (args.Button == Conf.ButtonShowMenu)
            {
                //Open Up Menu
                Game1.playSound("bigSelect");
                Game1.activeClickableMenu = new JoeMenu(1100, 548, this);
            }
            else if (args.Button == Conf.ButtonToggleBlackList)
            {
                Util.ToggleBlacklistUnderCursor();
            }
        }

        private void OnPostRenderHud(object sender, EventArgs args)
        {
            if (Game1.currentLocation is MineShaft shaft && Conf.MineInfoGui)
            {
                Util.DrawMineGui(Game1.spriteBatch, Game1.smallFont, Game1.player, shaft);
            }
            if (Context.IsPlayerFree && !string.IsNullOrEmpty(_hoverText) && Game1.player.CurrentItem != null)
            {
                Util.DrawSimpleTextbox(Game1.spriteBatch, _hoverText, Game1.dialogueFont, this, false, _unableToGift ? null : Game1.player.CurrentItem);
            }
            if (Conf.FishingProbabilitiesInfo && Game1.player.CurrentTool is FishingRod rod && rod.isFishing)
            {
                Util.PrintFishingInfo(rod);
            }
            if (_paused && Conf.PauseWhenIdle)
            {
                Util.DrawPausedHud();
            }
        }
        
        private void OnPostRenderGui(object sender, EventArgs args)
        {
            if (Game1.activeClickableMenu is BobberBar bar)
            {
                if (Conf.FishingInfo)
                {
                    Util.DrawFishingInfoBox(Game1.spriteBatch, bar, Game1.dialogueFont);
                }
                if (Conf.AutoFishing)
                {
                    Util.AutoFishing(bar);
                }
            }
            if (Conf.EstimateShippingPrice && Game1.activeClickableMenu is ItemGrabMenu menu)
            {
                Util.DrawShippingPrice(menu, Game1.smallFont);
            }
        }

        private void OnBeforeSave(object sender, EventArgs args)
        {
            if(!Context.IsWorldReady || !Conf.AutoAnimalDoor)
            {
                return;
            }
            Util.LetAnimalsInHome();
            Farm farm = Game1.getFarm();
            foreach (Building building in farm.buildings)
            {
                switch (building)
                {
                    case Coop coop:
                    {
                        if (coop.indoors.Value is AnimalHouse house)
                        {
                            if (house.animals.Any() && coop.animalDoorOpen.Value)
                            {
                                coop.animalDoorOpen.Value = false;
                                Helper.Reflection.GetField<NetInt>(coop, "animalDoorMotion").SetValue(new NetInt(2));
                            }
                        }

                        break;
                    }
                    case Barn barn:
                    {
                        if (barn.indoors.Value is AnimalHouse house)
                        {
                            if (house.animals.Any() && barn.animalDoorOpen.Value)
                            {
                                barn.animalDoorOpen.Value = false;
                                Helper.Reflection.GetField<NetInt>(barn, "animalDoorMotion").SetValue(new NetInt(2));
                            }
                        }

                        break;
                    }
                }
            }
        }

        private void OnDayStarted(object sender, EventArgs args)
        {
            //Reset LastTimeOfDay
            LastTimeOfDay = Game1.timeOfDay;

            if (!Context.IsWorldReady || !Conf.AutoAnimalDoor)
            {
                return;
            }
            _dayEnded = false;
            if (Game1.isRaining || Game1.isSnowing)
            {
                Monitor.Log("Don't open the animal door because of rainy/snowy weather.");
                return;
            }
            if(Game1.IsWinter)
            {
                Monitor.Log("Don't open the animal door because it's winter");
                return;
            }
            Farm farm = Game1.getFarm();
            foreach (Building building in farm.buildings)
            {
                switch (building)
                {
                    case Coop coop:
                    {
                        if (coop.indoors.Value is AnimalHouse house)
                        {
                            if (house.animals.Any() && !coop.animalDoorOpen.Value)
                            {
                                Monitor.Log($"Opening coop door @[{coop.animalDoor.X},{coop.animalDoor.Y}]", LogLevel.Trace);
                                coop.animalDoorOpen.Value = true;
                                Helper.Reflection.GetField<NetInt>(coop, "animalDoorMotion").SetValue(new NetInt(-2));
                            }
                        }
                        break;
                    }
                    case Barn barn:
                    {
                        if (barn.indoors.Value is AnimalHouse house)
                        {
                            if (house.animals.Any() && !barn.animalDoorOpen.Value)
                            {
                                Monitor.Log($"Opening barn door @[{barn.animalDoor.X},{barn.animalDoor.Y}]", LogLevel.Trace);
                                barn.animalDoorOpen.Value = true;
                                Helper.Reflection.GetField<NetInt>(barn, "animalDoorMotion").SetValue(new NetInt(-3));
                            }
                        }
                        break;
                    }
                }
            }
        }

        public void WriteConfig()
        {
            Helper.WriteConfig(Conf);
        }
    }
}
