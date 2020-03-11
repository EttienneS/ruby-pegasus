﻿using Needs;

public class Eat : CreatureTask
{
    public bool Eating;
    public bool Ate;

    public const string FoodCriteria = "Food";

    public Eat()
    {
    }

    public override void Complete()
    {
    }

    public Eat(Item food) : this()
    {
        AddSubTask(new Pickup(food, 1));
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            var food = creature.HeldItem;

            if (food?.IsType(FoodCriteria) == false)
            {
                creature.DropItem(creature.Cell);
                AddSubTask(new FindAndGetItem(FoodCriteria, 1));
                return false;
            }

            if (!Eating)
            {
                AddSubTask(new Wait(2, "Eating..."));
                BusyEmote = "*munch, chomp*";
                Eating = true;
            }
            else if (!Ate)
            {
                BusyEmote = "";
                creature.GetNeed<Hunger>().Current += food.ValueProperties["Nutrition"];
                creature.DropItem(creature.Cell);
                Game.IdService.DestroyEntity(food);
                Ate = true;

                if (creature.GetNeed<Hunger>().Current < 20)
                {
                    AddSubTask(new Eat());
                }
                else
                {
                    return true;
                }
            }
            else
            {
                Eating = false;
                Ate = false;
                return creature.GetNeed<Hunger>().Current < 10;
            }
        }
        return false;
    }
}