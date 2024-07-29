using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL
{
    public class NhaCungCap
    {
        public Propertyy NCCID { get; set; } = new() { DBName = Common.NCCID, DisplayName = "NCC ID", Type = typeof(int), AlowDatabase = true }; // ID
        public Propertyy TenNCC { get; set; } = new() { DBName = Common.TenNCC, DisplayName = "Tên NCC", Type = typeof(string), AlowDatabase = true };
        public Propertyy DiaChiNCC { get; set; } = new() { DBName = Common.DiaChi, DisplayName = "Địa chỉ", Type = typeof(string), AlowDatabase = true };
        public Propertyy SoDienThoaiNCC { get; set; } = new() { DBName = Common.SoDienThoai, DisplayName = "Số điện thoại", Type = typeof(int), AlowDatabase = true };
        public Propertyy EmailNCC { get; set; } = new() { DBName = Common.Email, DisplayName = "Email", Type = typeof(string), AlowDatabase = true };
        public Propertyy NguoiLienHe { get; set; } = new() { DBName = Common.NguoiLienHe, DisplayName = "Người liên hệ", Type = typeof(string), AlowDatabase = true };
        public Propertyy MaSoThue { get; set; } = new() { DBName = Common.MaSoThue, DisplayName = "Mã số thuế", Type = typeof(int), AlowDatabase = true };
        public Propertyy GhiChuNCC { get; set; } = new() { DBName = Common.GhiChuNCC, DisplayName = "Ghi chú", Type = typeof(string), AlowDatabase = true };

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(NhaCungCap);

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
