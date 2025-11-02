using BBP_Playables.Core;
using CriminalPack;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace BBP_Playables.Modded.Patches;

[ConditionalPatchMod("mtm101.rulerp.baldiplus.criminalpackroot"), HarmonyPatch(typeof(ItemScanner), "OnTriggerEnter")]
class JustCriminallyPatch
{
    private static FieldInfo _playerItemCount = AccessTools.DeclaredField(typeof(ItemScanner), "playerItemCount");
    private static FieldInfo _foundContraband = AccessTools.DeclaredField(typeof(ItemScanner), "foundContraband");
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => new CodeMatcher(instructions)
        .End()
        .MatchBack(false,
        new CodeMatch(OpCodes.Ldarg_0),
        new CodeMatch(OpCodes.Ldloc_0),
        new CodeMatch(CodeInstruction.Call(typeof(ItemScanner), "ActivateForPlayer"))
        ).ThrowIfInvalid("That sucked...")
        .InsertAndAdvance(
        new CodeInstruction(OpCodes.Ldloc_0),
        new CodeInstruction(OpCodes.Ldarg_0),
        Transpilers.EmitDelegate<Action<PlayerManager, ItemScanner>>((pm, __instance) =>
        {
            var backpacker = pm.GetComponent<BackpackerBackpack>();
            if (backpacker != null)
            {
                int itemCount = (int)_playerItemCount.GetValue(__instance);
                bool backpackContrabanded = false;
                for (int i = 0; i < pm.itm.maxItem + 1; i++)
                {
                    ItemMetaData meta = backpacker.items[i].GetMeta();
                    if (meta != null && meta.id != Items.None)
                    {
                        itemCount++;
                        if (meta.tags.Contains("crmp_contraband"))
                            backpackContrabanded = true;
                    }
                }
                if (backpackContrabanded)
                {
                    List<Items> contrabands = (List<Items>)_foundContraband.GetValue(__instance);
                    contrabands.Add(pm.itm.items[pm.itm.maxItem].itemType);
                    _foundContraband.SetValue(__instance, contrabands);
                }
                _playerItemCount.SetValue(__instance, itemCount);
            }
        })
        )
        .InstructionEnumeration();
}
