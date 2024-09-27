using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.SANPHAM
{
    public class SanPham
    {
        public Propertyy SPID { get; set; } = new() { DBName = Common.SPID, Type = typeof(int), AlowDatabase = false };
        public Propertyy MaSP { get; set; } = new() { DBName = Common.MaSP, Type = typeof(string), AlowDatabase = true };
        public Propertyy TenSanPham { get; set; } = new() { DBName = Common.TenSanPham, Type = typeof(string), AlowDatabase = true };
        public Propertyy NgayTao { get; set; } = new() { DBName = Common.NgayTao, Type = typeof(DateTime), AlowDatabase = true };
        public List<ChitietSanPham>? ChitietSanPhams { get; set; }
        public List<NVLofSanPham>? DanhSachNVLs { get; set; }

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
