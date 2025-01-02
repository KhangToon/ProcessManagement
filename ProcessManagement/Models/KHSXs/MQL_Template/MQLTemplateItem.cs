using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHSXs.MQL_Template
{
    public class MQLTemplateItem
    {
        public Propertyy MQLTLItemID { get; set; } = new() { DBName = DBName.MQLTLItemID, DisplayName = DispName.MQLTLItemID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // ID
        public Propertyy MQLTLID { get; set; } = new() { DBName = DBName.MQLTLID, DisplayName = DispName.MQLTLID, Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy MQLTLpropertyID { get; set; } = new() { DBName = DBName.MQLTLpropertyID, DisplayName = DispName.MQLTLpropertyID, Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy GiaTri { get; set; } = new() { DBName = DBName.Value, DisplayName = DispName.Value, Type = typeof(string), AlowDatabase = true };

        public MQLTemplateProperty MQLTemplateProperty { get; set; } = new();

        public static class DBName
        {
            public const string Table_MQLTemplateItem = "KHSX_MQLTemplateItem";
            public const string MQLTLItemID = "MQLTLItemID";
            public const string MQLTLID = "MQLTLID";
            public const string MQLTLpropertyID = "MQLTLpropertyID";
            public const string Value = "Value";
        }

        public static class DispName
        {
            public const string MQLTLItemID = "MQLTLItemID";
            public const string MQLTLID = "MQLTLID";
            public const string MQLTLpropertyID = "MQLTLpropertyID";
            public const string Value = "Giá trị";
        }

        // Get all property of this class
        public static List<Propertyy> GetClassProperties()
        {
            Type propertyType = typeof(MQLTemplateItem);

            MQLTemplateItem instance = new();

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
            Type propertyType = typeof(MQLTemplateItem);

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
