using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models
{
    public class Lot
    {
        public Propertyy LotID = new() { Name = Common.LotID, Type = typeof(int), DispDatagrid = false, AlowDatabase = true };

        public Propertyy LotNVL = new() { Name = Common.LotNVL, Type = typeof(string), AlowDatabase = true };

        public Propertyy NgayTao = new() { Name = Common.NgayTao, Type = typeof(DateTime), AlowDatabase = true };

        public Propertyy SoLuongNL = new() { Name = Common.SoLuongNVL, Type = typeof(int), AlowDatabase = true };

        public Propertyy GhiChu = new() { Name = Common.GhiChu, Type = typeof(string), AlowDatabase = true };

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(Lot);

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
