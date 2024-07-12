using ProcessManagement.Commons;

namespace ProcessManagement.Models
{
    public class KeHoachSX
    {
        public Propertyy KHSXID { get; set; } = new() { Name = Common.KHSXID, Type = typeof(int), AlowDatabase = true };
        public SanPham? SanPham { get; set; }
        public int SoluongSX { get; set; }
        public int DinhMuc { get; set; }
        public int Tileloi { get; set; }
        public List<NguyenCongofKHSX> DSachCongDoans { get; set; } = new();
        public List<NhanVien> DSachNhanViens { get; set; } = new();
        public List<MayMoc> DSachMayMocs { get; set; } = new();

        public string LoaiNL { get; set; } = string.Empty;
        public int SLLotChan { get; set; }
        public int SLmoiLotChan { get; set; }
        public int SLLotLe { get; set; }
        public int SLmoiLotLe { get; set; }
        public int SLLot { get; set; }

        public KeHoachSX()
        {
            List<string> listtencongdoan = new() { "Tiện phi", "Khoan lỗ", "Tiện ren", "Bavia + Rửa", "Kiểm Pin, M, Ren", "Ngoại quan + đóng thùng" };

            foreach (var item in listtencongdoan)
            {
                NguyenCongofKHSX congDoan = new();

                congDoan.TenCongDoan.Value = item;

                DSachCongDoans.Add(congDoan);
            }
        }

        public (int, int) CalculateSLLot()
        {
            int slLotChan = DinhMuc % SLmoiLotChan;

            int slLotLe = DinhMuc / SLmoiLotChan;

            SLLot = slLotChan + slLotLe;

            return (slLotChan, slLotLe);
        }
    }


}
