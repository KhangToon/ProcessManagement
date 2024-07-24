using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL
{
    public class NhapKho
    {
        public Propertyy MaNhapKho { get; set; } = new() { DBName = Common.MaNhapKho, DisplayName = "Mã nhập kho", Type = typeof(int), AlowDatabase = true }; // ID
        public Propertyy MaNVL { get; set; } = new() { DBName = Common.MaNVL, DisplayName = "Mã NVL", Type = typeof(int), AlowDatabase = true };
        public Propertyy MaNCC { get; set; } = new() { DBName = Common.MaNCC, DisplayName = "Mã NCC", Type = typeof(int), AlowDatabase = true };
        public Propertyy MaKho { get; set; } = new() { DBName = Common.MaKho, DisplayName = "Mã kho", Type = typeof(int), AlowDatabase = true };
        public Propertyy MaViTri { get; set; } = new() { DBName = Common.MaViTri, DisplayName = "Mã vị trí", Type = typeof(int), AlowDatabase = true };
        public Propertyy SoLuongNhapKho { get; set; } = new() { DBName = Common.SoLuongNhapKho, DisplayName = "Số lượng nhập", Type = typeof(decimal), AlowDatabase = true };
        public Propertyy NgayNhapKho { get; set; } = new() { DBName = Common.NgayNhap, DisplayName = "Ngày nhập", Type = typeof(DateTime), AlowDatabase = true };
        public Propertyy NgayHetHan { get; set; } = new() { DBName = Common.NgayHetHan, DisplayName = "Ngày hết hạn", Type = typeof(DateTime), AlowDatabase = true };
        public Propertyy SoLoNhapKho { get; set; } = new() { DBName = Common.SoLoNhapKho, DisplayName = "Số lô", Type = typeof(string), AlowDatabase = true };
        public Propertyy NguoiNhapKho { get; set; } = new() { DBName = Common.NguoiNhapKho, DisplayName = "Người nhập", Type = typeof(string), AlowDatabase = true };
        public Propertyy GhiChuNhapKho { get; set; } = new() { DBName = Common.GhiChuNhapKho, DisplayName = "Ghi chú", Type = typeof(string), AlowDatabase = true };

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(NhapKho);

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
