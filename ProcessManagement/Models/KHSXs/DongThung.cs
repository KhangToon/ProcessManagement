using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHSXs
{
    public class DongThung
    {
        public static class DongThungDBName
        {
            public const string Table_DongThung = "KHSX_DongThung";
            public const string DTID = "DTID";
            public const string KQGCID = "KQGCID";
            public const string SoIDThung = "SoIDThung";
            public const string SoLuong = "SoLuong";
            public const string NgayDongThung = "NgayDongThung";
        }

        public Propertyy DTID { get; set; } = new() { DBName = DongThungDBName.DTID, DisplayName = "DTID", Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // Identity ID
        public Propertyy KQGCID { get; set; } = new() { DBName = DongThungDBName.KQGCID, DisplayName = "KQGCID", Type = typeof(int), AlowDatabase = true };
        public Propertyy SoIDThung { get; set; } = new() { DBName = DongThungDBName.SoIDThung, DisplayName = "Số ID Thùng", Type = typeof(string), AlowDatabase = true };
        public Propertyy SoLuong { get; set; } = new() { DBName = DongThungDBName.SoLuong, DisplayName = "Số Lượng", Type = typeof(int), AlowDatabase = true };
        public Propertyy NgayDongThung { get; set; } = new() { DBName = DongThungDBName.NgayDongThung, DisplayName = "Ngày Đóng Thùng", Type = typeof(DateTime), AlowDatabase = true };

        // Get all property of this class
        public static List<Propertyy> GetClassProperties()
        {
            Type propertyType = typeof(DongThung); 

            DongThung instance = new();

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
            Type propertyType = typeof(DongThung);

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
