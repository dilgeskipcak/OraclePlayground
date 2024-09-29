using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OraclePlayground
{
    internal static class Loader
    {
        public static void Load(DataTable data) {
            data.TableName = "Address";
            string connString = @"DATA SOURCE=localhost:1521/XEPDB1;USER ID=system; password=sifre123;";
            // Connection string format: User Id=[username];Password=[password];Data Source=[hostname]:[port]/[DB service name];
            OracleConnection con = new OracleConnection(connString);
            con.Open();
            OracleCommand cmd = con.CreateCommand();
            cmd.CommandText = @"ALTER SESSION 
                                SET NLS_TIMESTAMP_FORMAT = 'DD-MM-YYYY HH24:MI:SS.FF'
                                NLS_DATE_FORMAT = 'yyyy-MM-dd HH24:MI:SS'
                                ";
            cmd.ExecuteNonQuery();
            string insertQuery = GenerateQuery(QueryType.Merge, data);
            cmd.Parameters.Add(":input", OracleDbType.Clob, ConvertToXml(data), ParameterDirection.Input);
            cmd.CommandText = insertQuery;
            cmd.Transaction = con.BeginTransaction();
            try
            {

                cmd.ExecuteNonQuery();
                cmd.Transaction.Commit();
            }
            catch (Exception)
            {
                cmd.Transaction.Rollback();
                throw;
            }

        }

        private static string ConvertToXml(DataTable data)
        {
            TextWriter writer = new StringWriter();
            data.WriteXml(writer);
            data.Dispose();
            GC.Collect();
            return writer.ToString().Replace("'", "''") ;
        }

        private static string GenerateQuery(QueryType queryType, DataTable data)
        {
            List<string> columns = new List<string>();
            List<string> pkColumns = new List<string>() { "ADDRESSLINE1", "CITY", "MODIFIEDDATE" };
            string schemaName = "AT", tableName = data.TableName;
            foreach (DataColumn item in data.Columns)
                columns.Add(item.ColumnName);
            string xmlToColumns = string.Join(',', columns.Select(name => $"EXTRACTVALUE(VALUE(a), '/{tableName}/{name}') AS {name} \n"));
            string targetSourceAssignToColumns = string.Join(',', columns.Where(c => !pkColumns.Contains(c.ToUpper())).Select(name => $"targetTable.{name} = sourceTable.{name} \n"));
            string sourceAssignToColumns = string.Join(',', columns.Select(name => $"sourceTable.{name}"));
            string mergeCondition = string.Join(" AND ", pkColumns.Select(name => $"targetTable.{name} = sourceTable.{name}"));
            string result = string.Empty;
            if (queryType == QueryType.Merge)
            {
                result = @$"
                            
                                MERGE INTO {schemaName}.{tableName} targetTable
                                USING (SELECT {xmlToColumns}
                                       FROM TABLE(XMLSEQUENCE(EXTRACT(v_xml, '/DocumentElement/{tableName}'))) a) sourceTable
                                ON ({mergeCondition})
                                WHEN MATCHED THEN
                                    UPDATE SET {targetSourceAssignToColumns}
                                WHEN NOT MATCHED THEN
                                    INSERT ( {string.Join(',', columns)} )
                                    VALUES ( {sourceAssignToColumns});";

            }
            else if ( queryType == QueryType.Insert)
            {
                result = @$"
                         
                         FOR sourceTable IN (SELECT {xmlToColumns}
                                       FROM TABLE(XMLSEQUENCE(EXTRACT(v_xml, '/DocumentElement/{tableName}'))) a) 
                            LOOP
                                INSERT INTO {schemaName}.{tableName} ({string.Join(',', columns)})
                                VALUES ({sourceAssignToColumns});
                            END LOOP;
                         ";

            }
            return $@"
                            DECLARE
                            v_xml XMLTYPE;
                            BEGIN
                            v_xml  := XMLTYPE(:input);
                            {result}
                            END;";
            
        }

        private enum QueryType
        {
            Insert,
            Merge
        }
    }
}
