﻿using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public static class Behaviours
{
    public static Dictionary<string, GetBehaviourTaskDelegate> BehaviourTypes = new Dictionary<string, GetBehaviourTaskDelegate>
    {
        { "AbyssWraith", AbyssWraith },
        { "Person", Person }
    };

    public static GetBehaviourTaskDelegate GetBehaviourFor(string type)
    {
        return BehaviourTypes[type];
    }

    public delegate EntityTask GetBehaviourTaskDelegate(CreatureData creature);

    public const int WraithRange = 10;

    public static EntityTask AbyssWraith(CreatureData creature)
    {
        EntityTask task = null;

        var rand = Random.value;

        if (rand > 0.999f)
        {
            if (FactionController.PlayerFaction.Creatures.Count > 0)
            {
                task = new Interact(new Bite(), FactionController.PlayerFaction.Creatures.GetRandomItem());
            }
        }
        else if (rand > 0.8f)
        {
            task = new Move(Game.Map.GetRectangle(creature.Cell.X - (WraithRange / 2),
                creature.Cell.Y - (WraithRange / 2), WraithRange, WraithRange).GetRandomItem());
        }
        else
        {
            task = new Wait(Random.value * 2f, "Lingering..");
        }
        if (FactionController.PlayerFaction.Creatures.Count > 0)
        {
        }
        else
        {

        }

        return task;
    }

    public static EntityTask Person(CreatureData creature)
    {
        EntityTask task = null;

        var enemy = FindEnemy(creature);

        if (enemy != null)
        {
            task = new Interact(new ManaBlast(), enemy);
        }
        else if (creature.ManaPool.Any(m => m.Value.Total > m.Value.Max && m.Value.Total > m.Value.Max))
        {
            foreach (var mana in creature.ManaPool)
            {
                if (mana.Value.Total - mana.Value.Desired > mana.Value.Max)
                {
                    task = Channel.GetChannelTo(mana.Key, mana.Value.Total - mana.Value.Desired, creature.GetFaction().Core);
                    break;
                }
            }
        }
        else if (creature.Cell.Creatures.Count > 1)
        {
            // split up
            task = new Move(Game.Map.GetPathableNeighbour(creature.Cell));
        }
        else if (creature.ValueProperties[Prop.Hunger] > 50)
        {
            task = new Eat(ManaColor.Green);
        }
        else if (creature.ValueProperties[Prop.Energy] < 15)
        {
            var bed = creature.Self.Structures.FirstOrDefault(s => s.Properties.ContainsKey("RecoveryRate"));

            if (bed == null)
            {
                bed = IdService.StructureIdLookup.Values
                                         .FirstOrDefault(s =>
                                                !s.InUseByAnyone
                                                && s.Properties.ContainsKey("RecoveryRate"));
            }

            if (bed == null)
            {
                task = new Sleep(creature.Cell, 0.75f);
            }
            else
            {
                task = new Sleep(bed);
            }
        }

        return task;
    }

    private static CreatureData FindEnemy(CreatureData creature)
    {
        return Game.CreatureController.CreatureLookup.Keys.FirstOrDefault(c => c.FactionName != creature.FactionName
        && creature.Awareness.Contains(c.Cell));
    }
}