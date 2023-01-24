using System;
using System.Collections.Generic;
using System.Text;

namespace dp3converter
{
    public class GpsLogHolder
    {
        public string Header { get; set; }
        public IEnumerable<GpsRecord> GpsRecords { get; set; }

    }
}
