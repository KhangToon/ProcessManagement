using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models
{
    public class KHSX
    {
        public Propertyy KHSXID { get; set; } = new() { Name = Common.KHSXID, Type = typeof(int), AlowDatabase = true };
        public Propertyy MaLSX { get; set; } = new() { Name = Common.MaLSX, Type = typeof(string), AlowDatabase = true };
        public Propertyy LoaiNL { get; set; } = new() { Name = Common.LoaiNL, Type = typeof(string), AlowDatabase = true };
        public Propertyy SLSanXuat { get; set; } = new() { Name = Common.SLSanXuat, Type = typeof(int), AlowDatabase = true };
        public Propertyy DinhMuc { get; set; } = new() { Name = Common.DinhMuc, Type = typeof(int), AlowDatabase = true };
        public Propertyy TileLoi { get; set; } = new() { Name = Common.TileLoi, Type = typeof(double), AlowDatabase = true };
        public Propertyy SLLot { get; set; } = new() { Name = Common.SLLot, Type = typeof(int), AlowDatabase = true };
        public Propertyy SLLotChan { get; set; } = new() { Name = Common.SLLotChan, Type = typeof(int), AlowDatabase = true };
        public Propertyy SLperLotChan { get; set; } = new() { Name = Common.SLperLotChan, Type = typeof(int), AlowDatabase = true };
        public Propertyy SLLotLe { get; set; } = new() { Name = Common.SLLotLe, Type = typeof(int), AlowDatabase = true };
        public Propertyy SLperLotLe { get; set; } = new() { Name = Common.SLperLotLe, Type = typeof(int), AlowDatabase = true };
        public Propertyy SPID { get; set; } = new() { Name = Common.SPID, Type = typeof(int), AlowDatabase = true };

        public Propertyy NgayTao = new() { Name = Common.NgayTao, Type = typeof(DateTime), AlowDatabase = true };

        public SanPham? SanPham { get; set; }
        public List<NguyenCongofKHSX> DSachCongDoans { get; set; } = new();

        //private List<string> Listtencongdoan = new() { "Tiện phi", "Khoan lỗ", "Tiện ren", "Bavia + Rửa", "Kiểm Pin, M, Ren", "Ngoại quan + đóng thùng" };

        public KHSX()
        {
            //foreach (var item in Listtencongdoan)
            //{
            //    CongDoan congDoan = new();

            //    congDoan.TenCongDoan.Value = item;

            //    DSachCongDoans.Add(congDoan);
            //}
        }

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(KHSX);

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
