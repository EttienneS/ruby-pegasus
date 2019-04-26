﻿using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CreatureInfoPanel : MonoBehaviour
{
    

    public Text CreatureName;
    public Text Hunger;
    public Text Thirst;
    public Text Energy;
    public Text Task;

    private bool _firstRun = true;

    public void Start()
    {
        if (_firstRun)
        {
            var children = GetComponentsInChildren<Text>().ToList();
            CreatureName = children.First(t => t.name == "CreatureName");
            Hunger = children.First(t => t.name == "Hunger");
            Thirst = children.First(t => t.name == "Thirst");
            Energy = children.First(t => t.name == "Energy");
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
            Hunger.text = CurrentCreature.Data.Hunger.ToString("0");
            Thirst.text = CurrentCreature.Data.Thirst.ToString("0");
            Energy.text = CurrentCreature.Data.Energy.ToString("0");

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