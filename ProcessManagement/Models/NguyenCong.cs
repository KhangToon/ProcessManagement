using ProcessManagement.Commons;
using ProcessManagement.Models.KHSXs;
using System.Reflection;

namespace ProcessManagement.Models
{
    public class NguyenCong
    {
        public Propertyy NCID { get; set; } = new() { DBName = DBName.NCID, DisplayName = DispName.NCID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false };
        public Propertyy TenNguyenCong { get; set; } = new() { DBName = DBName.TenNguyenCong, DisplayName = DispName.TenNguyenCong, Type = typeof(string), AlowDatabase = true };
        public Propertyy Ghichu { get; set; } = new() { DBName = DBName.Ghichu, DisplayName = DispName.Ghichu, Type = typeof(string), AlowDatabase = true };
        public Propertyy NGIDs { get; set; } = new() { DBName = DBName.NGIDs, DisplayName = DispName.NGIDs, Type = typeof(string), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy IsHide { get; set; } = new() { DBName = DBName.IsHide, DisplayName = DispName.IsHide, Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };

        public List<NGType> DSNGTypes = new();

        public bool isHiding { get; set; } = false;

        public bool IsPendingRemove { get; set; } = false;

        public static class DBName
        {
            public const string Table_NguyenCong = "Table_DSNguyenCong";
            public const string NCID = "NCID";
            public const string TenNguyenCong = "Nguyên công";
            public const string Ghichu = "Ghi chú";
            public const string NGIDs = "NGIDs";
            public const string IsHide = "IsHide";
        }

        private class DispName
        {
            public const string NCID = "NCID";
            public const string TenNguyenCong = "Công đoạn";
            public const string Ghichu = "Ghi chú";
            public const string NGIDs = "NGIDs";
            public const string IsHide = "IsHide";
        }

        public static List<Propertyy> GetClassProperties()
        {
            Type propertyType = typeof(NguyenCong);

            NguyenCong instance = new();

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
            Type propertyType = typeof(NguyenCong);

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
