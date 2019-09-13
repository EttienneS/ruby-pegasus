﻿using System.Linq;

public partial class OrderSelectionController //.Designate
{
    internal const string DefaultDesignateText = "Designate";

    internal const string CutText = "Cut Tree";
    internal const string CutIcon = "axe_t";

    internal const string HarvestText = "Harvest Plant";
    internal const string HarvestIcon = "wheat_t";

    internal const string MoveText = "Move Here";
    internal const string MoveIcon = "location_t";

    internal const string AttackText = "Attack";
    internal const string AttackIcon = "war_t";

    internal OrderButton TaskButton;

    public void DesignateTypeClicked()
    {
        if (Game.OrderTrayController.gameObject.activeInHierarchy)
        {
            DisableAndReset();
            TaskButton.Text = DefaultDesignateText;
        }
        else
        {
            EnableAndClear();

            CreateOrderButton(CutText, () => HarvestClicked("Tree", CutIcon), CutIcon);
            CreateOrderButton(HarvestText, () => HarvestClicked("Bush", HarvestIcon), HarvestIcon);
            CreateOrderButton(MoveText, MoveClicked, MoveIcon);
            CreateOrderButton(AttackText, AttackClicked, AttackIcon);
        }
    }

    private void MoveClicked()
    {
        Game.Controller.SelectionPreference = SelectionPreference.Cell;
        CellClickOrder = cells =>
        {
            var cell = cells.First();
            FactionController.PlayerFaction.AddTaskWithCellBadge(new Move(cell), null, cell, MoveIcon);
        };
    }

    private void AttackClicked()
    {
        Game.Controller.SelectionPreference = SelectionPreference.Cell;
        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                foreach (var creature in cell.GetCreatures())
                {
                    FactionController.PlayerFaction.AddTaskWithEntityBadge(new ExecuteAttack(creature, new FireBlast()), null, creature, AttackIcon);
                }
            }
        };
    }

    private void HarvestClicked(string type, string icon)
    {
        Game.Controller.SelectionPreference = SelectionPreference.Cell;
        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (cell.Bound && cell.Structure != null && cell.Structure.StructureType == type)
                {
                    cell.Structure.SetStatusSprite(Game.SpriteStore.GetSprite(icon));

                    FactionController.PlayerFaction.AddTaskWithCellBadge(new Harvest(cell.Structure), null, cell, icon);
                }
            }
            Game.Controller.DeselectCell();
        };
    }
}