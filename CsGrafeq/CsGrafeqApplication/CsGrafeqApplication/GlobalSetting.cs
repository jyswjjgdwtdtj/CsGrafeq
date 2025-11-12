using System;
using System.Collections.Generic;
using System.Text;

namespace CsGrafeqApplication
{
    public static class GlobalSetting
    {
        /// <summary>
        /// 标识鼠标指针在触摸屏上可被视为“触摸”操作的范围（以像素为单位）。
        /// </summary>
        public static readonly double PointerTouchRange = OS.GetOSType() == OSType.Android ? 15 : 5;

    }
}
