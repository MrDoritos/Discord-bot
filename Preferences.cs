using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;

namespace MusicBot9001
{
    class Preferences
    {
        private JObject _json;
        //Loaded Preferences and Defaults
        public LogLevel logLevel = (LogLevel) 1;
        public string token = "";
        public string[] botAdmins;
        public string adminFile = "admins.txt";
        public string musicPath = "Music\\";

        public enum LogLevel
        {
            dont = 0,
            chat = 1,
            debug = 2
        }
        


        public Preferences()
        {
            if (File.Exists("config.json"))
            {
                ParseConfig();
            }
            else
            {
                File.Create("config.json").Close();
                GenerateConfig();
            }
        }

        public void GenerateConfig()
        {
            //Clear json
            _json = new JObject();

            //Add objects
            _json.Add("token", token);
            _json.Add("musicPath", musicPath);
            _json.Add("adminFile", adminFile);
            _json.Add("logLevel", (int)logLevel);
            _json.Add("botAdmins", new JArray(new string[0]));

            try
            { File.WriteAllText("config.json", JsonConvert.SerializeObject(_json)); }
            catch (Exception)
            {
                Console.WriteLine("Could not save json");
            }
        }

        public void ParseConfig()
        {
            _json = JObject.Parse(File.ReadAllText("config.json"));

            //Tokens
            if (_json.ContainsKey("token"))
            { if (((string)_json["token"]).Length > 0) { token = (string)_json["token"]; } }
            if (_json.ContainsKey("adminFile"))
            { if (((string)_json["adminFile"]).Length > 0) { adminFile = (string)_json["adminFile"]; } }
            if (_json.ContainsKey("logLevel"))
            {
                if (((string)_json["logLevel"]) != null)
                {
                    if (((int)_json["logLevel"]) > (int)LogLevel.debug)
                    { logLevel = LogLevel.chat; }
                    else if (((int)_json["logLevel"]) < 0)
                    { logLevel = LogLevel.chat; }
                    else
                    { logLevel = (LogLevel)(int)_json["logLevel"]; }

                    //Arrays

                }
            }
            if (_json.ContainsKey("botAdmins"))
            {
                var botAdminArray = _json.Properties().Where(n => n.Name == "botAdmins").First();
                if (botAdminArray != null && botAdminArray.HasValues)
                {
                    List<string> admins__ = new List<string>();
                    for (int i = 0; i < botAdminArray.Values().Count(); i++)
                    {
                        admins__.Add((string)botAdminArray.Values().ElementAt(i));
                    }
                    botAdmins = admins__.ToArray();
                }
                else
                {
                    botAdmins = new string[0];
                }
            }
            else
            {
                if (File.Exists(adminFile))
                {
                    botAdmins = File.ReadAllLines(adminFile);
                }
                else
                {
                    botAdmins = new string[0];
                }
            }
            if (_json.ContainsKey("musicPath"))
            { musicPath = (string)_json["musicPath"]; }
        }

        public void SaveConfig()
        {

        }
    }
}
