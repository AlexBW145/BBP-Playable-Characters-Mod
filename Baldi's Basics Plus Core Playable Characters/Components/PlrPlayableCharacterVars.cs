using JetBrains.Annotations;
using UnityEngine;

namespace BBP_Playables.Core
{
    [RequireComponent(typeof(PlayerManager), typeof(PlayerEntity))]
    public class PlrPlayableCharacterVars : MonoBehaviour
    {
        [SerializeField] private PlayableCharacter curCharacter;
        [SerializeField] private PlayerManager curPlayer;

        void Start()
        {
            curPlayer = gameObject.GetComponent<PlayerManager>();
            if (PlayableCharsGame.Character != null) curCharacter = PlayableCharsGame.Character;
        }

        public PlayableCharacter GetCurrentPlayable() => curCharacter;
        public PlayerManager GetPlayer() => curPlayer;
    }
}
