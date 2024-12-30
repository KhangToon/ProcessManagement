using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHSXs
{
    public class ThungTPham
    {


        public Propertyy TTPID { get; set; } = new() { DBName = DBName.TTPID, DisplayName = DispName.TTPID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // Identity ID
        public Propertyy KHSXID { get; set; } = new() { DBName = DBName.KHSXID, DisplayName = DispName.KHSXID, Type = typeof(int), AlowDatabase = true };
        public Propertyy VTofTPID { get; set; } = new() { DBName = DBName.VTofTPID, DisplayName = DispName.VTofTPID, Type = typeof(int), AlowDatabase = true };
        public Propertyy MaQuanLyLot { get; set; } = new() { DBName = DBName.MaQuanLyLot, DisplayName = DispName.MaQuanLyLot, Type = typeof(string), AlowDatabase = true };
        public Propertyy IDThung { get; set; } = new() { DBName = DBName.IDThung, DisplayName = DispName.IDThung, Type = typeof(string), AlowDatabase = true };
        public Propertyy SoLuong { get; set; } = new() { DBName = DBName.SoLuong, DisplayName = DispName.SoLuong, Type = typeof(int), AlowDatabase = true };
        public Propertyy SLRequired { get; set; } = new() { DBName = DBName.SLRequired, DisplayName = DispName.SLRequired, Type = typeof(int), AlowDatabase = true };
        public Propertyy NgayDongThung { get; set; } = new() { DBName = DBName.NgayDongThung, DisplayName = DispName.NgayDongThung, Type = typeof(DateTime), AlowDatabase = true };
        public Propertyy NgayNhapKho { get; set; } = new() { DBName = DBName.NgayNhapKho, DisplayName = DispName.NgayNhapKho, Type = typeof(DateTime), AlowDatabase = true };
        public Propertyy SPID { get; set; } = new() { DBName = DBName.SPID, DisplayName = DispName.SPID, Type = typeof(int), AlowDatabase = true };


        public static class DBName
        {
            public const string Table_ThungTPham = "KHSX_ThungTPham";
            public const string TTPID = "TTPID";
            public const string KHSXID = "KHSXID";
            public const string SPID = "SPID";
            public const string VTofTPID = "VTofTPID";
            public const string MaQuanLyLot = "maquanlylot";
            public const string IDThung = "IDThung";
            public const string SoLuong = "soluong";
            public const string SLRequired = "slrequired";
            public const string NgayDongThung = "ngaydongthung";
            public const string NgayNhapKho = "ngaynhapkho";
        }

        public static class DispName
        {
            public const string TTPID = "TTPID";
            public const string KHSXID = "KHSXID";
            public const string SPID = "SPID";
            public const string VTofTPID = "VTofTPID";
            public const string MaQuanLyLot = "Mã quản lý LOT";
            public const string IDThung = "IDThung";
            public const string SoLuong = "Số lượng";
            public const string SLRequired = "Số lượng đóng thùng";
            public const string NgayDongThung = "Ngày đóng thùng";
            public const string NgayNhapKho = "Ngày nhập kho";
        }

        // Get all property of this class
        public static List<Propertyy> GetClassProperties()
        {
            Type propertyType = typeof(ThungTPham);

            ThungTPham instance = new();

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
            Type propertyType = typeof(ThungTPham);

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
