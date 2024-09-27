using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.SANPHAM
{
    public class ThongTinSanPham
    {
        public Propertyy TTSPID { get; set; } = new() { DBName = Common.SP_TTSPID, DisplayName = "TTSPID", Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // ID
        public Propertyy GiaTriThongTin { get; set; } = new() { DBName = Common.SP_GiaTriThongTin, DisplayName = "Giá trị", Type = typeof(string), AlowDatabase = true };
        public Propertyy SPID { get; set; } = new() { DBName = "SPID", DisplayName = "SPID", Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy LoaiTTSPID { get; set; } = new() { DBName = "LoaiTTSPID", DisplayName = "LoaiTTSPID", Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };

        // Loai thong tin san pham
        public LoaiThongTinSanPham LoaiThongTin { get; set; } = new();

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(ThongTinSanPham);

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
