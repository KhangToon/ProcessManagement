using ProcessManagement.Commons;
using ProcessManagement.Models.KHO_NVL;
using System.Reflection;

namespace ProcessManagement.Models.SANPHAM
{
    public class NVLwithSanPham
    {
        public Propertyy NVLSPID { get; set; } = new() { DBName = Common.SP_NVLSPID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false }; // ID
        public Propertyy SPID { get; set; } = new() { DBName = Common.SP_SPID, Type = typeof(int), AlowDatabase = true, };
        public Propertyy NVLID { get; set; } = new() { DBName = Common.NVLID, Type = typeof(int), AlowDatabase = true, };
        public Propertyy NgayThem { get; set; } = new() { DBName = Common.SP_NgayThem, DisplayName = "Ngày thêm", Type = typeof(DateTime), AlowDatabase = true };
        public Propertyy QuyCach { get; set; } = new() { DBName = Common.QuyCach, DisplayName = "Quy cách", Type = typeof(int), AlowDatabase = true, };
        public NguyenVatLieu TargetNgLieu { get; set; } = new();
        public SanPham TargetSP { get; set; } = new(); // Khong load trong DB (deadloop)

        public bool isEditingSoluong = false;
        public int allsoLuongcanLay = 0; // dung khi chi ding so luong sp trong KHSX
        public bool isPickNVLok = false; // dung khi chi ding so luong sp trong KHSX
        public double tileloi = 0; // dung khi chi ding so luong sp trong KHSX
        public int slloichophep = 0; // dung khi chi ding so luong sp trong KHSX
        public int dinhmuc = 0; // dung khi chi ding so luong sp trong KHSX

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(NVLwithSanPham);

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
