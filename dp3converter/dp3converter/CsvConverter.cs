using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Numpy;

namespace dp3converter
{
    public class CsvConverter
    {
        private static DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="longitude1"></param>
        /// <param name="latitude1"></param>
        /// <param name="longitude2"></param>
        /// <param name="latitude2"></param>
        /// <returns>Item1:Distance, Item2:Bearing</returns>
        private static Tuple<double,double> CalcDistanceAndBearing(double longitude1, double latitude1, double longitude2, double latitude2)
        {
            var r = 6378.137;//地球半径[km]
            var deltaX = longitude2 - longitude1;
            var d = r * Math.Acos(Math.Sin(ToRadian(latitude1)) * Math.Sin(ToRadian(latitude2)) + Math.Cos(ToRadian(latitude1)) * Math.Cos(ToRadian(latitude2)) * Math.Cos(ToRadian(deltaX)));
            var theta = ToAngle(Math.Atan2(Math.Sin(ToRadian(deltaX)), Math.Cos(ToRadian(latitude1)) * Math.Tan(ToRadian(latitude2)) - Math.Sin(ToRadian(latitude1)) * Math.Cos(ToRadian(deltaX))));

            theta = theta < 0 ? 360 + theta : theta;
            
            return new Tuple<double, double>(d, theta);
        }

        private static double CalcSpeed(double speed)
        {
            return speed * 1000 / 3600;
        }
        public static double ToRadian(double angle)
        {
            return (double)(angle * Math.PI / 180);
        }
        public static double ToAngle(double radian)
        {
            return (double)(radian * 180 / Math.PI);
        }

        public static double[] CreateAArray(double n)
        {
            var A0 = 1 + Math.Pow(n, 2) / 4.0 + Math.Pow(n, 4) / 64.0;
            var A1 = -(3.0 / 2) * (n - Math.Pow(n, 3) / 8.0 - Math.Pow(n, 5) / 64.0);
            var A2 = (15.0 / 16) * (Math.Pow(n, 2) - Math.Pow(n, 4) / 4.0);
            var A3 = -(35.0 / 48) * (Math.Pow(n, 3) - (5.0 / 16) * Math.Pow(n, 5));
            var A4 = (315.0 / 512) * Math.Pow(n, 4);
            var A5 = -(693.0/ 1280) * Math.Pow(n, 5);
            return new[] { A0, A1, A2, A3, A4, A5 };
        }

        public static double[] CreateAlphaArray(double n)
        {
            var a0 = double.NaN; //dummy
            var a1 = (1.0 / 2) * n - (2.0 / 3) * Math.Pow(n, 2) + (5.0 / 16) * Math.Pow(n, 3) + (41.0 / 180) * Math.Pow(n, 4) - (127.0 / 288) * Math.Pow(n, 5);
            var a2 = (13.0 / 48) * Math.Pow(n, 2) - (3.0 / 5) * Math.Pow(n, 3) + (557.0 / 1440) * Math.Pow(n, 4) + (281.0 / 630) * Math.Pow(n, 5);
            var a3 = (61.0 / 240) * Math.Pow(n, 3) - (103.0 / 140) * Math.Pow(n, 4) + (15061.0 / 26880) * Math.Pow(n, 5);
            var a4 = (49561.0 / 161280) * Math.Pow(n, 4) - (179.0 / 168) * Math.Pow(n, 5);
            var a5 = (34729.0 / 80640) * Math.Pow(n, 5);
            return new[] { a0, a1, a2, a3, a4, a5 };
        }

        public static Tuple<double, double> CalcXY(double longitude1, double latitude1, double longitude2, double latitude2)
        {
            //緯度経度・平面直角座標系原点をラジアンに直す
            var phi_rad = np.deg2rad(np.array(latitude2));
            var lambda_rad = np.deg2rad(np.array(longitude2));
            var phi0_rad = np.deg2rad(np.array(latitude1));
            var lambda0_rad = np.deg2rad(np.array(longitude1));

            //定数 (a, F: 世界測地系-測地基準系1980（GRS80）楕円体)
            var m0 = 0.9999;
            var a = 6378137.0;
            var F = 298.257222101;
            // (1) n, A_i, alpha_iの計算
            var n = 1.0 / (2 * F - 1);
            var A_array = CreateAArray(n);
            var alpha_array = CreateAlphaArray(n);

            // (2), S, Aの計算
            var A_ = ((m0 * a) / (1.0 + n)) * A_array[0]; // [m]
            var S_ = ((m0 * a) / (1.0 + n)) * (A_array[0] * phi0_rad + np.dot(A_array.Skip(1).ToArray(), np.sin(2 * phi0_rad * np.arange(1, 6, 1, null))));// # [m]

            //# (3) lambda_c, lambda_sの計算
            var lambda_c = np.cos(lambda_rad - lambda0_rad);
            var lambda_s = np.sin(lambda_rad - lambda0_rad);

            //# (4) t, t_の計算
            var t = np.sinh(np.arctanh(np.sin(phi_rad)) - ((2 * np.sqrt(np.array(n))) / (1 + n)) * np.arctanh(((2 * np.sqrt(np.array(n))) / (1 + n)) * np.sin(phi_rad)));
            var t_ = np.sqrt(1 + t * t);

            //# (5) xi', eta'の計算
            var xi2 = np.arctan(t / lambda_c); // # [rad]
            var eta2 = np.arctanh(lambda_s / t_);

            //# (6) x, yの計算
            var x = A_ * (xi2 + np.sum(np.multiply(alpha_array.Skip(1).ToArray(),
                                               np.multiply(np.sin(2 * xi2 * np.arange(1, 6, 1)),
                                                           np.cosh(2 * eta2 * np.arange(1, 6, 1)))))) - S_;// # [m]
            var y = A_ * (eta2 + np.sum(np.multiply(alpha_array.Skip(1).ToArray(),
                                                np.multiply(np.cos(2 * xi2 * np.arange(1, 6, 1)),
                                                            np.sinh(2 * eta2 * np.arange(1, 6, 1))))));// # [m]
            //# return
            return new Tuple<double, double>((double)x[0], (double)y[0]); //# [m]
        }
         

        public static void Save(string path, GpsLogHolder data)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            using (var writer = new StreamWriter(fs))
            {
                writer.WriteLine("This file is created using dp3converter.");
                writer.WriteLine("Format,2,Format 1 = static columns,Format 2 = new format with dynamic columns; only channel header string is fixed, number of columns and channels' column numbers are variable.,");
                writer.Write("Session title,");
                writer.WriteLine("title");
                writer.Write("Session type,");
                writer.WriteLine("Lap timing");
                writer.Write("Track name,");
                writer.WriteLine("Track name");
                writer.Write("Driver name,");
                writer.WriteLine("");
                writer.Write("Export scope,");
                writer.WriteLine("Whole session");
                writer.Write("Created,");
                var startTime = data.GpsRecords.First().Date;
                writer.Write(startTime.ToString("dd/MM/yyyy") + ",");
                writer.WriteLine(startTime.ToString("hh:mm:ss"));
                writer.Write("Note,");
                writer.WriteLine("");
                writer.WriteLine("");


                //Race Chrono CSV Format
                writer.WriteLine(
                    "Time (s)," +
                    "Session fragment #," +
                    "Lap #," +
                    "Trap name," +
                    "X-position (m)," +
                    "Y-position (m)," +
                    "Distance (m)," +
                    "Speed (m/s)," +
                    "Altitude (m)," +
                    "Bearing (deg)," +
                    "Lateral acceleration (G)," +
                    "Longitudinal acceleration (G)," +
                    "Device update rate (Hz)," +
                    "Elapsed time (s)," +
                    "Lean angle (deg)," +
                    "Combined acceleration (G)," +
                    "Latitude (deg)," +
                    "Longitude (deg)," +
                    "Satellites (sats)," +
                    "Fix type," +
                    "Accuracy (m)," +
                    "X acceleration (G) *acc," +
                    "Y acceleration (G) *acc," +
                    "Z acceleration (G) *acc," +
                    "Device update rate (Hz) *acc," +
                    "X rate of rotation (deg/s) *gyro," +
                    "Y rate of rotation (deg/s) *gyro," +
                    "Z rate of rotation (deg/s) *gyro," +
                    "Device update rate (Hz) *gyro," +
                    "Device update rate (Hz) *magn," +
                    "X magnetic field (uT) *magn," +
                    "Y magnetic field (uT) *magn," +
                    "Z magnetic field (uT) *magn" 
                    );
                

                var preLongitude = data.GpsRecords.First().Longitude;
                var preLatitude = data.GpsRecords.First().Latitude;
                var preTime = startTime.AddMilliseconds(-200);
                var preSpeed = CalcSpeed(data.GpsRecords.First().Speed);
                var preTheta = 0.0;

                foreach (var rec in data.GpsRecords)
                {
                    var utc = rec.Date.ToUniversalTime();

                    // UNIXエポックからの経過時間を取得
                    TimeSpan elapsedTime = utc - UNIX_EPOCH;


                    writer.Write((elapsedTime.TotalMilliseconds /1000.0).ToString() + ",");
                    writer.Write("0,");
                    writer.Write(",");
                    writer.Write(",");
                    writer.Write(",");
                    writer.Write(",");
                    writer.Write(",");
                    var nowSpeed = CalcSpeed(rec.Speed);
                    writer.Write(nowSpeed.ToString("#.000000") + ",");//m/s
                    writer.Write(",");
                    var (d, theta) = CalcDistanceAndBearing(preLongitude, preLatitude, rec.Longitude, rec.Latitude);


                    writer.Write(theta.ToString("#.000") + ",");//Bearing
                    var gLongitudinal = (nowSpeed - preSpeed) / ((utc - preTime).TotalMilliseconds / 1000.0) / 9.80655;
                    //var l12a = +()
                    //var gLateral = 

                    writer.Write( ",");//Lateral acceleration 横
                    writer.Write(gLongitudinal.ToString("#.000") + ",");//Longitudinal acceleration 縦



                    writer.Write(",");
                    writer.Write(((rec.Date - startTime).TotalMilliseconds / 1000.0).ToString() + ",");
                    writer.Write(",");
                    writer.Write(",");//Combined G
                    writer.Write(rec.Latitude.ToString("#.000000") + ",");
                    writer.Write(rec.Longitude.ToString("#.000000") + ",");
                    writer.Write(",");
                    writer.Write("1,");//20
                    writer.Write(",");
                    writer.Write(",");
                    writer.Write(",");
                    writer.Write(",");
                    writer.Write(",");
                    writer.Write(",");
                    writer.Write(",");
                    writer.Write(",");
                    writer.Write(",");
                    writer.Write(",");
                    writer.Write(",");
                    writer.Write(",");
                    writer.WriteLine(",");
                    preLongitude = rec.Longitude;
                    preLatitude = rec.Latitude;
                    preTime = utc;
                    preSpeed = nowSpeed;
                    preTheta = theta;

                }
                writer.Flush();
                writer.Close();

                fs.Close();
            }

        }
    }
}
