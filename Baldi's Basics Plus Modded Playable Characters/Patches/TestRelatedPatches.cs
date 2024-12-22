using BBP_Playables.Core;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Reflection;
using System.Collections.Generic;
using System.Linq;
using TestVariants;
using UnityEngine;

namespace BBP_Playables.Modded.Patches
{
    [ConditionalPatchMod("txv.bbplus.testvariants"), HarmonyPatch]
    class TestVariantsPatches
        /*
         * Characters that has no changes for an interaction with Test Subject:
         * Question Shelfman
         * Yestholomew
         * Me Mest
         * Testerly
         * AJOPI9
         * TestHolder9
         * Cren
         */
    {
        [HarmonyPatch(typeof(ThroneTest_Wander), nameof(ThroneTest_Wander.OnStateTriggerEnter)), HarmonyPrefix]
        static bool IgnoreSubjectMLG(Collider other, ThroneTest_Wander __instance, ref NPC ___npc, ref int ___combo, ref float ___combotimer)
        {
            if (!(other?.GetComponent<PlrPlayableCharacterVars>()?.GetCurrentPlayable().name.ToLower().Replace(" ", "") == "thetestsubject" & other.GetComponent<PlayerEntity>() != null & ___npc.Navigator.speed >= 20f))
                return true;

            Entity component = other.GetComponent<Entity>();
            ThroneTest mlg = ___npc as ThroneTest;

            GameObject gameObject = new GameObject();
            gameObject.transform.position = ___npc.transform.position + ___npc.transform.forward * 3.5f;
            SpriteRenderer spriteRenderer = ThroneTest_Wander.CreateSpriteRender("Zap!", true, gameObject.transform);
            spriteRenderer.sprite = TestPlugin.Instance.assetMan.Get<Sprite>("ThroneTest_Hitmarker");
            spriteRenderer.transform.localScale *= 25f;
            GameObject.Destroy(gameObject, 1f);
            component.AddForce(new Force((other.transform.position - ___npc.transform.position).normalized * 2f, 80f, -60f));
            if (___combo < 5)
            {
                if (___combo > 0)
                {
                    mlg.SayLine(4, ___combo);
                    if (___combo == 4)
                        mlg.SayLine(5, 0);
                }
                else
                    mlg.SayLine(2, 0);
            }
            else if (___combo == 5)
            {
                mlg.Shutit();
                mlg.SayLine(6, 0);
            }
            else
                mlg.SayLine(4, UnityEngine.Random.Range(1, 5));

            mlg.SayLine(1, UnityEngine.Random.Range(1, 5));
            ___combotimer = ___combotimer < 5f ? 5f : ___combotimer;

            return false;
        }

        [HarmonyPatch(typeof(TheBestOne_Wander), nameof(TheBestOne_Wander.PlayerSighted))]
        [HarmonyPatch(typeof(TheBestOne_Wander), nameof(TheBestOne_Wander.PlayerInSight))]
        [HarmonyPrefix]
        static bool AvoidYellowing(PlayerManager player) => player.GetComponent<PlrPlayableCharacterVars>().GetCurrentPlayable().name.ToLower().Replace(" ", "") != "thetestsubject";

        [HarmonyPatch(typeof(Bluxam), nameof(Bluxam.HelpPlayer)), HarmonyPrefix]
        static bool ToNoteboos(PlayerManager player, Bluxam __instance)
        {
            if (player.GetComponent<PlrPlayableCharacterVars>().GetCurrentPlayable().name.ToLower().Replace(" ", "") == "thetestsubject")
            {
                List<Vector3> list = new List<Vector3>();
                foreach (RoomController room in __instance.ec.rooms.FindAll(x => x.category == RoomCategory.Class))
                {
                    if (room.ec.activities.Find(x => x.room == room && !x.IsCompleted && !x.InBonusMode && !x.gameObject.name.ToLower().Contains("NoActivity".ToLower())) || (room.objectObject.gameObject.GetComponentInChildren<Notebook>(false) != null && room.objectObject.gameObject.GetComponentInChildren<Notebook>(false).GetComponentInChildren<SpriteRenderer>(true).gameObject.activeSelf))
                        list.Add(room.doors[0].aTile.CenterWorldPosition);
                }

                if (list.Count > 0)
                {
                    Vector3 vector = list[Random.Range(0, list.Count)];
                    player.Teleport(vector);
                    __instance.transform.position = vector;
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Bluxam_Wander), nameof(Bluxam_Wander.PlayerInSight)), HarmonyPrefix]
        static bool IGuessCheck(PlayerManager player, ref Bluxam ___npc)
        {
            if (player.GetComponent<PlrPlayableCharacterVars>().GetCurrentPlayable().name.ToLower().Replace(" ", "") != "thetestsubject")
                return true;

            bool flag = ___npc.ec.CellFromPosition(player.transform.position).room.category != RoomCategory.Class && (___npc.ec.activities.Count(x => x.room.category == RoomCategory.Class && !x.IsCompleted && !x.InBonusMode && !x.gameObject.name.ToLower().Contains("NoActivity".ToLower())) + GameObject.FindObjectsOfType<Notebook>(false).Count(x => x.gameObject.GetComponentInChildren<SpriteRenderer>(true).gameObject.activeSelf)) > 0;
            if (flag && !player.Tagged)
            {
                ___npc.behaviorStateMachine.ChangeState(new Bluxam_Chase(___npc));
                ___npc.SayTheLine(1);
            }
            return false;
        }

        [HarmonyPatch(typeof(Qrid_Wander), nameof(Qrid_Wander.Update)), HarmonyPostfix]
        static void CoolestFriend(Qrid_Wander __instance, ref Qrid ___npc)
        {
            if (CoreGameManager.Instance.GetPlayer(0).gameObject.GetComponent<PlrPlayableCharacterVars>().GetCurrentPlayable().name.ToLower().Replace(" ", "") == "thetestsubject"
                && ___npc.looker.PlayerInSight(___npc.ec.Players[0])
                && !___npc.ec.Players[0].Tagged && !___npc.onCooldown)
            {
                ___npc.Navigator.SetSpeed(34f);
                ___npc.Navigator.maxSpeed = 37f;
                __instance.ChangeNavigationState(new NavigationState_TargetPlayer(___npc, 2, ___npc.ec.Players[0].transform.position));
            }
            else if (__instance.CurrentNavigationState.GetType().Equals(typeof(NavigationState_TargetPlayer)) && !___npc.Navigator.HasDestination)
                __instance.ChangeNavigationState(new NavigationState_WanderRandom(___npc, 0));
        }

        [HarmonyPatch(typeof(Qrid_Friendly), nameof(Qrid_Friendly.Update)), HarmonyPrefix]
        static bool QridMoreFriendly(Qrid_Friendly __instance, ref Qrid ___npc)
        {
            if (CoreGameManager.Instance.GetPlayer(0).gameObject.GetComponent<PlrPlayableCharacterVars>().GetCurrentPlayable().name.ToLower().Replace(" ", "") != "thetestsubject")
                return true;

            __instance.CurrentNavigationState.UpdatePosition(___npc.ec.Players[0].transform.position);
            if ((___npc.ec.Players[0].transform.position - ___npc.transform.position).magnitude <= 20f)
            {
                ___npc.Navigator.SetSpeed(0f);
                ___npc.Navigator.maxSpeed = 0f;
                return false;
            }

            ___npc.Navigator.SetSpeed(19f);
            ___npc.Navigator.maxSpeed = 22f;

            foreach (NPC npc in ___npc.ec.Npcs)
                if (((npc.gameObject != ___npc.gameObject) & (bool)npc.gameObject.GetComponent<ActivityModifier>()) && (npc.transform.position - ___npc.transform.position).magnitude <= 20f)
                {
                    ___npc.ImMadAtNPCNow(npc);
                    return false;
                }
            if ((___npc.ec.Players[0].transform.position - ___npc.transform.position).magnitude >= 132f && !___npc.looker.PlayerInSight(___npc.ec.Players[0]))
                ___npc.ImMadNow();
            return false;
        }
        [HarmonyPatch(typeof(Gummin_Wander), nameof(Gummin_Wander.PlayerInSight))]
        [HarmonyPatch(typeof(Gummin_Wander), nameof(Gummin_Wander.PlayerSighted))]
        [HarmonyPrefix]
        static bool GumminNo(PlayerManager player) => player.gameObject.GetComponent<PlrPlayableCharacterVars>().GetCurrentPlayable().name.ToLower().Replace(" ", "") != "thetestsubject";
        [HarmonyPatch(typeof(Plit_Maul), nameof(Plit_Maul.Enter)), HarmonyPostfix]
        static void PainLess(ref Plit ___npc, ref float ___maultime)
        {
            if (___npc.mauling.gameObject?.GetComponent<PlrPlayableCharacterVars>()?.GetCurrentPlayable()?.name.ToLower().Replace(" ", "") != "thetestsubject") return;
            ___maultime = 3f;
        }
        [HarmonyPatch(typeof(PalbyFan_Show), nameof(PalbyFan_Show.Enter)), HarmonyPostfix]
        static void IPlead(ref float ___timr)
        {
            if (CoreGameManager.Instance.GetPlayer(0).gameObject.GetComponent<PlrPlayableCharacterVars>().GetCurrentPlayable().name.ToLower().Replace(" ", "") != "thetestsubject") return;
            ___timr += 5f;
        }
    }
}
