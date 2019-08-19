﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class CellData
{
    public IEntity Binding;

    public CellType CellType;

    public Coordinates Coordinates;

    [JsonIgnore]
    public CellData[] Neighbors = new CellData[8];

    public Structure Structure;

    [JsonIgnore]
    public bool Bound
    {
        get
        {
            return Binding != null;
        }
    }

    [JsonIgnore]
    public float Distance { get; set; }

    [JsonIgnore]
    public CellData NextWithSamePriority { get; set; }

    [JsonIgnore]
    public CellData PathFrom { get; set; }

    [JsonIgnore]
    public int SearchHeuristic { private get; set; }

    [JsonIgnore]
    public int SearchPhase { get; set; }

    [JsonIgnore]
    public int SearchPriority => (int)Distance + SearchHeuristic;

    [JsonIgnore]
    public float TravelCost
    {
        get
        {
            if (!Bound)
            {
                return 25;
            }

            switch (CellType)
            {
                case CellType.Water:
                case CellType.Mountain:
                    return -1;
            }

            return Structure != null && !Structure.IsBluePrint ? Structure.TravelCost : 1;
        }
    }

    public bool Buildable
    {
        get
        {
            return Bound && TravelCost > 0 && Structure == null;
        }
    }

    public bool Pathable
    {
        get
        {
            return Bound && TravelCost > 0;
        }
    }

    public void SetStructure(Structure structure)
    {
        if (Structure != null)
        {
            Game.StructureController.DestroyStructure(Structure);
        }

        structure.Coordinates = Coordinates;
        foreach (var cell in structure.GetCellsForStructure(Coordinates))
        {
            cell.Structure = structure;
        }

        Game.StructureController.RefreshStructure(structure);
    }

    internal IEnumerable<CreatureData> GetCreatures()
    {
        return Game.CreatureController.CreatureLookup.Where(c => c.Key.Coordinates == Coordinates).Select(c => c.Key);
    }

    public CellData GetNeighbor(Direction direction)
    {
        return Neighbors[(int)direction];
    }

    public void SetNeighbor(Direction direction, CellData cell)
    {
        Neighbors[(int)direction] = cell;
        cell.Neighbors[(int)direction.Opposite()] = this;
    }

    internal int CountNeighborsOfType(CellType? cellType)
    {
        if (!cellType.HasValue)
        {
            return Neighbors.Count(n => n == null);
        }

        return Neighbors.Count(n => n != null && n.CellType == cellType.Value);
    }
}