using BrokeProtocol.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CarSystem
{
    public class Main : Plugin
    {
        public static Main Instance;
        public Config config;

        public Main()
        {
            Instance = this;
            Info = new PluginInfo("Transport System for PalmStreet", "Csm");
            LoadJson();
            config = getJsonInfos();
        }

        public void LoadJson()
        {
            Debug.Log("[FuelSystem]: Initilazing Json Config ...");
            if (!Directory.Exists("./SkyrethPlugin/Fuel")) Directory.CreateDirectory("./SkyrethPlugin/Fuel");

            if (!File.Exists("./SkyrethPlugin/Fuel/config.json"))
            {
                Dictionary<string, int> tanks = new Dictionary<string, int>();
                tanks.Add("Car01", 1500);
                Config config = new Config()
                {
                    EmptyFuel = "Le véhicule n'a plus d'essence, descandez !",
                    blacklistedcars = new string[] { "Car01" },
                    fueltank = tanks
                };

                File.WriteAllText("./SkyrethPlugin/Fuel/config.json", JsonConvert.SerializeObject(config, Formatting.Indented));
            }
        }

        public Config getJsonInfos()
        {
            try
            {
                using (StreamReader reader = new StreamReader("./SkyrethPlugin/Fuel/config.json"))
                {
                    return JsonConvert.DeserializeObject<Config>(reader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }

            return null;
        }
    }

    public class Config
    {
        public string EmptyFuel { get; set; }

        public string[] blacklistedcars { get; set; }

        public Dictionary<string, int> fueltank { get; set; }
    }
}
