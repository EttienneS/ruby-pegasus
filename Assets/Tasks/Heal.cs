﻿using System.Collections.Generic;
using UnityEngine;
using Assets.Creature;
using Assets.ServiceLocator;

public class Heal : CreatureTask
{
    public override string Message
    {
        get
        {
            return $"Tend to wounds";
        }
    }

    public Heal()
    {
        RequiredSkill = SkillConstants.Healing;
        RequiredSkillLevel = 1;
    }

    public override void FinalizeTask()
    {
    }

    public override bool Done(CreatureData creature)
    {
        if (SubTasksComplete(creature))
        {
            var wound = creature.GetWorstWound();
            if (wound != null)
            {
                Loc.GetVisualEffectController().SpawnLightEffect(creature.Vector, ColorConstants.WhiteAccent, 2, 1, 1).Fades();

                wound.Treated = true;
                wound.HealRate /= 2;
            }

            return true;
        }
        return false;
    }
}