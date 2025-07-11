using MTM101BaldAPI.PlusExtensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using UnityEngine;

namespace BBP_Playables.Core
{
    public class TestSubjectMan : PlayableCharacterComponent
    {
        private HashSet<GameObject> spottedNPC = new HashSet<GameObject>();
        internal TimeScaleModifier subjectScale = new TimeScaleModifier();
        bool doingyour = false;
        private PlayerMovementStatModifier stats;
        public override void Initialize()
        {
            base.Initialize();
            stats = pm.GetMovementStatModifier();
        }
        public override void SpoopBegin(BaseGameManager manager) => doingyour = true;
        void Update()
        {
            if (!doingyour) return;
            foreach (NPC npc in pm.ec.Npcs)
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
            stats.ChangeBaseStat("walkSpeed", Mathf.Max(0f, stats.baseStats["runSpeed"] - (Mathf.Max(spottedNPC.Count, 1) / (Mathf.Max(pm.ec.Npcs.Count + 1, 2) % stats.baseStats["runSpeed"]) * stats.baseStats["runSpeed"])));
        }

        public override void GameBegin(BaseGameManager manager) => manager.Ec.AddTimeScale(subjectScale);
        internal void MessUpAndIncreaseTimeScale() => subjectScale.npcTimeScale += 0.2f;
    }
}
