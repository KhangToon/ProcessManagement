using ProcessManagement.Commons;
using ProcessManagement.Models.NHANVIEN;
using System.Reflection;

namespace ProcessManagement.Models.KHSXs
{
    public class KetQuaGC
    {
        public Propertyy KQGCID { get; set; } = new() { DBName = KQGCDBName.KQGCID, DisplayName = KQGCDisplayName.KQGCID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // Identity ID
        public Propertyy KHSXID { get; set; } = new() { DBName = KQGCDBName.KHSXID, DisplayName = KQGCDisplayName.KHSXID, Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy SubMitDay { get; set; } = new() { DBName = KQGCDBName.SubMitDay, DisplayName = KQGCDisplayName.SubMitDay, Type = typeof(DateTime), AlowDatabase = true, CheckErrors = new() { Propertyy.ErrType.NotEmptyValue } };
        public Propertyy NCID { get; set; } = new() { DBName = KQGCDBName.NCID, DisplayName = KQGCDisplayName.TenNguyenCong, Type = typeof(string), AlowDatabase = true, CheckErrors = new() { Propertyy.ErrType.NotEmptyValue } };
        public Propertyy SPID { get; set; } = new() { DBName = KQGCDBName.SPID, DisplayName = KQGCDisplayName.TenSanPham, Type = typeof(string), AlowDatabase = true, CheckErrors = new() { Propertyy.ErrType.NotEmptyValue } };
        public Propertyy MaQuanLyLot { get; set; } = new() { DBName = KQGCDBName.MaQuanLyLot, DisplayName = KQGCDisplayName.MaQuanLyLot, Type = typeof(string), AlowDatabase = true, CheckErrors = new() { Propertyy.ErrType.NotEmptyValue } };
        public Propertyy SLperLOT { get; set; } = new() { DBName = KQGCDBName.SLperLOT, DisplayName = KQGCDisplayName.SLperLOT, Type = typeof(int), AlowDatabase = true, CheckErrors = new() { Propertyy.ErrType.NotEmptyValue, Propertyy.ErrType.NotAllowEqualsZero } };
        public Propertyy SLOK { get; set; } = new() { DBName = KQGCDBName.SLOK, DisplayName = KQGCDisplayName.SLOK, Type = typeof(int), AlowDatabase = true, CheckErrors = new() { Propertyy.ErrType.NotEmptyValue } };
        public Propertyy SLNG { get; set; } = new() { DBName = KQGCDBName.SLNG, DisplayName = KQGCDisplayName.SLNG, Type = typeof(int), AlowDatabase = true, CheckErrors = new() { Propertyy.ErrType.NotEmptyValue } };
        public Propertyy NGIDs { get; set; } = new() { DBName = KQGCDBName.NGIDs, DisplayName = KQGCDisplayName.NGIDs, Type = typeof(string), AlowDatabase = true, CheckErrors = new() { Propertyy.ErrType.NotEmptyValue } };
        public Propertyy MMID { get; set; } = new() { DBName = KQGCDBName.MMID, DisplayName = KQGCDisplayName.MaMay, Type = typeof(string), AlowDatabase = true, CheckErrors = new() { Propertyy.ErrType.NotEmptyValue } };
        public Propertyy CaLamViec { get; set; } = new() { DBName = KQGCDBName.CaLamViec, DisplayName = KQGCDisplayName.CaLamViec, Type = typeof(string), AlowDatabase = true, CheckErrors = new() { Propertyy.ErrType.NotEmptyValue } };
        public Propertyy NVIDs { get; set; } = new() { DBName = KQGCDBName.NVIDs, DisplayName = KQGCDisplayName.DSTenNV, Type = typeof(string), AlowDatabase = true, CheckErrors = new() { Propertyy.ErrType.NotEmptyValue } };
        public Propertyy ThoiGianGC { get; set; } = new() { DBName = KQGCDBName.ThoiGianGC, DisplayName = KQGCDisplayName.ThoiGianGC, Type = typeof(int), AlowDatabase = true, CheckErrors = new() { Propertyy.ErrType.NotEmptyValue } };
        public Propertyy ThoiGianLamViec { get; set; } = new() { DBName = KQGCDBName.ThoiGianLamViec, DisplayName = KQGCDisplayName.ThoiGianLamViec, Type = typeof(double), AlowDatabase = true, CheckErrors = new() { Propertyy.ErrType.NotEmptyValue } };
        public Propertyy GhiChu { get; set; } = new() { DBName = KQGCDBName.GhiChu, DisplayName = KQGCDisplayName.GhiChu, Type = typeof(string), AlowDatabase = true, CheckErrors = new() { Propertyy.ErrType.NotEmptyValue } };

        //public List<DongThung> DSDongThung { get; set; } = new();
        public List<NhanVien> DSNhanVien { get; set; } = new();
        public List<NGType> DSNGType { get; set; } = new();
        public static class KQGCDBName
        {
            public const string Table_KetQuaGC = "KHSX_KetQuaGC";
            public const string KQGCID = "KQGCID";
            public const string KHSXID = "KHSXID";
            public const string SubMitDay = "SubMitDay";
            public const string SPID = "SPID";
            public const string NCID = "NCID";
            public const string MMID = "MMID";
            public const string CaLamViec = "CaLamViec";
            public const string ThoiGianGC = "ThoiGianGC";
            public const string MaQuanLyLot = "MaQuanLyLOT";
            public const string SLOK = "SLOK";
            public const string SLNG = "SLNG";
            public const string SLperLOT = "SLperLOT";
            public const string NGIDs = "NGIDs";
            public const string ThoiGianLamViec = "ThoiGianLamViec";
            public const string NVIDs = "NVIDs";
            public const string GhiChu = "GhiChu";
        }

        private class KQGCDisplayName
        {
            public const string KQGCID = "KQGCID";
            public const string KHSXID = "KHSXID";
            public const string SubMitDay = "Ngày gia công";
            public const string TenSanPham = "Sản phẩm";
            public const string TenNguyenCong = "Nguyên công";
            public const string MaMay = "Mã máy";
            public const string CaLamViec = "Ca";
            public const string ThoiGianGC = "TGian GC";
            public const string MaQuanLyLot = "Mã LOT";
            public const string SLOK = "SL OK";
            public const string SLNG = "SL NG";
            public const string SLperLOT = "Tổng";
            public const string NGIDs = "Nội dung NG";
            public const string ThoiGianLamViec = "TGian làm việc";
            public const string DSTenNV = "Nhân viên";
            public const string GhiChu = "Ghi chú";
        }

        // Get all property of this class
        public static List<Propertyy> GetClassProperties()
        {
            Type propertyType = typeof(KetQuaGC);

            KetQuaGC instance = new();

            FieldInfo[] fields = propertyType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            List<Propertyy> propertiesValue = new();

            foreach (FieldInfo field in fields)
            {
                Type ob = field.FieldType;

                if (ob == typeof(Propertyy))
                {
                    Propertyy? fieldValue = (Propertyy?)field.GetValue(instance);

                    if (fieldValue != null)
                    {
                        propertiesValue.Add(fieldValue);
                    }
                }
            }

            return propertiesValue;
        }

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(KetQuaGC);

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
