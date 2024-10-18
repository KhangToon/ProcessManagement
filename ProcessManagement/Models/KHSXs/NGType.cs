using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHSXs
{
    public class NGType
    {
        public Propertyy NGID { get; set; } = new() { DBName = NGTypeDBName.NGID, DisplayName = NGTypeDispName.NGID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // Identity ID
        public Propertyy NoiDungNG { get; set; } = new() { DBName = NGTypeDBName.NoiDungNG, DisplayName = NGTypeDispName.NoiDungNG, Type = typeof(string), AlowDatabase = true, CheckErrors = new() { Propertyy.ErrType.NotEmptyValue } };

        public static class NGTypeDBName
        {
            public const string Table_NGType = "KHSX_NGType";
            public const string NGID = "NGID";
            public const string NoiDungNG = "NoiDungNG";
        }

        private class NGTypeDispName
        {
            public const string NGID = "NGID";
            public const string NoiDungNG = "Nội dung NG";
        }

        // Get all property of this class
        public static List<Propertyy> GetClassProperties()
        {
            Type propertyType = typeof(NGType);

            NGType instance = new();

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
            Type propertyType = typeof(NGType);

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
