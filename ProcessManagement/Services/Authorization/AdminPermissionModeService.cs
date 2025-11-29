using System;

namespace ProcessManagement.Services.Authorization
{
    /// <summary>
    /// Lưu trạng thái "chế độ điều chỉnh phân quyền" cho Admin.
    /// Khi bật, các nút AuthorizedButton sẽ hiển thị icon/công cụ gán role cho button.
    /// </summary>
    public class AdminPermissionModeService
    {
        private bool _isEnabled;

        /// <summary>
        /// Cho biết chế độ điều chỉnh phân quyền đang bật hay tắt.
        /// </summary>
        public bool IsEnabled => _isEnabled;

        /// <summary>
        /// Sự kiện bắn ra khi trạng thái thay đổi để các component (AuthorizedButton, v.v.) refresh UI.
        /// </summary>
        public event Action? OnModeChanged;

        public void SetEnabled(bool enabled)
        {
            if (_isEnabled == enabled)
            {
                return;
            }

            _isEnabled = enabled;
            OnModeChanged?.Invoke();
        }

        public void Toggle()
        {
            SetEnabled(!_isEnabled);
        }
    }
}


