using ProcessManagement.Commons;
using ProcessManagement.Models.KHO_NVL;
using ProcessManagement.Models.KHO_NVL.XuatKho;
using ProcessManagement.Models.KHSXs;
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
        public Propertyy SLNVLSanXuat { get; set; } = new() { DBName = Common.SLNVLSanXuat, Type = typeof(int), AlowDatabase = true }; // SLNVLPO
        public Propertyy SLSanPhamPO { get; set; } = new() { DBName = Common.SLSanPhamPO, Type = typeof(int), AlowDatabase = true }; // SLSanPhamPO
        public Propertyy SLSanPhamSX { get; set; } = new() { DBName = Common.SLSanPhamSX, Type = typeof(int), AlowDatabase = true }; // SLSanPhamSX
        public Propertyy DinhMuc { get; set; } = new() { DBName = Common.DinhMuc, Type = typeof(int), AlowDatabase = true }; // SLNVLSanxuat
        public Propertyy TileLoi { get; set; } = new() { DBName = Common.TileLoi, Type = typeof(double), AlowDatabase = true };
        public Propertyy SLLot { get; set; } = new() { DBName = Common.SLLot, Type = typeof(int), AlowDatabase = true };
        public Propertyy SLLotChan { get; set; } = new() { DBName = Common.SLLotChan, Type = typeof(int), AlowDatabase = true };
        public Propertyy SLperLotChan { get; set; } = new() { DBName = Common.SLperLotChan, Type = typeof(int), AlowDatabase = true };
        public Propertyy SLLotLe { get; set; } = new() { DBName = Common.SLLotLe, Type = typeof(int), AlowDatabase = true };
        public Propertyy SLperLotLe { get; set; } = new() { DBName = Common.SLperLotLe, Type = typeof(int), AlowDatabase = true };
        public Propertyy SPID { get; set; } = new() { DBName = Common.SP_SPID, Type = typeof(int), AlowDatabase = true };

        public Propertyy NgayTao = new() { DBName = Common.NgayTao, Type = typeof(DateTime), AlowDatabase = true };
        public Propertyy PXKID { get; set; } = new() { DBName = Common.PXKID, DisplayName = "PXKID", Type = typeof(int), AlowDatabase = true };
        public Propertyy IsAllowDisplay { get; set; } = new() { DBName = Common.IsAllowDisplay, DisplayName = "IsAllowDisplay", Type = typeof(int), AlowDatabase = true };
        public Propertyy IsCollapsed { get; set; } = new() { DBName = Common.IsCollapsed, DisplayName = "IsCollapsed", Type = typeof(int), AlowDatabase = true };
        public Propertyy IsDoneKHSX { get; set; } = new() { DBName = Common.IsDoneKHSX, DisplayName = "IsDoneKHSX", Type = typeof(int), AlowDatabase = true };
        public SanPham? TargetSanPham { get; set; }
        public NguyenVatLieu? TargetNVL { get; set; }
        public LoaiNVL? LoaiNVL { get; set; }
        public List<NguyenCongofKHSX> DSachCongDoans { get; set; } = new();
        public List<KHSX_LOT> DSLOT_KHSXs { get; set; } = new();
        public List<NVLofKHSX> DSachNVLofKHSXs { get; set; } = new(); // Danh sach NVL cua KHSX (so luong, loai nvl, sp, ngay tao)

        public bool isDonePXK = false; // trang thai xuat kho NVL cua PXK of KHSX
        public bool isReturnedNVL = false; // trang thai return NVL cua PXK of KHSX

        public List<PhieuXuatKho> ListPXKBoSung = new(); // Dung trong PageDSachKHSXs
        public int SoluongBoSung = 0; // Dung trong PageDSachKHSXs

        public bool isCollapsed { get; set; } = true; // Dung cho PageDSachKHSX

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
