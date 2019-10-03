﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class Bind : EffectBase
{
    public int Size;
    [JsonIgnore]
    private List<Cell> _affectAbleCells;

    public Bind()
    {
    }

    public Bind(int size)
    {
        Size = size;
    }

    public override int Range { get { return -1; } }
    public override bool DoEffect()
    {
        if (_affectAbleCells == null)
        {
            _affectAbleCells = Game.Map.GetCircle(AssignedEntity.Cell, Size)
                                       .OrderBy(c => c.DistanceTo(AssignedEntity.Cell))
                                       .ToList();
        }

        if (SubTasksComplete())
        {
            var cellToBind = _affectAbleCells.Find(c => !c.Bound);
            if (cellToBind != null)
            {
                Game.Map.BindCell(cellToBind, AssignedEntity);
            }
            return true;
        }

        return false;
    }
}