﻿using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CreatureInfoPanel : MonoBehaviour
{
    public Text CreatureName;
    public Text Properties;
    public Text Task;

    private bool _firstRun = true;

    public void Start()
    {
        if (_firstRun)
        {
            var children = GetComponentsInChildren<Text>().ToList();
            CreatureName = children.First(t => t.name == "CreatureName");
            Properties = children.First(t => t.name == "Properties");
            Task = children.First(t => t.name == "Task");

            _firstRun = true;
        }
    }

    public Creature CurrentCreature;

    public void Update()
    {
        if (CurrentCreature != null)
        {
            CreatureName.text = CurrentCreature.Data.Name;
            Properties.text = string.Empty;
            foreach (var property in CurrentCreature.Data.ValueProperties)
            {
                Properties.text += $"{property.Key}:\t{property.Value.ToString()}\n";
            }

            foreach (var property in CurrentCreature.Data.StringProperties)
            {
                Properties.text += $"{property.Key}:\t{property.Value}\n";
            }

            if (CurrentCreature.Data.Task != null)
            {
                Task.text = CurrentCreature.Data.Task.ToString();
            }
            else
            {
                Task.text = "Finding Task";
            }
        }
    }

    public void Show(Creature creature)
    {
        Game.CellInfoPanel.Hide();
        gameObject.SetActive(true);
        CurrentCreature = creature;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}