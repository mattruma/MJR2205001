using Newtonsoft.Json;

namespace ConsoleApp1
{
    public class Person
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("object")]
        public string Object => "Person";

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("bio")]
        public string Bio { get; set; }
    }
}
