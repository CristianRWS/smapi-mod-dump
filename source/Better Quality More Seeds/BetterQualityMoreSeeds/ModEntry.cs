<<<<<<< HEAD
﻿using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Objects;
using StardewValley.Network;

namespace SB_BQMS
{

    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        Dictionary<StardewValley.Object, AllSeedMakerValueContainer> allSeedMakers;
        Dictionary<StardewValley.Object, AllChestsValueContainer> allChests;
        GameLocation previousLocation;
        bool isInitialized;
        bool hasAutomate;

        public StardewValley.Object PreviousHeldItem { get; set; }
        public Dictionary<StardewValley.Object, AllChestsValueContainer> AllChests { get => allChests; set => allChests = value; }

        /*********
** Public methods
*********/
        /// <summary>Initialise the mod.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            if (Helper.ModRegistry.IsLoaded("Pathoschild.Automate")) hasAutomate = true; else hasAutomate = false;

            Helper.Events.GameLoop.SaveLoaded += InitializeMod;
            Helper.Events.GameLoop.ReturnedToTitle += ResetMod;
            Helper.Events.GameLoop.UpdateTicked += ModUpdate;
            Helper.Events.World.ObjectListChanged += PopulateSeedmakers;
            allSeedMakers = new Dictionary<StardewValley.Object, AllSeedMakerValueContainer>();
            if (hasAutomate) AllChests = new Dictionary<StardewValley.Object, AllChestsValueContainer>();
            isInitialized = false;
        }

        //  For Automate  We now need to get all seed makers available.
        //  So making this a method on its own
        private void PopulateSeedmakersBase()
        {
            allSeedMakers.Clear();
            if (hasAutomate) AllChests.Clear();

            for (int i = 0; i < Game1.locations.Count; i++)
            {
                GameLocation location = Game1.locations[i];
                OverlaidDictionary allObjects = location.objects;
                for (var j = allObjects.Values.GetEnumerator(); j.MoveNext();)
=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace BetterQualityMoreSeeds
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private Dictionary<SObject, AllSeedMakerValueContainer> allSeedMakers;
        private Dictionary<SObject, AllChestsValueContainer> allChests;

        private SObject previousHeldItem;

        private bool isInitialized;
        private bool hasAutomate;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            hasAutomate = Helper.ModRegistry.IsLoaded("Pathoschild.Automate");

            Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            Helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            Helper.Events.World.ObjectListChanged += OnObjectListChanged;
            allSeedMakers = new Dictionary<SObject, AllSeedMakerValueContainer>();
            if (hasAutomate)
                allChests = new Dictionary<SObject, AllChestsValueContainer>();
            isInitialized = false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after objects are added or removed in a location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            populateSeedmakersBase();
        }

        /// <summary>Raised after the player loads a save slot and the world is initialised.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            //Initialize the Mod. Add all Seed Makers in the Game.
            //TODO: Add the allSeedMakers population code here.
            //TODO: add a LocationObjectsChanged method to check if the allSeedMakers variable needs to be updated
            //Note: The Method has been designed to do this since alpha! LOL
            populateSeedmakersBase();
            isInitialized = true;
        }

        /// <summary>Raised after the game returns to the title screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnReturnedToTitle(object sender, EventArgs e)
        {
            isInitialized = false;
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnUpdateTicked(object sender, EventArgs e)
        {
            if (isInitialized)
            {
                foreach (SObject seedMaker in allSeedMakers.Keys.ToArray())
                {
                    AllSeedMakerValueContainer container = allSeedMakers[seedMaker];
                    SObject heldObj = seedMaker.heldObject?.Value;

                    if (heldObj == null && container.hasBeenChecked)
                    {
                        container.droppedObject = null;
                        container.hasBeenChecked = false;
                    }
                    else if (heldObj != null && !container.hasBeenChecked && container.droppedObject == null)
                    {
                        SObject input = this.GetLastInput(seedMaker);
                        if (input != null)
                        {
                            container.droppedObject = input;
                            heldObj.Stack = Math.Min(heldObj.maximumStackSize(), heldObj.Stack + (input.Quality == 4 ? input.Quality - 1 : input.Quality));
                        }
                        container.hasBeenChecked = true;
                    }
                }

                previousHeldItem = Game1.player.ActiveObject;
            }
        }

        /// <summary>Get the last input received for a seed maker.</summary>
        /// <param name="seedMaker">The seed maker to check.</param>
        private SObject GetLastInput(SObject seedMaker)
        {
            if (hasAutomate)
            {
                // This should invoke checkChests, which will scan the seed maker for adjacent chests,
                // Then checks if those chests inventory decreased.
                SObject input = checkChests(seedMaker);
                if (input != null)
                    return input;
            }

            //  Checks if the Farmer is in the same location as the seed maker
            //  This will save up cpu time for the ones that are not on location
            if (previousHeldItem != null && Game1.player.currentLocation == allSeedMakers[seedMaker].location)
                return previousHeldItem;

            return null;
        }

        //  For Automate  We now need to get all seed makers available.
        //  So making this a method on its own
        private void populateSeedmakersBase()
        {
            allSeedMakers.Clear();
            if (hasAutomate) allChests.Clear();

            foreach (GameLocation location in Game1.locations)
            {
                OverlaidDictionary allObjects = location.objects;
                foreach (SObject ob in allObjects.Values)
>>>>>>> cb341e8d6d0ef384e6f156d4b3044d17f306c5f7
                {
                    StardewValley.Object ob = j.Current;
                    if (ob.Equals(null)) return;
                    if (ob.name.Equals("Seed Maker"))
                    {
                        if (!allSeedMakers.ContainsKey(ob))
                            allSeedMakers.Add(ob, new AllSeedMakerValueContainer(null, location, true));
                    }
                    if (hasAutomate)
                    {
<<<<<<< HEAD
                        if ((ob is Chest) && (ob as Chest).playerChest.Value)
                        {
                            if (!AllChests.ContainsKey(ob))
                                AllChests.Add(ob, new AllChestsValueContainer(null, location, false));
                        }
                    }
                }
            }
            previousLocation = Game1.player.currentLocation;
        }

        private void PopulateSeedmakers(object sender, ObjectListChangedEventArgs e)
        {
            PopulateSeedmakersBase();
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Initializes the Mod. Adds all Seed Makers in the Game
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void InitializeMod(object sender, EventArgs e)
        {
            previousLocation = Game1.player.currentLocation;
            //TODO: Add the allSeedMakers population code here.
            //TODO: add a LocationObjectsChanged method to check if the allSeedMakers variable needs to be updated
            //Note: The Method has been designed to do this since alpha! LOL
            PopulateSeedmakersBase();
            isInitialized = true;
        }

        private void ResetMod(object sender, EventArgs e)
        {
            isInitialized = false;
        }

        private StardewValley.Object CheckChests(StardewValley.Object seedmaker)
        {
            OverlaidDictionary allObjectsinLocation = allSeedMakers[seedmaker].location.objects;
            OverlaidDictionary.KeysCollection keyCollection = allObjectsinLocation.Keys;
            foreach (Vector2 objectKey in keyCollection)
            {
                if (allObjectsinLocation[objectKey] is Chest && Vector2.Distance(objectKey, seedmaker.TileLocation) <= 1.0)
                {
                    Chest thisChest = (allObjectsinLocation[objectKey] as Chest);
                    Netcode.NetObjectList<Item> currentItems = thisChest.items;
                    for (int index = 0; index < currentItems.Count; index++)
                    {
                        if (currentItems[index] == null || currentItems[index].ParentSheetIndex == 433) continue;
                        {
                            Dictionary<int, string> dictionary = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Crops");
                            foreach (KeyValuePair<int, string> keyValuePair in dictionary)
                            {
                                if (Convert.ToInt32(keyValuePair.Value.Split('/')[3]) == currentItems[index].ParentSheetIndex)
                                {
                                    AllChests[thisChest].previousItem = currentItems[index];
                                    return currentItems[index] as StardewValley.Object;
                                }
                            }
                        }
                    }
                    if (AllChests[thisChest].previousItem != null)
                    {
                        Item returnItem = AllChests[thisChest].previousItem;
                        AllChests[thisChest].previousItem = null;
                        return returnItem as StardewValley.Object;
                    }
                }
            }
            return null;
        }

        private void ModUpdate(object sender, EventArgs e)
        {
            if (isInitialized)
            {
                List<StardewValley.Object> seedMakers = allSeedMakers.Keys.ToList();

                for (int i = 0; i < seedMakers.Count; i++)
                {
                    StardewValley.Object seedMaker = seedMakers[i];
                    if (seedMaker.heldObject.Equals(null))
                    {
                        if (allSeedMakers[seedMaker].hasBeenChecked.Equals(true))
                        {
                            allSeedMakers[seedMaker].droppedObject = null;
                            allSeedMakers[seedMaker].hasBeenChecked = false;
                        }
                        if (allSeedMakers[seedMaker].hasBeenChecked.Equals(false) && allSeedMakers[seedMaker].droppedObject == null)
                        {
                            if (hasAutomate)
                            {
                                // This should invoke checkChests, which will scan the seed maker for adjacent chests,
                                // Then checks if those chests inventory decreased.
                                StardewValley.Object droppedChestObject = CheckChests(seedMaker);
                                if (droppedChestObject != null)
                                {
                                    allSeedMakers[seedMaker].droppedObject = droppedChestObject;
                                    seedMaker.addToStack(allSeedMakers[seedMaker].droppedObject.Quality == 4 ? allSeedMakers[seedMaker].droppedObject.Quality - 1 : allSeedMakers[seedMaker].droppedObject.Quality);
                                    allSeedMakers[seedMaker].hasBeenChecked = true;
                                    continue;
                                }
                            }
                        }
                        //  Checks if the Farmer is in the same location as the seed maker
                        //  This will save up cpu time for the ones that are not on location
                        if (PreviousHeldItem != null && Game1.player.currentLocation == allSeedMakers[seedMaker].location)
                        {
                            allSeedMakers[seedMaker].droppedObject = PreviousHeldItem;
                            seedMaker.addToStack(allSeedMakers[seedMaker].droppedObject.Quality == 4 ? allSeedMakers[seedMaker].droppedObject.Quality - 1 : allSeedMakers[seedMaker].droppedObject.Quality);
                            allSeedMakers[seedMaker].hasBeenChecked = true;
                            continue;
                        }
                    }
                }
                PreviousHeldItem = Game1.player.ActiveObject;
            }
        }
    }
}
=======
                        if (ob is Chest chest && chest.playerChest.Value)
                        {
                            if (!allChests.ContainsKey(chest))
                                allChests.Add(chest, new AllChestsValueContainer(null, location, false));
                        }
                    }
                }
            }
        }

        private SObject checkChests(SObject seedmaker)
        {
            OverlaidDictionary allObjectsinLocation = allSeedMakers[seedmaker].location.objects;
            OverlaidDictionary.KeysCollection keyCollection = allObjectsinLocation.Keys;
            foreach (Vector2 objectKey in keyCollection)
            {
                if (allObjectsinLocation[objectKey] is Chest && Vector2.Distance(objectKey, seedmaker.TileLocation) <= 1.0)
                {
                    Chest thisChest = (Chest)allObjectsinLocation[objectKey];
                    Netcode.NetObjectList<Item> currentItems = thisChest.items;
                    foreach (Item item in currentItems)
                    {
                        if (item == null || item.ParentSheetIndex == 433)
                            continue;

                        Dictionary<int, string> dictionary = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Crops");
                        foreach (KeyValuePair<int, string> keyValuePair in dictionary)
                        {
                            if (Convert.ToInt32(keyValuePair.Value.Split('/')[3]) == item.ParentSheetIndex)
                            {
                                allChests[thisChest].previousItem = item;
                                return item as SObject;
                            }
                        }
                    }
                    if (allChests[thisChest].previousItem != null)
                    {
                        Item returnItem = allChests[thisChest].previousItem;
                        allChests[thisChest].previousItem = null;
                        return returnItem as SObject;
                    }
                }
            }
            return null;
        }
    }
}
>>>>>>> cb341e8d6d0ef384e6f156d4b3044d17f306c5f7
