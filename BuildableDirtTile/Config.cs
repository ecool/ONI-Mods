using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;


namespace BuildableDirtTile
{
    public class Config
    {
        private readonly static string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config.json");
        public float BuildMass { get; set; }
        public float BlockMass { get; set; }
        public float BuildSpeed { get; set; }

		public void Save()
		{
			var json = JsonConvert.SerializeObject(this, Formatting.Indented);
			File.WriteAllText(path, json);
			//Console.WriteLine(json);
		}

		public static Config Load()
		{
			string json = null;
			Config newConfig = null;
			bool dirty = false;

			Dictionary<string, int> defaults = new Dictionary<string, int>(){
					{ "BuildMass", 50 },
					{ "BlockMass", 50 },
					{ "BuildSpeed", 3 }
				};

			if (!File.Exists(path)) {
				dirty = true;
				json = @"{
					BuildMass: 50,
					BlockMass: 50,
					BuildSpeed: 3
					}";
			} else {
				json = File.ReadAllText(path);
			}
			newConfig = JsonConvert.DeserializeObject<Config>(json);

			// below is very ugly, make dynamic eventually
			if (newConfig.BuildMass == 0) {
				newConfig.BuildMass = defaults["BuildMass"];
				dirty = true;
			}
			if (newConfig.BlockMass == 0) {
				newConfig.BlockMass = defaults["BlockMass"];
				dirty = true;
			}
			if (newConfig.BuildSpeed == 0) {
				newConfig.BuildSpeed = defaults["BuildSpeed"];
				dirty = true;
			}

			if (dirty) newConfig.Save();

		    return newConfig;
		}
    }
}
