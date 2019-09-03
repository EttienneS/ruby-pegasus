﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class MapGenerator
{
    public MapPreset MapPreset;

    public enum RoadSize
    {
        Single, Double, Triple
    }

    public CellData CreateCell(int x, int y)
    {
        var cell = new CellData
        {
            Coordinates = new Coordinates(x, y)
        };

        Game.MapGrid.Cells.Add(cell);

        Game.MapGrid.AddCellLabel(cell);

        return cell;
    }

    public void GenerateTowns()
    {
        // approach
        // City centers: Pick some points of the still empty map as main traffic nodes. They should be evenly distributed around the map
        // Highways: Connect the main traffic nodes to their neighbors and to the outside world using major roads.
        // Freeways: Subdivide the cells generated by the major roads by creating some minor roads.
        // Streets: Repeat the subdivision process recursively with smaller and smaller roads until you've reached the desired building block size
        // Blocks: Decide the purpose of each building block(residential, retail, corporate, industrial...).Relevant factors are the sizes of the neighboring roads and the distance from the center.
        // Allotments: Divide the edges of all building blocks into lots(this means each lot has at least one edge which is connected to a road).
        // Buildings: Generate a fitting building for each lot.

        var towns = new List<Town>();

        var townSize = Game.MapGrid.MapSize / 20;
        var squares = Game.MapGrid.MapSize / townSize;
        var optionsX = new List<int>();
        var optionsY = new List<int>();
        var townsCount = Mathf.Max(1, (Game.MapGrid.MapSize / (townSize * 2)) - 1);
        for (int i = 0; i < squares; i++)
        {
            optionsX.Add(i);
            optionsY.Add(i);
        }

        for (int i = 0; i < townsCount; i++)
        {
            var x = optionsX[Random.Range(0, optionsX.Count - 1)];
            var y = optionsY[Random.Range(0, optionsY.Count - 1)];
            optionsX.Remove(x);
            optionsY.Remove(y);

            var cell = Game.MapGrid.GetCellAtCoordinate(new Coordinates(x * townSize, y * townSize));
            var radius = Random.Range(Town.MinStructureSize, Town.MaxStructureSize);
            var cores = Random.Range(4, 8);

            var town = new Town(cell, cores, radius);

            if (town.ValidatePosition())
            {
                i--;
                continue;
            }

            towns.Add(town);
        }

        foreach (var town in towns)
        {
            town.Generate();
        }

        foreach (var town in towns)
        {
            LinkTowns(towns, town);
        }
    }

    public void MakeRoad(CellData point1, CellData point2, bool includeEnds = true, RoadSize roadSize = RoadSize.Double)
    {
        var path = Pathfinder.FindPath(point1.GetRandomNeighbor(),
                                                 point2.GetRandomNeighbor(),
                                                 Mobility.AbyssWalk);

        if (path == null || path.Count == 0)
        {
            return;
        }

        if (!includeEnds)
        {
            if (path.Contains(point1))
                path.Remove(point1);

            if (path.Contains(point2))
                path.Remove(point2);
        }

        foreach (var cell in path)
        {
            cell.SetStructure(Game.StructureController.GetStructure("Road", FactionController.WorldFaction));

            Direction[] dirs;
            switch (roadSize)
            {
                case RoadSize.Single:
                    dirs = new Direction[] { };
                    break;

                case RoadSize.Double:
                    dirs = new Direction[] { Direction.N, Direction.W };
                    break;

                case RoadSize.Triple:
                    dirs = new Direction[] { Direction.N, Direction.W, Direction.E, Direction.S };
                    break;

                default:
                    throw new Exception("Unknown road type");
            }

            foreach (var dir in dirs)
            {
                try
                {
                    var neighbour = cell.GetNeighbor(dir);
                    if (neighbour == null || neighbour.TravelCost < 0 || neighbour.Structure != null)
                    {
                        var tempDir = Direction.E;
                        if (dir == Direction.N)
                        {
                            tempDir = Direction.S;
                        }

                        neighbour = cell.GetNeighbor(tempDir);
                    }

                    if (neighbour == null || neighbour.TravelCost < 0 || neighbour.Structure != null)
                    {
                        continue;
                    }

                    neighbour.SetStructure(Game.StructureController.GetStructure("Road", FactionController.WorldFaction));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Road fail: {ex}");
                }
            }
        }
    }

    public Structure MakeRune(CellData location, string name, Faction faction)
    {
        Game.MapGrid.BindCell(location, faction.Core);

        if (location.Structure != null)
            Game.StructureController.DestroyStructure(location.Structure);

        var rune = Game.StructureController.GetStructure(name, faction);
        Game.MapGrid.BindCell(location, rune);
        location.SetStructure(rune);

        if (name == "BindRune")
        {
            foreach (var c in Game.MapGrid.BleedGroup(Game.MapGrid.GetCircle(location.Coordinates, Random.Range(4, 7))))
            {
                Game.MapGrid.BindCell(c, rune);
            }
        }

        if (rune.Spell != null)
        {
            Game.MagicController.AddRune(rune);
        }

        return rune;
    }

    public void SpawnCreatures()
    {
        MakeFactionCore(Game.MapGrid.Center, FactionController.PlayerFaction);

        for (int i = 0; i < 3; i++)
        {
            Game.CreatureController.SpawnCreature(Game.CreatureController.GetCreatureOfType("Person"),
                                         Game.MapGrid.Center.GetNeighbor(Helpers.RandomEnumValue<Direction>()).Coordinates,
                                         FactionController.PlayerFaction);
        }

        Game.CameraController.MoveToCell(Game.MapGrid.Center.GetNeighbor(Direction.E));
    }

    internal void GenerateMapFromPreset()
    {
        Game.MapGrid.Cells = new List<CellData>();

        for (var y = 0; y < Game.MapGrid.MapSize; y++)
        {
            for (var x = 0; x < Game.MapGrid.MapSize; x++)
            {
                CreateCell(x, y);
            }
        }

        LinkNeighbours();

        if (Game.MapGrid.Seed == 0)
        {
            Game.MapGrid.Seed = Random.Range(1, 10000);
        }

        GenerateMapCells();

        ResetSearchPriorities();
    }

    internal void LinkNeighbours()
    {
        for (var y = 0; y < Game.MapGrid.MapSize; y++)
        {
            for (var x = 0; x < Game.MapGrid.MapSize; x++)
            {
                var cell = Game.MapGrid.CellLookup[(x, y)];

                if (x > 0)
                {
                    cell.SetNeighbor(Direction.W, Game.MapGrid.CellLookup[(x - 1, y)]);

                    if (y > 0)
                    {
                        cell.SetNeighbor(Direction.SW, Game.MapGrid.CellLookup[(x - 1, y - 1)]);

                        if (x < Game.MapGrid.MapSize - 1)
                        {
                            cell.SetNeighbor(Direction.SE, Game.MapGrid.CellLookup[(x + 1, y - 1)]);
                        }
                    }
                }

                if (y > 0)
                {
                    cell.SetNeighbor(Direction.S, Game.MapGrid.CellLookup[(x, y - 1)]);
                }
            }
        }
    }

    internal void Make()
    {
        var sw = new Stopwatch();

        sw.Start();

        MapPreset = new MapPreset((0.80f, CellType.Mountain),
                                  (0.7f, CellType.Stone),
                                  (0.5f, CellType.Forest),
                                  (0.3f, CellType.Grass),
                                  (0.2f, CellType.Dirt),
                                  (0.0f, CellType.Water));

        GenerateMapFromPreset();
        Debug.Log($"Generated map in {sw.Elapsed}");
        sw.Restart();

        GenerateTowns();
        Debug.Log($"Generated towns in {sw.Elapsed}");
        sw.Restart();

        CreateLeyLines();
        Debug.Log($"Created ley lines in {sw.Elapsed}");
        sw.Restart();

        SpawnCreatures();
        Debug.Log($"Spawned creatures in {sw.Elapsed}");
        sw.Restart();

        SpawnMonsters();
        Debug.Log($"Spawned monsters in {sw.Elapsed}");
        sw.Restart();

        UpdateCells();
        Debug.Log($"Refreshed cells in {sw.Elapsed}");
    }

    private void UpdateCells()
    {
        var cells = Game.MapGrid.Cells;
        foreach (var cell in cells)
        {
            PopulateCell(cell);
        }

        var tiles = cells.Select(c => c.Tile).ToArray();
        var coords = cells.Select(c => c.Coordinates.ToVector3Int()).ToArray();
        Game.MapGrid.Tilemap.SetTiles(coords, tiles);
        Game.StructureController.DrawAllStructures();
    }



    internal void ResetSearchPriorities()
    {
        // ensure that all cells have their phases reset
        for (var y = 0; y < Game.MapGrid.MapSize; y++)
        {
            for (var x = 0; x < Game.MapGrid.MapSize; x++)
            {
                Game.MapGrid.CellLookup[(x, y)].SearchPhase = 0;
            }
        }
    }

    private static void SpawnMonsters()
    {
        for (int i = 0; i < Game.MapGrid.MapSize / 10; i++)
        {
            Game.CreatureController.SpawnCreature(Game.CreatureController.GetCreatureOfType("AbyssWraith"),
                                             Game.MapGrid.GetRandomCell().Coordinates,
                                             FactionController.MonsterFaction);
        }
    }

    private void CreateLeyLines()
    {
        var nexusPoints = new List<CellData>();
        for (int i = 0; i < Game.MapGrid.MapSize / 10; i++)
        {
            var point = Game.MapGrid.GetRandomCell();
            nexusPoints.Add(point);
            MakeRune(point, "LeySpring", FactionController.WorldFaction);
        }

        var v = Enum.GetValues(typeof(ManaColor));
        var counter = 0;
        foreach (var cell in nexusPoints)
        {
            var target = nexusPoints[(int)(Random.value * (nexusPoints.Count - 1))];
            Game.LeyLineController.MakeLine(Pathfinder.FindPath(cell, target, Mobility.Fly), (ManaColor)v.GetValue(counter));

            counter++;

            if (counter >= v.Length)
            {
                counter = 0;
            }
        }
    }

    private void GenerateMapCells()
    {
        for (int x = 0; x < Game.MapGrid.MapSize; x++)
        {
            for (int y = 0; y < Game.MapGrid.MapSize; y++)
            {
                var cell = Game.MapGrid
                    .GetCellAtCoordinate(new Coordinates(x, y));

                cell.Height = MapPreset.GetCellHeight(cell.Coordinates.X, cell.Coordinates.Y);
            }
        }
    }

    private void LinkTowns(List<Town> towns, Town town)
    {
        int max = Random.Range(0, Mathf.Min(2, towns.Count));
        foreach (var otherTown in towns)
        {
            if (town == otherTown)
                continue;
            max--;
            MakeRoad(town.Center, otherTown.Center);

            if (max <= 0)
            {
                break;
            }
        }
    }

    internal void PopulateCell(CellData cell)
    {
        if (cell.Structure != null)
        {
            return;
        }
        var value = Random.value;
        var world = FactionController.Factions[FactionConstants.World];
        switch (cell.CellType)
        {
            case CellType.Grass:
                if (value > 0.8)
                {
                    cell.SetStructure(Game.StructureController.GetStructure("Bush", world));
                }
                break;

            case CellType.Forest:
                if (value > 0.95)
                {
                    cell.SetStructure(Game.StructureController.GetStructure("Tree", world));
                }
                else if (value > 0.8)
                {
                    cell.SetStructure(Game.StructureController.GetStructure("Bush", world));
                }
                break;
        }
    }

    private void MakeFactionCore(CellData center, Faction faction)
    {
        Game.MapGrid.Center.SetStructure(FactionController.PlayerFaction.Core);
        foreach (var cell in Game.MapGrid.GetCircle(center.Coordinates, 5))
        {
            cell.Binding = faction.Core;
        }
    }
}