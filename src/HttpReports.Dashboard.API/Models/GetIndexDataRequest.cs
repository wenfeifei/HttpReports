﻿namespace HttpReports.Dashboard.Models
{
    public class GetIndexDataRequest
    {
        public string Node { get; set; }

        public string Start { get; set; }

        public string End { get; set; }

        public string Day { get; set; }

        public string Month { get; set; }

        public string Year { get; set; }

        public int TOP { get; set; }

        /// <summary>
        /// 是否倒序
        /// </summary>
        public bool IsDesc { get; set; }
    }
}