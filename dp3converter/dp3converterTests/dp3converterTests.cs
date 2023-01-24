using Microsoft.VisualStudio.TestTools.UnitTesting;
using dp3converter;
using System;
using System.Collections.Generic;
using System.Text;

namespace dp3converter.Tests
{
    [TestClass()]
    public class dp3converterTests
    {
        [TestMethod()]
        public void DoConvertTest()
        {
            var test = dp3converter.DoConvert("sample.dp3");
            CsvConverter.Save("test.csv", test);
        }
    }
}