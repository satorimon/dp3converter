using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace dp3converter
{
    public class CsvConverter
    {
        private static DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0);
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
                    writer.Write((rec.Speed * 1000 / 3600).ToString("#.000000") + ",");//m/s
                    writer.Write(",");
                    writer.Write(",");
                    writer.Write(",");
                    writer.Write(",");
                    writer.Write(",");
                    writer.Write(((rec.Date - startTime).TotalMilliseconds / 1000.0).ToString() + ",");
                    writer.Write(",");
                    writer.Write(",");
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

                }
                writer.Flush();
                writer.Close();

                fs.Close();
            }

        }
    }
}
