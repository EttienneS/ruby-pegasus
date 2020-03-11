﻿public class Vent : CreatureTask
{
    public Vent()
    {
        RequiredSkill = SkillConstants.Arcana;
        RequiredSkillLevel = 1;
    }

    public ManaColor Color { get; set; }
    public float Amount { get; set; }

    public Vent(ManaColor color, float amount) : this()
    {
        Color = color;
        Amount = amount;
    }

    public override void Complete()
    {
    }

    private bool _firstRun = true;

    public override bool Done(Creature creature)
    {
        if (_firstRun)
        {
            AddSubTask(new Wait(Amount, "Venting...", LPC.Spritesheet.Generator.Interfaces.Animation.Spellcast));
            _firstRun = false;
            Game.VisualEffectController.SpawnLightEffect(creature, creature.Vector, Color.GetActualColor(), 3, 3, Amount);
            return false;
        }
        if (SubTasksComplete(creature))
        {
            creature.ManaPool.BurnMana(Color, Amount);
            return true;
        }
        return false;
    }
}