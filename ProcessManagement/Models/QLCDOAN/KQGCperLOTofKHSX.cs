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
        public int DanhGia { get; set; } = 0; // OK is O, NG is 1

        private readonly SQLServerServices SQLServerServices = new();

        public KQGCperCDOANofLOTKHSX(KHSX_LOT targetlot)
        {
            _targetlot = targetlot;

            GetKQGCofKHSXLOTperCDoan();
        }

        public KQGCperCDOANofLOTKHSX() { }

        // Load dsach KQGC of LOT in target CongDoan with Calamviec (ca Ngay/Dem)
        private void GetKQGCofKHSXLOTperCDoan()
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

            // //
            Total = int.TryParse(_targetlot.SLLOT.Value?.ToString(), out int total) ? total : 0;
            TotalOK = ResultCalamviecs.Sum(rs => int.TryParse(rs.SLOK?.ToString(), out int totalok) ? totalok : 0);
            TotalNG = ResultCalamviecs.Sum(rs => int.TryParse(rs.SLNG?.ToString(), out int totalng) ? totalng : 0);

            DanhGia = ((TotalOK + TotalNG) == Total) ? 0 : 1;
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
