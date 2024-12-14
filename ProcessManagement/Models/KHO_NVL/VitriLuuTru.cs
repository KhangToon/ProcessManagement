using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL
{
    public class VitriLuuTru
    {
        public Propertyy VTID { get; set; } = new() { DBName = Common.VTID, Type = typeof(int), AlowDatabase = false }; // ID
        public Propertyy MaViTri { get; set; } = new() { DBName = Common.MaViTri, DisplayName = Common.MaViTri, Type = typeof(string), AlowDatabase = true, IsCheckSameValue = true };
        public Propertyy VTSucChua { get; set; } = new() { DBName = Common.VTSucChua, DisplayName = Common.VTSucChua, Type = typeof(int), AlowDatabase = true };
        public Propertyy ViTriHang { get; set; } = new() { DBName = Common.ViTriHang, DisplayName = Common.ViTriHang, Type = typeof(string), AlowDatabase = true };
        public Propertyy ViTriKe { get; set; } = new() { DBName = Common.ViTriKe, DisplayName = Common.ViTriKe, Type = typeof(string), AlowDatabase = true };
        public Propertyy ViTriCot { get; set; } = new() { DBName = Common.ViTriCot, DisplayName = Common.ViTriCot, Type = typeof(string), AlowDatabase = true };

        public int SLConTrong = 0;

        public List<ViTriofNVL> DSachViTriofNVL = new(); // chi dung khi can (khong load khi load VitriLuuTru)

        // Get all property of this class
        public static List<Propertyy> GetClassProperties()
        {
            Type propertyType = typeof(VitriLuuTru);

            VitriLuuTru instance = new();

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
            Type propertyType = typeof(VitriLuuTru);

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
