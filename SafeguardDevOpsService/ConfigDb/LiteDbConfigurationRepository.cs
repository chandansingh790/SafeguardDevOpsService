﻿using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using OneIdentity.SafeguardDevOpsService.Data;

namespace OneIdentity.SafeguardDevOpsService.ConfigDb
{
    internal class LiteDbConfigurationRepository : IConfigurationRepository, IDisposable
    {
        private bool _disposed;
        private LiteDatabase _configurationDb;
        private readonly LiteCollection<Setting> _settings;
        private readonly LiteCollection<Configuration> _configuration;
        private readonly LiteCollection<Plugin> _plugins;

        public LiteDbConfigurationRepository()
        {
            _configurationDb = new LiteDatabase(@"Configuration.db");
            _settings = _configurationDb.GetCollection<Setting>("settings");
            _configuration = _configurationDb.GetCollection<Configuration>("configuration");
            _plugins = _configurationDb.GetCollection<Plugin>("plugins");
        }

        private string GetSimpleSetting(string name)
        {
            if (_disposed)
                throw new ObjectDisposedException("LiteDbConfigurationRepository");
            var obj = _settings.Find(s => s.Name.Equals(name)).FirstOrDefault();
            return obj?.Value;
        }

        private void SetSimpleSetting(string name, string value)
        {
            if (_disposed)
                throw new ObjectDisposedException("LiteDbConfigurationRepository");
            var obj = new Setting()
            {
                Name = name,
                Value = value ?? ""
            };
            if (!_settings.Update(obj))
            {
                _settings.Insert(obj);
            }
        }

        public IEnumerable<Setting> GetAllSettings()
        {
            return _settings.FindAll();
        }

        public Setting GetSetting(string name)
        {
            return _settings.FindOne(s => s.Name.Equals(name));
        }

        public void SetSetting(Setting value)
        {
            _settings.Upsert(value);
        }

        public void RemoveSetting(string name)
        {
            _settings.Delete(s => s.Name.Equals(name));
        }

        public Configuration GetConfiguration()
        {
            return _configuration.FindById(1);
        }

        public void SaveConfiguration(Configuration configuration)
        {
            _configuration.Upsert(configuration);
        }

        public void DeleteConfiguration()
        {
            _configuration.Delete(1);
        }

        public IEnumerable<Plugin> GetAllPlugins()
        {
            return _plugins.FindAll();
        }

        public Plugin GetPluginByName(string name)
        {
            return _plugins.FindOne(s => s.Name.Equals(name));
        }

        public Plugin SavePluginConfiguration(Plugin plugin)
        {
            _plugins.Upsert(plugin);
            return plugin;
        }

        public void DeletePluginByName(string name)
        {
            _plugins.Delete(s => s.Name.Equals(name));
        }

        public string SafeguardAddress
        {
            get => GetSimpleSetting("SafeguardAddress");
            set => SetSimpleSetting("SafeguardAddress", value);
        }

        public string ClientCertificateThumbprint
        {
            get => GetSimpleSetting("ClientCertificateThumbprint");
            set => SetSimpleSetting("ClientCertificateThumbprint", value);
        }

        public int? ApiVersion
        {
            get
            {
                try
                {
                    return int.Parse(GetSimpleSetting("ApiVersion"));
                }
                catch
                {
                    return null;
                }
            }
            set => SetSimpleSetting("ApiVersion", value.ToString());
        }

        public bool? IgnoreSsl
        {
            get
            {
                try
                {
                    return bool.Parse(GetSimpleSetting("IgnoreSsl"));
                }
                catch
                {
                    return null;
                }
            }
            set => SetSimpleSetting("IgnoreSsl", value.ToString());
        }

        public void Dispose()
        {
            _configurationDb?.Dispose();
            _disposed = true;
            _configurationDb = null;
        }
    }
}
