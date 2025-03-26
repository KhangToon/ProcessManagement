using Microsoft.AspNetCore.Components;
using ProcessManagement.Models;
using Radzen;
using System.Globalization;
using System.Text;

namespace ProcessManagement.Commons
{
    public static class Common
    {

        public static readonly List<string> ListCaLamViecs = new() { "Ngày", "Đêm", "Ca 1", "Ca 2", "Ca 3" };

        public const string Format_yyyyMMdd = "yyyy-MM-dd HH:mm:ss";
        public const string Format_yyyyddMM = "MM-dd-yyyy HH:mm:ss";
        public const string FormatNoTime_yyyMMdd = "yyyy-MM-dd";
        public const string FormatNoTime_yyyddMM = "MM-dd-yyyy";
        public const string FormatNoTime_ddMMyyyy = "dd/MM/yyyy";

        // Table Sanpham //
        public const string Table_SanPham = "SP_SanPham";
        public const string SP_SPID = "SPID";
        public const string SP_MaSP = "Mã SP";
        public const string SP_TenSanPham = "Tên sản phẩm";

        // Table LoaiThongTinSanPham
        public const string Table_LoaiThongTinSanPham = "SP_LoaiThongTinSanPham";
        public const string SP_LoaiTTSPID = "LoaiTTSPID";
        public const string SP_TenLoaiThongTin = "Tên loại thông tin";
        public const string SP_KieuDuLieu = "Kiểu dữ liệu";
        public const string SP_GiaTriMacDinh = "Gía trị mặc định";
        public const string SP_IsDefault = "IsDefault";
        public const string SP_TenTruyXuat = "Tên truy xuất";
        public const string SP_IndexDisplay = "IndexDisplay";

        // Table ThongTinSanPham
        public const string Table_ThongTinSanPham = "SP_ThongTinSanPham";
        public const string SP_TTSPID = "TTSPID";
        public const string SP_GiaTriThongTin = "Giá trị";

        // Table NVLwithSanPham
        public const string Table_NVLwithSanPham = "SP_NVLwithSanPham";
        public const string SP_NVLSPID = "NVLSPID";
        public const string SP_NgayThem = "NgayThem";
        public const string QuyCach = "QuyCach";

        // Table ChitietSanPham //
        public const string TableChitietSanPham = "SP_ChitietSanPham";
        public const string CTSPID = "CTSPID";
        //public const string SPID = "SPID";
        public const string PropertyName = "PropertyName";
        public const string PropertyValue = "PropertyValue";
        public const string PropertyType = "PropertyType";


        public const string LoaiNL = "Loại NL";
        public const string MaQuanLy = "Mã quản lý";
        public const string LotNVL = "Lot NVL";
        public const string SoLuong = "Số lượng";
        public const string NgayNhap = "Ngày nhập";
        public const string NgayXuat = "Ngày xuất";
        public const string SoLuongXuat = "Số lượng xuất";
        public const string GhiChu = "Ghi chú";
        public const string NgayTao = "Ngày tạo";

        // Table KHSX Nguyen cong //
        public const string TableCongDoan = "Table_NguyenCongofKHSX";
        //public const string KHSXID = "KHSXID";
        public const string NCIDofKHSX = "NCIDofKHSX";
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
        public const string IsDoneKHSX = "IsDoneKHSX";
        public const string IsAllowDisplay = "IsAllowDisplay";
        public const string IsCollapsed = "IsCollapsed";
        public const string MaLSX = "Mã lệnh SX";
        public const string SLNVLSanXuat = "SL sản xuất";
        public const string SLSanPhamSX = "SLSanPhamSX";
        public const string SLSanPhamPO = "SLSanPhamPO";
        public const string DinhMuc = "Định mức";
        public const string TileLoi = "Tỉ lệ lỗi";
        public const string SLLot = "SL lot";
        public const string SLLotChan = "SL lot chẳn";
        public const string SLperLotChan = "SL mỗi lot chẳn";
        public const string SLLotLe = "SL lot lẻ";
        public const string SLperLotLe = "SL mỗi lot lẻ";
        //public const string PXKID = "PXKID";

        // Table KHSXpickNVL //
        public const string TableNVLofKHSX = "Table_NVLofKHSX";
        public const string NVLKHSXID = "NVLKHSXID";
        //public const string KHSXID = "KHSXID";
        //public const string SPID = "SPID";
        //public const string NVLSPID = "NVLSPID";
        //public const string SoLuong = "Số lượng";
        public const string ThoiDiem = "Thời điểm";

        // Table Nguyen Cong //
        public const string Table_NguyenCong = "Table_DSNguyenCong";
        //public const string TileLoi = "Tỉ lệ lỗi";
        public const string SoluongNG = "Số lượng lỗi";
        public const string SumOKSubmited = "SumOKSubmited";
        public const string SumNGSubmited = "SumNGSubmited";
        public const string SumTotalSubmited = "SumTotalSubmited";
        public const string SLlimit = "SLlimit";
        public const string NCID = "NCID";
        public const string NguyenCong = "Nguyên công";
        public const string Ghichu = "Ghi chú";
        public const string NGIDs = "NGIDs";

        // QUAN LY KHO NVL TABLES //
        // Table KHO_DanhMucNguyenVatLieu //
        public const string Table_DanhMucNVL = "KHO_DanhMucNguyenVatLieu";
        public const string DMID = "DMID";
        public const string TenDanhMuc = "TenDanhMuc";
        public const string DanhMucNguyenLieuGiaCong = "Nguyên liệu gia công";

        // Table KHO_LoaiNguyenVatLieu //
        public const string Table_LoaiNVL = "KHO_LoaiNguyenVatLieu";
        public const string LOAINVLID = "LOAINVLID";
        //public const string MaDanhMuc = "MaDanhMuc";
        public const string TenLoaiNVL = "TenLoaiNVL";

        // Table KHO_NguyenVatLieu //
        public const string Table_NguyenVatLieu = "KHO_NguyenVatLieu";
        public const string NVLID = "NVLID";
        public const string MaNVL = "MaNVL";
        public const string TenNVL = "TenNVL";
        public const string NVLTonKho = "Tồn kho";
        public const string DonViTinh = "DonViTinh";
        public const string NgaySuDung = "NgaySuDung";

        // Table KHO_ViTriLuuTru //
        public const string Table_ViTriLuuTru = "KHO_ViTriLuuTru";
        public const string VTID = "VTID";
        public const string MaViTri = "Mã vị trí";
        public const string ViTriKe = "Vị trí kệ";
        public const string ViTriHang = "Vị trí hàng";
        public const string ViTriCot = "Vị trí cột";
        public const string VTSucChua = "Sức chứa";
        public const string VTSLTrong = "SL trống";

        // Table KHO_VitriOfNVL //
        public const string Table_VitriOfNVL = "KHO_VitriOfNVL";
        public const string VTofNVLID = "VTofNVLID";
        public const string VTNVLSoLuong = "Số lượng";
        public const string LotViTri = "Lot";
        public const string QRIDLOT = "QRIDLOT";
        public const string NgayNhapKho = "Ngày nhập kho";
        public const string NgayXuatKho = "Ngày xuất kho";

        // Table KHO_LoaiThongTinNVL
        public const string Table_LoaiThongTinNVL = "KHO_LoaiThongTinNVL";
        public const string LoaiTTNVLID = "LoaiTTNVLID";
        public const string TenLoaiThongTin = "Tên loại thông tin";
        public const string KieuDuLieu = "Kiểu dữ liệu";
        public const string MacDinh = "Mặc định";
        public const string IsDefault = "IsDefault";
        public const string IsDisplayDatagrid = "IsDisplayDataGrid";
        public const string IsAllowEmptyValue = "IsAllowEmptyValue";
        public const string TenTruyXuat = "Tên truy xuất";
        public const string ThuTuHienThi = "Thứ tự";

        // Table KHO_ThongTinNVL
        public const string Table_ThongTinNVL = "KHO_ThongTinNVL";
        public const string TTNVLID = "TTNVLID";
        //public const string NVLID = "NVLID";
        //public const string LoaiTTNVLID = "LoaiTTNVLID";
        public const string GiaTri = "Giá trị";

        // Table KHO_PhieuNhapKho
        public const string Table_PhieuNhapKho = "KHO_PhieuNhapKho";
        public const string PNKID = "PNKID";
        public const string MaPhieuNhapKho = "Mã phiếu";
        public const string NguoiLapPNK = "Người lập phiếu";
        public const string NgayLapPNK = "Ngày lập phiếu";
        public const string GhiChuPNK = "Ghi chú";
        public const string IsDonePNK = "IsDonePNK";

        // Table KHO_PhieuNhapKhoTP
        public const string Table_PhieuNhapKhoTPham = "KHOTP_PhieuNhapKhoTP";
        public const string PNKTPID = "PNKTPID";
        public const string CodePallet = "CodePallet";
        public const string VTTPID = "VTTPID";
        public const string VTofTPID = "VTofTPID";


        // Table KHO_NVLofPhieuNhapKho
        public const string Table_NVLofPhieuNhapKho = "KHO_NVLofPhieuNhapKho";
        //public const string PNKID = "PNKID";
        //public const string NVLID = "NVLID";
        public const string NVLPNKID = "NVLPNKID";
        public const string NVLNKSoLuongAll = "Tổng số lượng";

        // Table KHO_LenhNhapKho
        public const string Table_LenhNhapKho = "KHO_LenhNhapKho";
        public const string LenhNKID = "LenhNKID";
        //public const string PNKID = "PNKID";
        //public const string NVLPNKID = "NVLPNKID";
        //public const string VTID = "VTID";
        public const string LNKSoLuong = "Số lượng";
        public const string LNKIsDone = "IsDone";

        // Table KHO_PhieuXuatKho
        public const string Table_PhieuXuatKho = "KHO_PhieuXuatKho";
        public const string PXKID = "PXKID";
        public const string MaPhieuXuatKho = "Mã phiếu";
        public const string NguoiLapPXK = "Người lập phiếu";
        public const string NgayLapPXK = "Ngày lập phiếu";
        public const string GhiChuPXK = "Ghi chú";
        public const string IsDonePXK = "IsDonePXK";
        public const string STT_PXK = "STT_PXK";
        public const string IsPhieuBoSungNVL = "IsPhieuBoSungNVL";
        public const string IsPhieuBSungAddedLOTNVL = "IsPhieuBSungAddedLOTNVL";
        public const string IsReturnedNVL = "IsReturnedNVL";

        // Table KHO_NVLofPhieuXuatKho
        public const string Table_NVLofPhieuXuatKho = "KHO_NVLofPhieuXuatKho";
        //public const string PXKID = "PXKID";
        //public const string NVLID = "NVLID";
        public const string NVLPXKID = "NVLPXKID";
        public const string NVLXKSoLuongAll = "Tổng số lượng";

        // Table KHO_LenhXuatKho
        public const string Table_LenhXuatKho = "KHO_LenhXuatKho";
        public const string LenhXKID = "LenhXKID";
        //public const string PXKID = "PXKID";
        //public const string NVLPXKID = "NVLPXKID";
        //public const string VTID = "VTID";
        public const string LXKSoLuong = "Số lượng";
        public const string LXKIsDone = "IsDone";

        // Table KHO_HistoryXuatNhapKho
        public const string Table_HistoryXNKho = "KHO_HistoryXuatNhapKho";
        public const string LogXNKID = "LogXNKID";
        public const string LogLoaiPhieu = "Loại phiếu";
        public const string LogMaPhieu = "Mã phiếu";
        public const string LogTenNVL = "Tên NVL";
        public const string LogMaViTri = "Mã vị trí";
        public const string LogTonKhoTruoc = "Tồn kho trước";
        public const string LogSoLuong = "SL Xuất/Nhập";
        public const string LogTonKhoSau = "Tồn kho sau";
        public const string LogNgThucHien = "Người thực hiện";
        public const string LogThoiDiem = "Thời điểm";
        public const string LogTypePNK = "Nhập kho";
        public const string LogTypeTraKho = "Trả kho";
        public const string LogTypeKiemKe = "Kiểm kê";
        public const string LogTypePNK_Manual = "Nhập tay";
        public const string LogTypePXK_Manual = "Xuất tay";
        public const string LogTypePXK = "Xuất kho";
        //public const string NVLID = "NVLID";
        //public const string VTID = "VTID";

        // Table Maymoc //
        public const string Table_MayMoc = "MAY_MayMoc";
        public const string MM_MMID = "MMID";
        public const string MM_MaMay = "Mã máy";
        public const string MM_TenMay = "Tên máy";
        public const string MM_Serial = "Serial";
        public const string MM_NgaySuDung = "Ngày sử dụng";
        public const string MM_BoPhanSuDung = "Bộ phận sử dụng";
        //public const string MM_TinhTrang = "Tình trạng";
        public const string MM_GhiChu = "Ghi chú";
        public const string NCIDs = "NCIDs";

        // Table LoaiThongTinMayMoc
        public const string Table_LoaiThongTinMayMoc = "MAY_LoaiThongTinMayMoc";
        public const string MM_LoaiTTMMID = "LoaiTTMMID";
        public const string MM_TenLoaiThongTin = "Tên loại thông tin";
        public const string MM_KieuDuLieu = "Kiểu dữ liệu";
        public const string MM_GiaTriMacDinh = "Gía trị mặc định";
        public const string MM_IsDefault = "IsDefault";
        public const string MM_TenTruyXuat = "Tên truy xuất";
        public const string MM_IndexDisplay = "IndexDisplay";

        // Table ThongTinMayMoc
        public const string Table_ThongTinMayMoc = "MAY_ThongTinMayMoc";
        public const string MM_TTMMID = "TTMMID";
        public const string MM_GiaTriThongTin = "Giá trị";
        //public const string MM_MMID = "MMID";
        //public const string MM_LoaiTTMMID = "LoaiTTMMID";

        // Table Nhanvien
        public const string Table_NhanVien = "NV_NhanVien";
        public const string NV_NVID = "NVID";
        public const string NV_MaNhanVien = "Mã nhân viên";

        // Table LoaiThongTinNhanVien
        public const string Table_LoaiThongTinNhanVien = "NV_LoaiThongTinNhanVien";
        public const string NV_LoaiTTNVID = "LoaiTTNVID";
        public const string NV_TenLoaiThongTin = "Tên loại thông tin";
        public const string NV_KieuDuLieu = "Kiểu dữ liệu";
        public const string NV_GiaTriMacDinh = "Gía trị mặc định";
        public const string NV_IsDefault = "IsDefault";
        public const string NV_TenTruyXuat = "Tên truy xuất";
        public const string NV_IndexDisplay = "IndexDisplay";
        public const string NV_IsDisplay = "IsDisplay";

        // Table ThongTinNhanVien
        public const string Table_ThongTinNhanVien = "NV_ThongTinNhanVien";
        public const string NV_TTNVID = "TTNVID";
        public const string NV_GiaTriThongTin = "Giá trị";
        //public const string NV_NVID = "NVID";
        //public const string NV_LoaiTTNVID = "LoaiTTNVID";

        // Socket 
        public const string socketEndKey = "<END>";
        public const string CMDTYPE = "CMDTYPE";
        public const string SCANCODE = "SCANCODE";
        public const string RETURNRESULT = "RETURNRESULT";
        public const string RESULTMESS = "RESULTMESS";
        public const string SUCCESS = "SUCCESS";
        public const string FAIL = "FAIL";

        // Socket PXK Tags
        public const string PXK_MPXK = "PXK_MPXK";
        public const string PXK_MVT = "PXK_MVT";
        public const string PXK_MNVL = "PXK_MNVL";
        public const string PXK_LOAD = "PXK_LOAD";
        public const string PXK_RETURN = "PXK_RETURN";
        public const string PXK_LXKSL = "PXK_LXKSL";
        public const string PXK_LXKID = "PXK_LXKID";
        public const string PXK_HANDLE_LXK = "PXK_HANDLE_LXK";
        public const string PXK_HANDLE_LXK_RETURN = "PXK_HANDLE_LXK_RETURN";
        public const string PXK_HANDLE_LXK_MANUAL = "PXK_HANDLE_LXK_MANUAL";
        // Socket PNK Tags
        public const string PNK_MPNK = "PNK_MPNK";
        public const string PNK_MVT = "PNK_MVT";
        public const string PNK_MNVL = "PNK_MNVL";
        public const string PNK_LOAD = "PNK_LOAD";
        public const string PNK_RETURN = "PNK_RETURN";
        public const string PNK_LNKSL = "PNK_LNKSL";
        public const string PNK_LNKID = "PNK_LNKID";
        public const string PNK_HANDLE_LNK = "PNK_HANDLE_LNK";
        public const string PNK_HANDLE_LNK_RETURN = "PNK_HANDLE_LNK_RETURN";
        public const string PNK_HANDLE_LNK_MANUAL = "PNK_HANDLE_LNK_MANUAL";


        // Socket CHECKING Tags
        public const string CHECK_LOAD = "CHECK_LOAD";
        public const string CHECK_LOAD_RETURN = "CHECK_LOAD_RETURN";
        public const string CHECK_SCANCODE = "CHECK_SCANCODE";

        public const int DispMode1 = 1;
        public const int DispMode2 = 2;
        public const int DispMode3 = 3;     // Mode them KHSX

        public static event EventHandler? ClickSaveEvent;
        public static event EventHandler<object?>? CongdoaUpdatedEvent;
        public static event EventHandler? ReloadPNKEvent;
        public static event EventHandler? ReloadPXKEvent;

        // KHSX static variable
        public static object? CurrentKHSXid;
        public static object? CurrentNVLMCDid;
        public static object? CurrentCDid;
        public static KHSX? CurrentKHSX;

        // NVL management static variable
        public static object? SelectedNVLid;

        // MayMoc management static variable
        public static object? SelectedMayMocid;

        // NhanVien management static variable
        public static object? SelectedNhanVienid;

        // SanPham management static variable
        public static object? SelectedSanPhamid;

        // PhieuXuatKho static variable
        public static object? SelectedPXKID;
        public static object? SelectedPNKID;

        // KQGC management static variable
        public static object? SelectedKQGCid;

        public static object? SelectedPNKTPID;
        public static object? SelectedPXKTPID;

        public static class Tentruyxuat
        {
            public const string VitriKe = "vitrike";
            public const string VitriHang = "vitrihang";
            public const string VitriCot = "vitricot";
            public const string DonViTinh = "donvitinh";
        }

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

        // Reload phieu nhap kho Event
        public static void RaiseReloadPNK_Event()
        {
            ReloadPNKEvent?.Invoke(null, EventArgs.Empty);
        }
        public static void ResetReloadPNK_Event()
        {
            ReloadPNKEvent = null;
        }
        public static bool IsReloadPNK_EventRegistered()
        {
            return ReloadPNKEvent != null;
        }

        // Reload phieu xuat kho Event
        public static void RaiseReloadPXK_Event()
        {
            ReloadPXKEvent?.Invoke(null, EventArgs.Empty);
        }
        public static void ResetReloadPXK_Event()
        {
            ReloadPXKEvent = null;
        }
        public static bool IsReloadPXK_EventRegistered()
        {
            return ReloadPXKEvent != null;
        }

        public static string RemoveDiacriticsAndSpaces(string text)
        {
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new();

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString()
                .Normalize(NormalizationForm.FormC)
                .ToLowerInvariant()
                .Replace(" ", "");
        }

        private static Random random = new Random();

        public static string Generate5UppercaseChars()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateUppercaseChars(int lenght)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, lenght)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static DateTime? ParseDate_ddMMyyyy(string? input)
        {
            string[] formats = { "d/M/yyyy", "dd-MM-yyyy", "dd/MM/yyyy", "yyyy/MM/dd", "dd-MM-yy", "dd/MM/yy", "ddMMyyyy", "ddMMyy", "dd-MM", "dd/MM", "ddMM" };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(input?.Trim(), format, null, DateTimeStyles.None, out var result))
                {
                    return result;
                }
            }

            return null;
        }

        public static DateTime? ParseSqlDateTime(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            string[] formats = {
            "MM/dd/yyyy h:mm:ss tt",    // 12/30/2024 5:33:00 PM
            "MM/dd/yyyy h:mm tt",       // 12/30/2024 5:33 PM
            "MM/dd/yyyy h tt",          // 12/30/2024 5 PM
            "MM/dd/yyyy"                // 12/30/2024
            };

            return DateTime.TryParseExact(
                input.Trim(),
                formats,
                new CultureInfo("en-US"),
                DateTimeStyles.None,
                out DateTime result) ? result : null;
        }

        public static DateTime? ParseDate_MMddyyyy(string? input)
        {
            string[] formats = { "M/d/yyyy", "MM-dd-yyyy", "MM/dd/yyyy", "MM-dd-yy", "MM/dd/yy", "MMddyyyy", "MMddyy", "MMdd", "MM/dd", "MMdd" };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(input?.Trim(), format, null, DateTimeStyles.None, out var result))
                {
                    return result;
                }
            }

            return null;
        }

        public static T GetElementOrNew<T>(List<T> list, int index) where T : new()
        {
            // Check if the index is within the list's bounds
            if (list == null || index < 0 || index >= list.Count)
            {
                // Return a new instance of T
                return new T();
            }

            // Return the element at the specified index
            return list[index];
        }

        public static List<string> GetListNCIDs(object? ncids)
        {
            string ids = ncids?.ToString()?.Trim() ?? string.Empty;

            if (!string.IsNullOrEmpty(ids))
            {
                List<string> nvids = ids.Split(",").ToList();

                return nvids;
            }
            else return new();
        }
    }
}
