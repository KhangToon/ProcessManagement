using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL.XuatKho
{
    public class PhieuXuatKho
    {
        public Propertyy PXKID { get; set; } = new() { DBName = Common.PXKID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // ID
        public Propertyy MaPhieuXK { get; set; } = new() { DBName = Common.MaPhieuXuatKho, Type = typeof(string), AlowDatabase = true };
        public Propertyy KHSXID { get; set; } = new() { DBName = Common.KHSXID, Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy NguoiLapPXK { get; set; } = new() { DBName = Common.NguoiLapPXK, Type = typeof(string), AlowDatabase = true };
        public Propertyy NgayLapPXK { get; set; } = new() { DBName = Common.NgayLapPXK, Type = typeof(DateTime), AlowDatabase = true };
        public Propertyy GhiChuPXK { get; set; } = new() { DBName = Common.GhiChuPXK, Type = typeof(string), AlowDatabase = true };
        public Propertyy IsConfirmedPXK { get; set; } = new() { DBName = Common.IsConfirmedPXK, Type = typeof(int), AlowDatabase = true };
        public Propertyy PNKID { get; set; } = new() { DBName = Common.PNKID, Type = typeof(int), AlowDatabase = true }; // using for detect PXK is created return phieu tra kho
        //public Propertyy IsDonePXK { get; set; } = new() { DBName = Common.IsDonePXK, Type = typeof(int), AlowDatabase = true };

        public bool IsPXKDoneXuatKho = false; // Da load o SQLServices

        public bool IsChiDinhDuSLXuatKho = false; // Da load o SQLServices

        public bool IsReturnedNVL = false; // using for detect PXK is returned phieu tra kho
        public string MaPNKreturnNVL = string.Empty; // using for detect PXK is returned phieu tra kho

        public List<NVLofPhieuXuatKho> DSNVLofPXKs { get; set; } = new();

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(PhieuXuatKho);

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
