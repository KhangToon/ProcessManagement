using ProcessManagement.Commons;
using ProcessManagement.Models.KHO_NVL;
using System.Reflection;

namespace ProcessManagement.Models
{
    public class NVLofSanPham
    {
        public Propertyy NVLSPID { get; set; } = new() { DBName = Common.NVLSPID, DisplayName = "NVLSPID", Type = typeof(int), AlowDatabase = false, AlowDisplay = false }; // ID
        public Propertyy SPID { get; set; } = new() { DBName = Common.SPID, DisplayName = "SP ID", Type = typeof(int), AlowDatabase = true, };
        public Propertyy MaNVL { get; set; } = new() { DBName = Common.MaNVL, DisplayName = "Mã NVL", Type = typeof(int), AlowDatabase = true, };
        public Propertyy NgayThem { get; set; } = new() { DBName = Common.NgayThem, DisplayName = "Ngày thêm", Type = typeof(DateTime), AlowDatabase = true };


        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(NVLofSanPham);

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
