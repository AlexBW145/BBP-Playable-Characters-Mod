using LibraryLibrary.AchievementsAPI;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BBP_Playables.Core;

internal class AchievementTracker : MonoBehaviour
{
    public delegate void AchievementAction(int queue);
    public enum AchievementQueue
    {
        FALSEGAMEOVER,
        MAXEDOUTPROFIT,
        NOCHEATING_CHEATED,
        NOCHEATING_CLEARED,
        FOOLEDBALDI_SUCCESS,
        MAINTAINED_INCORRECT,
        TIMEBENDER,
        JOBLESS_DETENTION,
        HIDEANDSEEK_CLEAR,
        LEVEL_CLEAR,
    }
    public enum AchievementTrigger
    {
        TURNEDTABLES_ACTIVE,
        TURNEDTABLES_FAILED,
        NOCHEATING_FAILED,
        ANTIHOARDING_FAILED,
        MAINTAINED_FAILED,
    }
    public static AchievementTracker Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    private List<int> ach_triggers = new List<int>();
    private Queue<int> ach_queue = new Queue<int>();
    private AchievementAction actions;
    internal int ytpsTotalRun = 0;

    private void LateUpdate()
    {
        while (ach_queue.Count > 0)
        {
            var dequeued = ach_queue.Dequeue(); // If I were to dequeue on one line it would not dequeue next to the action that appears to be not registered.
            actions?.Invoke(dequeued);
        }
    }

    public void QueueAchievement(int queue) => ach_queue.Enqueue(queue);
    public void UnregisterAction(AchievementAction action) => actions -= action;
    public void RegisterAction(AchievementAction action) => actions += action;
    public void RegisterTrigger(int trigger)
    {
        if (!ach_triggers.Contains(trigger))
            ach_triggers.Add(trigger);
    }
    public bool HasTrigger(int trigger) => ach_triggers.Contains(trigger);
    public void AwardAchievement(string id) => LLAchievementManager.RewardAchievement(id, true);

    internal void Reset()
    {
        ach_triggers.Clear();
        ach_queue.Clear();
        ytpsTotalRun = 0;
    }
    internal void RestartTriggers()
    {
        ach_triggers.RemoveAll(t =>
        t == (int)AchievementTrigger.TURNEDTABLES_ACTIVE ||
        t == (int)AchievementTrigger.TURNEDTABLES_FAILED ||
        t == (int)AchievementTrigger.NOCHEATING_FAILED);
    }
    internal void Save(BinaryWriter writer)
    {
        writer.Write(ach_triggers.Count);
        for (int k = 0; k < ach_triggers.Count; k++)
            writer.Write(ach_triggers[k]);
    }
    internal void Load(BinaryReader reader)
    {
        Reset();
        int count = reader.ReadInt32();
        for (int m = 0; m < count; m++)
            ach_triggers.Add(reader.ReadInt32());
    }
}