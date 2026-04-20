using System;
using System.Windows.Forms;

namespace AdminUI
{
    /// <summary>
    /// 极简防抖（无Timer、无跨线程，绝对不闪退）
    /// </summary>
    public static class GlobalDebounce
    {
        // 全局点击锁（500ms内只响应1次）
        private static DateTime _lastClickTime = DateTime.MinValue;
        private const int _debounceMs = 500;

        /// <summary>
        /// 防抖校验（核心：无线程、无Timer，100%稳定）
        /// </summary>
        public static bool Check()
        {
            var now = DateTime.Now;
            if ((now - _lastClickTime).TotalMilliseconds < _debounceMs)
                return false;

            _lastClickTime = now;
            return true;
        }
    }
}