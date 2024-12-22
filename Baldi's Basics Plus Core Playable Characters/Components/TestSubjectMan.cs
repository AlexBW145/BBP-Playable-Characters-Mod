using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using UnityEngine;

namespace BBP_Playables.Core
{
    [RequireComponent(typeof(PlayerManager), typeof(PlayerMovement))]
    public class TestSubjectMan : MonoBehaviour
    {
        private PlayerManager playerManager;
        [Range(0, 11)] public int num = 0;
        
        void Start()
        {
            playerManager = CoreGameManager.Instance.GetPlayer(num);
        }

        private HashSet<GameObject> spottedNPC = new HashSet<GameObject>();
        void Update()
        {
            foreach (NPC npc in playerManager.ec.Npcs)
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
            playerManager.plm.walkSpeed = Mathf.Max(0f, playerManager.plm.runSpeed - (Mathf.Max(spottedNPC.Count, 1) / (Mathf.Max(playerManager.ec.Npcs.Count + 1, 2) % playerManager.plm.runSpeed) * playerManager.plm.runSpeed));
        }
    }
}
