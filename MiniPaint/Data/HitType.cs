using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPaint.Data
{
    #region 히트 타입 - HitType
    /// <summary>
    /// 히트 타입
    /// </summary>
    public enum HitType
    {
        /// <summary>
        /// 해당 무
        /// </summary>
        NONE,

        /// <summary>
        /// 몸통
        /// </summary>
        BODY,

        /// <summary>
        /// 좌상단
        /// </summary>
        UPPER_LEFT,

        /// <summary>
        /// 우상단
        /// </summary>
        UPPER_RIGHT,

        /// <summary>
        /// 좌하단
        /// </summary>
        LOWER_RIGHT,

        /// <summary>
        /// 우하단
        /// </summary>
        LOWER_LEFT,

        /// <summary>
        /// 왼쪽
        /// </summary>
        LEFT,

        /// <summary>
        /// 오른쪽
        /// </summary>
        RIGHT,

        /// <summary>
        /// 위쪽
        /// </summary>
        TOP,

        /// <summary>
        /// 아래쪽
        /// </summary>
        BOTTOM
    }
    #endregion
}

