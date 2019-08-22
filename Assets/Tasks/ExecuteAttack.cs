﻿public class ExecuteAttack : TaskBase
{
    public ExecuteAttack()
    {
    }

    public IEntity Target;
    public IAttack Attack;

    public ExecuteAttack(IEntity target, IAttack attack)
    {
        Target = target;
        Attack = attack;
    }

    public override bool Done()
    {
        Attack.Attacker = AssignedEntity;
        Attack.Target = Target;

        (Attack.Attacker as CreatureData)?.Face(Target.Coordinates);

        if (Faction.QueueComplete(SubTasks) && Attack.Ready())
        {
            Attack.Resolve();
            return true;
        }

        return false;
    }
}