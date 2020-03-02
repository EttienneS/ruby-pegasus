﻿using LPC.Spritesheet.Generator;
using LPC.Spritesheet.Generator.Enums;
using LPC.Spritesheet.Generator.Interfaces;
using Needs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Animation = LPC.Spritesheet.Generator.Interfaces.Animation;
using Random = UnityEngine.Random;

public enum Mobility
{
    Walk, Fly
}

public class Creature : IEntity
{
    public const string SelfKey = "Self";

    public Animation Animation = Animation.Walk;

    public float AnimationDelta = 0f;

    [JsonIgnore]
    public List<Creature> Combatants = new List<Creature>();

    public Direction Facing = Direction.S;

    public Animation? FixedAnimation = null;

    public int? FixedFrame;

    public Gender Gender;

    [JsonIgnore]
    public Behaviours.GetBehaviourTaskDelegate GetBehaviourTask;

    public List<OffensiveActionBase> IncomingAttacks = new List<OffensiveActionBase>();

    public List<Feeling> Feelings = new List<Feeling>();

    [JsonIgnore]
    public int Mood
    {
        get
        {
            return Feelings.Sum(f => f.MoodImpact);
        }
    }


    [JsonIgnore]
    public string MoodString
    {
        get
        {
            if (Mood > 80)
            {
                return "Ecstatic";
            }
            else if (Mood > 50)
            {
                return "Very Happy";
            }
            else if (Mood > 20)
            {
                return "Happy";
            }
            else if (Mood > -20)
            {
                return "Fine";
            }
            else if (Mood > -40)
            {
                return "Sad";
            }
            else if (Mood > -60)
            {
                return "Very Sad";
            }
            else if (Mood > -80)
            {
                return "Terrible";
            }

            return "Fine";
        }
    }

    public Mobility Mobility;

    public bool Moving;
    public Race Race;
    public (float x, float y) TargetCoordinate;
    public bool UnableToFindPath;
    internal int Frame;
    internal float InternalTick = float.MaxValue;

    [JsonIgnore]
    internal Cell LastPercievedCoordinate;

    [JsonIgnore]
    private List<Cell> _awareness;

    private CharacterSpriteSheet _characterSpriteSheet;
    private Faction _faction;
    private List<Cell> _path = new List<Cell>();
    private ICharacterSpriteDefinition _spriteDef;
    private CreatureTask _task;

    public float Aggression { get; set; }

    [JsonIgnore]
    public List<Cell> Awareness
    {
        get
        {
            if (_awareness == null && Cell != null)
            {
                _awareness = Game.Map.GetCircle(Cell, Perception);
            }

            return _awareness;
        }
    }

    public string BehaviourName { get; set; }

    public List<string> CarriedItemIds { get; set; } = new List<string>();

    [JsonIgnore]
    public Cell Cell
    {
        get
        {
            return Game.Map.GetCellAtCoordinate(X, Y);
        }
        set
        {
            X = Cell.Vector.x;
            Y = Cell.Vector.y;

            CreatureRenderer?.UpdatePosition();
        }
    }

    [JsonIgnore]
    public CreatureRenderer CreatureRenderer { get; set; }

    public bool Dead { get; internal set; }

    public int Dexterity { get; set; }

    [JsonIgnore]
    public Faction Faction
    {
        get
        {
            if (_faction == null)
            {
                _faction = Game.FactionController.Factions[FactionName];
            }

            return _faction;
        }
    }

    public string FactionName { get; set; }

    public string Id { get; set; }

    [JsonIgnore]
    public bool InCombat
    {
        get
        {
            return Combatants.Count > 0;
        }
    }

    public List<Limb> Limbs { get; set; }

    public List<VisualEffectData> LinkedVisualEffects { get; set; } = new List<VisualEffectData>();

    public List<string> LogHistory { get; set; }

    public ManaPool ManaPool { get; set; }

    public string Name { get; set; }

    public List<NeedBase> Needs { get; set; }

    public int Perception { get; set; }

    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

    public List<Skill> Skills { get; set; }

    public float Speed { get; set; }

    public string Sprite { get; set; }

    public int Strenght { get; set; }

    public CreatureTask Task
    {
        get
        {
            return _task;
        }
        set
        {
            _task?.Destroy();
            _task = value;
        }
    }

    public Dictionary<string, float> ValueProperties { get; set; } = new Dictionary<string, float>();

    [JsonIgnore]
    public Vector2 Vector
    {
        get
        {
            return new Vector2(X, Y);
        }
    }

    public int Vitality { get; set; }

    public float X { get; set; }

    public float Y { get; set; }

    [JsonIgnore]
    internal CharacterSpriteSheet CharacterSpriteSheet
    {
        get
        {
            if (_characterSpriteSheet == null)
            {
                if (_spriteDef == null)
                {
                    _spriteDef = Game.SpriteStore.Generator.GetBaseCharacter(Gender, Race);
                    Game.SpriteStore.Generator.AddClothes(_spriteDef);
                }

                _characterSpriteSheet = new CharacterSpriteSheet(_spriteDef);
            }
            return _characterSpriteSheet;
        }
    }

    [JsonIgnore]
    private IEnumerable<Item> CarriedItems
    {
        get
        {
            return CarriedItemIds.Select(i => i.GetItem());
        }
    }

    public void AddLimb(Limb limb)
    {
        limb.Owner = this;
        Limbs.Add(limb);
    }

    public void ClearFixedAnimation()
    {
        FixedAnimation = null;
        FixedFrame = null;
    }

    public Item DropItem(Cell cell, string item, int amount)
    {
        var heldItem = GetItemOfType(item);
        if (heldItem == null)
        {
            throw new Exception("Cannot drop what you do not have!");
        }
        else
        {
            if (amount >= heldItem.Amount)
            {
                CarriedItemIds.Remove(heldItem.Id);
                heldItem.Coords = (cell.Vector.x, cell.Vector.y);
                heldItem.InUseById = null;

                heldItem.Renderer.SpriteRenderer.sortingLayerName = LayerConstants.Item;
                heldItem.Cell = cell;
                return heldItem;
            }
            else
            {
                var newItem = Game.ItemController.SpawnItem(item, cell);
                newItem.Amount = amount;
                heldItem.Amount -= amount;

                newItem.Cell = cell;
                return newItem;
            }
        }
    }

    public void Face(Cell cell)
    {
        if (cell.Y < Cell.Y)
        {
            Facing = Direction.S;
        }
        else if (cell.Y > Cell.Y)
        {
            Facing = Direction.N;
        }
        else if (cell.X < Cell.X)
        {
            Facing = Direction.W;
        }
        else if (cell.X > Cell.X)
        {
            Facing = Direction.E;
        }
        else
        {
            // default
            Facing = Direction.S;
        }
    }

    public void GainSkill(string skillName, float amount)
    {
        var skill = GetSkill(skillName);

        if (skill == null)
        {
            skill = new Skill(skillName);
            Skills.Add(skill);
        }

        skill.Level += amount;
    }

    public List<OffensiveActionBase> GetAvailableOffensiveOptions()
    {
        return Limbs
                    .Where(l => !l.Disabled && !l.Busy)
                    .SelectMany(l => l.OffensiveActions)
                    .ToList();
    }

    public float GetCurrentNeed<T>() where T : NeedBase
    {
        return GetNeed<T>().Current;
    }

    public List<OffensiveActionBase> GetDefendableIncomingAttacks()
    {
        return IncomingAttacks.Where(a => GetPossibleDefensiveActions(a).Count > 0).ToList();
    }

    public List<DefensiveActionBase> GetDefensiveOptions()
    {
        return Limbs
                .Where(l => !l.Disabled && !l.Busy)
                .SelectMany(l => l.DefensiveActions)
                .ToList();
    }

    public List<OffensiveActionBase> GetInRangeOffensiveOptions(Creature target)
    {
        var offensiveActions = GetAvailableOffensiveOptions();
        var distance = Cell.DistanceTo(target.Cell);
        return offensiveActions.Where(o => o.Range >= distance).ToList();
    }

    public Item GetItemOfType(string itemType)
    {
        var item = CarriedItems.FirstOrDefault(i => i?.IsType(itemType) == true);
        CarriedItemIds.RemoveAll(c => !Game.IdService.ItemIdLookup.ContainsKey(c));
        return item;
    }

    public int GetMinRange()
    {
        var options = GetAvailableOffensiveOptions();
        if (options.Count == 0)
        {
            return 1;
        }
        return options.Max(o => o.Range);
    }

    public NeedBase GetNeed<T>() where T : NeedBase
    {
        return Needs.OfType<T>().FirstOrDefault();
    }

    public float GetNeedMax<T>() where T : NeedBase
    {
        return GetNeed<T>().Max;
    }

    public Orientation GetOrientation()
    {
        switch (Facing)
        {
            case Direction.N:
            case Direction.SE:
            case Direction.NE:
                return Orientation.Back;

            case Direction.E:
                return Orientation.Right;

            case Direction.SW:
            case Direction.NW:
            case Direction.S:
                return Orientation.Front;

            case Direction.W:
                return Orientation.Left;

            default:
                return Orientation.Front;
        }
    }

    public List<DefensiveActionBase> GetPossibleDefensiveActions(OffensiveActionBase action)
    {
        return GetDefensiveOptions().Where(d => d.ActivationTIme <= action.TimeToComplete()).ToList();
    }

    public Skill GetSkill(string skillName)
    {
        return Skills.Find(s => s.Name == skillName);
    }

    public float GetSkillLevel(string skillName)
    {
        var skill = GetSkill(skillName);

        if (skill == null)
        {
            return 5; // untyped
        }

        if (skill.Enabled != true)
        {
            return float.MinValue;
        }

        return skill.Level;
    }

    public Wound GetWorstWound()
    {
        return Limbs.SelectMany(l => l.Wounds)
                    .Where(w => !w.Treated)
                    .OrderByDescending(w => w.Danger)
                    .FirstOrDefault();
    }

    public bool HasSkill(string skillName)
    {
        if (string.IsNullOrEmpty(skillName))
        {
            return true; // unskilled
        }
        var skill = GetSkill(skillName);
        return skill?.Enabled == true;
    }

    public void Log(string message = "")
    {
        Debug.Log(Name + ":" + message);
        LogHistory.Add(message);

        if (FactionName == FactionConstants.Player)
        {
            CreatureRenderer.ShowText(message, 0.75f);
        }
    }

    public void PickUpItem(Item item, int amount)
    {
        var heldItem = GetItemOfType(item.Name);

        if (heldItem == null)
        {
            if (item.Amount > amount)
            {
                var newItem = Game.ItemController.SpawnItem(item.Name, Cell);
                newItem.Amount = amount;
                item.Amount -= amount;

                CarriedItemIds.Add(newItem.Id);
            }
            else
            {
                CarriedItemIds.Add(item.Id);
                item.InUseBy = this;
            }
        }
        else
        {
            heldItem.Amount += amount;
            item.Amount -= amount;
        }
    }

    public void RefreshSprite()
    {
        _characterSpriteSheet = null;
    }

    public void SetAnimation(Animation animation, float duration)
    {
        Animation = animation;
        AnimationDelta = duration;
    }

    public void SetFixedAnimation(Animation animation, int? fixedFrame = null)
    {
        FixedAnimation = animation;
        FixedFrame = fixedFrame;
    }

    public void SetTargetCoordinate(float targetX, float targetY)
    {
        TargetCoordinate = (targetX, targetY);
        _path = null;
        UnableToFindPath = false;
    }

    public override string ToString()
    {
        var text = $"{Name}\n";

        foreach (var limb in Limbs)
        {
            text += $"\t{limb}\n";
        }
        text += "\n";
        foreach (var need in Needs)
        {
            text += $"\t{need}\n";
        }

        return text;
    }

    public void UpdateSprite()
    {
        if (Sprite == "Creature")
        {
            if (FixedAnimation != null)
            {
                if (FixedFrame.HasValue)
                {
                    Frame = FixedFrame.Value;
                }

                CreatureRenderer.MainRenderer.sprite = CharacterSpriteSheet.GetFrame(FixedAnimation.Value, GetOrientation(), ref Frame);
            }
            else
            {
                if (Animation == Animation.Walk && !Moving)
                {
                    // standing still, stay on frame 0
                    Frame = 0;
                }
                CreatureRenderer.MainRenderer.sprite = CharacterSpriteSheet.GetFrame(Animation, GetOrientation(), ref Frame);
            }

            return;
        }
        else
        {
            bool flip = Facing == Direction.W || Facing == Direction.NE || Facing == Direction.SW;
            CreatureRenderer.MainRenderer.flipX = flip;
            if (!Sprite.Contains("_"))
            {
                CreatureRenderer.MainRenderer.sprite = Game.SpriteStore.GetCreatureSprite(Sprite, ref Frame);
            }
            else
            {
                CreatureRenderer.MainRenderer.sprite = Game.SpriteStore.GetSprite(Sprite);
            }
        }
    }

    internal void CancelTask()
    {
        if (Task != null)
        {
            Log($"Canceled {Task} task");
            Faction.RemoveTask(Task);
            Task.Destroy();
            Task = null;
        }
    }

    internal bool CanDo(CreatureTask t)
    {
        if (string.IsNullOrEmpty(t.RequiredSkill))
        {
            return true;
        }
        if (HasSkill(t.RequiredSkill) && GetSkillLevel(t.RequiredSkill) >= t.RequiredSkillLevel)
        {
            foreach (var subTask in t.SubTasks)
            {
                if (!CanDo(subTask))
                {
                    Debug.LogError($"{Name} cant do: {subTask.GetType().Name}");
                    return false;
                }
            }
        }

        return true;
    }

    internal int CurrentItemCount(string itemType)
    {
        var item = GetItemOfType(itemType);
        if (item == null)
        {
            return 0;
        }
        return item.Amount;
    }

    internal int GetPriority(CreatureTask t)
    {
        if (string.IsNullOrEmpty(t.RequiredSkill))
        {
            return 5; // untyped return baseline
        }

        if (HasSkill(t.RequiredSkill))
        {
            return GetSkill(t.RequiredSkill).Priority;
        }

        return 0;
    }

    internal bool HasItem(string itemId)
    {
        return CarriedItems.FirstOrDefault(i => i?.Id.Equals(itemId, StringComparison.InvariantCultureIgnoreCase) == true) != null;
    }

    internal bool HasItem(string itemType, int amount)
    {
        var item = GetItemOfType(itemType);
        if (item != null)
        {
            return item.Amount >= amount;
        }
        return false;
    }

    internal void Live(float delta)
    {
        UpdateLimbs(delta);

        foreach (var need in Needs)
        {
            need.ApplyChange(delta);
            need.Update();
        }

        foreach (var feeling in Feelings.ToList())
        {
            if (feeling.DurationLeft <= -1f)
            {
                continue;
            }
            feeling.DurationLeft -= delta;

            if (feeling.DurationLeft <= 0)
            {
                Feelings.Remove(feeling);
            }
        }
    }

    internal void Perceive()
    {
        if (LastPercievedCoordinate != Cell)
        {
            _awareness = null;
            LastPercievedCoordinate = Cell;
        }
    }

    internal void Start()
    {
        foreach (var limb in Limbs)
        {
            limb.Link(this);
        }

        foreach (var need in Needs)
        {
            need.Creature = this;
        }

        LogHistory = new List<string>();

        ManaPool.EntityId = Id;
        TargetCoordinate = (Cell.X, Cell.Y);
    }

    internal bool Update(float timeDelta)
    {
        if (!Game.Instance.Ready)
            return false;

        if (Game.TimeManager.Paused)
            return false;

        InternalTick += timeDelta;
        if (InternalTick >= Game.TimeManager.CreatureTick)
        {
            if (InCombat)
            {
                ClearFixedAnimation();
                if (Task != null)
                {
                    CancelTask();
                }
                ProcessCombat();
            }
            else
            {
                ProcessTask();
            }

            ResolveIncomingAttacks(timeDelta);
            Combatants.RemoveAll(c => c.Dead);

            foreach (var item in CarriedItems)
            {
                if (item == null)
                {
                    continue;
                }
                item.Renderer.SpriteRenderer.sortingLayerName = LayerConstants.CarriedItem;
                item.Coords = (X, Y);
            }


            Perceive();
            Live(InternalTick);
            UpdateSprite();
            Move();

            InternalTick -= Game.TimeManager.CreatureTick;
            return true;
        }

        if (AnimationDelta > 0)
        {
            AnimationDelta -= Time.deltaTime;
        }
        else
        {
            Animation = Animation.Walk;
        }

        return false;
    }

    private OffensiveActionBase GetBestAttack(Creature target)
    {
        var most = int.MinValue;
        OffensiveActionBase bestAttack = null;
        Limb bestTarget = null;

        foreach (var offensiveAction in GetInRangeOffensiveOptions(target))
        {
            foreach (var limb in target.Limbs)
            {
                if (limb.Disabled)
                    continue;

                offensiveAction.TargetLimb = limb;
                var prediction = offensiveAction.PredictDamage(target);

                if (limb.Vital)
                {
                    prediction = (int)(prediction * 2f * Aggression);
                }
                else
                {
                    prediction = (int)(prediction / Aggression);
                }

                if (prediction > most)
                {
                    most = prediction;
                    bestAttack = offensiveAction;
                    bestTarget = limb;
                }
            }
        }

        if (bestAttack != null)
        {
            bestAttack.TargetLimb = bestTarget;
        }

        return bestAttack;
    }

    private (int buffEffect, BuffBase bestBuff) GetBestBuff()
    {
        var most = int.MinValue;
        BuffBase bestBuff = null;

        foreach (var buff in GetBuffOptions())
        {
            var effect = buff.EstimateBuffEffect();
            if (effect == int.MinValue)
            {
                continue;
            }

            if (effect > most || (effect == most && Random.value > 0.5f))
            {
                most = effect;
                bestBuff = buff;
            }
        }

        return (most, bestBuff);
    }

    private List<BuffBase> GetBuffOptions()
    {
        return Limbs
             .Where(l => !l.Disabled && !l.Busy)
             .SelectMany(l => l.BuffActions)
             .ToList();
    }

    private (float defendedDamage, DefensiveActionBase bestDefense) GetDefense(OffensiveActionBase incomingAttack)
    {
        var defendedDamage = float.MinValue;
        DefensiveActionBase bestDefense = null;
        if (incomingAttack != null)
        {
            var incomingDamage = incomingAttack.PredictDamage(this);
            bestDefense = GetMostEffectiveDefensiveAction(incomingAttack);

            if (bestDefense != null)
            {
                defendedDamage = (incomingDamage - bestDefense.PredictDamageAfterDefense(incomingAttack)) / Aggression;
            }
        }

        return (defendedDamage, bestDefense);
    }

    private OffensiveActionBase GetMostDangerousUnblockedIncomingAttack()
    {
        var most = int.MinValue;
        OffensiveActionBase mostPowerfulAttack = null;
        foreach (var attack in IncomingAttacks.Where(a => a.DefensiveActions.Count == 0))
        {
            var dmg = attack.PredictDamage(this);
            if (dmg > most || (dmg == most && Random.value > 0.5f))
            {
                most = dmg;
                mostPowerfulAttack = attack;
            }
        }

        return mostPowerfulAttack;
    }

    private DefensiveActionBase GetMostEffectiveDefensiveAction(OffensiveActionBase attack)
    {
        if (attack != null)
        {
            var least = int.MaxValue;
            DefensiveActionBase mostEffectiveDefense = null;
            foreach (var def in GetPossibleDefensiveActions(attack))
            {
                var dmg = def.PredictDamageAfterDefense(attack);
                if (dmg < least || (dmg == least && Random.value > 0.5f))
                {
                    least = dmg;
                    mostEffectiveDefense = def;
                }
            }

            return mostEffectiveDefense;
        }
        return null;
    }

    private (float outgoingDamage, OffensiveActionBase bestAttack) GetOffense(Creature target)
    {
        var outgoingDamage = float.MinValue;
        var bestAttack = GetBestAttack(target);
        if (bestAttack != null)
        {
            outgoingDamage = bestAttack.PredictDamage(target) * Aggression;
        }

        return (outgoingDamage, bestAttack);
    }

    private void Move()
    {
        if (X == TargetCoordinate.x && Y == TargetCoordinate.y)
        {
            // no need to move
            Moving = false;
            return;
        }
        Moving = true;

        var targetCell = Game.Map.GetCellAtCoordinate(TargetCoordinate.x, TargetCoordinate.y);
        if (_path == null || _path.Count == 0)
        {
            _path = Pathfinder.FindPath(Game.Map.GetCellAtCoordinate(X, Y), targetCell, Mobility);
        }

        if (_path == null || _path.Count == 0)
        {
            UnableToFindPath = true;
            Debug.LogWarning("Unable to find path!");
            return;
        }

        var nextCell = _path[0];
        var targetX = nextCell.Vector.x;
        var targetY = nextCell.Vector.y;

        if (X == targetX && Y == targetY)
        {
            // reached the cell
            _path.RemoveAt(0);
            return;
        }

        var maxX = Mathf.Max(targetX, X);
        var minX = Mathf.Min(targetX, X);

        var maxY = Mathf.Max(targetY, Y);
        var minY = Mathf.Min(targetY, Y);

        var yspeed = Mathf.Min(Speed + Random.Range(0f, 0.01f), maxY - minY);
        var xspeed = Mathf.Min(Speed + Random.Range(0f, 0.01f), maxX - minX);

        if (targetY > Y)
        {
            Facing = Direction.N;
            Y += yspeed;
        }
        else if (targetY < Y)
        {
            Facing = Direction.S;
            Y -= yspeed;
        }

        if (targetX > X)
        {
            X += xspeed;

            if (Facing == Direction.N)
            {
                Facing = Direction.NE;
            }
            else if (Facing == Direction.S)
            {
                Facing = Direction.SE;
            }
            else
            {
                Facing = Direction.E;
            }
        }
        else if (targetX < X)
        {
            X -= xspeed;
            if (Facing == Direction.N)
            {
                Facing = Direction.NW;
            }
            else if (Facing == Direction.S)
            {
                Facing = Direction.SW;
            }
            else
            {
                Facing = Direction.W;
            }
        }

        CreatureRenderer.UpdatePosition();
    }

    private void ProcessCombat()
    {
        try
        {
            var target = Combatants[0];
            var incomingAttack = GetMostDangerousUnblockedIncomingAttack();

            var (defendedDamage, bestDefense) = GetDefense(incomingAttack);
            var (outgoingDamage, bestAttack) = GetOffense(target);
            var (boostEffect, bestBuff) = GetBestBuff();

            if (bestAttack == null && bestDefense == null && bestBuff == null)
            {
                var minRange = GetMinRange();

                foreach (var combatant in Combatants)
                {
                    if (Cell.DistanceTo(combatant.Cell) > minRange)
                    {
                        SetTargetCoordinate(combatant.X, combatant.Y);
                        break;
                    }
                }

                return;
            }

            if (boostEffect > outgoingDamage && boostEffect > defendedDamage)
            {
                bestBuff.Activate();
            }
            else if (outgoingDamage > defendedDamage)
            {
                // aggro
                Log($"{Name} launches a {bestAttack.Name} at {target.Name}'s {bestAttack.TargetLimb.Name}");
                Game.VisualEffectController.SpawnSpriteEffect(this, Vector, "axe_t", 1f).Fades();

                bestAttack.Reset();

                if (!target.Combatants.Contains(this))
                    target.Combatants.Add(this);

                target.IncomingAttacks.Add(bestAttack);
                bestAttack.Limb.Busy = true;
            }
            else
            {
                // defend
                Log($"{Name} defends with a {bestDefense.Name} against {incomingAttack.Owner.Name}'s {incomingAttack.Name}");

                incomingAttack.DefensiveActions.Add(bestDefense);
                bestDefense.Limb.Busy = true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    private void ProcessTask()
    {
        if (Task == null)
        {
            var task = Faction.TakeTask(this);

            if (task != null)
            {
                Task = task;
            }
        }
        else
        {
            try
            {
                if (!CanDo(Task))
                {
                    throw new Exception("Unable to do assigned task!");
                }

                if (Task.Done(this))
                {
                    if (!(Task is Idle))
                    {
                        Log($"Completed {Task} task");
                    }

                    Task.ShowDoneEmote(this);

                    Faction.RemoveTask(Task);
                    Task = null;
                }
                else
                {
                    if (Random.value > 0.8)
                    {
                        Task.ShowBusyEmote(this);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Task failed: {ex}");

                if (!Cell.Pathable(Mobility))
                {
                    // unstuck
                    Debug.LogError("Unstuck!");
                    Cell = Game.Map.GetNearestPathableCell(Cell, Mobility, 10);
                }
                else
                {
                    CancelTask();
                }
            }
        }
    }

    private void ResolveIncomingAttacks(float timeDelta)
    {
        foreach (var attack in IncomingAttacks.ToList())
        {
            if (attack.Owner.Dead)
            {
                attack.Reset();
                IncomingAttacks.Remove(attack);
            }
            else if (attack.Done(timeDelta))
            {
                attack.Resolve();
                attack.Reset();
                IncomingAttacks.Remove(attack);
            }
        }
    }

    private void UpdateLimbs(float timeDelta)
    {
        foreach (var boost in Limbs.ToList().SelectMany(l => l.BuffActions))
        {
            boost.Update(timeDelta);
        }

        foreach (var limb in Limbs)
        {
            limb.Update(timeDelta);
        }
    }
}