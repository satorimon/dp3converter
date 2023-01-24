using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace dp3converter
{
    public static class dp3converter
    {
        public static GpsLogHolder DoConvert(string path, DateTime? recordingDay = null)
        {
            if(!recordingDay.HasValue )
            {
                recordingDay = DateTime.Today;
            }

            var res = new GpsLogHolder();
            


            using (var fs = new FileStream(path, FileMode.Open))
            {
                //Header read
                var headerBytes = new byte[0x100];

                fs.Read(headerBytes, 0, 0x100);

                res.Header = headerBytes.ToString();

                var today = new DateTime(recordingDay.Value.Year, recordingDay.Value.Month, recordingDay.Value.Day);

                var records = new List<GpsRecord>();
                var data4bytes = new byte[4];
                var data2bytes = new byte[2];
                while (true)
                {

                    var record = new GpsRecord();

                    //convert time
                    var readsize = fs.Read(data4bytes, 0, 4);
                    if(readsize < 1)
                    {
                        break;
                    }
                    var time_str_source = BitConverter.ToInt32(data4bytes[0..4].Reverse().ToArray());
                    var timestr = time_str_source.ToString("0000000");
                    record.Date = today.AddHours(int.Parse(timestr.Substring(0, 2)))
                    .AddMinutes(int.Parse(timestr.Substring(2, 2)))
                    .AddSeconds(int.Parse(timestr.Substring(4, 2)))
                    .AddMilliseconds(int.Parse(timestr.Substring(6)) * 100).ToUniversalTime();

                    //convert longitude
                    fs.Read(data4bytes, 0, 4);
                    record.Longitude = (double)BitConverter.ToInt32(data4bytes[0..4].Reverse().ToArray()) / 460800;

                    //convert latitude
                    fs.Read(data4bytes, 0, 4);
                    record.Latitude = (double)BitConverter.ToInt32(data4bytes[0..4].Reverse().ToArray()) / 460800;

                    //convert speed
                    fs.Read(data2bytes, 0, 2);
                    record.Speed = (double)BitConverter.ToInt16(data2bytes[0..2].Reverse().ToArray()) / 10;

                    //ignore
                    fs.Read(data2bytes, 0, 2);

                    records.Add(record);
                }

                res.GpsRecords = records;

                fs.Close();
                return res;

            }

        }



    }
}
