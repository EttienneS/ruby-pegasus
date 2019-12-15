﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Cell : IEquatable<Cell>
{
    [JsonIgnore]
    public Cell[] Neighbors = new Cell[8];

    public int X;
    public int Y;
    internal Color Color;

    private BiomeRegion _biomeRegion;

    [JsonIgnore]
    public BiomeRegion BiomeRegion
    {
        get
        {
            if (_biomeRegion == null)
            {
                _biomeRegion = Game.MapGenerator.Biome.GetRegion(Height);
            }
            return _biomeRegion;
        }
    }

    [JsonIgnore]
    public bool Buildable
    {
        get
        {
            return TravelCost > 0 && Structure == null;
        }
    }

    [JsonIgnore]
    public List<Creature> Creatures
    {
        get
        {
            return IdService.CreatureLookup.Values.Where(c => c.Cell == this).ToList();
        }
    }

    [JsonIgnore]
    public float Distance { get; set; }

    [JsonIgnore]
    public Structure Floor
    {
        get
        {
            return IdService.StructureCellLookup.ContainsKey(this) ? IdService.StructureCellLookup[this].Find(s => s.IsFloor()) : null;
        }
    }

    [JsonIgnore]
    public float Height
    {
        get
        {
            return Game.Map.GetCellHeight(X, Y);
        }
    }

    [JsonIgnore]
    public IEnumerable<Item> Items
    {
        get
        {
            return IdService.ItemLookup.Values.Where(i => i.Cell == this);
        }
    }

    [JsonIgnore]
    public Cell NextWithSamePriority { get; set; }

    [JsonIgnore]
    public IEnumerable<Cell> NonNullNeighbors
    {
        get
        {
            return Neighbors.Where(n => n != null);
        }
    }

    [JsonIgnore]
    public Cell PathFrom { get; set; }

    [JsonIgnore]
    public int SearchHeuristic { private get; set; }

    [JsonIgnore]
    public int SearchPhase { get; set; }

    [JsonIgnore]
    public int SearchPriority => (int)Distance + SearchHeuristic;

    [JsonIgnore]
    public Structure Structure
    {
        get
        {
            return IdService.StructureCellLookup.ContainsKey(this) ? IdService.StructureCellLookup[this].Find(s => !s.IsFloor()) : null;
        }
    }

    [JsonIgnore]
    public Tile Tile
    {
        get
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.RotateRandom90();

            RefreshColor();

            if (Floor == null)
            {
                tile.sprite = Game.SpriteStore.GetSpriteForTerrainType(BiomeRegion.SpriteName);
                tile.color = Color;
            }
            else
            {
                tile.sprite = Game.SpriteStore.GetSprite(Floor.SpriteName);
                var col = Color;

                if (Floor.IsBluePrint)
                {
                    col = ColorConstants.BluePrintColor;
                    col.a = 1;
                }

                tile.color = col;
            }

            return tile;
        }
    }

    [JsonIgnore]
    public float TravelCost
    {
        get
        {
            return Structure?.IsBluePrint == false ? Structure.TravelCost : BiomeRegion.TravelCost;
        }
    }

    [JsonIgnore]
    public Vector3 Vector
    {
        get
        {
            return new Vector3(X + 0.5f, Y + 0.5f, -1);
        }
    }

    public static Cell FromPosition(Vector2 position)
    {
        // add half a unit to each position to account for offset (cells are at point 0,0 in the very center)
        position += new Vector2(0.5f, 0.5f);
        return Game.Map.GetCellAtCoordinate(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
    }

    public static bool operator !=(Cell obj1, Cell obj2)
    {
        if (ReferenceEquals(obj1, null))
        {
            return !ReferenceEquals(obj2, null);
        }

        return !obj1.Equals(obj2);
    }

    public static bool operator ==(Cell obj1, Cell obj2)
    {
        if (ReferenceEquals(obj1, null))
        {
            return ReferenceEquals(obj2, null);
        }

        return obj1.Equals(obj2);
    }

    public int DistanceTo(Cell other)
    {
        return (X < other.X ? other.X - X : X - other.X)
                + (Y < other.Y ? other.Y - Y : Y - other.Y);
    }

    public bool Equals(Cell other)
    {
        if (ReferenceEquals(other, null))
        {
            return false;
        }

        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object obj)
    {
        var other = obj as Cell;
        if (other == null)
        {
            return false;
        }

        return this == other;
    }

    public override int GetHashCode()
    {
        return $"{X}:{Y}".GetHashCode();
    }

    public Cell GetNeighbor(Direction direction)
    {
        return Neighbors[(int)direction];
    }

    public bool IsInterlocking(Direction direction)
    {
        var neighbor = Neighbors[(int)direction];

        if (neighbor == null)
        {
            return false;
        }

        if (neighbor.Structure == null)
        {
            return false;
        }

        return neighbor.Structure.IsWall();
    }

    public bool Pathable(Mobility mobility)
    {
        switch (mobility)
        {
            case Mobility.Walk:
                return TravelCost > 0;

            case Mobility.Fly:
                return true;
        }

        return false;
    }

    public void RefreshColor()
    {
        const float totalShade = 1f;
        const float maxShade = 0.4f;
        var baseColor = new Color(totalShade, totalShade, totalShade, 1f);

        var scaled = Helpers.Scale(BiomeRegion.Min, BiomeRegion.Max, 0f, maxShade, Height);

        Color = new Color(baseColor.r - scaled, baseColor.g - scaled, baseColor.b - scaled, baseColor.a);
    }

    public void SetNeighbor(Direction direction, Cell cell)
    {
        Neighbors[(int)direction] = cell;
        cell.Neighbors[(int)direction.Opposite()] = this;
    }

    public override string ToString()
    {
        return $"X: {X}, Y: {Y}";
    }

    public string ToStringOnSeparateLines()
    {
        return $"X: {X}\nY: {Y}";
    }

    public void UpdateTile()
    {
        Game.Map.Tilemap.SetTile(new Vector3Int(X, Y, 0), null);
        Game.Map.Tilemap.SetTile(new Vector3Int(X, Y, 0), Tile);

        if (Structure != null)
        {
            Game.StructureController.RefreshStructure(Structure);
        }
    }

    internal void Clear()
    {
        if (Structure != null)
        {
            Game.StructureController.DestroyStructure(Structure);
        }

        if (Floor != null)
        {
            Game.StructureController.DestroyStructure(Floor);
        }
    }

    internal Structure CreateStructure(string structureName, string faction = FactionConstants.World)
    {
        var structure = Game.StructureController.GetStructure(structureName, Game.FactionController.Factions[faction]);
        structure.Cell = this;

        if (structure.AutoInteractions.Count > 0)
        {
            Game.MagicController.AddEffector(structure);
        }

        return structure;
    }

    internal bool Empty()
    {
        return Structure == null && Floor == null;
    }

    internal IEnumerable<Creature> GetEnemyCreaturesOf(string faction)
    {
        return Creatures.Where(c => c.FactionName != faction);
    }

    internal Cell GetRandomNeighbor()
    {
        var neighbors = Neighbors.Where(n => n != null).ToList();
        return neighbors[Random.Range(0, neighbors.Count - 1)];
    }

    internal void Populate()
    {
        if (Structure?.Name == "Reserved")
        {
            Game.StructureController.DestroyStructure(Structure);
        }

        if (!Empty())
        {
            return;
        }

        var content = BiomeRegion.GetContent();

        if (!string.IsNullOrEmpty(content))
        {
            if (Game.StructureController.StructureDataReference.ContainsKey(content))
            {
                var structure = Game.StructureController.GetStructure(content, Game.FactionController.Factions[FactionConstants.World]);
                structure.Cell = this;

                if (structure.AutoInteractions.Count > 0)
                {
                    Game.MagicController.AddEffector(structure);
                }
            }
            else
            {
                Game.ItemController.SpawnItem(content, this);
            }
        }
    }

    internal Vector3Int ToVector3Int()
    {
        return new Vector3Int(X, Y, 0);
    }
}