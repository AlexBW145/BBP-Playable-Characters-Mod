using BBP_Playables.Core;
using BBP_Playables.Extra.Foxo;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Audio;
using static UnityEngine.GraphicsBuffer;

namespace BBP_Playables.Extra.Patches
{
    [HarmonyPatch(typeof(MainMenu), "Start")]
    class FoxoRestartFixes
    {
        private static FieldInfo _speed = AccessTools.Field(typeof(MusicManager), "speed");
        static void Postfix()
        {
            Resources.FindObjectsOfTypeAll<AudioMixer>().ToList().Find(x => x.name == "Master").SetFloat("EchoWetMix", 0f);
            MusicManager.Instance.MidiPlayer.CoreAudioSource.pitch = 1f;
            MusicManager.Instance.MidiPlayer.MPTK_Speed = (float)_speed.GetValue(MusicManager.Instance);
        }
    }

    [ConditionalPatchMod("alexbw145.baldiplus.teacherapi"), HarmonyPatch]
    class FoxoTeacherAPIManualPatches
    {
        static InsanityModifier baldiAura = new InsanityModifier(-15.55f); // -5.55f
        static InsanityModifier foxoAura = new InsanityModifier(-99f);
        [HarmonyPatch("TeacherAPI.Teacher, TeacherAPI", "ActivateSpoopMode"), HarmonyPostfix]
        static void AuraOfInsane(Baldi __instance, ref bool ___tutorialMode)
        {
            if (___tutorialMode || __instance is WrathFoxo) return;
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
    }

    [HarmonyPatch]
    class FoxoCharacterPatches
    {
        [HarmonyPatch(typeof(Looker), nameof(Looker.Raycast), [typeof(Transform), typeof(float), typeof(PlayerManager), typeof(LayerMask), typeof(bool)],
            [ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out]), HarmonyPrefix]
        static bool CannotSee(Transform target, float rayDistance, PlayerManager player, LayerMask mask, out bool targetSighted)
        {
            targetSighted = false;
            return target.GetComponent<WrathFoxo>() == null;
        }

        [HarmonyPatch(typeof(Entity), nameof(Entity.CollisionValid)), HarmonyPostfix]
        static void CannotTouch(Entity otherEntity, ref bool __result)
        {
            if (__result == true && otherEntity.GetComponent<WrathFoxo>() != null)
                __result = false;
        }

        static InsanityModifier lightsoutAura = new InsanityModifier(-50f);
        [HarmonyPatch(typeof(PlayerManager), "Update"), HarmonyPostfix]
        static void PhobiaOfDarkness(PlayerManager __instance, ref bool ___hiddenByLight)
        {
            var sanity = __instance.gameObject.GetComponent<InsanityComponent>();
            if (sanity != null)
            {
                if (___hiddenByLight && !sanity.modifiers.Contains(lightsoutAura))
                    sanity.modifiers.Add(lightsoutAura);
                else if (sanity.modifiers.Contains(lightsoutAura))
                    sanity.modifiers.Remove(lightsoutAura);
            }
        }
        [HarmonyPatch(typeof(TimeOut), nameof(TimeOut.Begin)), HarmonyPostfix]
        static void AfterHoursRandomlyGo()
        {
            foreach (var fox in GameObject.FindObjectsOfType<InsanityComponent>(false))
                fox.modifiers.Add(new InsanityModifier(-5f));
        }

        static InsanityModifier baldiAura = new InsanityModifier(-15.55f); // -5.55f
        [HarmonyPatch(typeof(Baldi), nameof(Baldi.Initialize)), HarmonyPostfix]
        static void AuraOfInsane(Baldi __instance, ref bool ___tutorialMode)
        {
            if (___tutorialMode || __instance is WrathFoxo) return;
            var aura = __instance.gameObject.AddComponent<InsanityAura>();
            aura.radius = 90f;
            aura.lookOnly = true;
            aura.modifier = baldiAura;
            /*foreach (var fox in GameObject.FindObjectsOfType<InsanityComponent>(false))
                if ((__instance.transform.position - fox.transform.position).magnitude < 90f && !fox.modifiers.Contains(baldiAura))
                    fox.modifiers.Add(baldiAura);
                else if (fox.modifiers.Contains(baldiAura))
                    fox.modifiers.Remove(baldiAura);*/
        }

        static InsanityModifier reflexAura = new InsanityModifier(-60f); // -6f
        [HarmonyPatch(typeof(DrReflex_Hunting), nameof(DrReflex_Hunting.Enter)), HarmonyPostfix]
        static void AuraOfDoctor(DrReflex_Hunting __instance)
        {
            var aura = __instance.Npc.gameObject.GetOrAddComponent<InsanityAura>();
            aura.radius = 90f;
            aura.modifier = reflexAura;
        }
        [HarmonyPatch(typeof(DrReflex), "RapidHammer"), HarmonyPostfix]
        static IEnumerator RemoveAuraDoctorFix(IEnumerator result, bool final, DrReflex __instance)
        {
            while (result.MoveNext())
                yield return result.Current;
            if (final)
                GameObject.Destroy(__instance.gameObject.GetComponent<InsanityAura>());
        }

        static InsanityModifier pompAura = new InsanityModifier(-499.99999f); // -8.99999f
        [HarmonyPatch(typeof(NoLateTeacher), nameof(NoLateTeacher.Attack)), HarmonyPostfix]
        static void MyEars(NoLateTeacher __instance)
        {
            var aura = __instance.gameObject.AddComponent<InsanityAura>();
            aura.radius = 39f;
            aura.modifier = pompAura;
        }
        [HarmonyPatch(typeof(NoLateTeacher), nameof(NoLateTeacher.Dismiss)), HarmonyPostfix]
        static void OhUnexpected(NoLateTeacher __instance)
        {
            if (__instance.gameObject.GetComponent<InsanityAura>() != null)
                GameObject.Destroy(__instance.gameObject.GetComponent<InsanityAura>());
        }

        static InsanityModifier jumpropeAura = new InsanityModifier(20f, 10);
        [HarmonyPatch(typeof(Jumprope), "Start"), HarmonyPostfix]
        static void AuraOfAlr(Jumprope __instance)
        {
            if (__instance.player.gameObject.GetComponent<InsanityComponent>() != null)
                __instance.player.gameObject.GetComponent<InsanityComponent>().modifiers.Add(jumpropeAura);
        }
        [HarmonyPatch(typeof(Jumprope), "Destroy"), HarmonyPrefix]
        static void RemoveAuraOfAlr(Jumprope __instance)
        {
            if (__instance.player.gameObject.GetComponent<InsanityComponent>() != null)
                __instance.player.gameObject.GetComponent<InsanityComponent>().modifiers.Remove(jumpropeAura);
        }
        [HarmonyPatch(typeof(Jumprope), "RopeDown"), HarmonyPostfix]
        static void Alr(Jumprope __instance, ref float ___height, ref float ___jumpBuffer)
        {
            if (___height > ___jumpBuffer && __instance.player.gameObject.GetComponent<InsanityComponent>() != null)
                __instance.player.gameObject.GetComponent<InsanityComponent>().Anxiety += 5f;
        }

        static InsanityModifier testblindAura = new InsanityModifier(-10f);
        [HarmonyPatch(typeof(LookAtGuy), nameof(LookAtGuy.Blind)), HarmonyPostfix]
        static void Shit()
        {
            var fox = CoreGameManager.Instance.GetPlayer(0).GetComponent<InsanityComponent>();
            if (fox != null)
                fox.modifiers.Add(testblindAura);
        }
        [HarmonyPatch(typeof(LookAtGuy), nameof(LookAtGuy.Respawn)), HarmonyPostfix]
        static void Phew()
        {
            var fox = CoreGameManager.Instance.GetPlayer(0).GetComponent<InsanityComponent>();
            if (fox != null)
                fox.modifiers.Remove(testblindAura);
        }

        static InsanityModifier craftersAura = new InsanityModifier(-100f);
        [HarmonyPatch(typeof(ArtsAndCrafters), nameof(ArtsAndCrafters.Attack)), HarmonyPostfix]
        static void WOOSHAura(ArtsAndCrafters __instance)
        {
            var aura = __instance.gameObject.GetOrAddComponent<InsanityAura>();
            aura.radius = 555f;
            aura.modifier = craftersAura;
        }

        static InsanityModifier facultyAura = new InsanityModifier(-5f, 2);
        [HarmonyPatch(typeof(RoomFunction), nameof(RoomFunction.OnPlayerEnter)), HarmonyPostfix]
        static void AddAuraOfGuilt(RoomFunction __instance, PlayerManager player)
        {
            if (!__instance.GetType().Equals(typeof(FacultyTrigger))) return;
            var sanity = player.GetComponent<InsanityComponent>();
            if (sanity != null) sanity.modifiers.Add(facultyAura);
        }
        [HarmonyPatch(typeof(FacultyTrigger), nameof(FacultyTrigger.OnPlayerExit)), HarmonyPostfix]
        static void RemoveAuraOfGuilt(PlayerManager player)
        {
            var sanity = player.GetComponent<InsanityComponent>();
            if (sanity != null) sanity.modifiers.Remove(facultyAura);
        }
        static InsanityModifier detentionroomAura = new InsanityModifier(-5f, 9);
        [HarmonyPatch(typeof(DetentionRoomFunction), nameof(DetentionRoomFunction.OnPlayerExit)), HarmonyPostfix]
        static void NoWaiting(PlayerManager player)
        {
            var fox = player.GetComponent<InsanityComponent>();
            if (fox != null) // Removes even when inactive.
                fox.modifiers.Remove(detentionroomAura);
        }
        [HarmonyPatch(typeof(Principal), nameof(Principal.SendToDetention)), HarmonyPostfix]
        static void Offyougo(Principal __instance, ref PlayerManager ___targetedPlayer)
        {
            if (__instance.ec.offices.Count > 0)
            {
                var fox = ___targetedPlayer.GetComponent<InsanityComponent>();
                if (fox != null && !fox.modifiers.Contains(detentionroomAura))
                    fox.modifiers.Add(detentionroomAura);
            }
        }

        [HarmonyPatch(typeof(ItemManager), nameof(ItemManager.UseItem)), HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Satisfood(IEnumerable<CodeInstruction> instructions) => new CodeMatcher(instructions).Start()
                .MatchForward(false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ItemManager), nameof(ItemManager.selectedItem))),
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(ItemManager), nameof(ItemManager.RemoveItem)))
                )
                .Insert(new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemManager), nameof(ItemManager.pm))),
                Transpilers.EmitDelegate<Action<PlayerManager>>((__instance) =>
                {
                    var meta = __instance.itm.items[__instance.itm.selectedItem].GetMeta();
                    if ((meta.tags.Contains("food") && !meta.flags.HasFlag(ItemFlags.CreatesEntity)) || meta.tags.Contains("drink"))
                    {
                        InsanityComponent sane = __instance.gameObject.GetComponent<InsanityComponent>();
                        if (sane != null)
                        {
                            float consum = meta.tags.Contains("drink") ? -2f : 10f; // Sodas can be devastating...
                            if (meta.tags.ToList().Exists(x => x.StartsWith("playablechars_sanityconsumable_")))
                                consum = float.Parse(meta.tags.ToList().Find(x => x.StartsWith("playablechars_sanityconsumable_")).Remove(0, "playablechars_sanityconsumable_".Length), CultureInfo.InvariantCulture.NumberFormat);
                            sane.Anxiety += consum;
                        }
                    }
                }))
                .InstructionEnumeration();
    }
}
