﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EntityInfoPanel : MonoBehaviour
{
    public Text BlackLabel;
    public Text BlueLabel;
    public GameObject ButtonPanel;
    public Text CreatureName;
    public List<IEntity> CurrentEntities;
    public Text GreenLabel;
    public ImageButton ImageButtonPrefab;
    public Text Log;
    public Text PropertiesPanel;
    public Text RedLabel;
    public Text WhiteLabel;
    public Text HealthText;
    public Text MagicText;

    public Toggle FirstPanelToggle;
    private List<ImageButton> _contextButtons = new List<ImageButton>();

    public ImageButton AddButton(string title, string spriteName)
    {
        var button = Instantiate(ImageButtonPrefab, ButtonPanel.transform);
        button.SetText(title);
        button.SetImage(Game.SpriteStore.GetSprite(spriteName));

        _contextButtons.Add(button);

        return button;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show(IEnumerable<IEntity> entities)
    {
        gameObject.SetActive(true);

        // switch to overview panel
        FirstPanelToggle.isOn = true;

        CurrentEntities = entities.ToList();

        _contextButtons.Clear();
        foreach (Transform child in ButtonPanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in PropertiesPanel.transform)
        {
            Destroy(child.gameObject);
        }

        if (entities.First() is Creature)
        {
            var creatures = entities.OfType<Creature>();
            AddMoveButton(creatures);
            AddAttackButton(creatures);
        }
        else
        {
            AddRemoveStructureButton(entities.OfType<Structure>());
            AddCycleModeButton(entities.OfType<Structure>());
        }
    }

    public void Update()
    {
        if (CurrentEntities != null)
        {
            PropertiesPanel.text = string.Empty;

            if (CurrentEntities.Count == 1)
            {
                var currentEntity = CurrentEntities[0];

                Log.text = string.Empty;

                foreach (var line in currentEntity.LogHistory)
                {
                    Log.text += $"{line}\n";
                }

                var rt = Log.GetComponent(typeof(RectTransform)) as RectTransform;
                rt.sizeDelta = new Vector2(280, currentEntity.LogHistory.Count * 25);

                CreatureName.text = currentEntity.Name;


                foreach (var property in currentEntity.ValueProperties)
                {
                    PropertiesPanel.text += $"{property.Key}:\t{property.Value.ToString("N0")}\n";
                }

                foreach (var property in currentEntity.Properties)
                {
                    PropertiesPanel.text += $"{property.Key}:\t{property.Value}\n";
                }

                if (currentEntity is Creature creature)
                {
                    PropertiesPanel.text += $"\nLocation:\t{creature.X:F1}:{creature.Y:F1}\n\n";
                    LogHealth(creature);
                    LogTask(creature);
                }
                else
                {
                    PropertiesPanel.text += $"\nLocation:\t{currentEntity.Cell}\n\n";
                    var structure = currentEntity as Structure;

                    PropertiesPanel.text += $"Rotation:\t {structure.Rotation}\n";

                    if (structure.IsBluePrint)
                    {
                        PropertiesPanel.text += "\n*Blueprint, waiting for construction...*\n";
                    }
                    else
                    {
                        if (structure.InUseByAnyone)
                        {
                            PropertiesPanel.text += $"In use by:\t{structure.InUseBy.Name}\n";
                        }

                        if (structure is Pipe pipe)
                        {
                            if (pipe.Attunement.HasValue)
                            {
                                PropertiesPanel.text += $"Attunment:\t{pipe.Attunement.Value}\n";
                            }
                            else
                            {
                                PropertiesPanel.text += $"Attunment:\tNone\n";
                            }
                        }
                    }
                }
            }
            else
            {
                CreatureName.text = $"{CurrentEntities.Count} entities";
                foreach (var entity in CurrentEntities)
                {
                    PropertiesPanel.text += $"- {entity.Name}\n";
                }
            }

            SetManaLabels();
        }
    }

    private void LogTask(Creature creature)
    {
        if (creature.Task != null)
        {
            if (string.IsNullOrWhiteSpace(creature.Task.Message))
            {
                PropertiesPanel.text += $"Task: \t{creature.Task}\n";
            }
            else
            {
                PropertiesPanel.text += $"Task: \t{creature.Task.Message}\n";
            }
        }
        else
        {
            PropertiesPanel.text += "Finding Task\n";
        }
    }

    private void LogHealth(Creature creature)
    {
        HealthText.text = "\nBody: \n\n";

        foreach (var limb in creature.Limbs)
        {
            HealthText.text += $"\t{limb}\n";
        }
        HealthText.text += "\n";

        HealthText.text += "\nSkills: \n\n";

        foreach (var skill in creature.Skills)
        {
            HealthText.text += $"\t{skill}\n";
        }
        HealthText.text += "\n";
    }

    private void AddAttackButton(IEnumerable<Creature> creatures)
    {
        var btn = AddButton(OrderSelectionController.AttackText, OrderSelectionController.AttackIcon);
        btn.SetOnClick(() => AttackClicked(creatures, btn));
    }

    private void AddCycleModeButton(IEnumerable<Structure> structures)
    {
        var prime = structures.First();

        if (!structures.All(s => s.Name == prime.Name && s.AutoInteractions.Count > 0))
        {
            return;
        }

        foreach (var interaction in prime.AutoInteractions)
        {
            if (string.IsNullOrEmpty(interaction.DisplayName))
            {
                continue;
            }

            var btn = AddButton($"{interaction.DisplayName} - {(interaction.Disabled ? "Off" : "On")}", interaction.Disabled ? "check_mark_t" : "quest_complete_t");
            btn.SetOnClick(() =>
            {
                SetActiveButton(btn);
                interaction.Disabled = !interaction.Disabled;

                foreach (var structure in structures)
                {
                    structure.AutoInteractions[prime.AutoInteractions.IndexOf(interaction)].Disabled = interaction.Disabled;
                }

                btn.Text.text = $"{interaction.DisplayName} - {(interaction.Disabled ? "Off" : "On")}";
                btn.Image.sprite = Game.SpriteStore.GetSprite(interaction.Disabled ? "check_mark_t" : "quest_complete_t");
            });
        }
    }

    private void AddMoveButton(IEnumerable<Creature> creatures)
    {
        var btn = AddButton(OrderSelectionController.MoveText, OrderSelectionController.MoveIcon);
        btn.SetOnClick(() => MoveClicked(creatures, btn));
    }

    private void AddRemoveStructureButton(IEnumerable<Structure> structures)
    {
        var btn = AddButton(OrderSelectionController.DefaultRemoveText, OrderSelectionController.DefaultRemoveImage);
        btn.SetOnClick(() =>
        {
            SetActiveButton(btn);

            foreach (var structure in structures)
            {
                if (structure.IsBluePrint)
                {
                    Game.StructureController.DestroyStructure(structure);
                }
                else
                {
                    if (Game.FactionController.PlayerFaction.AvailableTasks.OfType<RemoveStructure>().Any(t => t.StructureToRemove == structure))
                    {
                        Debug.Log("Structure already flagged to remove");
                    }
                    else
                    {
                        Game.FactionController.PlayerFaction
                                         .AddTask(new RemoveStructure(structure))
                                         .AddCellBadge(structure.Cell,
                                                       OrderSelectionController.DefaultRemoveImage);
                    }
                }
            }
        });
    }

    private void AttackClicked(IEnumerable<Creature> creatures, ImageButton btn)
    {
        SetActiveButton(btn);

        Game.Controller.SelectionPreference = SelectionPreference.Cell;
        Game.Controller.SetMouseSprite(OrderSelectionController.AttackIcon,
                                        (c) => c.GetEnemyCreaturesOf(FactionConstants.Player).Any());

        Game.OrderSelectionController.CellClickOrder = cells =>
        {
            foreach (var creature in creatures)
            {
                foreach (var cell in cells)
                {
                    foreach (var enemy in cell.GetEnemyCreaturesOf(creature.FactionName))
                    {
                        creature.CancelTask();
                        creature.Combatants.Add(enemy);
                        break;
                    }
                }
            }
        };
    }

    private void MoveClicked(IEnumerable<Creature> creatures, ImageButton btn)
    {
        SetActiveButton(btn);
        Game.Controller.SelectionPreference = SelectionPreference.Cell;
        Game.Controller.SetMouseSprite(OrderSelectionController.MoveIcon, (c) => c.TravelCost > 0);

        Game.OrderSelectionController.CellClickOrder = cells =>
        {
            foreach (var creature in creatures)
            {
                var cell = cells[0];

                var faction = creature.GetFaction();
                var task = new Move(cell);
                task.AddCellBadge(cell, OrderSelectionController.MoveIcon);
                creature.CancelTask();
                creature.Task = task;
            }
        };
    }

    private void SetActiveButton(ImageButton btn)
    {
        foreach (var button in _contextButtons)
        {
            button.Image.color = Color.white;
        }
        btn.Button.image.color = Color.red;
    }

    private void SetManaLabel(Text text, ManaColor col)
    {
        var total = 0;
        foreach (var entity in CurrentEntities)
        {
            if (entity.ManaPool.ContainsKey(col))
            {
                total += entity.ManaPool[col].Total;
            }
        }

        if (total > 0)
        {
            text.text = total.ToString();
        }
        else
        {
            text.text = "--";
        }
    }

    private void SetManaLabels()
    {
        SetManaLabel(RedLabel, ManaColor.Red);
        SetManaLabel(GreenLabel, ManaColor.Green);
        SetManaLabel(BlueLabel, ManaColor.Blue);
        SetManaLabel(BlackLabel, ManaColor.Black);
        SetManaLabel(WhiteLabel, ManaColor.White);

        MagicText.text = "";
    }
}