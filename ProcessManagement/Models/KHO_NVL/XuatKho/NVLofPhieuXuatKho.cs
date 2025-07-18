﻿using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models.KHO_NVL.XuatKho
{
    public class NVLofPhieuXuatKho
    {
        public Propertyy NVLPXKID { get; set; } = new() { DBName = Common.NVLPXKID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false }; // ID
        public Propertyy PXKID { get; set; } = new() { DBName = Common.PXKID, Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy NVLID { get; set; } = new() { DBName = Common.NVLID, Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public Propertyy NVLXKSoLuongAll { get; set; } = new() { DBName = Common.NVLXKSoLuongAll, Type = typeof(int), AlowDatabase = true, AlowDisplay = false, DispDatagrid = false };
        public List<LenhXuatKho> DSLenhXKs { get; set; } = new();
        public NguyenVatLieu TargetNgLieu { get; set; } = new();

        public bool IsChidinhDuSoluongXuatKho = false;

        public bool IsXuatKhoDone = false; // trang thai cua lenh xuat kho tong ( bao gom trang thai cua cac vi tri cac lenh ) (da load o SQLServices)

        public bool IsEditingSLXKhoALL = false;

        public object TemSLXKhoAll = 0;

        public List<object> DsachPickedLXKs = new();

        public List<LXKExtendItems> DSachFILOLXKs = new();
        public class LXKExtendItems
        {
            public LenhXuatKho LXK { get; set; } = new();
            public int ID { get; set; }
            public bool IsSelected { get; set; } = false;
            public bool IsAllowSelect { get; set; } = true;
            public bool IsLastElement { get; set; } = false;
        }
        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(NVLofPhieuXuatKho);

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
