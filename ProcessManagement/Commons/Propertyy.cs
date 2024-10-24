namespace ProcessManagement.Commons
{
    public class Propertyy
    {
        public string? DBName { get; set; }
        public object? Value { get; set; } = null;
        public Type? Type { get; set; }
        public bool DispDatagrid { get; set; } = true;
        public bool AlowDatabase { get; set; } = false;
        public string DisplayName
        {
            get { return displayname; }
            set { displayname = value; NotifyMess = $"{DisplayName} không được để trống"; }
        }
        private string displayname = string.Empty;
        public bool AlowDisplay { get; set; } = true;

        public bool IsCheckValueOK { get; set; } = false; // Dung khi check value ok 
        public bool IsDisable { get; set; } = false; // Dung khi check value ok 
        
        public string NotifyMess { get; set; } = string.Empty; // Dung khi check value ok 

        public List<ErrType> CheckErrors { get; set; } = new() { ErrType.NotEmptyValue };

        public enum ErrType
        {
            NotEmptyValue,
            NotAllowEqualsZero
        }
    }
}
