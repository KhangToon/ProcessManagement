using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_TPHAM
{
    public class PhieuXuatKhoTPham
    {
        public Propertyy PXKTPID { get; set; } = new() { DBName = DBName.PXKTPID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // ID
        public Propertyy MaPhieuXK { get; set; } = new() { DBName = DBName.MaPhieuXK, Type = typeof(string), AlowDatabase = true };
        public Propertyy CodePallet { get; set; } = new() { DBName = DBName.CodePallet, Type = typeof(int), AlowDatabase = true };
        public Propertyy NguoiLapPXK { get; set; } = new() { DBName = DBName.NguoiLapPXK, Type = typeof(string), AlowDatabase = true };
        public Propertyy NgayLapPXK { get; set; } = new() { DBName = DBName.NgayLapPXK, Type = typeof(DateTime), AlowDatabase = true };
        public Propertyy GhiChuPXK { get; set; } = new() { DBName = DBName.GhiChuPXK, Type = typeof(string), AlowDatabase = true };
        public Propertyy IsDonePXK { get; set; } = new() { DBName = DBName.IsDonePXK, Type = typeof(int), AlowDatabase = true };

        public bool isPXKDoneXuatKho = false;

        public List<ThungTPham> ListKhoThungTPham { get; set; } = new();

        public object MaViTri { get; set; } = new();

        public static class DBName
        {
            public const string Table_PhieuXuatKhoTPham = "KHOTP_PhieuXuatKhoTP";
            public const string PXKTPID = "PXKTPID";
            public const string MaPhieuXK = "MaPhieuXK";
            public const string CodePallet = "CodePallet";
            public const string NguoiLapPXK = "NguoiLapPXK";
            public const string NgayLapPXK = "NgayLapPXK";
            public const string GhiChuPXK = "GhiChuPXK";
            public const string IsDonePXK = "IsDonePXK";
        }

        public static class DispName
        {
            public const string Table_PhieuXuatKhoTPham = "KHOTP_PhieuXuatKhoTP";
            public const string PXKTPID = "PXKTPID";
            public const string MaPhieuXK = "Mã phiếu";
            public const string CodePallet = "CodePallet";
            public const string NguoiLapPXK = "Người lập phiếu";
            public const string NgayLapPXK = "Ngày lập phiếu";
            public const string GhiChuPXK = "Ghi chú";
            public const string IsDonePXK = "IsDonePXK";
        }

        // Get all property of this class
        public static List<Propertyy> GetClassProperties()
        {
            Type propertyType = typeof(PhieuXuatKhoTPham);

            PhieuXuatKhoTPham instance = new();

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
            Type propertyType = typeof(PhieuXuatKhoTPham);

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
