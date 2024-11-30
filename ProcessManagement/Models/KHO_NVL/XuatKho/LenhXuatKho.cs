using ProcessManagement.Commons;
using System.Reflection;
using ProcessManagement.Services.SQLServer;

namespace ProcessManagement.Models.KHO_NVL.XuatKho
{
    public class LenhXuatKho
    {
        public Propertyy LenhXKID { get; set; } = new() { DBName = Common.LenhXKID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // ID
        public Propertyy PXKID { get; set; } = new() { DBName = Common.PXKID, Type = typeof(int), AlowDatabase = true };
        public Propertyy NVLPXKID { get; set; } = new() { DBName = Common.NVLPXKID, Type = typeof(int), AlowDatabase = true };
        public Propertyy VTID { get; set; } = new() { DBName = Common.VTID, Type = typeof(int), AlowDatabase = true };
        public Propertyy LotVitri { get; set; } = new() { DBName = Common.LotViTri, Type = typeof(string), AlowDatabase = true };
        public Propertyy NVLID { get; set; } = new() { DBName = Common.NVLID, Type = typeof(int), AlowDatabase = true };
        public Propertyy LXKSoLuong { get; set; } = new() { DBName = Common.LXKSoLuong, Type = typeof(int), AlowDatabase = true };
        public Propertyy LXKIsDone { get; set; } = new() { DBName = Common.LXKIsDone, Type = typeof(int), AlowDatabase = true };
        public Propertyy NgayXuatKho { get; set; } = new() { DBName = Common.NgayXuatKho, Type = typeof(string), AlowDatabase = true };
        public Propertyy QRIDLOT { get; set; } = new() { DBName = Common.QRIDLOT, Type = typeof(string), AlowDatabase = true };
        public Propertyy KHSXID { get; set; } = new() { DBName = Common.KHSXID, Type = typeof(int), AlowDatabase = true };
        public Propertyy NgayNhapKho { get; set; } = new() { DBName = Common.NgayNhapKho, Type = typeof(string), AlowDatabase = true };
        public ViTriofNVL ViTriofNVL { get; set; } = new(); // Chi dung cho phieu xuat kho

        public int id = 0; // dung khi chi dinh lot voi ngay nhap kho uu tien

        public bool isAllowselect = false; // dung khi chi dinh lot voi ngay nhap kho uu tien

        public bool isPicked = false; // dung khi chi dinh lot voi ngay nhap kho uu tien

        //public NguyenVatLieu NVLDetails { get; set; } = new();
        //public VitriLuuTru ViTriDetails { get; set; } = new();

        //public void LoadNVLDetails(SQLServerServices sQLServer)
        //{
        //    if (NVLID.Value != null)
        //    {
        //        NVLDetails = sQLServer.GetNguyenVatLieuByID(NVLID.Value);
        //    }
        //}

        //public void LoadViTriDetails(SQLServerServices sQLServer)
        //{
        //    if (VTID.Value != null)
        //    {
        //        ViTriDetails = sQLServer.GetViTriLuuTruByID(VTID.Value);
        //    }
        //}

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(LenhXuatKho);

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
