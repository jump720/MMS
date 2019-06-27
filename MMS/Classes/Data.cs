using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MMS.Classes
{
    public static class Data
    {
        private static List<string> GetParametersNames(this DbContext dbContext, string storedProcedureSchema, string storedProcedureName)
        {
            using (var sda = new SqlDataAdapter(string.Format("SELECT PARAMETER_NAME FROM INFORMATION_SCHEMA.PARAMETERS WHERE SPECIFIC_NAME='{0}' AND SPECIFIC_SCHEMA = '{1}' ORDER BY ORDINAL_POSITION ASC", storedProcedureName, storedProcedureSchema), dbContext.Database.Connection.ConnectionString))
            {
                DataTable dt = new DataTable();
                List<string> parametros = new List<string>();
                sda.Fill(dt);

                foreach (DataRow lo_fila in dt.Rows)
                    parametros.Add(lo_fila["PARAMETER_NAME"].ToString());

                return parametros;
            }
        }

        private static dynamic GetDataRow(DbDataReader dataReader)
        {
            var dataRow = new ExpandoObject() as IDictionary<string, object>;

            for (var fieldCount = 0; fieldCount < dataReader.FieldCount; fieldCount++)
                dataRow.Add(dataReader.GetName(fieldCount), dataReader[fieldCount]);

            return dataRow;
        }

        public static IEnumerable<dynamic> FillDynamicCollectionSql(this DbContext dbContext, string sql)
        {
            using (var cmd = dbContext.Database.Connection.CreateCommand())
            {
                cmd.CommandText = sql;
                if (cmd.Connection.State != ConnectionState.Open)
                    cmd.Connection.Open();

                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        var dataRow = GetDataRow(dataReader);
                        yield return dataRow;
                    }
                }
            }
        }

        public static object FillScalarSql(this DbContext dbContext, string sql)
        {
            using (var sda = new SqlDataAdapter(sql, dbContext.Database.Connection.ConnectionString))
            {
                DataTable dt = new DataTable();
                sda.Fill(dt);
                return dt.Rows[0][0];
            }
        }

        public static object FillScalarSP(this DbContext dbContext, string storedProcedureSchema, string storedProcedureName, params object[] paramValues)
        {
            using (var sda = new SqlDataAdapter(storedProcedureName, dbContext.Database.Connection.ConnectionString))
            {
                DataTable dt = new DataTable();
                List<string> paramNames = GetParametersNames(dbContext, storedProcedureSchema, storedProcedureName);
                List<SqlParameter> sqlParameters = new List<SqlParameter>();

                for (int i = 0; i < paramValues.Length; i++)
                    sqlParameters.Add(new SqlParameter(paramNames[i], paramValues[i]));

                sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                sda.SelectCommand.Parameters.AddRange(sqlParameters.ToArray());
                sda.Fill(dt);
                return dt.Rows[0][0];
            }
        }

        public static DataTable FillDataTableSql(this DbContext dbContext, string sql)
        {
            using (var sda = new SqlDataAdapter(sql, dbContext.Database.Connection.ConnectionString))
            {
                DataTable dt = new DataTable();
                sda.Fill(dt);
                return dt;
            }
        }

        public static DataTable FillDataTableSP(this DbContext dbContext, string storedProcedureSchema, string storedProcedureName, params object[] paramValues)
        {
            using (var sda = new SqlDataAdapter(storedProcedureName, dbContext.Database.Connection.ConnectionString))
            {
                DataTable dt = new DataTable();
                List<string> paramNames = GetParametersNames(dbContext, storedProcedureSchema, storedProcedureName);
                List<SqlParameter> sqlParameters = new List<SqlParameter>();

                for (int i = 0; i < paramValues.Length; i++)
                    sqlParameters.Add(new SqlParameter(paramNames[i], paramValues[i]));

                sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                sda.SelectCommand.Parameters.AddRange(sqlParameters.ToArray());
                sda.Fill(dt);
                return dt;
            }
        }

        public static DataSet FillDataSetSql(this DbContext dbContext, string sql)
        {
            using (var sda = new SqlDataAdapter(sql, dbContext.Database.Connection.ConnectionString))
            {
                DataSet ds = new DataSet();
                sda.Fill(ds);
                return ds;
            }
        }

        public static DataSet FillDataSetSP(this DbContext dbContext, string storedProcedureSchema, string storedProcedureName, params object[] paramValues)
        {
            using (var sda = new SqlDataAdapter(storedProcedureName, dbContext.Database.Connection.ConnectionString))
            {
                DataSet ds = new DataSet();
                List<string> paramNames = GetParametersNames(dbContext, storedProcedureSchema, storedProcedureName);
                List<SqlParameter> sqlParameters = new List<SqlParameter>();

                for (int i = 0; i < paramValues.Length; i++)
                    sqlParameters.Add(new SqlParameter(paramNames[i], paramValues[i]));

                sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                sda.SelectCommand.Parameters.AddRange(sqlParameters.ToArray());
                sda.Fill(ds);
                return ds;
            }
        }

        public static List<T> FillListSP<T>(this DbContext dbContext, string storedProcedureSchema, string storedProcedureName, params object[] paramValues)
        {
            List<string> paramNames = GetParametersNames(dbContext, storedProcedureSchema, storedProcedureName);
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            string sql = storedProcedureSchema + "." + storedProcedureName;

            for (int i = 0; i < paramValues.Length; i++)
            {
                if (paramValues[i] == null)
                    continue;

                sql += " " + paramNames[i] + ",";
                sqlParameters.Add(new SqlParameter(paramNames[i], paramValues[i]));
            }

            if (sql.EndsWith(","))
                sql = sql.Remove(sql.Length - 1);

            return dbContext.Database.SqlQuery<T>(sql, sqlParameters.ToArray()).ToList();
        }

        public static async Task<List<T>> FillListSPAsync<T>(this DbContext dbContext, string storedProcedureSchema, string storedProcedureName, params object[] paramValues)
        {
            List<string> paramNames = GetParametersNames(dbContext, storedProcedureSchema, storedProcedureName);
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            string sql = storedProcedureSchema + "." + storedProcedureName;

            for (int i = 0; i < paramValues.Length; i++)
            {
                if (paramValues[i] == null)
                    continue;

                sql += " " + paramNames[i] + ",";
                sqlParameters.Add(new SqlParameter(paramNames[i], paramValues[i]));
            }

            if (sql.EndsWith(","))
                sql = sql.Remove(sql.Length - 1);

            return await dbContext.Database.SqlQuery<T>(sql, sqlParameters.ToArray()).ToListAsync();
        }
    }
}