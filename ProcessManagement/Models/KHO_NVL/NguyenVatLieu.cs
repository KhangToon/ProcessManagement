using ProcessManagement.Commons;
using System.Reflection;
using System.Text.Json;

namespace ProcessManagement.Models.KHO_NVL
{
    public class NguyenVatLieu
    {
        public Propertyy NVLID { get; set; } = new() { DBName = Common.NVLID, DisplayName = "NVL ID", Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // ID
        public Propertyy DMID { get; set; } = new() { DBName = Common.DMID, DisplayName = "Danh mục", Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy LOAINVLID { get; set; } = new() { DBName = Common.LOAINVLID, DisplayName = "Loại nguyên liệu", Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy TenNVL { get; set; } = new() { DBName = Common.TenNVL, DisplayName = "Tên nguyên liệu", Type = typeof(string), AlowDatabase = true };
        //public Propertyy MoTa { get; set; } = new() { DBName = Common.MoTa, DisplayName = "Mô tả", Type = typeof(string), AlowDatabase = true, DispDatagrid = false };
        //public Propertyy DonViTinh { get; set; } = new() { DBName = Common.DonViTinh, DisplayName = "Đơn vị tính", Type = typeof(string), AlowDatabase = true, DispDatagrid = false };
        //public Propertyy SoLuongTonKhoToiThieu { get; set; } = new() { DBName = Common.SoLuongTonKhoToiThieu, DisplayName = "Số lượng tối thiểu", Type = typeof(int), AlowDatabase = true };
        //public Propertyy SoLuongTonKhoToiDa { get; set; } = new() { DBName = Common.SoLuongTonKhoToiDa, DisplayName = "Số lượng tối đa", Type = typeof(int), AlowDatabase = true };
        public Propertyy TonKhoHienTai { get; set; } = new() { DBName = Common.TonKhoHienTai, DisplayName = "Tồn kho", Type = typeof(int), AlowDatabase = true };
        //public Propertyy ThoiGianBaoQuan { get; set; } = new() { DBName = Common.ThoiGianBaoQuan, DisplayName = "Thời gian bảo quản", Type = typeof(int), AlowDatabase = true };
        public Propertyy NgayCapNhat { get; set; } = new() { DBName = Common.NgayCapNhat, DisplayName = "Ngày cập nhật", Type = typeof(DateTime), AlowDatabase = true };

        public int SLSanXuat = 0;

        public DanhMucNVL? DanhMuc { get; set; }
        public LoaiNVL? LoaiNVL { get; set; }

        public List<NguyenVatLieuDetail> DSNguyenVatLieuDetails { get; set; } = new();

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
