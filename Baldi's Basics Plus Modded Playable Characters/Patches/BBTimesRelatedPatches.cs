using System;
using System.Collections.Generic;
using System.Text;
using BBP_Playables.Core;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.CustomContent.NPCs;
using HarmonyLib;
using MTM101BaldAPI;
using UnityEngine;

namespace BBP_Playables.Modded.Patches
{
    [ConditionalPatchMod("pixelguy.pixelmodding.baldiplus.bbextracontent"), HarmonyPatch(typeof(MagicObject), "EntityTriggerEnter")]
    class UnlockMagical
    {
        static void Postfix(Collider other, ref MagicalStudent ___student, ref bool ___leftStudent)
        {
            if (other.transform == ___student.transform && ___leftStudent)
                PlayableCharsPlugin.UnlockCharacter(Plugin.info, "Magical Student");
        }
    }
}
