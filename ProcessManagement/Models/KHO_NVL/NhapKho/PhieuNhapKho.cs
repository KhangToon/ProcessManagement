using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL.NhapKho
{
    public class PhieuNhapKho
    {
        public Propertyy PNKID { get; set; } = new() { DBName = Common.PNKID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // ID
        public Propertyy MaPhieuNK { get; set; } = new() { DBName = Common.MaPhieuNhapKho, Type = typeof(string), AlowDatabase = true };
        public Propertyy NguoiLapPNK { get; set; } = new() { DBName = Common.NguoiLapPNK, Type = typeof(string), AlowDatabase = true };
        public Propertyy NgayLapPNK { get; set; } = new() { DBName = Common.NgayLapPNK, Type = typeof(DateTime), AlowDatabase = true };
        public Propertyy GhiChuPNK { get; set; } = new() { DBName = Common.GhiChuPNK, Type = typeof(string), AlowDatabase = true };
        public Propertyy IsDonePNK { get; set; } = new() { DBName = Common.IsDonePNK, Type = typeof(int), AlowDatabase = true };

        public List<NVLofPhieuNhapKho> DSNVLofPNKs { get; set; } = new();

        public bool isPXKDoneNhapKho = false;
        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(PhieuNhapKho);

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
