﻿using UnityEngine;
using UnityEngine.UI;

public class ZoneInfoPanel : MonoBehaviour
{
    public InputField Name;
    public RoomPanel RoomPanel;
    public RestrictionPanel RestrictionPanel;
    public StoragePanel StoragePanel;
    internal ZoneBase CurrentZone;

    public void DeleteZone()
    {
        Game.ZoneController.Delete(CurrentZone);
        Hide();
    }

    public void DoneEditing()
    {
        Game.Controller.Typing = false;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        Game.Controller.Typing = false;
    }

    public void NameChanged()
    {
        Game.Controller.Typing = true;
        CurrentZone.Name = Name.text;
        Game.ZoneController.Refresh(CurrentZone);
    }

  

    internal void Show(ZoneBase selectedZone)
    {
        gameObject.SetActive(true);

        CurrentZone = selectedZone;

        RoomPanel.Hide();
        StoragePanel.Hide();
        RestrictionPanel.Hide();

        if (CurrentZone is RoomZone rz)
        {
            RoomPanel.Show(rz);
        }
        else if (CurrentZone is StorageZone sz)
        {
            StoragePanel.Show(sz);

        }
        else if (CurrentZone is RestrictionZone rez)
        {
            RestrictionPanel.Show(rez);
        }

        Name.text = CurrentZone.Name;
    }
}