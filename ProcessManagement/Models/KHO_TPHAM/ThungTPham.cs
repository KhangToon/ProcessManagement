using ProcessManagement.Commons;
using ProcessManagement.Models.KHSXs;
using System.Reflection;

namespace ProcessManagement.Models.KHO_TPHAM
{
    public class ThungTPham
    {
        public Propertyy TTPID { get; set; } = new() { DBName = DBName.TTPID, DisplayName = DispName.TTPID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // Identity ID
        public Propertyy KHSXID { get; set; } = new() { DBName = DBName.KHSXID, DisplayName = DispName.KHSXID, Type = typeof(int), AlowDatabase = true };
        public Propertyy VTofTPID { get; set; } = new() { DBName = DBName.VTofTPID, DisplayName = DispName.VTofTPID, Type = typeof(int), AlowDatabase = true };
        public Propertyy VTTPID { get; set; } = new() { DBName = DBName.VTTPID, DisplayName = DispName.VTTPID, Type = typeof(int), AlowDatabase = true };
        public Propertyy MaQuanLyThung { get; set; } = new() { DBName = DBName.MaQuanLyThung, DisplayName = DispName.MaQuanLyThung, Type = typeof(string), AlowDatabase = true };
        public Propertyy IDThung { get; set; } = new() { DBName = DBName.IDThung, DisplayName = DispName.IDThung, Type = typeof(string), AlowDatabase = true };
        public Propertyy SoLuong { get; set; } = new() { DBName = DBName.SoLuong, DisplayName = DispName.SoLuong, Type = typeof(int), AlowDatabase = true };
        public Propertyy NgayDongThung { get; set; } = new() { DBName = DBName.NgayDongThung, DisplayName = DispName.NgayDongThung, Type = typeof(DateTime), AlowDatabase = true };
        public Propertyy NgayNhapKho { get; set; } = new() { DBName = DBName.NgayNhapKho, DisplayName = DispName.NgayNhapKho, Type = typeof(DateTime), AlowDatabase = true };
        public Propertyy NgayXuatKho { get; set; } = new() { DBName = DBName.NgayXuatKho, DisplayName = DispName.NgayXuatKho, Type = typeof(DateTime), AlowDatabase = true };
        public Propertyy SPID { get; set; } = new() { DBName = DBName.SPID, DisplayName = DispName.SPID, Type = typeof(int), AlowDatabase = true };
        public Propertyy PNKTPID { get; set; } = new() { DBName = DBName.PNKTPID, DisplayName = DispName.PNKTPID, Type = typeof(int), AlowDatabase = true };
        public Propertyy PXKTPID { get; set; } = new() { DBName = DBName.PXKTPID, DisplayName = DispName.PXKTPID, Type = typeof(int), AlowDatabase = true };
        public Propertyy PalletKey { get; set; } = new() { DBName = DBName.PalletKey, DisplayName = DispName.PalletKey, Type = typeof(string), AlowDatabase = true };
        public Propertyy InStock { get; set; } = new() { DBName = DBName.InStock, DisplayName = DispName.InStock, Type = typeof(int), AlowDatabase = true };

        public bool DaNhapKho { get; set; } = false;
        public bool IsExpand { get; set; } = false;
        public bool IsSelected { get; set; } = false;
        public bool IsScanned { get; set; } = false;

        public static class DBName
        {
            public const string Table_ThungTPham = "KHOTP_ThungTPham";
            public const string TTPID = "TTPID";
            public const string KHSXID = "KHSXID";
            public const string SPID = "SPID";
            public const string PNKTPID = "PNKTPID";
            public const string PXKTPID = "PXKTPID";
            public const string PalletKey = "PalletKey";
            public const string VTofTPID = "VTofTPID";
            public const string VTTPID = "VTTPID";
            public const string MaQuanLyThung = "maquanlythungtp";
            public const string IDThung = "IDThung";
            public const string SoLuong = "soluong";
            public const string NgayDongThung = "ngaydongthung";
            public const string NgayNhapKho = "ngaynhapkho";
            public const string NgayXuatKho = "ngayxuatkho";
            public const string InStock = "InStock";
        }

        public static class DispName
        {
            public const string TTPID = "TTPID";
            public const string KHSXID = "KHSXID";
            public const string SPID = "SPID";
            public const string PNKTPID = "PNKTPID";
            public const string PXKTPID = "PXKTPID";
            public const string PalletKey = "PalletKey";
            public const string VTofTPID = "VTofTPID";
            public const string VTTPID = "VTTPID";
            public const string MaQuanLyThung = "Mã quản lý thùng";
            public const string IDThung = "IDThung";
            public const string SoLuong = "Số lượng";
            public const string NgayDongThung = "Ngày đóng thùng";
            public const string NgayNhapKho = "Ngày nhập kho";
            public const string NgayXuatKho = "Ngày xuất kho";
            public const string InStock = "In Stock";
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
