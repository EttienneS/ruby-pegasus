﻿using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class Move : TaskBase
{
    public Coordinates TargetCoordinates;

    [JsonIgnore] private float _journeyLength;
    [JsonIgnore] private CellData _nextCell;
    [JsonIgnore] private List<CellData> _path = new List<CellData>();
    [JsonIgnore] private float _startTime;
    [JsonIgnore] private Vector3 _targetPos;

    public Move()
    {
    }

    public Move(Coordinates targetCoordinates, int maxSpeed = int.MaxValue)
    {
        TargetCoordinates = targetCoordinates;
        MaxSpeed = maxSpeed;

        Message = $"Moving to {TargetCoordinates}";
    }

    public int MaxSpeed { get; set; }

    public override bool Done()
    {
        return Creature.Coordinates == TargetCoordinates;
    }

    public override void Update()
    {
        if (Creature.Coordinates == TargetCoordinates)
            return;

        if (_nextCell == null)
        {
            var currentCreatureCell = MapGrid.Instance.GetCellAtCoordinate(Creature.Coordinates);

            if (_path == null || _path.Count == 0)
            {
                _path = Pathfinder.FindPath(currentCreatureCell, MapGrid.Instance.GetCellAtCoordinate(TargetCoordinates));
            }

            if (_path == null)
            {
                throw new TaskFailedException("Unable to find path");
            }

            _nextCell = _path[_path.IndexOf(currentCreatureCell) - 1];
            if (_nextCell.TravelCost < 0)
            {
                // something changed the path making it unusable
                _path = null;
            }
            else
            {
                // found valid next cell
                _targetPos = _nextCell.Coordinates.ToMapVector();

                // calculate the movement journey to the next cell, include the cell travelcost to make moving through
                // difficults cells take longer
                _journeyLength = Vector3.Distance(currentCreatureCell.Coordinates.ToMapVector(), _targetPos) + _nextCell.TravelCost;

                Creature.MoveDirection = MapGrid.Instance.GetDirection(currentCreatureCell, _nextCell);
                _startTime = Time.time;
            }
        }

        if (_nextCell != null && Creature.LinkedGameObject.transform.position != _targetPos)
        {
            // move between two cells
            var distCovered = (Time.time - _startTime) * Mathf.Min(Creature.Speed, MaxSpeed);
            var fracJourney = distCovered / _journeyLength;
            Creature.LinkedGameObject.transform.position = Vector3.Lerp(Creature.CurrentCell.Coordinates.ToMapVector(),
                                      _targetPos,
                                      fracJourney);
        }
        else
        {
            // reached next cell
            Creature.Coordinates = _nextCell.Coordinates;
            _nextCell = null;
            _path = null;
        }
    }
}