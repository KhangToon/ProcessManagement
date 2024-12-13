using ProcessManagement.Commons;
using ProcessManagement.Models.SANPHAM;
using System.Reflection;
using System.Text.Json;

namespace ProcessManagement.Models.KHO_NVL
{
    public class NguyenVatLieu
    {
        public Propertyy NVLID { get; set; } = new() { DBName = Common.NVLID, DisplayName = "NVL ID", Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // ID
        public Propertyy MaNVL { get; set; } = new() { DBName = Common.MaNVL, DisplayName = "Mã nguyên liệu", Type = typeof(string), AlowDatabase = true };
        public Propertyy TenNVL { get; set; } = new() { DBName = Common.TenNVL, DisplayName = "Tên nguyên liệu", Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy LOAINVLID { get; set; } = new() { DBName = Common.LOAINVLID, DisplayName = "Loại nguyên liệu", Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy DMID { get; set; } = new() { DBName = Common.DMID, DisplayName = "Danh mục", Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy DonViTinh { get; set; } = new() { DBName = Common.DonViTinh, DisplayName = "Đơn vị tính", Type = typeof(string), AlowDatabase = true, DispDatagrid = false };
        //public Propertyy NgaySuDung { get; set; } = new() { DBName = Common.NgaySuDung, DisplayName = "Ngày sử dụng", Type = typeof(DateTime), AlowDatabase = true };

        public int SLSanXuat = 0;

        public int TonKho = 0;

        public DanhMucNVL? DanhMuc { get; set; }
        public LoaiNVL? LoaiNVL { get; set; }

        public List<ThongTinNVL> DSThongTin { get; set; } = new();

        public List<ViTriofNVL> DSViTri { get; set; } = new();

        // Danh sach thanh pham cua NVL
        public List<NVLwithSanPham> DSachSPofNVLs { get; set; } = new(); // Load rieng/ tranh dead loop

        // Get nvl details infor by detail name
        public string GetValue(string proName)
        {
            var tagertItem = this?.DSThongTin.FirstOrDefault(dt => dt.LoaiThongTin?.TenTruyXuat.Value?.ToString()?.Trim() == proName);

            if (tagertItem != null)
            {
                return tagertItem.GiaTri.Value?.ToString()?.Trim() ?? string.Empty;
            }
            else return string.Empty;
        }

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
