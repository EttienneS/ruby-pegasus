﻿using Assets.Creature;
using Assets.Structures;
using Structures;
using System;
using System.Collections.Generic;
using System.Linq;

public class Build : CreatureTask
{
    public Blueprint Blueprint;

    private int _waitCount = 0;

    public override void FinalizeTask()
    {
    }

    public Build()
    {
        RequiredSkill = SkillConstants.Build;
        RequiredSkillLevel = 1;
    }

    public Build(Blueprint blueprint) : this()
    {
        Blueprint = blueprint;
    }

    public bool Built = false;

    public override string Message
    {
        get
        {
            return $"Building {Blueprint.StructureName} at {Blueprint.Cell}";
        }
    }

    public override bool Done(CreatureData creature)
    {
        if (Blueprint == null)
        {
            throw new TaskFailedException();
        }

        if (SubTasksComplete(creature))
        {
            if (!Clean()) return false;
            if (!HasItems()) return false;
            if (!InPosition(creature)) return false;
            if (!CellOpen()) return false;
            if (!BuildComplete(creature)) return false;

            FinishStructure(creature.GetFaction());

            return true;
        }
        return false;
    }

    private bool BuildComplete(CreatureData creature)
    {
        if (!Built)
        {
            creature.Face(Blueprint.Cell);
            var time = Blueprint.Cost.Items.Sum(i => i.Value) * 5;
            AddSubTask(new Wait(time, "Building", AnimationType.Interact));
            Built = true;
            return false;
        }
        return true;
    }

    private bool CellOpen()
    {
        if (Blueprint.Cell.Creatures.Count > 0)
        {
            _waitCount++;

            if (_waitCount > 10)
            {
                throw new TaskFailedException("Cannot build, cell occupied");
            }
            AddSubTask(new Wait(1, "Cell occupied", AnimationType.Interact));
            return false;
        }
        return true;
    }

    private bool InPosition(CreatureData creature)
    {
        if (!creature.Cell.NonNullNeighbors.Contains(Blueprint.Cell))
        {
            AddSubTask(new Move(Blueprint.Cell.GetPathableNeighbour()));
            return false;
        }
        return true;
    }

    private bool HasItems()
    {
        var needed = GetNeededItems();

        if (needed.Count > 0)
        {
            foreach (var item in needed)
            {
                AddSubTask(new FindAndHaulItem(item.Key, item.Value, Blueprint.Cell));
            }
            return false;
        }
        return true;
    }

    private bool Clean()
    {
        var nonStructureItems = Blueprint.Cell.Items.Where(i => !Blueprint.Cost.Items.ContainsKey(i.Name));
        if (nonStructureItems.Any())
        {
            foreach (var item in nonStructureItems)
            {
                item.Free();
                AddSubTask(new Pickup(item));
                AddSubTask(new Drop(Blueprint.Cell.GetPathableNeighbour()));
            }

            return false;
        }

        var structuresToClean = Game.Instance.IdService
                                             .GetStructuresInCell(Blueprint.Cell)
                                             .Where(c => !c.Buildable);

        if (structuresToClean.Any())
        {
            foreach (var structureToClean in structuresToClean)
            {
                AddSubTask(new RemoveStructure(structureToClean));
            }
            return false;
        }
        return true;
    }

    public void FinishStructure(Faction faction)
    {
        Game.Instance.StructureController.SpawnStructure(Blueprint.StructureName, Blueprint.Cell, Blueprint.Faction);
        foreach (var item in Blueprint.Cell.Items.ToList())
        {
            Game.Instance.ItemController.DestroyItem(item);
        }
        Game.Instance.StructureController.DestroyBlueprint(Blueprint);
    }

    public Dictionary<string, int> GetNeededItems()
    {
        var current = Blueprint.Cell.Items.ToList();
        var desired = new Dictionary<string, int>();
        foreach (var item in Blueprint.Cost.Items)
        {
            var desiredAmount = item.Value;
            foreach (var existing in current.Where(i => i.Name.Equals(item.Key, StringComparison.OrdinalIgnoreCase)))
            {
                desiredAmount -= existing.Amount;
            }

            if (desiredAmount > 0)
            {
                desired.Add(item.Key, desiredAmount);
            }
        }

        return desired;
    }
}