using BBP_Playables.Extra.Foxo;
using HarmonyLib;
using MTM101BaldAPI;
using TeacherAPI;

namespace BBP_Playables.Extra.Patches;

[ConditionalPatchMod("alexbw145.baldiplus.teacherapi"), HarmonyPatch]
class FoxoTeacherAPIManualPatches
{
    static InsanityModifier baldiAura = new InsanityModifier(-15.55f); // -5.55f
    static InsanityModifier foxoAura = new InsanityModifier(-99f);
    [HarmonyPatch(typeof(Teacher), "ActivateSpoopMode"), HarmonyPostfix]
    static void AuraOfInsane(Teacher __instance, ref bool ___tutorialMode)
    {
        if (___tutorialMode) return;
        var aura = __instance.gameObject.AddComponent<InsanityAura>();
        aura.radius = 90f;
        aura.lookOnly = true;
        aura.modifier = __instance.Character == FoxoPlayablePlugin.Foxo.Character ? foxoAura : baldiAura;
        /*foreach (var fox in GameObject.FindObjectsOfType<InsanityComponent>(false))
            if ((__instance.transform.position - fox.transform.position).magnitude < 90f && !fox.modifiers.Contains(baldiAura))
                fox.modifiers.Add(baldiAura);
            else if (fox.modifiers.Contains(baldiAura))
                fox.modifiers.Remove(baldiAura);*/
    }

    internal static bool IsTeacher(Baldi baldi) => baldi is Teacher;
}
