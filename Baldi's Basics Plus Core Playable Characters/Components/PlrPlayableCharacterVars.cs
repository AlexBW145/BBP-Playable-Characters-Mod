using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BBP_Playables.Core
{
    [RequireComponent(typeof(PlayerManager), typeof(PlayerEntity))]
    public class PlrPlayableCharacterVars : MonoBehaviour
    {
        [SerializeField] private PlayableCharacter curCharacter;
        [SerializeField] private PlayerManager curPlayer;
        public static PlrPlayableCharacterVars[] PlayerPlayables { get; private set; } = new PlrPlayableCharacterVars[12];

        internal void Init(PlayerManager pm)
        {
            curPlayer = pm;
            if (PlayableCharsGame.Character != null) curCharacter = PlayableCharsGame.Character;
            PlayerPlayables[curPlayer.playerNumber] = this;
        }

        public PlayableCharacter GetCurrentPlayable() => curCharacter;
        public PlayerManager GetPlayer() => curPlayer;
        public static PlrPlayableCharacterVars GetPlayable(int player) => PlayerPlayables[player];
        public static PlrPlayableCharacterVars GetLocalPlayable() => CoreGameManager.Instance.GetPlayer(0).GetPlayable();
    }
}
