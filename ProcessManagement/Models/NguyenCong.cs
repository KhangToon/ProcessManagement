using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models
{
    public class NguyenCong
    {
        public Propertyy NCID { get; set; } = new() { Name = Common.NCID, Type = typeof(int), AlowDatabase = true };
        public Propertyy TenNguyenCong { get; set; } = new() { Name = Common.NguyenCong, Type = typeof(string), AlowDatabase = true };
        public Propertyy Ghichu { get; set; } = new() { Name = Common.Ghichu, Type = typeof(string), AlowDatabase = true };


        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(NguyenCong);

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

            Propertyy? tagetProperty = propertiesValue.FirstOrDefault(f => f.Name == propertyName);

            if (tagetProperty != null)
            {
                tagetProperty.Value = newValue;
            }
        }

        public object? GetPropertyValue(string propertyName)
        {
            List<Propertyy> propertiesValue = GetPropertiesValues();

            Propertyy? tagetProperty = propertiesValue.FirstOrDefault(f => f.Name == propertyName);

            return tagetProperty?.Value;
        }
    }
}
