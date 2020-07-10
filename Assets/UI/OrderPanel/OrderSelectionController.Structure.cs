﻿using Structures;

public partial class OrderSelectionController //.Structure
{
    internal const string DefaultBuildText = "Select Building";

    internal OrderButton BuildButton;

    public void BuildClicked(string structureName)
    {
        var structure = Game.Instance.StructureController.StructureDataReference[structureName];
        Game.Instance.SelectionPreference = SelectionPreference.Cell;

        Game.Instance.Cursor.SetMesh(structureName, (cell) => structure.ValidateCellLocationForStructure(cell));
        UpdateStuctureOrder(structureName);

        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (structure.ValidateCellLocationForStructure(cell))
                {
                    Game.Instance.StructureController.SpawnBlueprint(structureName, cell, Game.Instance.FactionController.PlayerFaction);
                }
            }
        };
    }

    public void UpdateStuctureOrder(string structureName)
    {
        var structure = Game.Instance.StructureController.StructureDataReference[structureName];
        Game.Instance.OrderInfoPanel.Show($"Build {structureName}",
                                           "Select a location to place the structure.  A creature with the build skill will gather the required cost of material and then make the structure.",
                                           structure.Description,
                                           $"{structure.Cost}");
    }


    public void BuildTypeClicked()
    {
        if (Game.Instance.OrderTrayController.gameObject.activeInHierarchy)
        {
            DisableAndReset();
        }
        else
        {
            EnableAndClear();

            foreach (var structureData in Game.Instance.StructureController.StructureDataReference.Values)
            {
                if (!structureData.Buildable) continue;

                var button = CreateOrderButton(() => BuildClicked(structureData.Name), () => UpdateStuctureOrder(structureData.Name), structureData.Icon, structureData.ColorHex);
            }
        }
    }
}