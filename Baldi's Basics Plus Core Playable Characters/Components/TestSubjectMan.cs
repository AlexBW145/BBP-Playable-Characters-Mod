using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.PlusExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BBP_Playables.Core
{
    public class TestSubjectMan : PlayableCharacterComponent
    {
        private readonly HashSet<GameObject> spottedNPC = new HashSet<GameObject>();
        internal TimeScaleModifier 
            subjectScale = new TimeScaleModifier(),
            benderTime = new TimeScaleModifier();
        private bool doingyour = false;
        private PlayerMovementStatModifier stats;
        private readonly Sticker 
            bender = EnumExtensions.GetFromExtendedName<Sticker>("TestSubjectTimeBender"),
            destabilizer = EnumExtensions.GetFromExtendedName<Sticker>("TestSubjectPenalty");
        private static FieldInfo timeScales = AccessTools.DeclaredField(typeof(EnvironmentController), "timeScaleModifiers");
        public override void Initialize()
        {
            base.Initialize();
            stats = pm.GetMovementStatModifier();
        }
        public override void SpoopBegin(BaseGameManager manager) => doingyour = true;
        void Update()
        {
            if (!doingyour) return;
            var npccount = pm.ec.Npcs.Where(n => n.Character != Character.Null).ToList(); // They are not one of them...
            foreach (NPC npc in npccount)
            {
                if (npc == null || npc.looker == null) continue;

                if (npc.looker.PlayerInSight())
                    spottedNPC.Add(npc.gameObject);
                else
                    spottedNPC.Remove(npc.gameObject);
            }
            if (spottedNPC.Contains(null)) // Mod conflict with BBT or if NPCs get deleted on purpose
                spottedNPC.RemoveWhere(n => n.gameObject == null);
            // 30 is default, came from the ScriptableObject's runSpeed.
            stats.ChangeBaseStat("walkSpeed", Mathf.Max(0f, Mathf.Max(3f, stats.baseStats["runSpeed"]) - (Mathf.Max(spottedNPC.Count, 1) / (Mathf.Max(npccount.Count + 1, 2) % Mathf.Max(3f, stats.baseStats["runSpeed"])) * Mathf.Max(3f, stats.baseStats["runSpeed"]))));

            var timeScaleModifiers = timeScales.GetValue(pm.ec) as List<TimeScaleModifier>;
            if (StickerManager.Instance.StickerValue(bender) > 0)
            {
                if (!timeScaleModifiers.Contains(benderTime))
                    pm.ec.AddTimeScale(benderTime);
                var npcsInCurrentRoom = pm.ec.Npcs.Count(npc => npc.Navigator?.Entity.CurrentRoom == pm.plm.Entity.CurrentRoom && npc.Character != Character.Null);
                var npcCountNoStudents = pm.ec.Npcs.Count(npc => npc.Character != Character.Null);
                float a = ((float)npcsInCurrentRoom + (float)npcCountNoStudents) / (float)npcCountNoStudents - 1;
                float b = ((float)spottedNPC.Count / (float)(pm.ec.Npcs.Count + 1));
                benderTime.npcTimeScale = Mathf.Max(0.5f, Mathf.Min(Mathf.Abs(a - b), 1f));
                benderTime.environmentTimeScale = Mathf.Max(0.5f, Mathf.Min(Mathf.Abs(a - b), 1f));
            }
            else if (timeScaleModifiers.Contains(benderTime))
                pm.ec.RemoveTimeScale(benderTime);
        }

        public override void GameBegin(BaseGameManager manager) => manager.Ec.AddTimeScale(subjectScale);
        internal void MessUpAndIncreaseTimeScale() => subjectScale.npcTimeScale += 0.2f - (0.05f * StickerManager.Instance.StickerValue(destabilizer));
    }
}
