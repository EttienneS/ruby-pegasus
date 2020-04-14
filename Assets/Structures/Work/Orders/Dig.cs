﻿namespace Structures.Work.Orders
{
    public class Dig : WorkOrderBase
    {
        public override void OrderComplete()
        {
            Complete = true;
        }

        public override void UnitComplete(float quality)
        {
            if (!Active())
            {
                return;
            }
            Game.Instance.ItemController.SpawnItem(Option.Name, Structure.Cell, 1);
        }
    }
}