﻿using Assets.ServiceLocator;
using System.Collections.Generic;
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

public class TimeManager : MonoBehaviour, IGameService
{
    public List<(int min, int max, Color start, Color end)> ColorZones;

    public TimeData Data = new TimeData()
    {
        Hour = 6,
        Minute = 0
    };

    public Light GlobalLight;

    public float LightAngleY = 30f;
    public float LightAngleZ = 30f;
    public float MaxLightAngle = 160f;
    public float MinLightAngle = 20f;

    private TimeStep _timeStep;
    private float _timeTicks = int.MaxValue;

    public string Now
    {
        get
        {
            return $"{Data.Hour}:{Data.Minute}:{_timeTicks}";
        }
    }

    internal bool Paused
    {
        get
        {
            return _timeStep == TimeStep.Paused;
        }
    }

    public TimeStep GetTimeStep()
    {
        return _timeStep;
    }

    public void SetTimeStep(TimeStep timeStep)
    {
        _timeStep = timeStep;

        if (_timeStep == TimeStep.Paused)
        {
            Loc.GetGameController().Paused = true;
            Time.timeScale = 0.000000001f;
            Time.fixedDeltaTime = 0.02f;
        }
        else
        {
            Loc.GetGameController().Paused = false;
            Time.timeScale = (int)_timeStep;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }

    public int StartTimer(int totalMinutes)
    {
        return Data.CreateTimer(totalMinutes);
    }

    public void Update()
    {
        _timeTicks += Time.deltaTime;

        if (_timeTicks >= 1)
        {
            _timeTicks = 0;

            Data.Minute++;
            Data.UpdateTimers();

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

    internal Timer GetTimer(int timerId)
    {
        return Data.GetTimer(timerId);
    }

    internal void Pause()
    {
        SetTimeStep(TimeStep.Paused);
    }

    private void UpdateGlobalLight()
    {
        var (min, max, start, end) = ColorZones.First(c => Data.Hour >= c.min && Data.Hour < c.max);
        var range = max - min;
        var total = range * 60f;
        var current = ((Data.Hour - min) * 60) + Data.Minute;

        GlobalLight.color = Color.Lerp(start, end, current / total);

        if (Data.Hour < 4 || Data.Hour > 20)
        {
            GlobalLight.transform.localEulerAngles = new Vector3(MaxLightAngle, LightAngleY, LightAngleZ);
        }
        else
        {
            GlobalLight.transform.localEulerAngles = new Vector3(Mathf.Lerp(MinLightAngle, MaxLightAngle, ((Data.Hour * 60) + Data.Minute) / 1440f), LightAngleY, LightAngleZ);
        }
    }

    public void Initialize()
    {
        SetTimeStep(TimeStep.Normal);

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

        UpdateGlobalLight();
    }
}