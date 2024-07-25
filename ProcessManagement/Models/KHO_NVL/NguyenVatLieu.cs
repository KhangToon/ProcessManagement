using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL
{
    public class NguyenVatLieu
    {
        public Propertyy MaNVL { get; set; } = new() { DBName = Common.MaNVL, DisplayName = "Mã NVL", Type = typeof(int), AlowDatabase = false, AlowDisplay = false }; // ID
        public Propertyy MaDanhMuc { get; set; } = new() { DBName = Common.MaDanhMuc, DisplayName = "Danh mục NVL", Type = typeof(int), AlowDatabase = true, };
        public Propertyy MaLoaiNVL { get; set; } = new() { DBName = Common.MaLoaiNVL, DisplayName = "Loại NVL", Type = typeof(int), AlowDatabase = true, };
        public Propertyy TenNVL { get; set; } = new() { DBName = Common.TenNVL, DisplayName = "Tên NVL", Type = typeof(string), AlowDatabase = true };
        public Propertyy MoTa { get; set; } = new() { DBName = Common.MoTa, DisplayName = "Mô tả", Type = typeof(string), AlowDatabase = true };
        public Propertyy DonViTinh { get; set; } = new() { DBName = Common.DonViTinh, DisplayName = "Đơn vị tính", Type = typeof(string), AlowDatabase = true };
        public Propertyy SoLuongTonKhoToiThieu { get; set; } = new() { DBName = Common.SoLuongTonKhoToiThieu, DisplayName = "Số lượng tối thiểu", Type = typeof(int), AlowDatabase = true };
        public Propertyy SoLuongTonKhoToiDa { get; set; } = new() { DBName = Common.SoLuongTonKhoToiDa, DisplayName = "Số lượng tối đa", Type = typeof(int), AlowDatabase = true };
        public Propertyy TonKhoHienTai { get; set; } = new() { DBName = Common.TonKhoHienTai, DisplayName = "Tồn kho", Type = typeof(int), AlowDatabase = true };
        public Propertyy ThoiGianBaoQuan { get; set; } = new() { DBName = Common.ThoiGianBaoQuan, DisplayName = "Thời gian bảo quản", Type = typeof(int), AlowDatabase = true };
        public Propertyy NgayCapNhat { get; set; } = new() { DBName = Common.NgayCapNhat, DisplayName = "Ngày cập nhật", Type = typeof(DateTime), AlowDatabase = true };

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(NguyenVatLieu);

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
