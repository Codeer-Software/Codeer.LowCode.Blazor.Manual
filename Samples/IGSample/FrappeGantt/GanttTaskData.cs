using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace FrappeGanttJS.Blazor
{
    public class GanttTaskData
    {
        [JsonIgnore]
        public string? Id { get; set; }
        public string? Name { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public int? Progress { get; set; }
        [JsonIgnore]
        public string[]? Dependencies { get; set; }

        [JsonPropertyName("id")]
        public string? SerializableId
        {
            get => SerializeId(Id);
            set => Id = ParseSerializableId(value);
        }

        [JsonPropertyName("dependencies")]
        public string[]? SerializableDependencies
        {
            get => Dependencies?.Select(SerializeId).ToArray();
            set => Dependencies = value?.Select(ParseSerializableId).ToArray();
        }

        private string? SerializeId(string id) => string.IsNullOrEmpty(id) ? null : "Task_" + id;

        private string? ParseSerializableId(string? id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (!id.StartsWith("Task_")) return null;
            var parsed = id.Substring("Task_".Length);
            return string.IsNullOrEmpty(parsed) ? null : parsed;
        }
    }
}
