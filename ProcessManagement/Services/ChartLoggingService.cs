using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace ProcessManagement.Services
{
    /// <summary>
    /// Service quản lý logs và thông tin của tất cả charts đang chạy trên Blazor Server
    /// </summary>
    public class ChartLoggingService
    {
        // Enable/disable logging
        public bool IsLoggingEnabled { get; set; } = false;
        
        // Dictionary lưu thông tin các chart đang chạy: Key = KHSXID, Value = ChartInfo
        private readonly ConcurrentDictionary<string, ChartInfo> _runningCharts = new();
        
        public class ChartInfo
        {
            public string KHSXID { get; set; } = string.Empty;
            public string MaLSX { get; set; } = string.Empty;
            public bool IsMaster { get; set; } = false;
            public long LastQueryTime { get; set; } = 0; // ms
            public long LastTotalTime { get; set; } = 0; // ms
            public long LastLoopTime { get; set; } = 0; // ms
            public DateTime LastUpdateTime { get; set; } = DateTime.Now;
            public long TotalQueries { get; set; } = 0;
            public bool IsRunning { get; set; } = false;
        }
        
        /// <summary>
        /// Register chart đang chạy
        /// </summary>
        public void RegisterChart(string khsxId, string maLSX, bool isMaster)
        {
            var info = new ChartInfo
            {
                KHSXID = khsxId,
                MaLSX = maLSX,
                IsMaster = isMaster,
                IsRunning = true,
                LastUpdateTime = DateTime.Now
            };
            
            _runningCharts.AddOrUpdate(khsxId, info, (key, oldValue) => 
            {
                oldValue.IsMaster = isMaster;
                oldValue.IsRunning = true;
                oldValue.LastUpdateTime = DateTime.Now;
                return oldValue;
            });
            
            // Log summary khi chart start
            LogChartSummary("START", khsxId, maLSX, isMaster);
        }
        
        /// <summary>
        /// Update thông tin chart (query time, loop time, etc.)
        /// </summary>
        public void UpdateChartInfo(string khsxId, long queryTime, long totalTime, long loopTime)
        {
            if (_runningCharts.TryGetValue(khsxId, out var info))
            {
                info.LastQueryTime = queryTime;
                info.LastTotalTime = totalTime;
                info.LastLoopTime = loopTime;
                info.LastUpdateTime = DateTime.Now;
                info.TotalQueries++;
            }
        }
        
        /// <summary>
        /// Unregister chart khi stop hoặc dispose
        /// </summary>
        public void UnregisterChart(string khsxId)
        {
            if (_runningCharts.TryRemove(khsxId, out var info))
            {
                // Log summary khi chart stop
                LogChartSummary("STOP", khsxId, info.MaLSX, info.IsMaster);
            }
        }
        
        /// <summary>
        /// Lấy danh sách tất cả charts đang chạy
        /// </summary>
        public List<ChartInfo> GetRunningCharts()
        {
            // Remove stale charts (không update > 10 giây)
            var now = DateTime.Now;
            var staleKeys = _runningCharts
                .Where(kvp => (now - kvp.Value.LastUpdateTime).TotalSeconds > 10)
                .Select(kvp => kvp.Key)
                .ToList();
            
            foreach (var key in staleKeys)
            {
                _runningCharts.TryRemove(key, out _);
            }
            
            return _runningCharts.Values
                .Where(c => c.IsRunning)
                .OrderBy(c => c.KHSXID)
                .ToList();
        }
        
        /// <summary>
        /// Get summary: tổng số charts, master charts, follower charts
        /// </summary>
        public (int Total, int Masters, int Followers) GetSummary()
        {
            var charts = GetRunningCharts();
            return (
                charts.Count,
                charts.Count(c => c.IsMaster),
                charts.Count(c => !c.IsMaster)
            );
        }
        
        /// <summary>
        /// Log summary khi chart start/stop - hiển thị số lượng charts (Total/Master/Follower) và mã KHSX đang hoạt động
        /// </summary>
        private void LogChartSummary(string action, string khsxId, string maLSX, bool isMaster)
        {
            var charts = GetRunningCharts();
            var masters = charts.Count(c => c.IsMaster);
            var followers = charts.Count(c => !c.IsMaster);
            var maLSXList = charts
                .OrderBy(c => c.MaLSX)
                .Select(c => c.MaLSX)
                .Distinct()
                .ToList();
            
            var maLSXListStr = maLSXList.Count > 0 
                ? string.Join(", ", maLSXList) 
                : "None";
            
            System.Console.WriteLine(
                $"[CHART {action}] Charts: {charts.Count} (Master: {masters} / Follower: {followers}) | Active KHSX: [{maLSXListStr}]"
            );
        }
    }
}

