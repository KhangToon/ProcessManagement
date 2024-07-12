using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models
{
    public class SanPham
    {
        public Propertyy SPID { get; set; } = new() { Name = Common.SPID, Type = typeof(int), AlowDatabase = false };
        public Propertyy MaSP { get; set; } = new() { Name = Common.MaSP, Type = typeof(string), AlowDatabase = true };
        public Propertyy TenSanPham { get; set; } = new() { Name = Common.TenSanPham, Type = typeof(string), AlowDatabase = true };
        public Propertyy NgayTao { get; set; } = new() { Name = Common.NgayTao, Type = typeof(DateTime), AlowDatabase = true };
        public List<ChitietSanPham>? ChitietSanPhams { get; set; }
        public List<NVL>? DanhSachNVLs { get; set; }

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(SanPham);

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

        public class ChitietSanPham
        {
            public Propertyy CTSPID { get; set; } = new() { Name = Common.CTSPID, Type = typeof(int), AlowDatabase = false };
            public Propertyy SPID { get; set; } = new() { Name = Common.SPID, Type = typeof(int), AlowDatabase = true };
            public Propertyy PropertyName { get; set; } = new() { Name = Common.PropertyName, Type = typeof(string), AlowDatabase = true };
            public Propertyy PropertyValue { get; set; } = new() { Name = Common.PropertyValue, Type = typeof(string), AlowDatabase = true };
            public Propertyy PropertyType { get; set; } = new() { Name = Common.PropertyType, Type = typeof(string), AlowDatabase = true };

            public List<Propertyy> GetPropertiesValues()
            {
                Type propertyType = typeof(ChitietSanPham);

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
}
