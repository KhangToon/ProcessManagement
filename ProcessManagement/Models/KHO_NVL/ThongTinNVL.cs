using ProcessManagement.Commons;
using ProcessManagement.Pages.KehoachSX;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL
{
    public class ThongTinNVL
    {
        public Propertyy TTNVLID { get; set; } = new() { DBName = Common.TTNVLID, DisplayName = "TTNVLID", Type = typeof(int), AlowDatabase = false, AlowDisplay = false }; // ID
        public Propertyy NVLID { get; set; } = new() { DBName = Common.NVLID, DisplayName = "NVL ID", Type = typeof(int), AlowDisplay = false, AlowDatabase = true };
        public Propertyy LoaiTTNVLID { get; set; } = new() { DBName = Common.LoaiTTNVLID, DisplayName = "LoaiTTNVLID", Type = typeof(int), AlowDisplay = false, AlowDatabase = true };
        public Propertyy GiaTri { get; set; } = new() { DBName = Common.GiaTri, DisplayName = "Giá trị", Type = typeof(string), AlowDatabase = true };

        public LoaiThongTinNVL LoaiThongTin { get; set; } = new();

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(ThongTinNVL);

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
