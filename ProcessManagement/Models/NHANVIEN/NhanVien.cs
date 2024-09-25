using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.NHANVIEN
{
    public class NhanVien
    {   
        public Propertyy NVID { get; set; } = new() { DBName = Common.NV_NVID, DisplayName = "NVID", Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // ID
        public Propertyy MaNhanVien { get; set; } = new() { DBName = Common.NV_MaNhanVien, DisplayName = "Mã nhân viên", Type = typeof(string), AlowDatabase = true };

        // Danh sach thong tin nha vien
        public List<ThongTinNhanVien> DSThongTin = new();

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(NhanVien);

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
