using LogExpert.Core.Classes.Columnizer;
using LogExpert.Core.Classes.Log;
using LogExpert.Core.Entities;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.IO;

namespace LogExpert.Tests;

[TestFixture]
public class SquareBracketColumnizerTest
{
    [TestCase(@".\TestData\SquareBracketColumnizerTest_01.txt", 5)]
    [TestCase(@".\TestData\SquareBracketColumnizerTest_02.txt", 5)]
    [TestCase(@".\TestData\SquareBracketColumnizerTest_03.txt", 6)]
    [TestCase(@".\TestData\SquareBracketColumnizerTest_05.txt", 3)]
    public void GetPriority_HappyFile_ColumnCountMatches(string fileName, int count)
    {
        SquareBracketColumnizer squareBracketColumnizer = new();
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

        LogfileReader logFileReader = new(path, new EncodingOptions(), true, 40, 50, new MultiFileOptions(), false, PluginRegistry.PluginRegistry.Instance);
        //logFileReader.StartMonitoring();
        logFileReader.ReadFiles();
        List<ILogLine> loglines =
        [
            // Sampling a few lines to select the correct columnizer
            logFileReader.GetLogLine(0),
            logFileReader.GetLogLine(1),
            logFileReader.GetLogLine(2),
            logFileReader.GetLogLine(3),
            logFileReader.GetLogLine(4),
            logFileReader.GetLogLine(5),
            logFileReader.GetLogLine(25),
            logFileReader.GetLogLine(100),
            logFileReader.GetLogLine(200),
            logFileReader.GetLogLine(400)
        ];

        squareBracketColumnizer.GetPriority(path, loglines);
        Assert.That(count, Is.EqualTo(squareBracketColumnizer.GetColumnCount()));
    }

}