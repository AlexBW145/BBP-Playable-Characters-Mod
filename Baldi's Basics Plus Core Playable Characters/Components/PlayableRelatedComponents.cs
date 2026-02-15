using HarmonyLib;
using MTM101BaldAPI;
using System;
using UnityEngine;

namespace BBP_Playables.Core;

[RequireComponent(typeof(PlayerManager), typeof(ItemManager), typeof(PlayerMovement))]
public class PlayableCharacterComponent : MonoBehaviour
{
    protected PlayerManager pm;
    /// <summary>
    /// This function gets called after initialization
    /// </summary>
    protected virtual void Start()
    {
    }
    /// <summary>
    /// This function gets called within the player manager initialization
    /// </summary>
    public virtual void Initialize() { pm = gameObject.GetComponent<PlayerManager>(); }
    /// <summary>
    /// This function gets called after the game has begun
    /// </summary>
    /// <param name="manager"></param>
    public virtual void GameBegin(BaseGameManager manager) { }
    /// <summary>
    /// This function gets called after spoop mode begins
    /// </summary>
    /// <param name="manager"></param>
    public virtual void SpoopBegin(BaseGameManager manager) { }
}
internal class PlayableRandomizer : PlayableCharacterComponent
{
    public static void RandomizePlayable()
    {
        PlayableCharsPlugin.gameStarted = false;
        var chars = PlayableCharacterMetaStorage.Instance.FindAll(chara => chara.value.unlocked && chara.value.componentType != typeof(PlayableRandomizer));
        WeightedSelection<PlayableCharacter>[] weights = new WeightedSelection<PlayableCharacter>[0];
        foreach (var character in chars)
            weights = weights.AddToArray(new() { selection = character.value, weight = 100 });
        PlayableCharsGame.Character = WeightedSelection<PlayableCharacter>.RandomSelection(weights);
#if DEBUG
        PlayableCharsPlugin.Log.LogInfo($"Randomly selected {PlayableCharsGame.Character.name} for Player!");
#endif
    }
}
/// <summary>
/// The <seealso cref="ExtendedStickerData"/> that is exclusive to certain playable characters.
/// </summary>
[Serializable]
public class PlayableCharacterStickerData : ExtendedStickerData
{
    /// <summary>
    /// The playable character that the sticker belongs to.
    /// </summary>
    public PlayableCharacter playableCharacter;

    public override bool CanBeApplied()
    {
        if (PlrPlayableCharacterVars.GetLocalPlayable().GetCurrentPlayable() != playableCharacter)
            return false;
        return base.CanBeApplied();
    }
}
