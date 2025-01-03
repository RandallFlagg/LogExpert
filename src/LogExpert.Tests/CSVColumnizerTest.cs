using System;
using System.IO;
using LogExpert.Classes.Log;
using LogExpert.Entities;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace LogExpert.Tests
{
    [TestFixture]
    public class CSVColumnizerTest
    {
        [TestCase(@".\TestData\organizations-10000.csv", new[] {"Index","Organization Id","Name","Website","Country","Description","Founded","Industry","Number of employees"})]
        [TestCase(@".\TestData\organizations-1000.csv", new[] {"Index","Organization Id","Name","Website","Country","Description","Founded","Industry","Number of employees"})]
        [TestCase(@".\TestData\people-10000.csv", new[] {"Index","User Id","First Name","Last Name","Sex","Email","Phone","Date of birth","Job Title"})]
        public void Instantiat_CSVFile_BuildCorrectColumnizer(string filename, string[] expectedHeaders)
        {
            Console.WriteLine("A");
            CsvColumnizer.CsvColumnizer csvColumnizer = new CsvColumnizer.CsvColumnizer();
            Console.WriteLine("B");
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            Console.WriteLine("C");
            LogfileReader reader = new LogfileReader(path, new EncodingOptions(), true, 40, 50, new MultiFileOptions());
            Console.WriteLine("D");
            reader.ReadFiles();
            Console.WriteLine("E");
            ILogLine line = reader.GetLogLineWithWait(0).Result;
            Console.WriteLine("F");
            IColumnizedLogLine logline = new ColumnizedLogLine();
            Console.WriteLine("G");
            if (line != null)
            {
                Console.WriteLine("H");
                logline = csvColumnizer.SplitLine(null, line);
            }
            Console.WriteLine("I");
            string expectedResult = string.Join(",", expectedHeaders);
            Console.WriteLine("J");
            ClassicAssert.AreEqual(expectedResult, logline.LogLine.FullLine);
            Console.WriteLine("K");
        }
    }
}
