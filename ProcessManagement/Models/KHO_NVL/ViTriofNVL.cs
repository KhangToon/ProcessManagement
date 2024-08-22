using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL
{
    public class ViTriofNVL
    {
        public Propertyy VTofNVLID { get; set; } = new() { DBName = Common.VTofNVLID, Type = typeof(int), AlowDatabase = false }; // ID
        public Propertyy VTID { get; set; } = new() { DBName = Common.VTID, Type = typeof(int), AlowDatabase = true };
        public Propertyy NVLID { get; set; } = new() { DBName = Common.NVLID, Type = typeof(int), AlowDatabase = true };
        public Propertyy VTNVLSoLuong { get; set; } = new() { DBName = Common.VTNVLSoLuong, Type = typeof(int), AlowDatabase = true };

        public VitriLuuTru VitriInfor = new();

        public NguyenVatLieu NgLieuInfor = new(); // chi load khi can (khong load luc load ViTriofNVL)

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(ViTriofNVL);

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
