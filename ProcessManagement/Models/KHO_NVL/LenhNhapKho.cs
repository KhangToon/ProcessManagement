using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL
{
    public class LenhNhapKho
    {
        public Propertyy LenhNKID { get; set; } = new() { DBName = Common.LenhNKID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // ID
        public Propertyy PNKID { get; set; } = new() { DBName = Common.PNKID, Type = typeof(int), AlowDatabase = true };
        public Propertyy NVLPNKID { get; set; } = new() { DBName = Common.NVLPNKID, Type = typeof(int), AlowDatabase = true }; 
        public Propertyy VTID { get; set; } = new() { DBName = Common.VTID, Type = typeof(int), AlowDatabase = true };
        public Propertyy LNKSoLuong { get; set; } = new() { DBName = Common.LNKSoLuong, Type = typeof(int), AlowDatabase = true };
        public Propertyy LNKIsDone { get; set; } = new() { DBName = Common.LNKIsDone, Type = typeof(int), AlowDatabase = true };

        public VitriLuuTru TagertVitri { get; set; } = new();

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(LenhNhapKho);

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
