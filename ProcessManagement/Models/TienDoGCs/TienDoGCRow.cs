using ProcessManagement.Commons;
using ProcessManagement.Models.NHANVIEN;
using System.Reflection;
using static ProcessManagement.Models.KHSXs.KetQuaGC;

namespace ProcessManagement.Models.TienDoGCs
{
    public class TienDoGCRow
    {
        public Propertyy TDGCRowID { get; set; } = new() { DBName = DBName.TDGCRowID, DisplayName = DispName.TDGCRowID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false };
        public Propertyy TDGCID { get; set; } = new() { DBName = DBName.TDGCID, DisplayName = DispName.TDGCID, Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy NgayGC { get; set; } = new() { DBName = DBName.NgayGC, DisplayName = DispName.NgayGC, Type = typeof(DateTime), AlowDatabase = true };
        public Propertyy SPID { get; set; } = new() { DBName = DBName.SPID, DisplayName = DispName.SPID, Type = typeof(int), AlowDatabase = true };
        public Propertyy NCID { get; set; } = new() { DBName = DBName.NCID, DisplayName = DispName.NCID, Type = typeof(int), AlowDatabase = true };
        public Propertyy MMID { get; set; } = new() { DBName = DBName.MMID, DisplayName = DispName.MMID, Type = typeof(int), AlowDatabase = true };
        public Propertyy CaLamViec { get; set; } = new() { DBName = DBName.CaLamViec, DisplayName = DispName.CaLamViec, Type = typeof(string), AlowDatabase = true };
        public Propertyy ThoiGianGiaCong { get; set; } = new() { DBName = DBName.ThoiGianGiaCong, DisplayName = DispName.ThoiGianGiaCong, Type = typeof(int), AlowDatabase = true };
        public Propertyy ThoiGianLamViec { get; set; } = new() { DBName = DBName.ThoiGianLamViec, DisplayName = DispName.ThoiGianLamViec, Type = typeof(double), AlowDatabase = true };
        public Propertyy NVIDs { get; set; } = new() { DBName = DBName.NVIDs, DisplayName = DispName.DSTenNV, Type = typeof(string), AlowDatabase = true, CheckErrors = new() { Propertyy.ErrType.NotEmptyValue } };
        public Propertyy KeHoach { get; set; } = new() { DBName = DBName.KeHoach, DisplayName = DispName.KeHoach, Type = typeof(int), AlowDatabase = true };
        public Propertyy ThucTe { get; set; } = new() { DBName = DBName.ThucTe, DisplayName = DispName.ThucTe, Type = typeof(int), AlowDatabase = true, AlowDisplay = false };
        public Propertyy SLNG { get; set; } = new() { DBName = DBName.SLNG, DisplayName = DispName.SLNG, Type = typeof(int), AlowDatabase = true, AlowDisplay = false };
        public Propertyy TienDo { get; set; } = new() { DBName = DBName.TienDo, DisplayName = DispName.TienDo, Type = typeof(int), AlowDatabase = true, AlowDisplay = false };
        public Propertyy TiLeNG_CD { get; set; } = new() { DBName = DBName.TiLeNG_CD, DisplayName = DispName.TiLeNG_CD, Type = typeof(double), AlowDatabase = true, AlowDisplay = false };
        public Propertyy TiLeNG_TT { get; set; } = new() { DBName = DBName.TiLeNG_TT, DisplayName = DispName.TiLeNG_TT, Type = typeof(double), AlowDatabase = true, AlowDisplay = false };
        public Propertyy GhiChu { get; set; } = new() { DBName = DBName.GhiChu, DisplayName = DispName.GhiChu, Type = typeof(string), AlowDatabase = true, CheckErrors = new() { } };

        public List<NhanVien> DSNhanVien { get; set; } = new();
        public string MaSanPham { get; set; } = string.Empty;
        public string TenCongDoan { get; set; } = string.Empty;

        public bool IsEditing { get; set; } = false;
        // Get all property of this class
        public static List<Propertyy> GetClassProperties()
        {
            Type propertyType = typeof(TienDoGCRow);

            TienDoGCRow instance = new();

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
            Type propertyType = typeof(TienDoGCRow);

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
        public static class DBName
        {
            public const string Table_TienDoGCRow = "TDGC_TienDoGCRow";
            public const string TDGCRowID = "TDGCRowID";
            public const string NgayGC = "ngaygc";
            public const string SPID = "SPID";
            public const string TDGCID = "TDGCID";
            public const string NCID = "NCID";
            public const string MMID = "MMID";
            public const string CaLamViec = "calamviec";
            public const string ThucTe = "thucte";
            public const string SLNG = "SLNG";
            public const string KeHoach = "slkehoach";
            public const string TienDo = "tiendo";
            public const string ThoiGianLamViec = "thoigianlamviec";
            public const string ThoiGianGiaCong = "thoigiangiacong";
            public const string GhiChu = "ghichu";
            public const string NVIDs = "NVIDs";
            public const string TiLeNG_CD = "tile_ngcd";
            public const string TiLeNG_TT = "tile_ngtt";
        }

        public static class DispName
        {
            public const string TDGCRowID = "TDGCRowID";
            public const string NgayGC = "Ngày gia công";
            public const string SPID = "Sản phẩm";
            public const string TDGCID = "TDGCID";
            public const string NCID = "Công đoạn";
            public const string MMID = "Mã máy";
            public const string CaLamViec = "Ca làm việc";
            public const string ThucTe = "Thực tế";
            public const string SLNG = "SLNG";
            public const string KeHoach = "Kế hoạch";
            public const string TienDo = "Tiến độ";
            public const string ThoiGianLamViec = "Thời gian làm việc";
            public const string ThoiGianGiaCong = "Thời gian gia công";
            public const string GhiChu = "Ghi chú";
            public const string DSTenNV = "Nhân viên";
            public const string TiLeNG_CD = "% NG CĐ";
            public const string TiLeNG_TT = "% NG TT";
        }
    }
}
