using System;
using System.Collections.Generic;

namespace LogExpert.Config
{
    [Serializable]
    public class RegexHistory
    {
        public List<string> ExpressionHistoryList { get; set; } = [];

        public List<string> TesttextHistoryList { get; set; } = [];
    }
}