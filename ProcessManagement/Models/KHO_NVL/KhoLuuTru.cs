using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL
{
    public class KhoLuuTru
    {
        public Propertyy KHOID { get; set; } = new() { DBName = Common.KHOID, DisplayName = "Kho ID", Type = typeof(int), AlowDatabase = true }; // ID
        public Propertyy TenKho { get; set; } = new() { DBName = Common.TenKho, DisplayName = "Tên kho", Type = typeof(string), AlowDatabase = true };
        public Propertyy DiaChiKho { get; set; } = new() { DBName = Common.DiaChi, DisplayName = "Địa chỉ", Type = typeof(string), AlowDatabase = true };
        public Propertyy SucChua { get; set; } = new() { DBName = Common.SucChua, DisplayName = "Sức chứa", Type = typeof(int), AlowDatabase = true };
        public Propertyy NguoiQuanLy { get; set; } = new() { DBName = Common.NguoiQuanLy, DisplayName = "Người quản lý", Type = typeof(string), AlowDatabase = true };
        public Propertyy SoDienThoaiKho { get; set; } = new() { DBName = Common.SoDienThoai, DisplayName = "Số điện thoại", Type = typeof(int), AlowDatabase = true };
        public Propertyy TrangThai { get; set; } = new() { DBName = Common.TrangThai, DisplayName = "Trạng thái", Type = typeof(string), AlowDatabase = true };

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(KhoLuuTru);

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
