using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models
{
    public class NguyenLieu
    {
        public Propertyy NLID = new() { DBName = Common.NLID, Type = typeof(int), DispDatagrid = false, AlowDatabase = true };

        public Propertyy MaSP = new() { DBName = Common.MaSP, Type = typeof(string), AlowDatabase = true };

        public Propertyy LoaiNL = new() { DBName = Common.LoaiNL, Type = typeof(string), AlowDatabase = true };

        public Propertyy NgayNhap = new() { DBName = Common.NgayNhap, Type = typeof(DateTime), AlowDatabase = true };

        public Propertyy LotNVL = new() { DBName = Common.LotNVL, Type = typeof(string), AlowDatabase = true };

        public Propertyy SoLuongNL = new() { DBName = Common.SoLuongNVL, Type = typeof(int), AlowDatabase = true };

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(NguyenLieu);

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
