﻿using System.Collections.Generic;
using UnityEngine;

public class Move : ITask
{
    public Cell NextCell;
    public Creature Creature { get; set; }
    private float _journeyLength;
    private int _navigationFailureCount;
    private List<Cell> Path = new List<Cell>();
    private float startTime;
    private Vector3 targetPos;

    public Move(Cell targetCell, int maxSpeed = int.MaxValue)
    {
        TargetCell = targetCell;
        TaskId = $"Move to {TargetCell}";
        MaxSpeed = maxSpeed;
    }

    public Queue<ITask> SubTasks { get; set; }
    public Cell TargetCell { get; set; }
    public string TaskId { get; set; }

    public int MaxSpeed { get; set; }

    public bool Done()
    {
        return Creature.CurrentCell == TargetCell;
    }

    public override string ToString()
    {
        return $"Moving to {TargetCell}";
    }

    public void Update()
    {
        if (Creature.CurrentCell != TargetCell)
        {
            if (NextCell == null)
            {
                if (Path == null || Path.Count == 0)
                {
                    Path = Pathfinder.FindPath(Creature.CurrentCell, TargetCell);
                }

                if (Path == null)
                {
                    // failure, task is no longer possible
                    _navigationFailureCount++;

                    if (_navigationFailureCount > 10)
                    {
                        _navigationFailureCount = 0;
                        // failed to find a path too many times, short circuit
                        TargetCell = Creature.CurrentCell;
                        return;
                    }
                }

                NextCell = Path[Path.IndexOf(Creature.CurrentCell) - 1];
                if (NextCell.TravelCost < 0)
                {
                    // something changed the path making it unusable
                    Path = null;
                }
                else
                {
                    // found valid next cell
                    targetPos = NextCell.GetCreaturePosition();

                    // calculate the movement journey to the next cell, include the cell travelcost to make moving through
                    // difficults cells take longer
                    _journeyLength = Vector3.Distance(Creature.CurrentCell.transform.position, targetPos) + NextCell.TravelCost;

                    if (Creature.SpriteAnimator != null)
                    {
                        Creature.SpriteAnimator.MoveDirection = MapGrid.Instance.GetDirection(Creature.CurrentCell, NextCell);
                    }
                    startTime = Time.time;
                }
            }

            if (NextCell != null && Creature.transform.position != targetPos)
            {
                // move between two cells
                var distCovered = (Time.time - startTime) * Mathf.Min(Creature.Speed, MaxSpeed);
                var fracJourney = distCovered / _journeyLength;
                Creature.transform.position = Vector3.Lerp(Creature.CurrentCell.transform.position,
                                          targetPos,
                                          fracJourney);
            }
            else
            {
                // reached next cell
                NextCell.AddCreature(Creature);

                NextCell = null;
                Path = null;
            }
        }
    }
}