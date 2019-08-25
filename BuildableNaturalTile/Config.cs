using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace BuildableNaturalTile
{
    public class Config
    {
        private readonly static string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), "config.json");
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
			var json = File.ReadAllText(path);
			// var json = @"{
			//     ServerAddress: null,
			// 	ServerPort: null,
			// 	ServerTimeout: null }";
		    return JsonConvert.DeserializeObject<Config>(json);
		}
    }
}
