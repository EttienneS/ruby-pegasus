﻿using Assets.ServiceLocator;
using System.Linq;
using Random = UnityEngine.Random;

namespace Assets.Creature.Behaviour
{
    public class GrazerBehavior : IBehaviour
    {
        public CreatureTask GetTask(CreatureData creature)
        {
            var creatures = creature.Awareness.SelectMany(c => c.Creatures);

            var enemies = creatures.Where(c => c.FactionName != creature.FactionName);
            var herd = creatures.Where(c => c.FactionName == creature.FactionName);

            if (enemies.Any())
            {
                var target = Loc.GetMap().GetCellAttRadian(enemies.GetRandomItem().Cell, 10, Random.Range(1, 360));
                return new Move(target);
            }
            else if (herd.Any())
            {
                return new Move(Loc.GetMap().GetCircle(herd.GetRandomItem().Cell, 3).GetRandomItem());
            }

            return null;
        }
    }
}