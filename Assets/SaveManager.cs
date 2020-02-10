﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

public static class SaveManager
{
    public static Save SaveToLoad { get; set; }

    public static void Restart(Save save = null)
    {
        Game.Instance = null;
        SaveToLoad = save;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    public const string SaveDir = "Saves";

    public static void Load()
    {
        Game.TimeManager.Pause();

        var latest = Directory.EnumerateFiles(SaveDir).Last();
        var save = JsonConvert.DeserializeObject<Save>(File.ReadAllText(latest), new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
        });

        Restart(save);
    }

    public static void Save()
    {
        try
        {
            var serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };

            serializer.Converters.Add(new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter());

            var serializeSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
            };

            Directory.CreateDirectory(SaveDir);
            using (var sw = new StreamWriter($"{SaveDir}\\Save_{DateTime.Now.ToString("yyyy-mm-dd_HH-MM-ss")}.json"))
            using (var writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, MakeSave(), typeof(Save));
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to save: {ex}");
        }
    }

    public static Save MakeSave()
    {
        return new Save
        {
            Seed = Game.Map.Seed,
            Factions = Game.FactionController.Factions.Values.ToList(),
            Time = Game.TimeManager.Data,
            Items = Game.IdService.ItemLookup.Values.ToList(),
            CameraData = new CameraData(Game.CameraController.Camera),
            Rooms = Game.ZoneController.RoomZones,
            Stores = Game.ZoneController.StorageZones,
            Areas = Game.ZoneController.AreaZones,
            Chunks = Game.Map.Chunks.Values.Select(s => s.Data).ToList(),
        };
    }
}