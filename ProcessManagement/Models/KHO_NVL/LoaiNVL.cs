using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL
{
    public class LoaiNVL
    {
        public Propertyy MaLoaiNVL { get; set; } = new() { DBName = Common.MaLoaiNVL, DisplayName = "Mã loại NVL", Type = typeof(int), AlowDatabase = true }; // ID
        public Propertyy MaDanhMuc { get; set; } = new() { DBName = Common.MaDanhMuc, DisplayName = "Mã danh mục", Type = typeof(int), AlowDatabase = true };
        public Propertyy TenLoaiNVL { get; set; } = new() { DBName = Common.TenLoaiNVL, DisplayName = "Loại NVL", Type = typeof(string), AlowDatabase = true };
        public Propertyy NgayThem { get; set; } = new() { DBName = Common.NgayThem, DisplayName = "Ngày tạo", Type = typeof(DateTime), AlowDatabase = true };

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(LoaiNVL);

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
