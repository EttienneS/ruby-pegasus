﻿using Assets.Item;
using Assets.Structures;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public abstract class ZoneBase
{
    private List<Cell> _cells = new List<Cell>();

    public string CellString
    {
        get
        {
            if (_cells.Count == 0)
            {
                return "";
            }
            _cells = _cells.Distinct().ToList();
            return _cells.Select(c => c.X + ":" + c.Z).Aggregate((s1, s2) => s1 + "," + s2);
        }
        set
        {
            _cells = new List<Cell>();
            foreach (var xy in value.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                var split = xy.Split(':').Select(i => int.Parse(i)).ToList();
                _cells.Add(Map.Instance.GetCellAtCoordinate(split[0], split[1]));
            }
        }
    }

    [JsonIgnore]
    public string ColorString { get; set; } = ColorExtensions.GetRandomColor().ToColorHexString();

    public string FactionName { get; set; }

    [JsonIgnore]
    public List<ItemData> Items
    {
        get
        {
            return Game.Instance.IdService.ItemLookup.Values.Where(i => _cells.Contains(i.Cell)).ToList();
        }
    }

    public string Name { get; set; }

    [JsonIgnore]
    public IEntity Owner
    {
        get
        {
            if (string.IsNullOrEmpty(OwnerId))
            {
                return null;
            }

            return OwnerId.GetEntity();
        }
    }

    public string OwnerId { get; set; }

    [JsonIgnore]
    // get structures in cells from id service
    public List<Structure> Structures
    {
        get
        {
            var structures = new List<Structure>();

            foreach (var cell in _cells.Where(c => Game.Instance.IdService.StructureCellLookup.ContainsKey(c)))
            {
                structures.AddRange(Game.Instance.IdService.StructureCellLookup[cell]);
            }

            return structures;
        }
    }

    public void AddCells(List<Cell> cells)
    {
        _cells.AddRange(cells);
        _cells.Distinct();
    }

    internal void RemoveCell(Cell cell)
    {
        _cells.Remove(cell);
    }

    public bool CanUse(IEntity entity)
    {
        return string.IsNullOrEmpty(OwnerId) || OwnerId.Equals(entity.Id);
    }

    public List<Cell> GetCells()
    {
        if (_cells == null)
        {
            _cells = new List<Cell>();
        }
        return _cells;
    }
}