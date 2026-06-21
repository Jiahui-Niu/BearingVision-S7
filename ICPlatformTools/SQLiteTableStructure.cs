using System;
using System.Collections.Generic;
using System.Text;

namespace ICPlatformTools
{
    public class SQLiteTableStructure
    {
        public string TableName = "";
        public SQLiteColumnList Columns = new SQLiteColumnList();

        public SQLiteTableStructure()
        { }

        public SQLiteTableStructure(string name)
        {
            TableName = name;
        }
    }
}