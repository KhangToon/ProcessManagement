using ProcessManagement.Models.KHO_NVL;
using ProcessManagement.Models.SANPHAM;
using ProcessManagement.Services.SQLServer;

namespace ProcessManagement.Models
{
    public class GenerateLot_V3
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

        public List<TemLotNVL>? ListTempLOT_NVLs { get; set; }
        private List<SanPham> DSachSanPhams { get; set; } = new List<SanPham>();

        public bool isErrorSLloiChophep = true;
        public bool isComfirmSLLoiNguyenCong = false;
        public bool isGenerateListLotOK = false;
        public bool isCheckSLNVLisOK = false;

        IEnumerable<NguyenCong>? _defaultListCDnames = new List<NguyenCong>();
        public IEnumerable<NguyenCong>? DefaultListCDnames
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

        IEnumerable<NguyenCong>? _selectListCDnames = new List<NguyenCong>();
        public IEnumerable<NguyenCong>? SelectListCDnames
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
                        congDoan.TenCongDoan.Value = nguyencong.TenNguyenCong.Value;
                        congDoan.NCID.Value = nguyencong.NCID.Value;
                        congDoan.IsUsing = true;
                        NewKHSX.DSachCongDoans.Add(congDoan);
                    }
                }

                CheckSoluongLoiChophep();
            }
        }

        // First step for initial KHSX 
        public GenerateLot_V3()
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

        public void AddNewNguyenCongtoList(NguyenCong newnguyencong)
        {
            if (newnguyencong.NCID.Value == null) { return; }

            if (_defaultListCDnames != null)
            {
                var convertlist = _defaultListCDnames.ToList();

                convertlist.Add(newnguyencong);

                _defaultListCDnames = convertlist;
            }
            else
            {
                var convertlist = new List<NguyenCong>
                {
                    newnguyencong
                };
                _defaultListCDnames = convertlist;
            }
        }

        private void LoadListTenNguyenCong()
        {
            List<NguyenCong> nguyenCongs = SQLServerServices.GetListNguyenCongs();

            //List<string> tenNClist = new();

            //foreach (var nguyencong in nguyenCongs)
            //{
            //    var tenNC = nguyencong.TenNguyenCong.Value?.ToString();

            //    if (tenNC != null)
            //    {
            //        tenNClist.Add(tenNC);
            //    }
            //}

            //_defaultListCDnames = tenNClist;
            _defaultListCDnames = nguyenCongs;
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
            NewKHSX.SPID.Value = NewKHSX.TargetSanPham?.SP_SPID.Value;
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
            ListTempLOT_NVLs = new();
            NewKHSX.DSachNVLofKHSXs = new();
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
            ListTempLOT_NVLs = new(); // reset danh sach LOT NVL

            int result = -1; string error = string.Empty;

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

            ListTempLOT_NVLs = new();

            // Tinh dinh muc
            //DinhMuc = (int)(SLNgVatLieuSX + SLNgVatLieuSX * (TiLeLoi / 100));

            // Tinh so luong lot chan
            SLLotChan = DinhMuc / SLperLotChan;

            // Calculate and generate list lot nvl
            for (int index = 1; index <= SLLotChan; index++)
            {
                TemLotNVL nVL = new();
                nVL.LoaiNVL.Value = NewKHSX.LoaiNVL?.TenLoaiNVL.Value;
                nVL.MaSP.Value = NewKHSX.TargetSanPham?.SP_MaSP.Value;
                nVL.MaQuanLy.Value = MaLSX + "-" + NewKHSX.TargetSanPham?.SP_MaSP.Value + "-" + index.ToString(NumberDigit);
                nVL.SoLuong.Value = SLperLotChan;
                nVL.NgayXuat.Value = DateTime.Now;
                ListTempLOT_NVLs?.Add(nVL);
            }

            if (DinhMuc % SLperLotChan > 0)
            {
                SLLotLe = 1;

                // Tinh so luong per lot le
                SLperLotLe = DinhMuc - SLperLotChan * SLLotChan;

                // Add lot le
                TemLotNVL lotnvlle = new();
                lotnvlle.LoaiNVL.Value = NewKHSX.LoaiNVL?.TenLoaiNVL.Value;
                lotnvlle.MaSP.Value = NewKHSX.TargetSanPham?.SP_MaSP.Value;
                lotnvlle.MaQuanLy.Value = MaLSX + "-" + NewKHSX.TargetSanPham?.SP_MaSP.Value + "-" + (SLLotChan + 1).ToString(NumberDigit);
                lotnvlle.SoLuong.Value = SLperLotLe;
                lotnvlle.NgayXuat.Value = DateTime.Now;
                ListTempLOT_NVLs?.Add(lotnvlle);
            }
            else { SLLotLe = 0; SLperLotLe = 0; }


            SLLot = ListTempLOT_NVLs?.Count ?? 0;

            return (1, error);
        }


        public List<TemLotNVL>? GenerateLotNVL_perVitriofNVL(List<ViTriofNVL> dsvitris, int soluong, int slperlotchan)
        {
            if (slperlotchan == 0 || soluong == 0 || dsvitris.Count == 0)
            {
                return null;
            }

            int sllotchan = 0;
            int sllotle = 0;

            List<TemLotNVL> temLotNVLs = new();

            int startIndex = 0;

            foreach (var vitri in dsvitris)
            {
                var results = GenerateLotNVL_withNgayNhapKho(vitri, slperlotchan, startIndex);

                startIndex = results.lastIndex;

                sllotchan += results.sllotchan;

                sllotle += results.sllotle;

                if (results.temLotNVLs != null) { temLotNVLs.AddRange(results.temLotNVLs); }
            }

            SLLotChan = sllotchan;
            SLLotLe = sllotle;

            SLLot = SLLotChan + SLLotLe;

            return temLotNVLs;
        }

        private (int lastIndex, int sllotchan, int sllotle, int slperlotle, List<TemLotNVL>? temLotNVLs) GenerateLotNVL_withNgayNhapKho(ViTriofNVL viTriofNVL, int slperlotchan, int startIndex)
        {
            int soluong = viTriofNVL.SLTake;

            if (SLNgVatLieuSX == 0 || slperlotchan == 0 || (soluong / slperlotchan < 1))
            {
                return (0, 0, 0, 0, null);
            }

            Thread.Sleep(500);

            List<TemLotNVL> temLotNVLs = new();

            int sllotchan;
            int sllotle;
            int slperlotle;

            // Tinh dinh muc
            //soluong = (int)(SLNgVatLieuSX + SLNgVatLieuSX * (TiLeLoi / 100));

            // Tinh so luong lot chan
            sllotchan = soluong / slperlotchan;

            // Calculate and generate list lot nvl
            for (int index = 1; index <= sllotchan; index++)
            {
                startIndex++;

                TemLotNVL nVL = new();
                nVL.LoaiNVL.Value = NewKHSX.LoaiNVL?.TenLoaiNVL.Value;
                nVL.MaSP.Value = NewKHSX.TargetSanPham?.SP_MaSP.Value;
                nVL.MaQuanLy.Value = MaLSX + "-" + NewKHSX.TargetSanPham?.SP_MaSP.Value + "-" + startIndex.ToString(NumberDigit);
                nVL.SoLuong.Value = slperlotchan;
                nVL.NgayNhap.Value = viTriofNVL.NgayNhapKho.Value;
                //nVL.NgayXuat.Value = DateTime.Now;
                temLotNVLs?.Add(nVL);
            }

            if (soluong % slperlotchan > 0)
            {
                startIndex++;

                sllotle = 1;

                // Tinh so luong per lot le
                slperlotle = soluong - slperlotchan * sllotchan;

                // Add lot le
                TemLotNVL lotnvlle = new();
                lotnvlle.LoaiNVL.Value = NewKHSX.LoaiNVL?.TenLoaiNVL.Value;
                lotnvlle.MaSP.Value = NewKHSX.TargetSanPham?.SP_MaSP.Value;
                lotnvlle.MaQuanLy.Value = MaLSX + "-" + NewKHSX.TargetSanPham?.SP_MaSP.Value + "-" + startIndex.ToString(NumberDigit);
                lotnvlle.SoLuong.Value = slperlotle;
                lotnvlle.NgayNhap.Value = viTriofNVL.NgayNhapKho.Value;
                //lotnvlle.NgayXuat.Value = DateTime.Now;
                temLotNVLs?.Add(lotnvlle);
            }
            else { sllotle = 0; slperlotle = 0; }

            return (startIndex, sllotchan, sllotle, slperlotle, temLotNVLs);
        }
    }
}
