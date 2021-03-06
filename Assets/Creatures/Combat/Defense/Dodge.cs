﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class Dodge : DefensiveActionBase
{
    public Dodge(string name) : base(name)
    {
        ActivationTIme = 0.5f;
    }

    public override int Defend(string attackName, int incomingDamage, DamageType damageType)
    {
        var roll = RandomHelper.Roll(20) + (Owner.Dexterity / 2);

        var message = $"{Owner.Name} {Name} to dodge the {attackName}";

        var (outcome, mod) = RollDef(roll, 10);
        switch (outcome)
        {
            case OutcomeLevel.Critical:
                message += " and manages to completely avoid the attack!";
                break;

            case OutcomeLevel.Success:
                message += " and manages to avoid some of the damage!";
                break;

            case OutcomeLevel.Failure:
                message += " but fails to avoid the hit!";
                break;

            case OutcomeLevel.Catastrophe:
                message += " but zigs where they should have zagged, right into the hit!";
                break;
        }

        Owner.Log(message);
        return (int)(incomingDamage * mod);
    }

    public override int EstimateDefense(OffensiveActionBase offensiveActionBase)
    {
        var roll = 10 + (Owner.Dexterity / 2);
        return (int)(offensiveActionBase.PredictDamage(Owner) * RollDef(roll, 10).mod);
    }

    public (OutcomeLevel outcome, float mod) RollDef(int roll, int target)
    {
        var mod = 1.0f;
        var outcome = ContestHelper.Contest(roll, target);
        switch (outcome)
        {
            case OutcomeLevel.Critical:
                mod = 0.0f;
                break;

            case OutcomeLevel.Success:
                mod = 0.5f;
                break;

            case OutcomeLevel.Catastrophe:
                mod = 1.5f;
                break;
        }

        return (outcome, mod);
    }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum OutcomeLevel
{
    Critical, Success, Failure, Catastrophe
}
