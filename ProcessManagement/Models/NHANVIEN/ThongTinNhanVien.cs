using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.NHANVIEN
{
    public class ThongTinNhanVien
    {
        public Propertyy TTNVID { get; set; } = new() { DBName = Common.NV_TTNVID, DisplayName = "TTNVID", Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // ID
        public Propertyy GiaTri { get; set; } = new() { DBName = Common.NV_GiaTriThongTin, DisplayName = "Giá trị", Type = typeof(string), AlowDatabase = true };
        public Propertyy NVID { get; set; } = new() { DBName = Common.NV_NVID, DisplayName = "NVID", Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy LoaiTTNVID { get; set; } = new() { DBName = Common.NV_LoaiTTNVID, DisplayName = "LoaiTTNVID", Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };

        // Loai thong tin nhan vien
        public LoaiThongTinNhanVien LoaiThongTin { get; set; } = new();

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(ThongTinNhanVien);

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
