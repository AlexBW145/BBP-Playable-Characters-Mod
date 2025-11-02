using MTM101BaldAPI.PlusExtensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BBP_Playables.Core
{
    public class TestSubjectMan : PlayableCharacterComponent
    {
        private readonly HashSet<GameObject> spottedNPC = new HashSet<GameObject>();
        internal TimeScaleModifier subjectScale = new TimeScaleModifier();
        private bool doingyour = false;
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
        }

        public override void GameBegin(BaseGameManager manager) => manager.Ec.AddTimeScale(subjectScale);
        internal void MessUpAndIncreaseTimeScale() => subjectScale.npcTimeScale += 0.2f;
    }
}
