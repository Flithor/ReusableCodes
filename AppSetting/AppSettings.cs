using System;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace Flithor_ReusableCodes
{
    /// <summary>
    /// Saved instantly configruation class
    /// </summary>
    public class AppSettings
    {
        public static string MyConfig
        {
            get => GetSetting() ?? "";
            set => SetSetting(value);
        }

        public static string MySqlConnStr
        {
            get => GetConnStr() ?? "";
            set => SetConnStr(value);
        }

        private static Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        private static string GetConnStr([CallerMemberName] string propName = "")
            => configuration.ConnectionStrings.ConnectionStrings[propName]?.ConnectionString;
        private static void SetConnStr(string connStr, [CallerMemberName] string propName = "")
        {
            try
            {
                var connStrItem = configuration.ConnectionStrings.ConnectionStrings[propName];
                if (connStrItem == null)
                    configuration.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings(propName, connStr));
                else
                    connStrItem.ConnectionString = connStr;
                configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception err)
            {
                //WriteDebugLog(err.ToString());
            }
        }

        private static string GetSetting([CallerMemberName] string propName = "") { return configuration.AppSettings.Settings[propName]?.Value; }

        private static void SetSetting(object value, [CallerMemberName] string propName = "") => SetSetting(value.ToString(), propName);
        private static void SetSetting(string value, [CallerMemberName] string propName = "")
        {
            try
            {
                var confItem = configuration.AppSettings.Settings[propName];
                if (confItem == null)
                    configuration.AppSettings.Settings.Add(propName, value);
                else
                    confItem.Value = value;
                configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception err)
            {
                
            }
        }
    }
}
