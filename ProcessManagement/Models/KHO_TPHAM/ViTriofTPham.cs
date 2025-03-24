using ProcessManagement.Commons;
using ProcessManagement.Models.KHSXs;
using ProcessManagement.Models.SANPHAM;
using System.Reflection;

namespace ProcessManagement.Models.KHO_TPHAM
{
    public class ViTriofTPham
    {
        public Propertyy VTofTPID { get; set; } = new() { DBName = DBName.VTofTPID, Type = typeof(int), AlowDatabase = false }; // ID
        public Propertyy VTTPID { get; set; } = new() { DBName = DBName.VTTPID, Type = typeof(int), AlowDatabase = true };
        public Propertyy VTTPSoLuong { get; set; } = new() { DBName = DBName.VTTPSoLuong, DisplayName = DispName.VTTPSoLuong, Type = typeof(int), AlowDatabase = true };
        public Propertyy NgayNhapKho { get; set; } = new() { DBName = DBName.NgayNhapKho, DisplayName = DispName.NgayNhapKho, Type = typeof(string), AlowDatabase = true };
        public Propertyy LotVitri { get; set; } = new() { DBName = DBName.LotVitri, DisplayName = DispName.LotVitri, Type = typeof(string), AlowDatabase = true };
        public SanPham TargetSanPham { get; set; } = new(); // chi dung khi can (khong load khi load ViTriofTPham)

        public KHSX_LOT TargetLot { get; set; } = new(); // chi dung khi can 

        public PalletThungTPham PalletThungTPham { get; set; } = new();

        public ThungTPhamExtend ThungTPhamExtend { get; set; } = new();
        public ThungTPham ThungTPham { get; set; } = new();

        public bool IsPicked { get; set; } = false;

        public object? MaviTri { get; set; }

        public static class DBName
        {
            public const string Table_ViTriofTPham = "KHO_VTofTPham";
            public const string VTofTPID = "VTofTPID";
            public const string VTTPID = "VTTPID";
            public const string VTTPSoLuong = "Soluong";
            public const string NgayNhapKho = "NgayNhapKho";
            public const string LotVitri = "Lot";
        }

        private class DispName
        {
            public const string VTofTPID = "VTofTPID";
            public const string VTTPID = "VTTPID";
            public const string VTTPSoLuong = "Số lượng";
            public const string NgayNhapKho = "Ngày nhập";
            public const string LotVitri = "Lot";
        }

        // Get all property of this class
        public static List<Propertyy> GetClassProperties()
        {
            Type propertyType = typeof(ViTriofTPham);

            ViTriofTPham instance = new();

            FieldInfo[] fields = propertyType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            List<Propertyy> propertiesValue = new();

            foreach (FieldInfo field in fields)
            {
                Type ob = field.FieldType;

                if (ob == typeof(Propertyy))
                {
                    Propertyy? fieldValue = (Propertyy?)field.GetValue(instance);

                    if (fieldValue != null)
                    {
                        propertiesValue.Add(fieldValue);
                    }
                }
            }

            return propertiesValue;
        }

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(ViTriofTPham);

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
