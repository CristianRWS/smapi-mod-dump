﻿using StardewModdingAPI;
using Harmony;
using StardewValley;
using StardewValley.Menus;
using System;

namespace PlatonicRelationships
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Content.AssetEditors.Add(new AddDatingPrereq());

            //apply harmony patches
            ApplyPatches();
        }

        public void ApplyPatches()
        {
            var harmony = HarmonyInstance.Create("cherry.platonicrelationships");

            try
            {
                this.Monitor.Log("Transpile patching SocialPage.drawNPCSlot", StardewModdingAPI.LogLevel.Debug);
                harmony.Patch(
                    original: AccessTools.Method(typeof(SocialPage), name: "drawNPCSlot"),
                    transpiler: new HarmonyMethod(type: typeof(PatchDrawNPCSlot), nameof(PatchDrawNPCSlot.Transpiler))
                );
            }
            catch (Exception e)
            {
                Monitor.Log($"Failed in Patching SocialPage.drawNPCSlot: \n{e}", LogLevel.Error);
                return;
            }

            try
            {
                this.Monitor.Log("Postfix patching Utility.GetMaximumHeartsForCharacter", StardewModdingAPI.LogLevel.Debug);
                harmony.Patch(
                    original: AccessTools.Method(typeof(Utility), name: "GetMaximumHeartsForCharacter"),
                    postfix: new HarmonyMethod(typeof(patchGetMaximumHeartsForCharacter), nameof(patchGetMaximumHeartsForCharacter.Postfix))
                );
            }
            catch (Exception e)
            {
                Monitor.Log($"Failed in Patching Utility.GetMaximumHeartsForCharacter: \n{e}", LogLevel.Error);
                return;
            }
        }
    }
}