using System.Collections.Generic;

namespace DDBook
{
    class PageInfo
    {
        public PageInfo()
        {
            Blocks = new List<DdBlock>();
        }

        /// <summary>
        /// DPI=96的宽度
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// DPI=96的高度
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 点读区
        /// </summary>
        public List<DdBlock> Blocks { get; set; }
    }
}