using ProcessManagement.Commons;
using ProcessManagement.Models.SANPHAM;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL
{
    public class SanPhamofNVL
    {
        public Propertyy SPofNVLID { get; set; } = new() { DBName = DBName.SPofNVLID, DisplayName = DispName.SPofNVLID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false };
        public Propertyy SPID { get; set; } = new() { DBName = DBName.SPID, DisplayName = DispName.SPID, Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy NVLID { get; set; } = new() { DBName = DBName.NVLID, DisplayName = DispName.NVLID, Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy NgayThem { get; set; } = new() { DBName = DBName.NgayThem, DisplayName = DispName.NgayThem, Type = typeof(DateTime), AlowDatabase = true };
        public Propertyy SoLuong { get; set; } = new() { DBName = DBName.SoLuong, DisplayName = DispName.SoLuong, Type = typeof(int), AlowDatabase = true, AlowDisplay = false };
        public bool isEditingSoluong = false;
        public SanPham TargetSP { get; set; } = new();

        public static class DBName
        {
            public const string Table_SPofNVL = "KHO_SPofNVL";
            public const string SPofNVLID = "SPofNVLID";
            public const string SPID = "SPID";
            public const string NVLID = "NVLID";
            public const string NgayThem = "ngaythem";
            public const string SoLuong = "soluong";
        }

        public static class DispName
        {
            public const string SPofNVLID = "SPofNVLID";
            public const string SPID = "SPID";
            public const string NVLID = "NVLID";
            public const string NgayThem = "Ngày thêm";
            public const string SoLuong = "Số lượng";
        }

        // Get all property of this class
        public static List<Propertyy> GetClassProperties()
        {
            Type propertyType = typeof(SanPhamofNVL);

            SanPhamofNVL instance = new();

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
            Type propertyType = typeof(SanPhamofNVL);

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
