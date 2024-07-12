using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ProcessManagement.Commons;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ProcessManagement.Models
{
    public class NVL
    {
        public Propertyy NVLID = new() { Name = Common.NVLID, Type = typeof(int), DispDatagrid = false, AlowDatabase = false };

        public Propertyy LoaiNVL = new() { Name = Common.LoaiNL, Type = typeof(string), AlowDatabase = true };

        public Propertyy MaSP = new() { Name = Common.MaSP, Type = typeof(string), AlowDatabase = true };

        public Propertyy MaQuanLy = new() { Name = Common.MaQuanLy, Type = typeof(string), AlowDatabase = true };

        public Propertyy LotNVL = new() { Name = Common.LotNVL, Type = typeof(string), AlowDatabase = true };

        public Propertyy SoLuong = new() { Name = Common.SoLuong, Type = typeof(int), AlowDatabase = true };

        public Propertyy NgayNhap = new() { Name = Common.NgayNhap, Type = typeof(DateTime), AlowDatabase = true };

        public Propertyy NgayXuat = new() { Name = Common.NgayXuat, Type = typeof(DateTime), AlowDatabase = true };

        public Propertyy SoLuongXuat = new() { Name = Common.SoLuongXuat, Type = typeof(int), AlowDatabase = true };

        public Propertyy GhiChu = new() { Name = Common.GhiChu, Type = typeof(string), AlowDatabase = true };

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

            Propertyy? tagetProperty = propertiesValue.FirstOrDefault(f => f.Name == propertyName);

            if (tagetProperty != null)
            {
                tagetProperty.Value = newValue;
            }
        }

        public object? GetPropertyValue(string propertyName)
        {
            List<Propertyy> propertiesValue = GetPropertiesValues();

            Propertyy? tagetProperty = propertiesValue.FirstOrDefault(f => f.Name == propertyName);

            return tagetProperty?.Value;
        }
    }
}
