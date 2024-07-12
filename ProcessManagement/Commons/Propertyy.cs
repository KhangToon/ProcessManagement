namespace ProcessManagement.Commons
{
    public class Propertyy
    {
        public string? Name { get; set; } 
        public object? Value { get; set; } = null;
        public Type? Type { get; set; }
        public bool DispDatagrid { get; set; } = true;
        public bool AlowDatabase { get; set; } = false;
    }
}
