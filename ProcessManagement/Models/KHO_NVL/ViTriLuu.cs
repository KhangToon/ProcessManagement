using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL
{
    public class ViTriLuu
    {
        public Propertyy VTID { get; set; } = new() { DBName = Common.VTID, DisplayName = "Vị trí ID", Type = typeof(int), AlowDatabase = true }; // ID
        public Propertyy KHOID { get; set; } = new() { DBName = Common.KHOID, DisplayName = "Kho ID", Type = typeof(string), AlowDatabase = true };
        public Propertyy TenViTri { get; set; } = new() { DBName = Common.TenViTri, DisplayName = "Tên vị trí", Type = typeof(string), AlowDatabase = true };
        public Propertyy SucChua { get; set; } = new() { DBName = Common.SucChua, DisplayName = "Sức chứa", Type = typeof(string), AlowDatabase = true };

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(ViTriLuu);

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
