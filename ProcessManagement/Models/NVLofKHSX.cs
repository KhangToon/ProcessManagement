using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models
{
    public class NVLofKHSX
    {
        public Propertyy NVLKHSXID { get; set; } = new() { DBName = Common.NVLKHSXID, Type = typeof(int), AlowDatabase = true };
        public Propertyy KHSXID { get; set; } = new() { DBName = Common.KHSXID, Type = typeof(int), AlowDatabase = true };
        public Propertyy SPID { get; set; } = new() { DBName = Common.SP_SPID, Type = typeof(int), AlowDatabase = true };
        public Propertyy NVLID { get; set; } = new() { DBName = Common.NVLID, Type = typeof(int), AlowDatabase = true };
        public Propertyy SoLuong { get; set; } = new() { DBName = Common.SoLuong, Type = typeof(int), AlowDatabase = true };
        public Propertyy TiLeLoi { get; set; } = new() { DBName = Common.TileLoi, Type = typeof(double), AlowDatabase = true };
        public Propertyy DinhMuc { get; set; } = new() { DBName = Common.DinhMuc, Type = typeof(int), AlowDatabase = true };
        public Propertyy ThoiDiem { get; set; } = new() { DBName = Common.ThoiDiem, Type = typeof(DateTime), AlowDatabase = true };

        public string TenNVL { get; set; } = string.Empty;

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(NVLofKHSX);

            FieldInfo[] fields = propertyType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            List<Propertyy> propertiesValue = new();

            foreach (FieldInfo field in fields)
            {
                Type ob = field.FieldType;

                if (ob == typeof(Propertyy))
                {
                    Propertyy? fieldValue = (Propertyy?)field.GetValue(this);

                    if (fieldValue != null) { propertiesValue.Add(fieldValue); }
                }
            }

            return propertiesValue;
        }

        public void SetPropertyValue(string propertyName, object newValue)
        {
            List<Propertyy> propertiesValue = GetPropertiesValues();

            Propertyy? tagetProperty = propertiesValue.FirstOrDefault(f => f.DBName == propertyName);

            if (tagetProperty != null)
            {
                tagetProperty.Value = newValue;
            }
        }

        public object? GetPropertyValue(string propertyName)
        {
            List<Propertyy> propertiesValue = GetPropertiesValues();

            Propertyy? tagetProperty = propertiesValue.FirstOrDefault(f => f.DBName == propertyName);

            return tagetProperty?.Value;
        }


    }
}
