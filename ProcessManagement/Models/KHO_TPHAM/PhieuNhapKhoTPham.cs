using ProcessManagement.Commons;
using ProcessManagement.Models.KHSXs;
using System.Reflection;

namespace ProcessManagement.Models.KHO_TPHAM
{
    public class PhieuNhapKhoTPham
    {
        public Propertyy PNKTPID { get; set; } = new() { DBName = Common.PNKTPID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // ID
        public Propertyy MaPhieuNK { get; set; } = new() { DBName = Common.MaPhieuNhapKho, Type = typeof(string), AlowDatabase = true };
        public Propertyy CodePallet { get; set; } = new() { DBName = Common.CodePallet, Type = typeof(string), AlowDatabase = true };
        public Propertyy NguoiLapPNK { get; set; } = new() { DBName = Common.NguoiLapPNK, Type = typeof(string), AlowDatabase = true };
        public Propertyy NgayLapPNK { get; set; } = new() { DBName = Common.NgayLapPNK, Type = typeof(DateTime), AlowDatabase = true };
        public Propertyy GhiChuPNK { get; set; } = new() { DBName = Common.GhiChuPNK, Type = typeof(string), AlowDatabase = true };
        public Propertyy IsDonePNK { get; set; } = new() { DBName = Common.IsDonePNK, Type = typeof(int), AlowDatabase = true };
        public Propertyy VTTPID { get; set; } = new() { DBName = Common.VTTPID, DisplayName = Common.VTTPID, Type = typeof(int), AlowDatabase = true };
        public Propertyy VTofTPID { get; set; } = new() { DBName = Common.VTofTPID, DisplayName = Common.VTofTPID, Type = typeof(int), AlowDatabase = true };

        public bool isPNKDoneNhapKho = false;

        public List<ThungTPham> ListKhoThungTPham { get; set; } = new();

        public object MaViTri { get; set; } = new();

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(PhieuNhapKhoTPham);

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
