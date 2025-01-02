using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHSXs.MQL_Template
{
    public class MQLTemplate
    {
        public Propertyy MQLTLID { get; set; } = new() { DBName = DBName.MQLTLID, DisplayName = DispName.MQLTLID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // Identity ID

        public List<MQLTemplateItem> MQLTemplateItems { get; set; } = new();

        public static class DBName
        {
            public const string Table_MQLTemplate = "KHSX_MQLTemplate";
            public const string MQLTLID = "MQLTLID";
        }

        public static class DispName
        {
            public const string MQLTLID = "MQLTLID";
        }

        public static readonly string Property_MaLenhSX = "malenhsanxuat";
        public static readonly string Property_MaSanPham = "masanpham";
        public static readonly string Property_NgayNhapKho = "ngaynhapkho";

        // Get all property of this class
        public static List<Propertyy> GetClassProperties()
        {
            Type propertyType = typeof(MQLTemplate);

            MQLTemplate instance = new();

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
            Type propertyType = typeof(MQLTemplate);

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
