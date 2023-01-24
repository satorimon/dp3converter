using System;
using System.Collections.Generic;
using System.Text;

namespace dp3converter
{
    public class GpsRecord
    {
        /// <summary>
        /// 日時
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// 速度[km/h]
        /// </summary>
        public double Speed { get; set; }
        /// <summary>
        /// 経度
        /// </summary>
        public double Longitude { get; set; }
        /// <summary>
        /// 緯度
        /// </summary>
        public double Latitude { get; set; }

    }
}
