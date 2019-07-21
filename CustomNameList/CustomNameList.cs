using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using Harmony;

namespace CustomNameList {
    [HarmonyPatch(typeof(CharacterContainer), "SetInfoText")]
    public class CustomNameList{
        public static NameList names;
		public static string path;
		public static string filepath;
		public static string filename = "namelists.json";
		public static Config config;

        public static void Prefix(CharacterContainer __instance){
			if(path == null){
				path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "");
				filepath = Path.Combine(path, filename);
			}

			// Only use mod if config file exists
			if(File.Exists(filepath)){
				if(config == null) config = new Config(path, filename);
				if(names == null) names = new NameList(config);

				// new List<MinionIdentity>(Components.LiveMinionIdentities.Items); // NOTE: Maybe eventually track other dupe names and limit new name unless all used.
				var stats = Traverse.Create(__instance).Property("Stats");
				var name = stats.Field("Name");
				var gender_val = (String)stats.Field("GenderStringKey").GetValue();

				var old_name = name.GetValue();

				name.SetValue(names.Next(gender_val));

				Debug.Log("Name changed from `" + old_name + "` => `" + name.GetValue() + "`");
				
			}
        }
    }

    public class NameList{
		// REF: ONI - MinionIdentities.NameList
		public Dictionary<string, List<string>> names = new Dictionary<string, List<string>>();
		public Dictionary<string, int> idx = new Dictionary<string, int>();
		public List<string> genders = new List<string>{ "MALE", "FEMALE", "NB" };

		public NameList(Config config){
			foreach(string gender in genders){
				Debug.Log("--- Populating NameList for " + gender + " ---");
				// create default index for each Gender
				this.idx.Add(gender, 0);

				// get `gender` key from Config and add to the `gender` NameList
				this.names.Add(gender, config.Get(gender));

				// shuffle each NameList
				this.names[gender].Shuffle<string>();
			}
		}

		public string Next(string gender){
			return this.names[gender][this.idx[gender]++ % this.names[gender].Count];
		}
	}

	public class Config{
		// TODO: separate into a helper lib and expand writing capabilities
		public string json;
		public Dictionary<string, List<string>> keys;

		public Config(string path, string filename){
			using (StreamReader file = new StreamReader(Path.Combine(path, filename))){
				this.json = file.ReadToEnd();
				this.keys = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
			}
		}

		public List<string> Get(string key){
			return this.keys[key];
		}
	}
}