using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models
{
    public class NVLmoiNguyenCong
    {
        public Propertyy NVLMCDID { get; set; } = new() { Name = Common.NVLMCDID, Type = typeof(int), AlowDatabase = true };
        public Propertyy KHSXID { get; set; } = new() { Name = Common.KHSXID, Type = typeof(int), AlowDatabase = true };
        public Propertyy CDID { get; set; } = new() { Name = Common.CDID, Type = typeof(int), AlowDatabase = true };
        public Propertyy TenCongDoan { get; set; } = new() { Name = Common.NguyenCong, Type = typeof(string), AlowDatabase = true };

        public Propertyy LoaiNVL = new() { Name = Common.LoaiNL, Type = typeof(string), AlowDatabase = true };
        public Propertyy MaSP { get; set; } = new() { Name = Common.MaSP, Type = typeof(string), AlowDatabase = true };
        public Propertyy MaQuanLy { get; set; } = new() { Name = Common.MaQuanLy, Type = typeof(string), AlowDatabase = true };
        public Propertyy SLGoccuaLOTNVL { get; set; } = new() { Name = Common.SLGoccuaLOTNVL, Type = typeof(int), AlowDatabase = true };
        public Propertyy SLTruocGiaCong { get; set; } = new() { Name = Common.SLTruocGiaCong, Type = typeof(int), AlowDatabase = true };
        public Propertyy SLSauGiaCong { get; set; } = new() { Name = Common.SLSauGiaCong, Type = typeof(int), AlowDatabase = true };

        public Propertyy NgayXuat = new() { Name = Common.NgayXuat, Type = typeof(DateTime), AlowDatabase = true };
        public CaLamViec CaNgay { get; set; } = new(Common.Cangay);
        public CaLamViec CaDem { get; set; } = new(Common.Cadem);
        public Propertyy TongOK { get; set; } = new() { Name = Common.TongOK, Type = typeof(int), AlowDatabase = true };
        public Propertyy TongNG { get; set; } = new() { Name = Common.TongNG, Type = typeof(int), AlowDatabase = true };
        public Propertyy DanhGia { get; set; } = new() { Name = Common.Danhgia, Type = typeof(string), AlowDatabase = true };
        public Propertyy IsUpdated { get; set; } = new() { Name = Common.IsUpdated, Type = typeof(int), AlowDatabase = true };

        public object? IndexNguyenCong = -1;

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(NVLmoiNguyenCong);

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

            Propertyy? tagetProperty = propertiesValue.FirstOrDefault(f => f.Name == propertyName);

            if (tagetProperty != null)
            {
                tagetProperty.Value = newValue;
            }
        }

        public object? GetPropertyValue(string propertyName)
        {
            List<Propertyy> propertiesValue = GetPropertiesValues();

            Propertyy? tagetProperty = propertiesValue.FirstOrDefault(f => f.Name == propertyName);

            return tagetProperty?.Value;
        }

    }
    public class CaLamViec
    {
        public Propertyy NVLMCDID { get; set; } = new() { Name = Common.NVLMCDID, Type = typeof(int), AlowDatabase = true };
        public Propertyy Ca { get; set; } = new() { Name = Common.Ca, Type = typeof(string), AlowDatabase = true };
        public Propertyy NgayGiaCong { get; set; } = new() { Name = Common.NgayGiaCong, Type = typeof(DateTime), AlowDatabase = true };
        public Propertyy NhanVien { get; set; } = new() { Name = Common.NhanVien, Type = typeof(string), AlowDatabase = true };
        public Propertyy OK { get; set; } = new() { Name = Common.OK, Type = typeof(int), AlowDatabase = true };
        public Propertyy NG { get; set; } = new() { Name = Common.NG, Type = typeof(int), AlowDatabase = true };
        public Propertyy IsUpdated { get; set; } = new() { Name = Common.IsUpdated, Type = typeof(int), AlowDatabase = true };

        public CaLamViec(string calamviec)
        {
            Ca.Value = calamviec;
        }

        public CaLamViec() { }

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(CaLamViec);

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

            Propertyy? tagetProperty = propertiesValue.FirstOrDefault(f => f.Name == propertyName);

            if (tagetProperty != null)
            {
                tagetProperty.Value = newValue;
            }
        }

        public object? GetPropertyValue(string propertyName)
        {
            List<Propertyy> propertiesValue = GetPropertiesValues();

            Propertyy? tagetProperty = propertiesValue.FirstOrDefault(f => f.Name == propertyName);

            return tagetProperty?.Value;
        }
    }
}
