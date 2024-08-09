using Microsoft.AspNetCore.Components;
using ProcessManagement.Models;

namespace ProcessManagement.Commons
{
    public static class Common
    {
        public const string DayTimeFormat = "yyyy-MM-dd HH:mm:ss";
        public const string DayTimeFormatnoTime = "yyyy-MM-dd";

        // Table Sanpham //
        public const string TableSanPham = "Table_SanPham";
        public const string SPID = "SPID";
        public const string MaSP = "Mã SP";
        public const string TenSanPham = "Tên SP";
        public const string SoLuong = "Số lượng";
        public const string NgayTao = "Ngày tạo";

        // Table Sanpham //
        public const string TableChitietSanPham = "Table_ChitietSanPham";
        public const string CTSPID = "CTSPID";
        //public const string SPID = "SPID";
        public const string PropertyName = "PropertyName";
        public const string PropertyValue = "PropertyValue";
        public const string PropertyType = "PropertyType";

        // Table NVL of Sanpham
        public const string TableNVLofSanPham = "Table_NVLofSanPham";
        //public const string NVLID = "NVLID";
        //public const string SPID = "SPID";
        //public const string MaLoaiSP = "MaLoaiSP";
        public const string NgayThem = "NgayThem";

        // Table NVL //
        public const string TableNVL = "Table_NVL";
        //public const string NVLID = "NVLID";
        public const string LoaiNL = "Loại NL";
        //public const string TenSanPham = "Tên sản phẩm";
        public const string MaQuanLy = "Mã quản lý";
        public const string LotNVL = "Lot NVL";
        //public const string SoLuong = "Số lượng";
        public const string NgayNhap = "Ngày nhập";
        public const string NgayXuat = "Ngày xuất";
        public const string SoLuongXuat = "Số lượng xuất";
        public const string GhiChu = "Ghi chú";

        // Table Lot //
        public const string TableLot = "Table_Lot";
        public const string LotID = "LotID";
        //public const string LotNVL = "Lot NVL";
        //public const string NgayTao = "Ngày tạo";
        public const string SoLuongNVL = "Số lượng NVL";
        //public const string GhiChu = "Ghi chú";

        // Table NguyenLieu //
        public const string TableNguyenLieu = "Table_NguyenLieu";
        public const string NLID = "NLID";
        //public const string LotNVL = "Lot NVL";
        //public const string NgayTao = "Ngày tạo";
        //public const string SoLuongNVL = "Số lượng NVL";

        // Table KHSX Nguyen cong //
        public const string TableCongDoan = "Table_NguyenCongofKHSX";
        //public const string KHSXID = "KHSXID";
        public const string CDID = "CDID";
        public const string TOTAL = "TOTAL"; // tong cua so luong nvl trong moi cong doan


        // Table NVLmoiCongDoan //
        public const string TableNVLmoiCongDoan = "Table_NVLmoiCongDoan";
        //public const string CDID = "CDID";
        public const string NVLMCDID = "NVLMCDID";
        //public const string MaSP = "Mã SP";
        //public const string MaQuanLy = "Mã quản lý";
        //public const string SoLuong = "Số lượng";
        //public const string NguyenCong = "Nguyên công";
        public const string TongOK = "Tổng OK";
        public const string TongNG = "Tổng NG";
        public const string STTNguyencong = "STT nguyên công";
        public const string Danhgia = "Đánh giá";
        public const string IsUpdated = "IsUpdated";
        public const string SLGoccuaLOTNVL = "SL gốc LOT NVL";
        public const string SLTruocGiaCong = "SL trước gia công";
        public const string SLSauGiaCong = "SL sau gia công";
        // Table Calamviec //
        public const string TableCalamviec = "Table_Calamviec";
        public const string CLVID = "CLVID";
        public const string Cangay = "Ngày";
        public const string Cadem = "Đêm";
        //public const string CDID = "CDID";
        public const string Ca = "Ca";
        public const string NgayGiaCong = "Ngày gia công";
        public const string NhanVien = "Nhân viên";
        public const string OK = "OK";
        public const string NG = "NG";

        // Table Kehoachsanxuat //
        public const string TableKHSX = "Table_KHSX";
        public const string KHSXID = "KHSXID";
        public const string MaLSX = "Mã lệnh SX";
        public const string SLSanXuat = "SL sản xuất";
        public const string DinhMuc = "Định mức";
        public const string TileLoi = "Tỉ lệ lỗi";
        public const string SLLot = "SL lot";
        public const string SLLotChan = "SL lot chẳn";
        public const string SLperLotChan = "SL mỗi lot chẳn";
        public const string SLLotLe = "SL lot lẻ";
        public const string SLperLotLe = "SL mỗi lot lẻ";

        // Table KHSXpickNVL //
        public const string TableNVLofKHSX = "Table_NVLofKHSX";
        public const string NVLKHSXID = "NVLKHSXID";
        //public const string KHSXID = "KHSXID";
        //public const string SPID = "SPID";
        //public const string NVLSPID = "NVLSPID";
        //public const string SoLuong = "Số lượng";
        public const string ThoiDiem = "Thời điểm";


        // Table Nhanvien //
        public const string TableNhanVien = "Table_NhanVien";
        public const string NVID = "NVID";
        public const string MaNhanVien = "Mã nhân viên";
        public const string TenNhanVien = "Tên nhân viên";

        // Table Maymoc //
        public const string TableMayMoc = "Table_MayMoc";
        public const string MMID = "MMID";
        public const string MaMay = "Mã máy";
        public const string TenMay = "Tên máy";

        // Table Nguyen Cong //
        public const string TableNguyenCong = "Table_DSNguyenCong";
        //public const string TileLoi = "Tỉ lệ lỗi";
        public const string SoluongNG = "Số lượng lỗi";
        public const string NCID = "NCID";
        public const string NguyenCong = "Nguyên công";
        public const string Ghichu = "Ghi chú";

        // QUAN LY KHO NVL TABLES //
        // Table KHO_DanhMucNguyenVatLieu //
        public const string TableDanhMucNVL = "KHO_DanhMucNguyenVatLieu";
        public const string DMID = "DMID";
        public const string TenDanhMuc = "TenDanhMuc";
        public const string DanhMucNguyenLieuGiaCong = "Nguyên liệu gia công";

        // Table KHO_LoaiNguyenVatLieu //
        public const string TableLoaiNVL = "KHO_LoaiNguyenVatLieu";
        public const string LOAINVLID = "LOAINVLID";
        //public const string MaDanhMuc = "MaDanhMuc";
        public const string TenLoaiNVL = "TenLoaiNVL";

        // Table KHO_NguyenVatLieu //
        public const string TableNguyenVatLieu = "KHO_NguyenVatLieu";
        public const string NVLID = "NVLID";
        //public const string MaDanhMuc = "MaDanhMuc";
        public const string TenNVL = "TenNVL";
        public const string MoTa = "MoTa";
        public const string DonViTinh = "DonViTinh";
        public const string SoLuongTonKhoToiThieu = "SoLuongTonKhoToiThieu";
        public const string SoLuongTonKhoToiDa = "SoLuongTonKhoToiDa";
        public const string TonKhoHienTai = "TonKhoHienTai";
        public const string ThoiGianBaoQuan = "ThoiGianBaoQuan";
        public const string NgayCapNhat = "NgayCapNhat";

        // Table KHO_NhaCungCap //
        public const string TableNhaCungCap = "KHO_NhaCungCap";
        public const string NCCID = "MaNCC";
        public const string TenNCC = "TenNCC";
        public const string DiaChi = "DiaChi";
        public const string SoDienThoai = "SoDienThoai";
        public const string Email = "Email";
        public const string NguoiLienHe = "NguoiLienHe";
        public const string MaSoThue = "MaSoThue";
        public const string GhiChuNCC = "GhiChu";

        // Table KHO_KhoLuuTru //
        public const string TableKhoLuuTru = "KHO_KhoLuuTru";
        public const string KHOID = "KHOID";
        public const string TenKho = "TenKho";
        //public const string DiaChi = "DiaChi";
        public const string SucChua = "SucChua";
        public const string NguoiQuanLy = "NguoiQuanLy";
        //public const string SoDienThoai = "SoDienThoai";
        public const string TrangThai = "TrangThai";

        // Table KHO_ViTriLuuTru //
        public const string TableViTriLuuTru = "KHO_ViTriLuuTru";
        public const string VTID = "VTID";
        //public const string MaKho = "MaKho";
        public const string TenViTri = "TenViTri";
        //public const string SucChua = "SucChua";

        // Table KHO_NhapKho //
        public const string TableNhapKho = "KHO_NhapKho";
        public const string NKID = "NKID";
        //public const string MaNVL = "MaNVL";
        //public const string MaNCC = "MaNCC";
        //public const string MaKho = "MaKho";
        //public const string MaViTri = "MaViTri";
        public const string SoLuongNhapKho = "SoLuongNhap";
        public const string NgayNhapKho = "NgayNhap";
        public const string NgayHetHan = "NgayHetHan";
        public const string SoLoNhapKho = "SoLo";
        public const string NguoiNhapKho = "NguoiNhap";
        public const string GhiChuNhapKho = "GhiChu";

        // Table KHO_XuatKho //
        public const string TableXuatKho = "KHO_XuatKho";
        public const string XKID = "XKID";
        //public const string MaNVL = "MaNVL";
        //public const string MaKho = "MaKho";
        //public const string MaViTri = "MaViTri";
        public const string SoLuongXuatKho = "SoLuongXuat";
        public const string NgayXuatKho = "NgayXuat";
        public const string NguoiXuatKho = "NguoiXuat";
        public const string NguoiNhan = "NguoiNhan";
        public const string MucDichSuDung = "MucDichSuDung";
        public const string GhiChuXuatKho = "GhiChu";

        // Table KHO_KiemKe //
        public const string TableKiemKe = "KHO_KiemKe";
        public const string KIEMKEID = "KIEMKEID";
        //public const string MaNVL = "MaNVL";
        //public const string MaKho = "MaKho";
        public const string NgayKiemKe = "NgayKiemKe";
        public const string SoLuongThucTe = "SoLuongThucTe";
        public const string SoLuongHeThong = "SoLuongHeThong";
        public const string ChenhLech = "ChenhLech";
        public const string NguyenNhan = "NguyenNhan";
        public const string NguoiKiemKe = "NguoiKiemKe";

        // Table KHO_NVLDetailsListName
        public const string Table_NVLDetailsListName = "KHO_NVLDetailsListName";
        public const string TenTTID = "TenTTID";
        public const string TenThongTin = "Tên thông tin";
        public const string KieuDulieu = "Kiểu dữ liệu";
        public const string MacDinh = "Mặc định";
        public const string IsDefault = "IsDefault";
        public const string TenTruyXuat = "Tên truy xuất";
        // Danh sach TenTruyXuat cac cot duoc su dung
        public const string TTX_Donvitinh = "donvitinh";

        // Table KHO_NguyenLieuDetails
        public const string Table_NguyenLieuDetails = "KHO_NguyenLieuDetails";
        public const string TTNVLID = "TTNVLID";
        //public const string NVLID = "NVLID";
        //public const string TenTTID = "TenTTID";
        public const string GiaTri = "Gía trị";



        // Socket 
        public const string socketEndKey = "<END>";
        public const string CMDTYPE = "CMDTYPE";
        public const string CHECKOUT = "CHECKOUT";
        public const string SCANCODE = "SCANCODE";
        public const string SLOK = "SLOK";
        public const string SLNG = "SLNG";
        public const string RETURNRESULT = "RETURNRESULT";
        public const string RESULTMESS = "RESULTMESS";
        public const string SUCCESS = "SUCCESS";
        public const string FAIL = "FAIL";
        public const string RETURNCHECKOUT = "RETURNCHECKOUT";

        public const int DispMode1 = 1;
        public const int DispMode2 = 2;
        public const int DispMode3 = 3;     // Mode them KHSX

        public static event EventHandler? ClickSaveEvent;
        public static event EventHandler<object?>? CongdoaUpdatedEvent;

        // KHSX static variable
        public static object? CurrentKHSXid;
        public static object? CurrentNVLMCDid;
        public static object? CurrentCDid;
        public static KHSX? CurrentKHSX;

        // NVL management static variable
        public static object? SelectedNVLid;

        public static void RaiseClickSaveEvent()
        {
            ClickSaveEvent?.Invoke(null, EventArgs.Empty);
        }

        public static bool IsClickSaveEventRegistered()
        {
            return ClickSaveEvent != null;
        }

        public static void RaiseCongdoanUpdatedEvent(object? nvlmcdid)
        {
            CongdoaUpdatedEvent?.Invoke(null, nvlmcdid);
        }

        public static void ResetCongdoanUpdatedEvent()
        {
            CongdoaUpdatedEvent = null;
        }

        public static bool IsCongdoanUpdatedEventRegistered()
        {
            return CongdoaUpdatedEvent != null;
        }
    }
}
