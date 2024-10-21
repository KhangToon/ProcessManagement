using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.TienDoGCs
{
    public class TienDoGCRow
    {
        public Propertyy TDGCRowID { get; set; } = new() { DBName = DBName.TDGCRowID, DisplayName = DispName.TDGCRowID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false };
        public Propertyy NgayGC { get; set; } = new() { DBName = DBName.NgayGC, DisplayName = DispName.NgayGC, Type = typeof(DateTime), AlowDatabase = true, AlowDisplay = true };
        public Propertyy SPID { get; set; } = new() { DBName = DBName.SPID, DisplayName = DispName.SPID, Type = typeof(int), AlowDatabase = true, AlowDisplay = true };
        public Propertyy NCID { get; set; } = new() { DBName = DBName.NCID, DisplayName = DispName.NCID, Type = typeof(int), AlowDatabase = true, AlowDisplay = true };
        public Propertyy MMID { get; set; } = new() { DBName = DBName.MMID, DisplayName = DispName.MMID, Type = typeof(int), AlowDatabase = true, AlowDisplay = true };
        public Propertyy CaLamViec { get; set; } = new() { DBName = DBName.CaLamViec, DisplayName = DispName.CaLamViec, Type = typeof(string), AlowDatabase = true, AlowDisplay = true };
        public Propertyy ThoiGianGC { get; set; } = new() { DBName = DBName.ThoiGianGC, DisplayName = DispName.ThoiGianGC, Type = typeof(double), AlowDatabase = true, AlowDisplay = true };
        public Propertyy ThucTe { get; set; } = new() { DBName = DBName.ThucTe, DisplayName = DispName.ThucTe, Type = typeof(int), AlowDatabase = true, AlowDisplay = true };
        public Propertyy SLNG { get; set; } = new() { DBName = DBName.SLNG, DisplayName = DispName.SLNG, Type = typeof(int), AlowDatabase = true, AlowDisplay = true };
        public Propertyy KeHoach { get; set; } = new() { DBName = DBName.KeHoach, DisplayName = DispName.KeHoach, Type = typeof(int), AlowDatabase = true, AlowDisplay = true };
        public Propertyy TienDo { get; set; } = new() { DBName = DBName.TienDo, DisplayName = DispName.TienDo, Type = typeof(int), AlowDatabase = true, AlowDisplay = true };
        public Propertyy ThoiGianLamViec { get; set; } = new() { DBName = DBName.ThoiGianLamViec, DisplayName = DispName.ThoiGianLamViec, Type = typeof(int), AlowDatabase = true, AlowDisplay = true };
        public Propertyy ThoiGianGiaCong { get; set; } = new() { DBName = DBName.ThoiGianGiaCong, DisplayName = DispName.ThoiGianGiaCong, Type = typeof(double), AlowDatabase = true, AlowDisplay = true };
        public Propertyy GhiChu { get; set; } = new() { DBName = DBName.GhiChu, DisplayName = DispName.GhiChu, Type = typeof(string), AlowDatabase = true, AlowDisplay = true };

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
            public const string Table_TienDoGCRow = "KHSX_TienDoGCRow";
            public const string TDGCRowID = "TDGCRowID";
            public const string NgayGC = "ngaygc";
            public const string SPID = "SPID";
            public const string NCID = "NCID";
            public const string MMID = "MMID";
            public const string CaLamViec = "calamviec";
            public const string ThoiGianGC = "tggiacong";
            public const string ThucTe = "thucte";
            public const string SLNG = "SLNG";
            public const string KeHoach = "SLkehoach";
            public const string TienDo = "tiendo";
            public const string ThoiGianLamViec = "tglamviec";
            public const string ThoiGianGiaCong = "tggiacong";
            public const string GhiChu = "ghichu";
        }

        public static class DispName
        {
            public const string TDGCRowID = "TDGCRowID";
            public const string NgayGC = "Ngày gia công";
            public const string SPID = "SPID";
            public const string NCID = "NCID";
            public const string MMID = "MMID";
            public const string CaLamViec = "Ca làm việc";
            public const string ThoiGianGC = "Thời gian gia công";
            public const string ThucTe = "Thực tế";
            public const string SLNG = "SLNG";
            public const string KeHoach = "Kế hoạch";
            public const string TienDo = "Tiến độ";
            public const string ThoiGianLamViec = "Thời gian làm việc";
            public const string ThoiGianGiaCong = "Thời gian gia công";
            public const string GhiChu = "Ghi chú";
        }
    }
}
