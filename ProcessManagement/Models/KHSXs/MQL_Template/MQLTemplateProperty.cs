using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHSXs.MQL_Template
{
    public class MQLTemplateProperty
    {
        public Propertyy MQLTLpropertyID { get; set; } = new() { DBName = DBName.MQLTLpropertyID, DisplayName = DispName.MQLTLpropertyID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // Identity ID
        public Propertyy TemplateName { get; set; } = new() { DBName = DBName.TemplateName, DisplayName = DispName.TemplateName, Type = typeof(string), AlowDatabase = true };
        public Propertyy DefaultValue { get; set; } = new() { DBName = DBName.DefaultValue, DisplayName = DispName.DefaultValue, Type = typeof(string), AlowDatabase = true };
        public Propertyy IsDefault { get; set; } = new() { DBName = DBName.IsDefault, DisplayName = DispName.IsDefault, Type = typeof(bool), AlowDatabase = true };
        public Propertyy IsDisplay { get; set; } = new() { DBName = DBName.IsDisplay, DisplayName = DispName.IsDisplay, Type = typeof(bool), AlowDatabase = true };
        public Propertyy TenTruyXuat { get; set; } = new() { DBName = DBName.TenTruyXuat, DisplayName = DispName.TenTruyXuat, Type = typeof(string), AlowDatabase = true };
        public Propertyy IndexDisplay { get; set; } = new() { DBName = DBName.IndexDisplay, DisplayName = DispName.IndexDisplay, Type = typeof(int), AlowDatabase = true };
        public Propertyy KieuDuLieu { get; set; } = new() { DBName = DBName.KieuDuLieu, DisplayName = DispName.KieuDuLieu, Type = typeof(int), AlowDatabase = true };

        public static class DBName
        {
            public const string Table_MQLTLProperty = "KHSX_MQLTemplateProperty";
            public const string MQLTLpropertyID = "MQLTLpropertyID";
            public const string TemplateName = "templatename";
            public const string DefaultValue = "defaulvalue";
            public const string IsDefault = "IsDefault";
            public const string IsDisplay = "IsDisplay";
            public const string TenTruyXuat = "tentruyxuat";
            public const string IndexDisplay = "IndexDisplay";
            public const string KieuDuLieu = "kieudulieu";
        }

        public static class DispName
        {
            public const string MQLTLpropertyID = "MQLTLpropertyID";
            public const string TemplateName = "Template Name";
            public const string DefaultValue = "Default Value";
            public const string IsDefault = "IsDefault";
            public const string IsDisplay = "IsDisplay";
            public const string TenTruyXuat = "tentruyxuat";
            public const string IndexDisplay = "IndexDisplay";
            public const string KieuDuLieu = "kieudulieu";
        }

        // Get all property of this class
        public static List<Propertyy> GetClassProperties()
        {
            Type propertyType = typeof(MQLTemplateProperty);

            MQLTemplateProperty instance = new();

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
            Type propertyType = typeof(MQLTemplateProperty);

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
