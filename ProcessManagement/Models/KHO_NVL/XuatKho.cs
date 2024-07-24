using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL
{
    public class XuatKho
    {
        public Propertyy MaXuatKho { get; set; } = new() { DBName = Common.MaXuatKho, DisplayName = "Mã xuất kho", Type = typeof(int), AlowDatabase = true }; // ID
        public Propertyy MaNVL { get; set; } = new() { DBName = Common.MaNVL, DisplayName = "Mã NVL", Type = typeof(int), AlowDatabase = true };
        public Propertyy MaKho { get; set; } = new() { DBName = Common.MaKho, DisplayName = "Mã kho", Type = typeof(int), AlowDatabase = true };
        public Propertyy MaViTri { get; set; } = new() { DBName = Common.MaViTri, DisplayName = "Mã vị trí", Type = typeof(int), AlowDatabase = true };
        public Propertyy SoLuong { get; set; } = new() { DBName = Common.SoLuongXuat, DisplayName = "Số lượng xuất", Type = typeof(decimal), AlowDatabase = true };
        public Propertyy Ngay { get; set; } = new() { DBName = Common.NgayXuat, DisplayName = "Ngày xuất", Type = typeof(DateTime), AlowDatabase = true };
        public Propertyy NguoiXuatKho { get; set; } = new() { DBName = Common.NguoiXuatKho, DisplayName = "Người xuất", Type = typeof(string), AlowDatabase = true };
        public Propertyy NguoiNhan { get; set; } = new() { DBName = Common.NguoiNhan, DisplayName = "Người nhận", Type = typeof(string), AlowDatabase = true };
        public Propertyy MucDichSuDung { get; set; } = new() { DBName = Common.MucDichSuDung, DisplayName = "Mục đích sử dụng", Type = typeof(string), AlowDatabase = true };
        public Propertyy GhiChuXuatKho { get; set; } = new() { DBName = Common.GhiChuXuatKho, DisplayName = "Ghi chú", Type = typeof(string), AlowDatabase = true };

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(XuatKho);

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
