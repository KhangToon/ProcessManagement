using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ProcessManagement.Commons;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ProcessManagement.Models
{
    public class NVL
    {
        public Propertyy NVLID = new() { DBName = Common.NVLID, Type = typeof(int), DispDatagrid = false, AlowDatabase = false };

        public Propertyy LoaiNVL = new() { DBName = Common.LoaiNL, Type = typeof(string), AlowDatabase = true };

        public Propertyy MaSP = new() { DBName = Common.SP_MaSP, Type = typeof(string), AlowDatabase = true };

        public Propertyy MaQuanLy = new() { DBName = Common.MaQuanLy, Type = typeof(string), AlowDatabase = true };

        public Propertyy LotNVL = new() { DBName = Common.LotNVL, Type = typeof(string), AlowDatabase = true };

        public Propertyy SoLuong = new() { DBName = Common.SoLuong, Type = typeof(int), AlowDatabase = true };

        public Propertyy NgayNhap = new() { DBName = Common.NgayNhap, Type = typeof(DateTime), AlowDatabase = true };

        public Propertyy NgayXuat = new() { DBName = Common.NgayXuat, Type = typeof(DateTime), AlowDatabase = true };

        public Propertyy SoLuongXuat = new() { DBName = Common.SoLuongXuat, Type = typeof(int), AlowDatabase = true };

        public Propertyy GhiChu = new() { DBName = Common.GhiChu, Type = typeof(string), AlowDatabase = true };

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(NVL);

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
