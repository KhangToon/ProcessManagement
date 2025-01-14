using ProcessManagement.Commons;
using ProcessManagement.Models.KHO_NVL.NhapKho;
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
        public Propertyy PNKID { get; set; } = new() { DBName = Common.PNKID, Type = typeof(int), AlowDatabase = true }; // using for detect PXK is created return phieu tra kho
        public Propertyy IsDonePXK { get; set; } = new() { DBName = Common.IsDonePXK, Type = typeof(int), AlowDatabase = true };
        public Propertyy IsPhieuBoSungNVL { get; set; } = new() { DBName = Common.IsPhieuBoSungNVL, Type = typeof(int), AlowDatabase = true };
        public Propertyy IsPhieuBSungAddedLOTNVL { get; set; } = new() { DBName = Common.IsPhieuBSungAddedLOTNVL, Type = typeof(int), AlowDatabase = true };
        public Propertyy STT_PXK { get; set; } = new() { DBName = Common.STT_PXK, Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };

        public bool isPXKDoneXuatKho = false; // Da load o SQLServices

        public bool isChiDinhDuSLXuatKho = false; // Da load o SQLServices

        public bool isReturnedNVL = false; // using for detect PXK is returned phieu tra kho

        public string maPNKreturnNVL = string.Empty; // using for detect PXK is returned phieu tra kho

        public bool isPhieuBoSungNVL = false; // Da load o SQLServices

        public bool isPhieuBSungAddedLOTNVL = false; // Da load o SQLServices

        public List<NVLofPhieuXuatKho> DSNVLofPXKs { get; set; } = new();

        public PhieuNhapKho PhieuTraKho { get; set; } = new(); // Dung cho tra kho

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
