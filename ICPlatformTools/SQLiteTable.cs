using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Threading;
using System.Data;

namespace ICPlatformTools
{
    public class SQLiteVariant
    {
        public enum ValueType { Number, String }

        public ValueType Type { get; set; }

        public object Value { get; set; }

        public string ValueString
        {
            get
            {
                return this.Type == ValueType.Number ? Value.ToString() : string.Format(@"'{0}'", Value);
            }
        }

        public SQLiteVariant(ValueType type, object value)
        {
            this.Type = type;
            this.Value = value;
        }

        public SQLiteVariant(string value)
        {
            this.Type = ValueType.String;
            this.Value = value;
        }

        public SQLiteVariant(object value)
        {
            this.Type = ValueType.Number;
            this.Value = value;
        }
    }

    public class SQLiteFieldVariant
    {
        public enum ValueType { Number, String, RawString }

        public ValueType Type { get; set; }

        public string Field { get; set; }

        public object Value { get; set; }

		// true 使用Sqlite Parameter
        public bool EnableParameter { get; set; }

        public string ValueString
        {
            get
            {
                switch (this.Type)
                {
                case ValueType.Number:
                case ValueType.RawString:
                    return Value.ToString();
                case ValueType.String:
                    return string.Format(@"'{0}'", Value);
                }
                return string.Empty;
            }
        }

        public SQLiteFieldVariant(ValueType type, string field, object value, bool enableParameter = true)
        {
            this.Type = type;
            this.Field = field;
            this.Value = value;
            EnableParameter = enableParameter;	
        }

        public SQLiteFieldVariant(string field, object value, bool enableParameter = true)
        {
            this.Type = ValueType.String;
            this.Field = field;
            this.Value = value;
            EnableParameter = enableParameter;
        }
    }

    public class SelectCondition
    {
        public override string ToString()
        {
            return string.Empty;
        }

        public virtual string ToParameterString()
        {
            return string.Empty;
        }

        public virtual SQLiteParameter[] GetParameters()
        {
            return new SQLiteParameter[0];
        }
    }

    public class SQLiteTable : SQLiteHelper
    {
        protected Thread m_threadBatchInsert;
        protected bool m_batchInsertRunning = false;
        protected Queue<SQLiteVariant[]> m_insertRecordQueue = new Queue<SQLiteVariant[]>();

        public SQLiteConnection Connection { get; set; }

        public string Name { get; protected set; }

        public int InsertQueueSize { get; set; }

        public int BatchInsertCount { get; set; }

        public bool Exists
        {
            get
            {
                return this.GetTableNames().Contains(Name);
            }
        }

        public SQLiteTable(SQLiteConnection connection, string tableName) : base(connection)
        {
            this.Connection = connection;
            this.Name = tableName;
            this.InsertQueueSize = 1024;
            this.BatchInsertCount = 32;
        }

        public string[] GetColumnNames()
        {
            var dt = this.GetColumnStatus(this.Name);
            var columnNames = new string[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; ++i)
            {
                columnNames[i] = dt.Rows[i]["name"] + "";
            }
            return columnNames;
        }

        public virtual bool Create()
        {
            return false;
        }

        public virtual bool StartBatchInsert()
        {
            m_batchInsertRunning = true;
            m_threadBatchInsert = new Thread(this.BatchInsertThreadProc);
            m_threadBatchInsert.Start();
            return true;
        }

        public virtual void StopBatchInsert()
        {
            if (m_threadBatchInsert != null)
            {
                m_batchInsertRunning = false;
                m_threadBatchInsert.Join();
            }
        }

        public virtual bool AddRecord(SQLiteVariant[] fields)
        {
            lock (m_insertRecordQueue)
            {
                if (m_insertRecordQueue.Count < InsertQueueSize)
                {
                    m_insertRecordQueue.Enqueue(fields);
                    return true;
                }
            }
            return false;
        }

        public virtual DataTable Select(SelectCondition condition)
        {
            try
            {
                string sql = string.Format(@"select * from {0} {1}", Name, condition.ToString());
                return this.Select(sql);
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("select db records error", ex);
            }
            return new DataTable();
        }

        public virtual DataTable Select(string[] fields, SelectCondition condition)
        {
            try
            {
                string sql = string.Format(@"select {0} from {1} {2}", string.Join(",", fields), Name, condition.ToParameterString());
                return this.Select(sql, condition.GetParameters());
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("select db records error", ex);
            }
            return new DataTable();
        }

        public virtual bool Insert(SQLiteVariant[] fields)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append(string.Format("insert into {0} values (null", Name));
            for (int i = 0; i < fields.Length; ++i)
            {
                sbSql.AppendFormat(", {0}", "@" + i);
            }
            sbSql.Append(");");
            List<SQLiteParameter> parameters = new List<SQLiteParameter>();
            for (int i = 0; i < fields.Length; ++i)
            {
                SQLiteParameter pa = new SQLiteParameter("@" + i, fields[i].Value);
                parameters.Add(pa);
            }
            try
            {
                this.Execute(sbSql.ToString(), parameters);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("insert db record failed.", ex);
            }
            return false;
        }

        public virtual bool Insert(SQLiteFieldVariant[] fields)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append(string.Format("insert into {0} ", Name));
            sbSql.Append("(");

            for (int i = 0; i < fields.Length; i++)
            {
                if (i == 0)
                {
                    sbSql.AppendFormat("{0}", fields[i].Field);
                }
                else
                {
                    sbSql.AppendFormat(", {0}", fields[i].Field);
                }
            }

            sbSql.Append(") values (");

            List<SQLiteParameter> parameters = new List<SQLiteParameter>();

            for (int i = 0; i < fields.Length; ++i)
            {
                if (i == 0)
                {
                    sbSql.AppendFormat("{0}", "@" + i);
                }
                else
                {
                    sbSql.AppendFormat(", {0}", "@" + i);
                }

                SQLiteParameter pa = new SQLiteParameter("@" + i, fields[i].Value);
                parameters.Add(pa);
            }
            sbSql.Append(");");

            try
            {
                this.Execute(sbSql.ToString(), parameters);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("insert db record failed.", ex);
            }
            return false;
        }

        public virtual bool Update(SQLiteFieldVariant[] variant2s, SelectCondition condition)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.AppendFormat("update {0} set ", Name);
            for (int i = 0; i < variant2s.Length; ++i)
            {
                // NeedFormat 为 false时 不使用 parameter 直接拼接值, 用于解决 sql语句 (列) + 1 在parameter不生效问题 
                if (!variant2s[i].EnableParameter)
                {
                    sbSql.AppendFormat("{0} = {1}", variant2s[i].Field, variant2s[i].ValueString);
                }
                else
                {
                    sbSql.AppendFormat("{0} = {1}", variant2s[i].Field, "@" + i);
                }
                if (i + 1 < variant2s.Length)
                {
                    sbSql.Append(", ");
                }
            }
            sbSql.AppendFormat(" {0}", condition.ToParameterString());
            List<SQLiteParameter> parameters = new List<SQLiteParameter>();
            for (int i = 0; i < variant2s.Length; ++i)
            {
                if (!variant2s[i].EnableParameter)
                {
                    continue;
                }
                SQLiteParameter pa = new SQLiteParameter("@" + i, variant2s[i].Value);
                parameters.Add(pa);
            }
            parameters.AddRange(condition.GetParameters());
            try
            {
                this.Execute(sbSql.ToString(), parameters);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("update db record failed.", ex);
            }
            return false;
        }

        //public virtual bool Update(SQLiteFieldVariant[] variant2s, SelectCondition condition)
        //{
        //    StringBuilder sbSql = new StringBuilder();
        //    sbSql.AppendFormat("update {0} set ", Name);
        //    for (int i = 0; i < variant2s.Length; i++)
        //    {
        //        sbSql.AppendFormat("{0} = {1}", variant2s[i].Field, variant2s[i].ValueString);
        //        if (i + 1 < variant2s.Length)
        //        {
        //            sbSql.Append(", ");
        //        }
        //    }
        //    sbSql.AppendFormat(" {0}", condition.ToString());
        //    try
        //    {
        //        this.Execute(sbSql.ToString());
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.Log.Error("update db record failed.", ex);
        //    }
        //    return false;
        //}

        protected virtual void BatchInsertThreadProc()
        {
            List<SQLiteVariant[]> recordList = new List<SQLiteVariant[]>();

            while (m_batchInsertRunning)
            {
                try
                {
                    recordList.Clear();

                    lock (m_insertRecordQueue)
                    {
                        for (int i = 0; i < this.BatchInsertCount; ++i)
                        {
                            if (m_insertRecordQueue.Count > 0)
                            {
                                recordList.Add(m_insertRecordQueue.Dequeue());
                            }
                        }
                    }

                    if (recordList.Count == 0)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    try
                    {
                        this.BeginTransaction();
                        foreach (var record in recordList)
                        {
                            Insert(record);
                        }
                        this.Commit();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log.Error("Error, try to begin a transaction", ex);
                        try
                        {
                            this.Rollback();
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log.Error(ex.Message, ex);
                }
				
				 Thread.Sleep(10);
            }
        }
    }
}
