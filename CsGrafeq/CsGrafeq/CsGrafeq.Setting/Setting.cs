using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;
using Timer=System.Timers.Timer;

namespace CsGrafeq.Setting
{
    public partial class Setting
    {
        private static Timer _savingTimer = new();
        static Setting()
        {
            //Instance = new();
            _savingTimer.Interval = 1000;
            _savingTimer.Elapsed += (sender, e) =>
            {
                if (_propertyChanged)
                {
                    Save();
                    _propertyChanged = false;
                }
            };
            _savingTimer.Start();
            TryLoad(out var setting);
            Instance=setting;
            Instance.PropertyChanged += InstanceOnPropertyChanged;
        }
        private static bool _propertyChanged=false;
        private static void InstanceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            _propertyChanged = true;
        }

        static bool TryLoad(out Setting instance)
        {
            var settingFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Setting.json");
            instance = new();
            if (File.Exists(settingFilePath))
            {
                var json = File.ReadAllText(settingFilePath);
                try
                {
                    var setting = JsonSerializer.Deserialize<Setting>(json, SourceGenerationContext.Default.Setting);
                    if (setting != null)
                    {
                        instance = setting;
                        return true;
                    }
                }
                catch (Exception _)
                {
                    // ignored
                }
            }
            return false;
        }

        public static void Save()
        {
            var settingFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Setting.json");
            Console.WriteLine(settingFilePath);
            string json;
            try
            {
                json = JsonSerializer.Serialize(Instance.MemberwiseClone() as Setting, SourceGenerationContext.Default.Setting);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            Console.WriteLine(settingFilePath+"\r\n"+json);
            File.WriteAllText(settingFilePath, json);
        }
    }
    [JsonSerializable(typeof(Setting))]
    [JsonSourceGenerationOptions(WriteIndented = true)]
    internal partial class SourceGenerationContext : JsonSerializerContext
    { }
}
