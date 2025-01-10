using ProcessManagement.Commons;
using ProcessManagement.Models.QLCDOAN;

namespace ProcessManagement.Models.KHSXs
{
    public class ResultPerLOTKHSX
    {
        public object? MaSP { get; set; }
        public object? MaQuanLyLot { get; set; }
        public object? SLperLOT { get; set; }
        public bool IsLastCDoanDone { get; set; }
        public bool IsCreatedThungTP { get; set; } = false;

        public List<KQGCperCDOANofLOTKHSX> DsachKQGCperCDOAN { get; set; } = new(); // KQGC of current LOT per cong doan

        public List<ThungTPham> ThungTPhams { get; set; } = new(); // danh sach thung thanh pham cua moi LOT

        // public int GetMaxColumnIndex() // get max column of lot result in dsachKQGCperCDOAN
        // {
        //     int maxindex = dsachKQGCperCDOAN.MaxBy(kqgc => kqgc.ResultCalamviecs.Count)?.ResultCalamviecs.Count ?? 2;

        //     return maxindex;
        // }

        public KQGCperCDOANofLOTKHSX TargetKQGC(object? cdid)
        {
            KQGCperCDOANofLOTKHSX targetKQ = DsachKQGCperCDOAN.FirstOrDefault(rs => rs.TargetLOT.NCID.Value?.ToString()?.Trim() == cdid?.ToString()?.Trim()) ?? new();
            return targetKQ;
        }

        public static List<int> GenerateListColumnIndex(int lenght)
        {
            var list = new List<int>();
            for (int i = 0; i < lenght; i++)
            {
                list.Add(i);
            }
            return list;
        }

        public (int sumOK, int sumNG) GetTotalNGOKlastCDoan()
        {
            int sumOK = DsachKQGCperCDOAN.LastOrDefault()?.TotalOK ?? 0;
            int sumNG = DsachKQGCperCDOAN.LastOrDefault()?.TotalNG ?? 0;

            return (sumOK, sumNG);
        }

        public int GetTotalNGlastCDoan()
        {
            int sumNG = DsachKQGCperCDOAN.LastOrDefault()?.TotalNG ?? 0;

            return sumNG;
        }

        public int GetTotalOKAllCDoan()
        {
            int sumAllOK = DsachKQGCperCDOAN.Where(kq => (kq.TotalOK + kq.TotalNG) > 0).LastOrDefault()?.TotalOK ?? 0;

            return sumAllOK;
        }

        public int GetTotalNGAllCDoan()
        {
            int sumAllNG = DsachKQGCperCDOAN.Sum(cd => cd.TotalNG);

            return sumAllNG;
        }

        public object? GetCaLamViec(object? cdid, int caindex)
        {
            object calamviec = " ";

            KQGCperCDOANofLOTKHSX targetKQ = DsachKQGCperCDOAN.FirstOrDefault(rs => rs.TargetLOT.NCID.Value?.ToString()?.Trim() == cdid?.ToString()?.Trim()) ?? new();

            calamviec = Common.GetElementOrNew(targetKQ.ResultCalamviecs, caindex).Ca ?? " ";

            return calamviec;
        }

        public object? GetNhanVien(object? cdid, int caindex)
        {
            object? nhanvien = null;

            KQGCperCDOANofLOTKHSX targetKQ = DsachKQGCperCDOAN.FirstOrDefault(rs => rs.TargetLOT.NCID.Value?.ToString()?.Trim() == cdid?.ToString()?.Trim()) ?? new();

            nhanvien = Common.GetElementOrNew(targetKQ.ResultCalamviecs, caindex).NhanVien ?? null;

            return nhanvien;
        }

        public object? GetMayMoc(object? cdid, int caindex)
        {
            object? maymoc = null;

            KQGCperCDOANofLOTKHSX targetKQ = DsachKQGCperCDOAN.FirstOrDefault(rs => rs.TargetLOT.NCID.Value?.ToString()?.Trim() == cdid?.ToString()?.Trim()) ?? new();

            maymoc = Common.GetElementOrNew(targetKQ.ResultCalamviecs, caindex).MayMoc ?? null;

            return maymoc;
        }

        public object? GetNgayGC(object? cdid, int caindex)
        {
            object ngayGC = " ";

            KQGCperCDOANofLOTKHSX targetKQ = DsachKQGCperCDOAN.FirstOrDefault(rs => rs.TargetLOT.NCID.Value?.ToString()?.Trim() == cdid?.ToString()?.Trim()) ?? new();

            ngayGC = Common.GetElementOrNew(targetKQ.ResultCalamviecs, caindex).NgayGiaCong ?? " ";

            // Convert to datetime

            if (DateTime.TryParse(ngayGC?.ToString(), out DateTime dateGC))
            {
                string dateGCfomarted = dateGC.ToString(Common.FormatNoTime_ddMMyyyy);

                return dateGCfomarted;
            }

            return ngayGC;
        }

        public object? GetSLOK(object? cdid, int caindex)
        {
            object slok = 0;

            KQGCperCDOANofLOTKHSX targetKQ = DsachKQGCperCDOAN.FirstOrDefault(rs => rs.TargetLOT.NCID.Value?.ToString()?.Trim() == cdid?.ToString()?.Trim()) ?? new();

            slok = Common.GetElementOrNew(targetKQ.ResultCalamviecs, caindex).SLOK ?? 0;

            return slok;
        }

        public object? GetSLNG(object? cdid, int caindex)
        {
            object slng = 0;

            KQGCperCDOANofLOTKHSX targetKQ = DsachKQGCperCDOAN.FirstOrDefault(rs => rs.TargetLOT.NCID.Value?.ToString()?.Trim() == cdid?.ToString()?.Trim()) ?? new();

            slng = Common.GetElementOrNew(targetKQ.ResultCalamviecs, caindex).SLNG ?? 0;

            return slng;
        }
    }
}
