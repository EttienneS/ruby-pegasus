﻿using Structures;
using System.Linq;

public class RemoveStructure : CreatureTask
{
    public Structure StructureToRemove;

    public override string Message
    {
        get
        {
            return $"Remove {StructureToRemove.Name} at {StructureToRemove.Cell}";
        }
    }

    public RemoveStructure()
    {
        RequiredSkill = SkillConstants.Build;
        RequiredSkillLevel = 1;
    }

    public override void Complete()
    {
    }

    public RemoveStructure(Structure structure) : this()
    {
        StructureToRemove = structure;

    }

    public bool Decontructed;

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            if (!creature.Cell.Neighbors.Contains(StructureToRemove.Cell))
            {
                var pathable = Game.Instance.Map.TryGetPathableNeighbour(StructureToRemove.Cell);

                if (pathable != null)
                {
                    AddSubTask(new Move(pathable));
                }
                else
                {
                    creature.SuspendTask();
                    return false;
                }
            }

            if (!Decontructed)
            {
                AddSubTask(new Wait(StructureToRemove.Cost.Items.Sum(c => c.Value), "Deconstructing..."));
                Decontructed = true;
                return false;
            }

            foreach (var item in StructureToRemove.Cost.Items)
            {
                var spawnedItem = Game.Instance.ItemController.SpawnItem(item.Key, StructureToRemove.Cell);
                spawnedItem.Amount = item.Value;
                spawnedItem.FactionName = creature.FactionName;

                // claim the entity to ensure that it can be used even if outside the 'home' area
                creature.ClaimEntityForFaction(spawnedItem);
            }

            Game.Instance.StructureController.DestroyStructure(StructureToRemove);
            StructureToRemove = null;
            return true;
        }

        return false;
    }
}