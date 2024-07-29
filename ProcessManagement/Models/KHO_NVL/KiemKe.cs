using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL
{
    public class KiemKe
    {
        public Propertyy KIEMKEID { get; set; } = new() { DBName = Common.KIEMKEID, DisplayName = "Kiểm kê ID", Type = typeof(int), AlowDatabase = true }; // ID
        public Propertyy NVLID { get; set; } = new() { DBName = Common.NVLID, DisplayName = "NVL ID", Type = typeof(int), AlowDatabase = true };
        public Propertyy KHOID { get; set; } = new() { DBName = Common.KHOID, DisplayName = "Kho ID", Type = typeof(int), AlowDatabase = true };
        public Propertyy NgayKiemKe { get; set; } = new() { DBName = Common.NgayKiemKe, DisplayName = "Ngày kiểm kê", Type = typeof(DateTime), AlowDatabase = true };
        public Propertyy SoLuongThucTe { get; set; } = new() { DBName = Common.SoLuongThucTe, DisplayName = "Số lượng thực tế", Type = typeof(decimal), AlowDatabase = true };
        public Propertyy SoLuongHeThong { get; set; } = new() { DBName = Common.SoLuongHeThong, DisplayName = "Số lượng hệ thống", Type = typeof(decimal), AlowDatabase = true };
        public Propertyy ChenhLech { get; set; } = new() { DBName = Common.ChenhLech, DisplayName = "Chênh lệch", Type = typeof(decimal), AlowDatabase = true };
        public Propertyy NguyenNhan { get; set; } = new() { DBName = Common.NguyenNhan, DisplayName = "Nguyên nhân", Type = typeof(string), AlowDatabase = true };
        public Propertyy NguoiKiemKe { get; set; } = new() { DBName = Common.NguoiKiemKe, DisplayName = "Người kiểm kê", Type = typeof(string), AlowDatabase = true };

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(KiemKe);

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
