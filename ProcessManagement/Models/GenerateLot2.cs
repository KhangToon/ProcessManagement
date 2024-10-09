using ProcessManagement.Models.KHO_NVL;
using ProcessManagement.Models.SANPHAM;
using ProcessManagement.Services.SQLServer;

namespace ProcessManagement.Models
{
    public class GenerateLot2
    {
        private static readonly SQLServerServices SQLServerServices = new();
        private static readonly int LimitRange = 999;
        private static readonly string NumberDigit = "D3";

        public string MaLSX { get; set; } = string.Empty;
        public int SLSanPhamSX { get; set; } = 0;
        public int SLNgVatLieuSX { get; set; } = 0;
        public double TiLeLoi { get; set; } = 0;
        public int SLLoi { get; set; } = 0;
        public int DinhMuc { get; set; } = 0;
        public int SLLot { get; set; } = 0;
        public int SLperLotChan { get; set; } = 0;
        public int SLLotChan { get; set; } = 0;
        public int SLLotLe { get; set; } = 0;
        public int SLperLotLe { get; set; } = 0;
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public KHSX NewKHSX { get; set; } = new KHSX();

        public List<NVL>? ListNVLs { get; set; }
        private List<SanPham> DSachSanPhams { get; set; } = new List<SanPham>();
        private List<LoaiNVL> DSachLoaiNVLs { get; set; } = new List<LoaiNVL>();

        public bool isErrorSLloiChophep = true;
        public bool isComfirmSLLoiNguyenCong = false;
        public bool isGenerateListLotOK = false;
        public bool isCheckSLNVLisOK = false;

        IEnumerable<string>? _defaultListCDnames = new List<string>();
        public IEnumerable<string>? DefaultListCDnames
        {
            get
            {
                return _defaultListCDnames;
            }
            set
            {
                if (_defaultListCDnames != value)
                {
                    _defaultListCDnames = value;
                }
            }
        }

        IEnumerable<string>? _selectListCDnames = new List<string>();
        public IEnumerable<string>? SelectListCDnames
        {
            get
            {
                return _selectListCDnames;
            }
            set
            {
                if (_selectListCDnames != value)
                {
                    _selectListCDnames = value;
                }

                if (_selectListCDnames == null)
                {
                    NewKHSX.DSachCongDoans = new();
                }
                else
                {
                    NewKHSX.DSachCongDoans = new();

                    foreach (var nguyencong in _selectListCDnames)
                    {
                        NguyenCongofKHSX congDoan = new();
                        congDoan.TenCongDoan.Value = nguyencong;
                        congDoan.IsUsing = true;
                        NewKHSX.DSachCongDoans.Add(congDoan);
                    }
                }

                CheckSoluongLoiChophep();
            }
        }

        // First step for initial KHSX 
        public GenerateLot2()
        {
            // Tao LSX unique id
            MaLSX = AutoGenerateLSXCode();

            LoadListTenNguyenCong();
        }

        public bool CheckSLNVLisOK()
        {
            int sumSL = NewKHSX.DSachNVLofKHSXs?.Sum(nvl => (int.TryParse(nvl.SoLuong.Value?.ToString(), out int sl) ? sl : 0)) ?? 0;

            int dinhmuc = int.TryParse(NewKHSX?.DinhMuc.Value?.ToString(), out int vl) ? vl : 0;

            return (sumSL > 0);
        }

        public bool CheckSoluongLoiChophep()
        {
            int tongSLloi = DinhMuc - SLNgVatLieuSX;

            int slloinew = 0;

            foreach (var nguyencong in NewKHSX.DSachCongDoans)
            {
                slloinew += (int.TryParse(nguyencong.SoluongNG.Value?.ToString(), out int slloi) ? slloi : 0);
            }

            if (slloinew != tongSLloi)
            {
                isErrorSLloiChophep = true; isComfirmSLLoiNguyenCong = false; return false;
            }
            else { isErrorSLloiChophep = false; return true; }
        }

        public void AddNewNguyenCongtoList(string newnguyencong)
        {
            if (newnguyencong == string.Empty) { return; }

            if (_defaultListCDnames != null)
            {
                var convertlist = _defaultListCDnames.ToList();

                convertlist.Add(newnguyencong);

                _defaultListCDnames = convertlist;
            }
            else
            {
                var convertlist = new List<string>
                {
                    newnguyencong
                };
                _defaultListCDnames = convertlist;
            }
        }

        private void LoadListTenNguyenCong()
        {
            List<NguyenCong> nguyenCongs = SQLServerServices.GetListNguyenCongs();

            List<string> tenNClist = new();

            foreach (var nguyencong in nguyenCongs)
            {
                var tenNC = nguyencong.TenNguyenCong.Value?.ToString();

                if (tenNC != null)
                {
                    tenNClist.Add(tenNC);
                }
            }

            _defaultListCDnames = tenNClist;
        }

        public void SetCurrentKHSXsanPham(string tenSP)
        {
            NewKHSX.SanPham = DSachSanPhams.FirstOrDefault(sp => sp.SP_TenSanPham.Value?.ToString() == tenSP) ?? new();
        }

        public void AsignKHSXdata()
        {
            NewKHSX.MaLSX.Value = MaLSX;
            NewKHSX.LOAINVLID.Value = NewKHSX.LoaiNVL?.LOAINVLID.Value;
            NewKHSX.SLSanPhamSX.Value = SLSanPhamSX;    
            NewKHSX.SLNVLSanXuat.Value = SLNgVatLieuSX;
            NewKHSX.DinhMuc.Value = DinhMuc;
            NewKHSX.TileLoi.Value = TiLeLoi;
            NewKHSX.SLLot.Value = SLLot;
            NewKHSX.SLLotChan.Value = SLLotChan;
            NewKHSX.SLperLotChan.Value = SLperLotChan;
            NewKHSX.SLLotLe.Value = SLLotLe;
            NewKHSX.SLperLotLe.Value = SLperLotLe;
            NewKHSX.SPID.Value = NewKHSX.SanPham?.SP_SPID.Value;
            NewKHSX.NgayTao.Value = NgayTao;
        }

        public void ResetDefault()
        {
            SLSanPhamSX = 0;
            SLNgVatLieuSX = 0;
            TiLeLoi = 0;
            SLLoi = 0;
            DinhMuc = 0;
            SLLot = 0;
            SLperLotChan = 0;
            SLLotChan = 0;
            SLLotLe = 0;
            SLperLotLe = 0;
            ListNVLs = new();
            NewKHSX.DSachNVLofKHSXs = new();
        }

        public List<string> GetDSMaSPs()
        {
            DSachSanPhams = SQLServerServices.GetlistSanphams();

            List<string> result = DSachSanPhams.Select(sp => sp.SP_TenSanPham.Value?.ToString() ?? string.Empty).ToList();

            return result;
        }

        private static string AutoGenerateLSXCode()
        {
            string lastheaderKey = "A"; int lastNum = 0; int total = 1;

            KHSX lastKHSX = SQLServerServices.GetLastKHSX();

            if (lastKHSX != null && lastKHSX.MaLSX.Value != null)
            {
                string maqualy = lastKHSX.MaLSX.Value.ToString() ?? string.Empty;

                if (maqualy == string.Empty) { return string.Empty; }

                lastheaderKey = maqualy.First().ToString();

                lastNum = int.Parse(maqualy.Substring(1, 3));
            }


            if ((lastNum + 1) > LimitRange)
            {
                string nextheaderKey = GetHeaderKey(lastheaderKey);

                return GenerateIncreaseCode(nextheaderKey, 1, total).FirstOrDefault() ?? string.Empty;
            }
            else
            {
                return GenerateIncreaseCode(lastheaderKey, lastNum + 1, total).FirstOrDefault() ?? string.Empty;
            }
        }

        private static List<string> GenerateIncreaseCode(string headerKey, int startNum, int total)
        {
            List<string> lotlistcode = new();

            if (startNum > LimitRange)
            {
                return lotlistcode;
            }

            int endNum = startNum + total - 1;

            if (endNum < LimitRange)
            {
                for (int index = startNum; index <= endNum; index++)
                {
                    string lot = $"{headerKey}{index.ToString(NumberDigit)}";
                    lotlistcode.Add(lot);
                }
            }
            else
            {
                for (int index = startNum; index <= LimitRange; index++)
                {
                    string lot = $"{headerKey}{index.ToString(NumberDigit)}";
                    lotlistcode.Add(lot);
                }

                int lastTotal = endNum - LimitRange;
                string nextHeader = GetHeaderKey(headerKey);
                List<string> lastlot = GenerateIncreaseCode(nextHeader, 1, lastTotal);
                lotlistcode.AddRange(lastlot);
            }
            return lotlistcode;
        }

        private static string GetHeaderKey(string beforeKey)
        {
            string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            int beforekeyIndex = letters.IndexOf(beforeKey);

            if (beforekeyIndex != (letters.Length - 1))
            {
                return letters[beforekeyIndex + 1].ToString();
            }
            else
                return letters[0].ToString();
        }

        public (int, string) ReCalculate() // Tinh Dinhmuc, SLLot, SLperLot,...
        {
            // Reset so luong cua moi nvl cua lsx khi bat ki thong so nao thay doi (loai nvl, ti le loi, slsx, ...)
            isGenerateListLotOK = false; // false bit generate LOT
            ListNVLs = new(); // reset danh sach LOT NVL

            int result = -1; string error = string.Empty;

            // Tinh dinh muc
            //DinhMuc = (int)(SLNgVatLieuSX + SLNgVatLieuSX * (TiLeLoi / 100));

            if (SLNgVatLieuSX == 0 || SLperLotChan == 0 || (DinhMuc / SLperLotChan < 1))
            {
                SLLot = 0;

                return (result, "Số lượng nhập vào không hợp lệ");
            }
            else
            {
                // Tinh so luong 
                SLLotChan = DinhMuc / SLperLotChan;

                if (DinhMuc % SLperLotChan > 0)
                {
                    SLLotLe = 1;

                    SLperLotLe = DinhMuc - SLperLotChan * SLLotChan;
                }
                else { SLLotLe = 0; SLperLotLe = 0; }

                // Tinh tong so luong lot
                SLLot = (int)(SLLotChan + SLLotLe);

                return (1, error);
            }
        }

        // Generate lot nvl
        public (int, string) GenerateLotNVL()
        {
            int result = -1; string error = string.Empty;

            if (SLNgVatLieuSX == 0 || SLperLotChan == 0 || (DinhMuc / SLperLotChan < 1))
            {
                return (result, "Số lượng nhập vào không hợp lệ");
            }

            Thread.Sleep(500);

            ListNVLs = new();

            // Tinh dinh muc
            //DinhMuc = (int)(SLNgVatLieuSX + SLNgVatLieuSX * (TiLeLoi / 100));

            // Tinh so luong lot chan
            SLLotChan = DinhMuc / SLperLotChan;

            // Calculate and generate list lot nvl
            for (int index = 1; index <= SLLotChan; index++)
            {
                NVL nVL = new();
                nVL.LoaiNVL.Value = NewKHSX.LoaiNVL?.TenLoaiNVL.Value;
                nVL.MaSP.Value = NewKHSX.SanPham?.SP_MaSP.Value;
                nVL.MaQuanLy.Value = MaLSX + "-" + NewKHSX.SanPham?.SP_MaSP.Value + "-" + index.ToString(NumberDigit);
                nVL.SoLuong.Value = SLperLotChan;
                nVL.NgayXuat.Value = DateTime.Now;
                ListNVLs?.Add(nVL);
            }

            if (DinhMuc % SLperLotChan > 0)
            {
                SLLotLe = 1;

                // Tinh so luong per lot le
                SLperLotLe = DinhMuc - SLperLotChan * SLLotChan;

                // Add lot le
                NVL lotnvlle = new();
                lotnvlle.LoaiNVL.Value = NewKHSX.LoaiNVL?.TenLoaiNVL.Value;
                lotnvlle.MaSP.Value = NewKHSX.SanPham?.SP_MaSP.Value;
                lotnvlle.MaQuanLy.Value = MaLSX + NewKHSX.SanPham?.SP_MaSP.Value + "-" + (SLLotChan + 1).ToString(NumberDigit);
                lotnvlle.SoLuong.Value = SLperLotLe;
                lotnvlle.NgayXuat.Value = DateTime.Now;
                ListNVLs?.Add(lotnvlle);
            }
            else { SLLotLe = 0; SLperLotLe = 0; }


            SLLot = ListNVLs?.Count ?? 0;

            return (1, error);
        }
    }
}
