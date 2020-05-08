﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TimeStep
{
    Paused = 0,
    Slow = 1,
    Normal = 4,
    Fast = 12,
    Hyper = 50
}

public class TimeData
{
    public int Hour;
    public int Minute;
}

public class TimeManager : MonoBehaviour
{
    public List<(int min, int max, Color start, Color end)> ColorZones;

    public TimeData Data = new TimeData()
    {
        Hour = 6,
        Minute = 0
    };

    internal float CreatureTick = 0.1f;

    private TimeStep _timeStep;

    private float _timeTicks;

    public string Now
    {
        get
        {
            return $"{Data.Hour}:{Data.Minute}:{_timeTicks}";
        }
    }

    public TimeStep TimeStep
    {
        get
        {
            return _timeStep;
        }
        set
        {
            _timeStep = value;
            if (_timeStep == TimeStep.Paused)
            {
                // camera and other systems run on fixedDeltaTime, ensure they are always set to something
                Time.timeScale = 0.000000001f;
                Time.fixedDeltaTime = 0.02f;
                Game.Instance.Paused = true;
            }
            else
            {
                Time.timeScale = ((int)_timeStep) * 0.25f;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
                Game.Instance.Paused = false;
            }
        }
    }

    internal bool Paused
    {
        get
        {
            return TimeStep == TimeStep.Paused;
        }
    }

    public void Awake()
    {
        TimeStep = TimeStep.Normal;

        var light = Color.white;
        var dark = ColorConstants.DarkBlueAccent;
        ColorZones = new List<(int min, int max, Color start, Color end)>
        {
            (0,4, dark, dark),
            (4,7, dark, light),
            (7,17, light, light),
            (17,22, light, dark),
            (22,24, dark, dark)
        };
    }

    public void Update()
    {
        _timeTicks += Time.deltaTime;

        if (_timeTicks >= (CreatureTick * 5))
        {
            _timeTicks = 0;
            Data.Minute += 5;

            if (Data.Minute >= 60)
            {
                Data.Hour++;
                Data.Minute = 0;

                if (Data.Hour > 23)
                {
                    Data.Hour = 0;
                }
            }
            UpdateGlobalLight();
        }
    }

    internal void Pause()
    {
        TimeStep = TimeStep.Paused;
    }

    private void UpdateGlobalLight()
    {
        var (min, max, start, end) = ColorZones.First(c => Data.Hour >= c.min && Data.Hour < c.max);
        var range = max - min;
        var total = range * 60f;
        var current = ((Data.Hour - min) * 60) + Data.Minute;
        Game.Instance.Map.GlobalLight.color = Color.Lerp(start, end, current / total);

        var y = -35f;
        var minx = -30f;
        var maxx = -100f;
        if (Data.Hour < 4 || Data.Hour > 20)
        {
            Game.Instance.Map.GlobalLight.transform.localEulerAngles = new Vector3(maxx, y, 0);
        }
        else
        {
            Game.Instance.Map.GlobalLight.transform.localEulerAngles = new Vector3(Mathf.Lerp(minx, maxx, ((Data.Hour * 60) + Data.Minute) / 1440f), y, 0);
        }
    }
}