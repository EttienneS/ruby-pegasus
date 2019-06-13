﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Faction : MonoBehaviour
{
    public const int RecyleCount = 5;
    public const int RecyleTime = 3;
    public int LastRecyle;
    public Dictionary<ManaColor, Mana> ManaPool = new Dictionary<ManaColor, Mana>();
    internal string FactionName;
    internal List<TaskBase> Tasks = new List<TaskBase>();

    internal List<CreatureData> Creatures = new List<CreatureData>();

    public static void ProcessQueue(Queue<TaskBase> queue)
    {
        if (queue == null || queue.Count == 0)
        {
            return;
        }

        var current = queue.Peek();

        if (current.Done())
        {
            queue.Dequeue();
        }
        else
        {
            current.Update();
        }
    }

    public static bool QueueComplete(Queue<TaskBase> queue)
    {
        return queue == null || queue.Count == 0;
    }

    public TaskBase AddTask(TaskBase task, string originatorId)
    {
        task.Originator = originatorId;
        Tasks.Add(task);
        return task;
    }

    public void AssignTask(CreatureData creature, TaskBase task, string originator = "")
    {
        task.AssignedCreatureId = creature.Id;

        if (!string.IsNullOrEmpty(originator))
        {
            task.Originator = originator;
        }

        if (task.SubTasks != null)
        {
            foreach (var subTask in task.SubTasks.ToList())
            {
                subTask.Context = task.Context;
                AssignTask(creature, subTask, task.Originator);
            }
        }
    }

    public TaskBase GetNextAvailableTask(Creature creature)
    {
        TaskBase task = null;
        foreach (var availableTask in Tasks.Where(t => t.AssignedCreatureId <= 0 && !t.Failed))
        {
            //var craftTask = availableTask as Craft;
            //if (craftTask != null)
            //{
            //    if (IdService.IsStructure(craftTask.Originator))
            //    {
            //        var structure = IdService.GetStructureFromId(craftTask.Originator);

            //        if (structure.InUseByAnyone)
            //        {
            //            continue;
            //        }
            //        else
            //        {
            //            structure.Reserve(creature.Data.GetGameId());
            //        }
            //    }
            //}

            task = availableTask;
            break;
        }
        return task;
    }

    public TaskBase GetTask(Creature creature)
    {
        var task = creature.Data.GetBehaviourTask?.Invoke(creature.Data);
        if (task == null)
        {
            task = GetNextAvailableTask(creature) ?? new Idle(creature.Data);
        }

        task.AssignedCreatureId = creature.Data.Id;
        return AddTask(task, creature.Data.GetGameId());
    }

    public IEnumerable<TaskBase> GetTaskByOriginator(string originatorId)
    {
        return Tasks.Where(t => t.Originator == originatorId);
    }

    public void Update()
    {
        if (Game.TimeManager.Data.Hour - LastRecyle > RecyleTime)
        {
            LastRecyle = Game.TimeManager.Data.Hour;

            var failedTasks = Tasks.Where(t => t.Failed);

            foreach (var task in failedTasks.Take(RecyleCount))
            {
                task.Failed = false;
            }
        }
    }

    internal void TaskComplete(TaskBase task)
    {
        Tasks.Remove(task);
    }

    internal void TaskFailed(TaskBase task, string reason)
    {
        task.Failed = true;

        task.Message += $"\n{reason}";
        task.AssignedCreatureId = -1;

        // move task to bottom of the list
        Tasks.Remove(task);
        Tasks.Add(task);
    }
}