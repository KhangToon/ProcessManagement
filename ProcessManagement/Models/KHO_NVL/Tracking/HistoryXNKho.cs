using ProcessManagement.Commons;
using System.ComponentModel;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL.Tracking
{
    public class HistoryXNKho
    {
        public Propertyy LogXNKID { get; set; } = new() { DBName = Common.LogXNKID, DisplayName = Common.LogXNKID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // ID
        public Propertyy LogLoaiPhieu { get; set; } = new() { DBName = Common.LogLoaiPhieu, DisplayName = Common.LogLoaiPhieu, Type = typeof(string), AlowDatabase = true };
        public Propertyy LogMaPhieu { get; set; } = new() { DBName = Common.LogMaPhieu, DisplayName = Common.LogMaPhieu, Type = typeof(string), AlowDatabase = true };
        public Propertyy LogTenNVL { get; set; } = new() { DBName = Common.LogTenNVL, DisplayName = Common.LogTenNVL, Type = typeof(string), AlowDatabase = true };
        public Propertyy LogMaViTri { get; set; } = new() { DBName = Common.LogMaViTri, DisplayName = Common.LogMaViTri, Type = typeof(string), AlowDatabase = true };
        public Propertyy LogSoLuong { get; set; } = new() { DBName = Common.LogSoLuong, DisplayName = Common.LogSoLuong, Type = typeof(int), AlowDatabase = true };
        public Propertyy LogTonKhoTruoc { get; set; } = new() { DBName = Common.LogTonKhoTruoc, DisplayName = Common.LogTonKhoTruoc, Type = typeof(int), AlowDatabase = true };
        public Propertyy LogTonKhoSau { get; set; } = new() { DBName = Common.LogTonKhoSau, DisplayName = Common.LogTonKhoSau, Type = typeof(int), AlowDatabase = true };
        public Propertyy LogNgThucHien { get; set; } = new() { DBName = Common.LogNgThucHien, DisplayName = Common.LogNgThucHien, Type = typeof(int), AlowDatabase = true };
        public Propertyy LogThoiDiem { get; set; } = new() { DBName = Common.LogThoiDiem, DisplayName = Common.LogThoiDiem, Type = typeof(DateTime), AlowDatabase = true };
        public Propertyy NVLID { get; set; } = new() { DBName = Common.NVLID, Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy VTID { get; set; } = new() { DBName = Common.VTID, Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };

        public static HistoryXNKhoConvert HistoryXNKhoConvertMethod(HistoryXNKho inputHistoryXNKho)
        {
            return new HistoryXNKhoConvert
            {
                LogLoaiPhieu = inputHistoryXNKho.LogLoaiPhieu.Value?.ToString() ?? string.Empty,
                LogMaPhieu = inputHistoryXNKho.LogMaPhieu.Value?.ToString() ?? string.Empty,
                LogTenNVL = inputHistoryXNKho.LogTenNVL.Value?.ToString() ?? string.Empty,
                LogMaViTri = inputHistoryXNKho.LogMaViTri.Value?.ToString() ?? string.Empty,
                LogSoLuong = Convert.ToInt32(inputHistoryXNKho.LogSoLuong.Value),
                LogTonKhoTruoc = Convert.ToInt32(inputHistoryXNKho.LogTonKhoTruoc.Value),
                LogTonKhoSau = Convert.ToInt32(inputHistoryXNKho.LogTonKhoSau.Value),
                LogNgThucHien = inputHistoryXNKho.LogNgThucHien.Value?.ToString() ?? string.Empty,
                LogThoiDiem = Convert.ToDateTime(inputHistoryXNKho.LogThoiDiem.Value)
            };
        }



        public class HistoryXNKhoConvert
        {
            [DisplayName("Loại Phiếu")]
            public string LogLoaiPhieu { get; set; } = string.Empty;

            [DisplayName("Mã Phiếu")]
            public string LogMaPhieu { get; set; } = string.Empty;

            [DisplayName("Tên NVL")]
            public string LogTenNVL { get; set; } = string.Empty;

            [DisplayName("Mã Vị Trí")]
            public string LogMaViTri { get; set; } = string.Empty;

            [DisplayName("Số Lượng")]
            public int LogSoLuong { get; set; }

            [DisplayName("Tồn Kho Trước")]
            public int LogTonKhoTruoc { get; set; }

            [DisplayName("Tồn Kho Sau")]
            public int LogTonKhoSau { get; set; }

            [DisplayName("Người Thực Hiện")]
            public string LogNgThucHien { get; set; } = string.Empty;

            [DisplayName("Thời Điểm")]
            public DateTime LogThoiDiem { get; set; }
        }

        public static string GetDisplayName(string propertyName)
        {
            var property = typeof(HistoryXNKhoConvert).GetProperty(propertyName);
            if (property == null)
            {
                return propertyName; // Return the original name if property not found
            }

            var attribute = property.GetCustomAttribute<DisplayNameAttribute>();
            return attribute?.DisplayName ?? propertyName;
        }

        public static PropertyInfo[] GetProperties()
        {
            return typeof(HistoryXNKhoConvert).GetProperties();
        }

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(HistoryXNKho);

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
