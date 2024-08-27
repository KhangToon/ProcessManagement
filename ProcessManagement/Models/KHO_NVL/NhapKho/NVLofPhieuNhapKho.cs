using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL.NhapKho
{
    public class NVLofPhieuNhapKho
    {
        public Propertyy NVLPNKID { get; set; } = new() { DBName = Common.NVLPNKID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // ID
        public Propertyy PNKID { get; set; } = new() { DBName = Common.PNKID, Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy NVLID { get; set; } = new() { DBName = Common.NVLID, Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy NVLNKSoLuongAll { get; set; } = new() { DBName = Common.NVLNKSoLuongAll, Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public List<LenhNhapKho> DSLenhNKs { get; set; } = new();

        public bool IsChidinhDuSoluongNhapKho = false;

        public NguyenVatLieu TargetNgLieu { get; set; } = new();

        public bool IsAsignedVitri = false;

        public bool IsNhapKhoDone = false; // trang thai cua lenh nhap kho tong (bao gom trang thai cua cac vi tri cac lenh )

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(NVLofPhieuNhapKho);

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
