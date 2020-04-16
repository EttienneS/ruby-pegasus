﻿using Newtonsoft.Json;

public class Move : CreatureTask
{
    public float TargetX;
    public float TargetY;

    public override string Message
    {
        get
        {
            return $"Move to {TargetX}:{TargetY}";
        }
    }

    public Move()
    {
    }

    public override void Complete()
    {
    }

    public Move(Cell targetCoordinates) : this()
    {
        TargetX = targetCoordinates.Vector.x;
        TargetY = targetCoordinates.Vector.y;
    }

    [JsonIgnore]
    public Cell TargetCell
    {
        get
        {
            return Game.Instance.Map.GetCellAtCoordinate(TargetX, TargetY);
        }
    }

    public override bool Done(Creature creature)
    {
        if (creature.TargetCoordinate.x != TargetX || creature.TargetCoordinate.y != TargetY)
        {
            creature.SetTargetCoordinate(TargetX, TargetY);
        }
        if (creature.UnableToFindPath)
        {
            throw new TaskFailedException("Unable to find path");
        }
        if (creature.X == TargetX && creature.Y == TargetY)
        {
            // dynamic map expansion
            // Game.Instance.Map.ExpandChunksAround(creature.Cell);
            return true;
        }
        return false;
    }
}