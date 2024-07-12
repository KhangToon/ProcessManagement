namespace ProcessManagement.Models
{
    public class TemNVLMCDValues
    {
        public string Ca { get; set; } = string.Empty;
        public int slOK { get; set; } = 0;
        public int slNG { get; set; } = 0;
        public int slConlai { get; set; } = 0;
        public string tenNV { get; set; } = string.Empty;
        public DateTime ngayGC { get; set; } = DateTime.Now;

        public TemNVLMCDValues(string ca)
        {
            Ca = ca;
        }
    }
}
