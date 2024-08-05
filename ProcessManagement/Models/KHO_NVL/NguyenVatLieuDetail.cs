using ProcessManagement.Commons;
using ProcessManagement.Pages.KehoachSX;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL
{
    public class NguyenVatLieuDetail
    {
        public Propertyy TTNVLID { get; set; } = new() { DBName = Common.TTNVLID, DisplayName = "TTNVLID", Type = typeof(int), AlowDatabase = false, AlowDisplay = false }; // ID
        public Propertyy NVLID { get; set; } = new() { DBName = Common.NVLID, DisplayName = "NVL ID", Type = typeof(int), AlowDisplay = false };
        public Propertyy TenTTID { get; set; } = new() { DBName = Common.TenTTID, DisplayName = "TenTTID", Type = typeof(int), AlowDisplay = false };
        public Propertyy NoiDung { get; set; } = new() { DBName = Common.NoiDung, DisplayName = "Nội dung", Type = typeof(string), AlowDatabase = true };

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(NguyenVatLieuDetail);

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
