using ProcessManagement.Commons;
using ProcessManagement.Models.KHO_NVL;
using ProcessManagement.Models.NHANVIEN;
using ProcessManagement.Models.SANPHAM;
using System.Reflection;

namespace ProcessManagement.Models
{
    public class KHSX
    {
        public Propertyy KHSXID { get; set; } = new() { DBName = Common.KHSXID, Type = typeof(int), AlowDatabase = true };
        public Propertyy MaLSX { get; set; } = new() { DBName = Common.MaLSX, Type = typeof(string), AlowDatabase = true };
        public Propertyy LOAINVLID { get; set; } = new() { DBName = Common.LOAINVLID, DisplayName = "Loại NVL", Type = typeof(int), AlowDatabase = true };
        public Propertyy SLSanXuat { get; set; } = new() { DBName = Common.SLSanXuat, Type = typeof(int), AlowDatabase = true };
        public Propertyy DinhMuc { get; set; } = new() { DBName = Common.DinhMuc, Type = typeof(int), AlowDatabase = true };
        public Propertyy TileLoi { get; set; } = new() { DBName = Common.TileLoi, Type = typeof(double), AlowDatabase = true };
        public Propertyy SLLot { get; set; } = new() { DBName = Common.SLLot, Type = typeof(int), AlowDatabase = true };
        public Propertyy SLLotChan { get; set; } = new() { DBName = Common.SLLotChan, Type = typeof(int), AlowDatabase = true };
        public Propertyy SLperLotChan { get; set; } = new() { DBName = Common.SLperLotChan, Type = typeof(int), AlowDatabase = true };
        public Propertyy SLLotLe { get; set; } = new() { DBName = Common.SLLotLe, Type = typeof(int), AlowDatabase = true };
        public Propertyy SLperLotLe { get; set; } = new() { DBName = Common.SLperLotLe, Type = typeof(int), AlowDatabase = true };
        public Propertyy SPID { get; set; } = new() { DBName = Common.SP_SPID, Type = typeof(int), AlowDatabase = true };

        public Propertyy NgayTao = new() { DBName = Common.NgayTao, Type = typeof(DateTime), AlowDatabase = true };

        public SanPham? SanPham { get; set; }
        public LoaiNVL? LoaiNVL { get; set; }   
        public List<NguyenCongofKHSX> DSachCongDoans { get; set; } = new();

        public List<NVLofKHSX> DSachNVLs { get; set; } = new(); // Danh sach NVL cua KHSX (so luong, loai nvl, sp, ngay tao)

        public KHSX()
        {

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

        // Control mothods //
        public int GetSLcongdoantruoc(NguyenCongofKHSX currentNguyenCong, NVLmoiNguyenCong nvlmnc)
        {
            int lastSLgiacong = 0;

            int indexCD = this.DSachCongDoans.IndexOf(currentNguyenCong);

            if (indexCD > 0)
            {
                NguyenCongofKHSX? lastNC = this.DSachCongDoans[indexCD - 1];
                NVLmoiNguyenCong? lastnvlcd = lastNC.DSachNVLCongDoans.FirstOrDefault(nvl => nvl.MaQuanLy.Value?.ToString() == nvlmnc.MaQuanLy.Value?.ToString());
                var lastOK = (int.TryParse(lastnvlcd?.CaNgay.OK.Value?.ToString(), out int lastcnok) ? lastcnok : 0) + (int.TryParse(lastnvlcd?.CaDem.OK.Value?.ToString(), out int lastcdok) ? lastcdok : 0);
                lastSLgiacong = lastOK;
            }
            else
            {
                lastSLgiacong = int.TryParse(nvlmnc.SLGoccuaLOTNVL.Value?.ToString(), out int vl) ? vl : 0;
            }

            return lastSLgiacong;
        }
    }
}
