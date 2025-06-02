using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_TPHAM
{
    public class ThanhPhamView
    {
        public Propertyy TPhamID { get; set; } = new() { DBName = DBName.TPhamID, Type = typeof(int), DispDatagrid = false, AlowDatabase = false }; // ID
        public Propertyy MaThanhPham { get; set; } = new() { DBName = DBName.MaThanhPham, DisplayName = DispName.MaThanhPham, Type = typeof(string), AlowDatabase = true, IsCheckSameValue = true };
        public Propertyy TenThanhPham { get; set; } = new() { DBName = DBName.TenThanhPham, DisplayName = DispName.TenThanhPham, Type = typeof(string), DispDatagrid = false, AlowDatabase = true };
        public Propertyy Soluong { get; set; } = new() { DBName = DBName.Soluong, DisplayName = DispName.Soluong, Type = typeof(string), AlowDatabase = true };

        public List<PalletThungTPham> PalletThungTPhams = new();

        public static class DBName
        {
            public const string Table_ThanhPhamView = "KHO_ThanhPhamView";
            public const string TPhamID = "TPhamID";
            public const string MaThanhPham = "MaThanhPham";
            public const string TenThanhPham = "TenThanhPham";
            public const string Soluong = "Soluong";
        }
        private class DispName
        {
            public const string MaThanhPham = "Mã thành phẩm";
            public const string TenThanhPham = "Tên thành phẩm";
            public const string Soluong = "Số lượng";
        }

        public static List<Propertyy> GetClassProperties()
        {
            Type propertyType = typeof(ThanhPhamView);

            ThanhPhamView instance = new();

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
            Type propertyType = typeof(ThanhPhamView);

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
