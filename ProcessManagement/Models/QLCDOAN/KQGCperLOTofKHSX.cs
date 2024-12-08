using Microsoft.SqlServer.Server;
using ProcessManagement.Models.KHSXs;
using ProcessManagement.Services.SQLServer;
using System.Collections.Generic;
using System.Globalization;
using static ProcessManagement.Models.QLCDOAN.KQGCperCDOANofLOTKHSX;

namespace ProcessManagement.Models.QLCDOAN
{
    public class KQGCperCDOANofLOTKHSX
    {
        private readonly KHSX_LOT _targetlot = new();
        public KHSX_LOT TargetLOT { get { return _targetlot; } }
        public List<ResultCalamviec> ResultCalamviecs { get; set; } = new();
        public int TotalOK { get; set; } = 0;
        public int TotalNG { get; set; } = 0;
        public int Total { get; set; } = 0;
        public int DanhGia { get; set; } = 2; // OK is 1, NG is 0, Not yet submit is 2

        private readonly SQLServerServices SQLServerServices = new();

        public KQGCperCDOANofLOTKHSX(KHSX_LOT targetlot, KQGCperCDOANofLOTKHSX? preKQGCperCDOANofLOTKHSX)
        {
            _targetlot = targetlot;

            GetKQGCofKHSXLOTperCDoan(preKQGCperCDOANofLOTKHSX);
        }

        public KQGCperCDOANofLOTKHSX() { }

        // Load dsach KQGC of LOT in target CongDoan with Calamviec (ca Ngay/Dem)
        private void GetKQGCofKHSXLOTperCDoan(KQGCperCDOANofLOTKHSX? preKQGCperCDOANofLOTKHSX = null)
        {
            // Load ds KQGC for targetlot
            List<KetQuaGC> ketQuaGCperlots = GetKQGCperLOTKHSX(_targetlot).Item1;

            ResultCalamviecs = new();

            foreach (var kqgc in ketQuaGCperlots)
            {
                ResultCalamviec resultperCDoan = new()
                {
                    Ca = kqgc.CaLamViec.Value,
                    NgayGiaCong = DateTime.TryParse(kqgc.SubMitDay.Value?.ToString()?.Trim(), out var result) ? result.ToShortDateString() : null,
                    NhanVien = kqgc.NVIDs.Value,
                    SLOK = kqgc.SLOK.Value,
                    SLNG = kqgc.SLNG.Value,
                };

                if (resultperCDoan != null)
                {
                    ResultCalamviecs.Add(resultperCDoan);
                }
            }

            // // current 
            Total = int.TryParse(_targetlot.SLLOT.Value?.ToString(), out int cur_total) ? cur_total : 0;
            TotalOK = ResultCalamviecs.Sum(rs => int.TryParse(rs.SLOK?.ToString(), out int cur_totalok) ? cur_totalok : 0);
            TotalNG = ResultCalamviecs.Sum(rs => int.TryParse(rs.SLNG?.ToString(), out int cur_totalng) ? cur_totalng : 0);

            if (preKQGCperCDOANofLOTKHSX != null) // Not first element
            {
                // previous total done (OK + NG)
                int predoneOK = preKQGCperCDOANofLOTKHSX.TotalOK;
                int predoneNG = preKQGCperCDOANofLOTKHSX.TotalNG;

                if (predoneOK > 0 || predoneNG > 0)
                {
                    DanhGia = ((TotalOK + TotalNG) == predoneOK) ? 1 : 0;
                }
                else if ((predoneOK + predoneNG) == 0)
                {
                    DanhGia = 2;
                }
            }
            else // for first element 
            {
                if (TotalOK > 0 || TotalNG > 0)
                {
                    DanhGia = ((TotalOK + TotalNG) == Total) ? 1 : 0;
                }
                else if ((TotalOK + TotalNG) == 0)
                {
                    DanhGia = 2;
                }
            }

            // Set null for current element if it not submited
            if ((TotalOK + TotalNG) == 0)
            {
                DanhGia = 2;
            }
        }

        // Load dsach KQGC per LOT with targetCDoan
        private (List<KetQuaGC>, string) GetKQGCperLOTKHSX(KHSX_LOT lot)
        {
            Dictionary<string, object?> parameters = new()
            {
                { $"{KetQuaGC.KQGCDBName.KHSXID}", lot.KHSXID.Value },
                { $"{KetQuaGC.KQGCDBName.NCID}", lot.NCID.Value },
                { $"{KetQuaGC.KQGCDBName.MaQuanLyLot}", lot.MaQuanLyLot.Value },
            };

            (List<KetQuaGC> targetKQGClist, string getError) = SQLServerServices.GetListKetQuaGC(parameters);

            return (targetKQGClist, getError);
        }

        public class ResultCalamviec
        {
            public object? Ca { get; set; }
            public object? NgayGiaCong { get; set; }
            public object? NhanVien { get; set; }
            public object? SLOK { get; set; }
            public object? SLNG { get; set; }
        }
    }
}
