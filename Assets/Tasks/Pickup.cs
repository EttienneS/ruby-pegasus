﻿public class Pickup : CreatureTask
{
    public int Amount;
    public string ItemId;

    public Pickup(Item item, int amount = -1)
    {
        ItemId = item.Id;
        Amount = amount;
        AddSubTask(new Move(item.Cell));
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            var item = ItemId.GetItem();
            creature.PickUpItem(item, Amount < 0 ? item.Amount : Amount);
            return true;
        }
        return false;
    }
}