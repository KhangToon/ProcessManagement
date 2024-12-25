using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_TPHAM
{
    public class ViTriTPham
    {
        public Propertyy VTTPID { get; set; } = new() { DBName = DBName.VTTPID, Type = typeof(int), AlowDatabase = false }; // ID
        public Propertyy MaViTri { get; set; } = new() { DBName = DBName.MaViTri, DisplayName = DispName.MaViTri, Type = typeof(string), AlowDatabase = true, IsCheckSameValue = true };
        public Propertyy VTSucChua { get; set; } = new() { DBName = DBName.VTSucChua, DisplayName = DispName.VTSucChua, Type = typeof(int), AlowDatabase = true };
        public Propertyy ViTriKe { get; set; } = new() { DBName = DBName.ViTriKe, DisplayName = DispName.ViTriKe, Type = typeof(string), AlowDatabase = true };
        public Propertyy ViTriHang { get; set; } = new() { DBName = DBName.ViTriHang, DisplayName = DispName.ViTriHang, Type = typeof(string), AlowDatabase = true };
        public Propertyy ViTriCot { get; set; } = new() { DBName = DBName.ViTriCot, DisplayName = DispName.ViTriCot, Type = typeof(string), AlowDatabase = true };

        public static class DBName
        {
            public const string Table_ViTriTPham = "KHO_ViTriTPham";
            public const string VTTPID = "VTTPID";
            public const string MaViTri = "MaViTri";
            public const string VTSucChua = "VTSucChua";
            public const string ViTriKe = "ViTriKe";
            public const string ViTriHang = "ViTriHang";
            public const string ViTriCot = "ViTriCot";
        }

        private class DispName
        {
            public const string MaViTri = "Mã vị trí";
            public const string VTSucChua = "Sức chứa";
            public const string ViTriKe = "Vị trí kệ";
            public const string ViTriHang = "Vị trí hàng";
            public const string ViTriCot = "Vị trí cột";
        }

        // Get all property of this class
        public static List<Propertyy> GetClassProperties()
        {
            Type propertyType = typeof(ViTriTPham);

            ViTriTPham instance = new();

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
            Type propertyType = typeof(ViTriTPham);

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
