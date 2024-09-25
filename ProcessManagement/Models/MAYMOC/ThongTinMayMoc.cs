using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.MAYMOC
{
    public class ThongTinMayMoc
    {
        public Propertyy TTMMID { get; set; } = new() { DBName = Common.MM_TTMMID, DisplayName = "TTMMID", Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // ID
        public Propertyy GiaTri { get; set; } = new() { DBName = Common.MM_GiaTriThongTin, DisplayName = "Giá trị", Type = typeof(string), AlowDatabase = true };
        public Propertyy MMID { get; set; } = new() { DBName = Common.MM_MMID, DisplayName = "MMID", Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy LoaiTTMMID { get; set; } = new() { DBName = Common.MM_LoaiTTMMID, DisplayName = "LoaiTTMMID", Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };


        // Loai thong tin may moc
        public LoaiThongTinMayMoc LoaiThongTin { get; set; } = new();

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(ThongTinMayMoc);

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
