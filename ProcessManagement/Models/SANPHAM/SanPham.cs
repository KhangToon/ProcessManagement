using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.SANPHAM
{
    public class SanPham
    {
        public Propertyy SP_SPID { get; set; } = new() { DBName = Common.SP_SPID, Type = typeof(int), AlowDisplay = false, AlowDatabase = false };
        public Propertyy SP_MaSP { get; set; } = new() { DBName = Common.SP_MaSP, DisplayName = "Mã sản phẩm", Type = typeof(string), AlowDatabase = true };
        public Propertyy SP_TenSanPham { get; set; } = new() { DBName = Common.SP_TenSanPham, DisplayName = "Tên sản phẩm", Type = typeof(string), AlowDatabase = true };
        public List<ChitietSanPham>? ChitietSanPhams { get; set; }
        public List<NVLofSanPham> DanhSachNVLs { get; set; } = new();


        // Danh sach thong tin san pham
        public List<ThongTinSanPham> DSThongTin = new();

        // Return thong tin nhan vien by tentruyxuat
        public ThongTinSanPham GetThongTinSanPhamByName(string tenthongtin)
        {
            string tentruyxuat = Common.RemoveDiacriticsAndSpaces(tenthongtin);

            ThongTinSanPham targetThongTin = DSThongTin.FirstOrDefault(thongtin => thongtin.LoaiThongTin.TenTruyXuat.Value?.ToString()?.Trim() == tentruyxuat) ?? new();

            return targetThongTin;
        }

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
