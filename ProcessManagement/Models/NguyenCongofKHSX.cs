using ProcessManagement.Commons;
using System.Reflection;

namespace ProcessManagement.Models
{
    public class NguyenCongofKHSX
    {
        public Propertyy NCIDofKHSX { get; set; } = new() { DBName = Common.NCIDofKHSX, Type = typeof(int) };
        public Propertyy KHSXID { get; set; } = new() { DBName = Common.KHSXID, Type = typeof(int), AlowDatabase = true };
        public Propertyy NCID { get; set; } = new() { DBName = Common.NCID, Type = typeof(int), AlowDatabase = true };
        public Propertyy TenCongDoan { get; set; } = new() { DBName = Common.NguyenCong, Type = typeof(string), AlowDatabase = true };
        public Propertyy TileNG { get; set; } = new() { DBName = Common.TileLoi, Type = typeof(double), AlowDatabase = true, Value = 0 };
        public Propertyy SoluongNG { get; set; } = new() { DBName = Common.SoluongNG, Type = typeof(int), AlowDatabase = true, Value = 0 };
        public List<NVLmoiNguyenCong> DSachNVLCongDoans { get; set; } = new();

        public bool isExpandColumn = false; // Using in QLCongDoanPage

        public int SLlimit = 0;

        public bool IsUsing { get; set; } = false;

        public List<int> GenerateListColumnIndex(int lenght)
        {
            var list = new List<int>();
            for (int i = 0; i < lenght; i++)
            {
                list.Add(i);
            }
            return list;
        }

        public void CreateDSachNVLCongDoans(List<TemLotNVL> nVLs)
        {
            foreach (var nvl in nVLs)
            {
                NVLmoiNguyenCong nVLmoiCongDoan = new();

                nVLmoiCongDoan.MaSP.Value = nvl.MaSP.Value;

                nVLmoiCongDoan.NCIDofKHSX.Value = NCIDofKHSX.Value;

                nVLmoiCongDoan.KHSXID.Value = KHSXID.Value;

                nVLmoiCongDoan.LoaiNVL.Value = nvl.LoaiNVL.Value;

                nVLmoiCongDoan.TenCongDoan.Value = TenCongDoan.Value;

                nVLmoiCongDoan.MaQuanLy.Value = nvl.MaQuanLy.Value;

                nVLmoiCongDoan.SLGoccuaLOTNVL.Value = nvl?.SoLuong.Value;

                nVLmoiCongDoan.NgayXuat.Value = nvl?.NgayXuat.Value;

                DSachNVLCongDoans.Add(nVLmoiCongDoan);
            }
        }

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(NguyenCongofKHSX);

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

        public int CalculateTongSLNGnguyenCong()
        {
            int sumNG = 0;

            foreach (var item in DSachNVLCongDoans)
            {
                int NG = (int.TryParse(item.CaNgay.NG.Value?.ToString(), out int cnng) ? cnng : 0) + (int.TryParse(item.CaDem.NG.Value?.ToString(), out int cdng) ? cdng : 0);

                sumNG += NG;
            }

            return sumNG;
        }

        public int GetSoluongloichophep()
        {
            int slloichophep = int.TryParse(this.SoluongNG.Value?.ToString(), out int sllcp) ? sllcp : 0;

            return slloichophep;
        }
    }
}
