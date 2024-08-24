using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL.XuatKho
{
    public class LenhXuatKho
    {
        public Propertyy LenhXKID { get; set; } = new() { DBName = Common.LenhXKID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // ID
        public Propertyy PXKID { get; set; } = new() { DBName = Common.PXKID, Type = typeof(int), AlowDatabase = true };
        public Propertyy NVLPXKID { get; set; } = new() { DBName = Common.NVLPXKID, Type = typeof(int), AlowDatabase = true };
        public Propertyy VTID { get; set; } = new() { DBName = Common.VTID, Type = typeof(int), AlowDatabase = true };
        public Propertyy NVLID { get; set; } = new() { DBName = Common.NVLID, Type = typeof(int), AlowDatabase = true };
        public Propertyy LXKSoLuong { get; set; } = new() { DBName = Common.LXKSoLuong, Type = typeof(int), AlowDatabase = true };
        public Propertyy LXKIsDone { get; set; } = new() { DBName = Common.LXKIsDone, Type = typeof(int), AlowDatabase = true };

        public ViTriofNVL ViTriofNVL { get; set; } = new(); // Chi dung cho phieu xuat kho

        public bool isEditingLXK = false; // chi dung khi xuat kho

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(LenhXuatKho);

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
