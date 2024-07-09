namespace FrappeGanttJS.Blazor
{
    public class GanttTaskData
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public int? Progress { get; set; }
        public string[]? Dependencies { get; set; }
    }
}
