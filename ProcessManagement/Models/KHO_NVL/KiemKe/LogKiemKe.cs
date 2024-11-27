using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL.KiemKe
{
    public class LogKiemKe
    {
        public Propertyy LOGKKEID { get; set; } = new() { DBName = DBName.LOGKKEID, DisplayName = DispName.LOGKKEID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false };
        public Propertyy VTofNVLID { get; set; } = new() { DBName = DBName.VTofNVLID, DisplayName = DispName.VTofNVLID, Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy NgayKiemKe { get; set; } = new() { DBName = DBName.NgayKiemKe, DisplayName = DispName.NgayKiemKe, Type = typeof(DateTime), AlowDatabase = true };
        public Propertyy SLTruoc { get; set; } = new() { DBName = DBName.SLTruoc, DisplayName = DispName.SLTruoc, Type = typeof(int), AlowDatabase = true, AlowDisplay = false };
        public Propertyy SLSau { get; set; } = new() { DBName = DBName.SLSau, DisplayName = DispName.SLSau, Type = typeof(int), AlowDatabase = true, AlowDisplay = false };

        public static class DBName
        {
            public const string Table_LogKiemKe = "KHO_LogKiemKe";
            public const string LOGKKEID = "LOGKKEID";
            public const string VTofNVLID = "VTofNVLID";
            public const string NgayKiemKe = "ngaykiemke";
            public const string SLTruoc = "soluongtruoc";
            public const string SLSau = "soluongsau";
        }

        public static class DispName
        {
            public const string LOGKKEID = "LOGKKEID";
            public const string VTofNVLID = "VTofNVLID";
            public const string NgayKiemKe = "Ngày kiểm";
            public const string SLTruoc = "Số lượng trước";
            public const string SLSau = "Số lượng sau";
        }

        // Get all property of this class
        public static List<Propertyy> GetClassProperties()
        {
            Type propertyType = typeof(LogKiemKe);

            LogKiemKe instance = new();

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
            Type propertyType = typeof(LogKiemKe);

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
