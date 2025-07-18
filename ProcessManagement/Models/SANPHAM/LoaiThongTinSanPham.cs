﻿using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.SANPHAM
{
    public class LoaiThongTinSanPham
    {
        public Propertyy LoaiTTSPID { get; set; } = new() { DBName = Common.SP_LoaiTTSPID, DisplayName = "LoaiTTSPID", Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // ID
        public Propertyy TenLoaiThongTin { get; set; } = new() { DBName = Common.SP_TenLoaiThongTin, DisplayName = "Tên loại thông tin", Type = typeof(string), AlowDatabase = true };
        public Propertyy KieuDuLieu { get; set; } = new() { DBName = Common.SP_KieuDuLieu, DisplayName = "Kiểu dữ liệu", Type = typeof(string), AlowDatabase = true };
        public Propertyy GiaTriMacDinh { get; set; } = new() { DBName = Common.SP_GiaTriMacDinh, DisplayName = "Gía trị mặc định", Type = typeof(string), AlowDatabase = true };
        public Propertyy IsDefault { get; set; } = new() { DBName = Common.SP_IsDefault, DisplayName = "IsDefault", Type = typeof(bool), AlowDatabase = true };
        public Propertyy TenTruyXuat { get; set; } = new() { DBName = Common.SP_TenTruyXuat, DisplayName = "Tên truy xuất", Type = typeof(string), AlowDatabase = true };
        public Propertyy IndexDisplay { get; set; } = new() { DBName = Common.SP_IndexDisplay, DisplayName = "IndexDisplay", Type = typeof(int), AlowDatabase = true };


        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(LoaiThongTinSanPham);

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
