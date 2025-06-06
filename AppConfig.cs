using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace ToDoList
{
    public enum TaskFilter
    {
        All,
        Pending,
        Completed
    }

    public sealed class AppConfig
    {
        private static readonly Lazy<AppConfig> _instance = new Lazy<AppConfig>(() => new AppConfig());
        private static readonly object _lock = new object();

        public TaskFilter CurrentFilter { get; set; } = TaskFilter.All;

        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ToDoList",
            "config.json");

        private AppConfig() 
        {
            Load();
        }

        public static AppConfig Instance => _instance.Value;

        private class AppConfigData
        {
            public TaskFilter CurrentFilter { get; set; }
        }

        private void Load()
        {
            try
            {
                string directory = Path.GetDirectoryName(ConfigPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    var loaded = JsonSerializer.Deserialize<AppConfigData>(json);
                    if (loaded != null)
                    {
                        CurrentFilter = loaded.CurrentFilter;
                    }
                }
                else
                {
                    Save();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading config: {ex.Message}\nPath: {ConfigPath}",
                    "Configuration Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                CurrentFilter = TaskFilter.All;
            }
        }

        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(ConfigPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var data = new AppConfigData { CurrentFilter = this.CurrentFilter };
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(data, options);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving config: {ex.Message}\nPath: {ConfigPath}",
                    "Configuration Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
} 