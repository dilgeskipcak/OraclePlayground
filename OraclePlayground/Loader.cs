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
            string input = ConvertToXml(data);
            string insertQuery = GenerateQuery(QueryType.Insert, data);
            cmd.CommandText = insertQuery;
            cmd.Parameters.Add(":input",OracleDbType.Varchar2, input,ParameterDirection.Input);
            OracleDataReader reader = cmd.ExecuteReader();
            reader.Read();
            Console.WriteLine(reader.GetString(0));

        }

        private static string ConvertToXml(DataTable data)
        {
            StringWriter writer = new StringWriter();
            data.WriteXml(writer);
            return writer.ToString();
        }

        private static string GenerateQuery(QueryType queryType, DataTable data)
        {
            List<string> columns = new List<string>();
            List<string> pkColumns = new List<string>();
            string schemaName = "AT", tableName = data.TableName;
            foreach (DataColumn item in data.Columns)
                columns.Add(item.ColumnName);

            string xmlToColumns = string.Join(',', columns.Select(name => $"EXTRACTVALUE(VALUE(a), '/{tableName}/{name}') AS {name}, \n"));
            string targetSourceAssignToColumns = string.Join(',', columns.Select(name => $"targetTable.{name} = sourceTable.{name}, \n"));
            string sourceAssignToColumns = string.Join(',', columns.Select(name => $"sourceTable.{name}"));
            string mergeCondition = string.Join(" AND", pkColumns.Select(name => $"targetTable.{name} = sourceTable.{name} \n"));
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
                                INSERT INTO Address ({string.Join(',', columns)})
                                VALUES ({sourceAssignToColumns});
                            END LOOP;
                         ";

            }
            return $@"ALTER SESSION 
                            SET NLS_TIMESTAMP_FORMAT = 'YYYY-MM-DD HH24:MI:SS.FF'
                            NLS_DATE_FORMAT = 'yyyy-MM-dd HH24:MI:SS';
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
