using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using Harmony;

namespace CustomNameList
{
    [HarmonyPatch(typeof(CharacterContainer), "SetInfoText")]
    public class CustomNameList
    {
        public static NameList names;
		public static string path;
		public static string filepath;

		public static IniFile config2;
		public static Config config;

        public static void Prefix(CharacterContainer __instance)
        {
			if(path == null){
				path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "");
				filepath = Path.Combine(path, "namelists.json");
			}

			// Only use mod if namelists.txt exists
			if(File.Exists(filepath)){
				if(config == null){
					
					//Debug.Log(config.ReadConfig());
				}
				/*if(config2 == null){
					Debug.Log("--- Creating Config2 IniFile ---");
					config2 = new IniFile(filepath);
					Debug.Log(config2);
				}

				if(names == null){
					Debug.Log("--- Creating NameList ---");
					names = new NameList(config2);
					Debug.Log(names);
				}*/

				if(names == null){

				}

				// new List<MinionIdentity>(Components.LiveMinionIdentities.Items); // NOTE: Maybe eventually track other dupe names and limit new name unless all used.
				var stats = Traverse.Create(__instance).Property("Stats");
				var name = stats.Field("Name");
				var gender_val = (String)stats.Field("GenderStringKey").GetValue();

				var old_name = name.GetValue();

				//name.SetValue(names.Next(gender_val));

				Debug.Log("Name changed from `" + old_name + "` => `" + name.GetValue() + "`");
				
			}
        }
    }

    public class NameList
	{
		/*
		Modified from ONI - MinionIdentities.NameList
		*/
		public Dictionary<string, List<string>> names = new Dictionary<string, List<string>>();
		public Dictionary<string, int> idx = new Dictionary<string, int>();

		public List<string> genders = new List<string>{ "MALE", "FEMALE", "NB" };

		public NameList(IniFile config)
		{
			foreach(string gender in genders){
				Debug.Log("--- Populating NameList for " + gender + " ---");
				// create default index for each Gender
				this.idx.Add(gender, 0);

				List<string> tmp = new List<string>(config.Read(gender, "Names").Split(','));
				this.names.Add(gender, tmp);

				// shuffle each NameList
				this.names[gender].Shuffle<string>();
			}
		}

		public string Next(String gender)
		{
			return this.names[gender][this.idx[gender]++ % this.names[gender].Count];
		}
	}

	public class Config
	{

	}
}