using System;
using System.Collections.Generic;

namespace DataPreview.Models
{
    public class Table
    {
        public string Name { get; set; }

        public string Schema { get; set; }

        public List<Column> Columns { get; } = new List<Column>();
    }
}