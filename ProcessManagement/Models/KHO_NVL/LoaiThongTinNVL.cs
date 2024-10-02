using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL
{
    public class LoaiThongTinNVL
    {
        public Propertyy LoaiTTNVLID { get; set; } = new() { DBName = Common.LoaiTTNVLID, DisplayName = "LoaiTTNVLID", Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // ID
        public Propertyy TenLoaiThongTin { get; set; } = new() { DBName = Common.TenLoaiThongTin, DisplayName = "Tên loại thông tin", Type = typeof(string), AlowDatabase = true, };
        public Propertyy KieuDuLieu { get; set; } = new() { DBName = Common.KieuDuLieu, DisplayName = "Kiểu dữ liệu", Type = typeof(int), AlowDatabase = true, };
        // Kieudulieu :  1-string, 2-number, 3-datetime
        public Propertyy GiaTriMacDinh { get; set; } = new() { DBName = Common.MacDinh, DisplayName = "Gía trị mặc định", Type = typeof(string), AlowDatabase = true, };
        public Propertyy IsDefault { get; set; } = new() { DBName = Common.IsDefault, DisplayName = "IsDefault", Type = typeof(bool), AlowDatabase = true, };
        public Propertyy TenTruyXuat { get; set; } = new() { DBName = Common.TenTruyXuat, DisplayName = "Tên truy xuất", Type = typeof(string), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy IsDisplayDatagrid { get; set; } = new() { DBName = Common.IsDisplayDatagrid, DisplayName = "IsDisplayDatagrid", Type = typeof(bool), AlowDatabase = true, };

        public bool IsAddForAll { get; set; } = false; // Add truong thong tin cho tat cac NVL (chi dung luc them moi truong thong tin)

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(LoaiThongTinNVL);

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
