using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.MAYMOC
{
    public class MayMoc
    {
        public Propertyy MMID { get; set; } = new() { DBName = Common.MM_MMID, DisplayName = "MMID", Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // ID
        public Propertyy MaMay { get; set; } = new() { DBName = Common.MM_MaMay, DisplayName = "Mã máy", Type = typeof(string), AlowDatabase = true };
        public Propertyy TenMay { get; set; } = new() { DBName = Common.MM_TenMay, DisplayName = "Tên máy/Thiết bị", Type = typeof(string), AlowDatabase = true };
        public Propertyy Serial { get; set; } = new() { DBName = Common.MM_Serial, DisplayName = "Serial", Type = typeof(string), AlowDatabase = true };

        // Danh sach thong tin may moc
        public List<ThongTinMayMoc> DSThongTin = new();


        // Return thong tin may moc by tentruyxuat
        public ThongTinMayMoc GetThongTinMayMocByName(string tentruyxuat)
        {
            ThongTinMayMoc targetThongTin = DSThongTin.FirstOrDefault(thongtin => thongtin.LoaiThongTin.TenTruyXuat.Value?.ToString()?.Trim() == tentruyxuat) ?? new();

            return targetThongTin;
        }

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(MayMoc);

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
