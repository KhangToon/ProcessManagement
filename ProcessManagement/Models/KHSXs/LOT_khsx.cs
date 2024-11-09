using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHSXs
{
    public class LOT_khsx
    {
        public Propertyy KHSXLOTID { get; set; } = new() { DBName = DBName.KHSXLOTID, DisplayName = DispName.KHSXLOTID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false };
        public Propertyy KHSXID { get; set; } = new() { DBName = DBName.KHSXID, DisplayName = DispName.KHSXID, Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy NCID { get; set; } = new() { DBName = DBName.NCID, DisplayName = DispName.NCID, Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy MaQuanLyLot { get; set; } = new() { DBName = DBName.MaQuanLyLot, DisplayName = DispName.MaQuanLyLot, Type = typeof(string), AlowDatabase = true };
        public Propertyy SLLOT { get; set; } = new() { DBName = DBName.SLLOT, DisplayName = DispName.SLLOT, Type = typeof(int), AlowDatabase = true, };
        public Propertyy NgayNhapKho { get; set; } = new() { DBName = DBName.NgayNhapKho, DisplayName = DispName.NgayNhapKho, Type = typeof(string), AlowDatabase = true };
        public Propertyy NgayXuatKho { get; set; } = new() { DBName = DBName.NgayXuatKho, DisplayName = DispName.NgayXuatKho, Type = typeof(string), AlowDatabase = true };
        public Propertyy IsDone { get; set; } = new() { DBName = DBName.IsDone, DisplayName = DispName.IsDone, Type = typeof(int), AlowDatabase = true, DispDatagrid = false };
        public Propertyy SLOKsubmited { get; set; } = new() { DBName = DBName.SLOKsubmited, DisplayName = DispName.SLOKsubmited, Type = typeof(int), AlowDatabase = true, DispDatagrid = false };
        public Propertyy SLNGsubmited { get; set; } = new() { DBName = DBName.SLNGsubmited, DisplayName = DispName.SLNGsubmited, Type = typeof(int), AlowDatabase = true, DispDatagrid = false };


        public static class DBName
        {
            public const string Table_KHSXLOT = "KHSX_LOT";
            public const string KHSXLOTID = "KHSXLOTID";
            public const string KHSXID = "KHSXID";
            public const string NCID = "NCID";
            public const string MaQuanLyLot = "maquanlylot";
            public const string SLLOT = "soluongcualot";
            public const string IsDone = "isdone";
            public const string SLOKsubmited = "sloksubmited";
            public const string SLNGsubmited = "slngsubmited";
            public const string NgayNhapKho = "ngaynhapkho";
            public const string NgayXuatKho = "ngayxuatkho";
        }

        public static class DispName
        {
            public const string KHSXLOTID = "KHSXLOTID";
            public const string KHSXID = "Mã KHSX";
            public const string NCID = "Công đoạn";
            public const string MaQuanLyLot = "Mã quản lý LOT";
            public const string SLLOT = "Số lượng";
            public const string IsDone = "isdone";
            public const string SLOKsubmited = "Đã hoàn thành";
            public const string SLNGsubmited = "Số lượng lỗi";
            public const string NgayNhapKho = "Ngày nhập kho";
            public const string NgayXuatKho = "Ngày xuất kho";
        }

        public static List<Propertyy> GetClassProperties()
        {
            Type propertyType = typeof(LOT_khsx);

            LOT_khsx instance = new();

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
            Type propertyType = typeof(LOT_khsx);

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
