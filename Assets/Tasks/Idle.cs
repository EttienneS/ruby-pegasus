﻿using System.Linq;
using UnityEngine;

public class Idle : Task
{
    public Idle()
    {
    }

    public Idle(CreatureData creature)
    {
        if (Random.value > 0.6)
        {
            var wanderCircle = Game.Map.GetCircle(creature.Cell, 2).Where(c => c.Bound && c.TravelCost == 1).ToList();
            if (wanderCircle.Count > 0)
            {
                AddSubTask(new Move(wanderCircle[Random.Range(0, wanderCircle.Count - 1)], (int)creature.Speed / Random.Range(2, 6)));
            }
        }
        else
        {
            AddSubTask(new Wait(Random.Range(1f, 2f), "Chilling"));
        }

        Message = "Waiting for something to do.";
    }

    public override bool Done()
    {
        return Creature.TaskQueueComplete(SubTasks);
    }
}