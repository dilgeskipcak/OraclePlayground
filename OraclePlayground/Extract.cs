using Microsoft.EntityFrameworkCore;
using OraclePlayground.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OraclePlayground
{
    internal static class Extract
    {

        public static DataTable GetData(int take = 10)
        {
            DbContextOptionsBuilder<AdventureWorks> contextBuilder = BuildContext();
            using var db = new AdventureWorks(contextBuilder.Options);
            var addresses = db.Addresses.AsNoTracking().Take(take).ToList();
            DataTable result = new DataTable();
            result.Columns.Add(nameof(Address.City));
            result.Columns.Add(nameof(Address.CountryRegion));
            result.Columns.Add(nameof(Address.PostalCode));
            result.Columns.Add(nameof(Address.AddressLine1));
            result.Columns.Add(nameof(Address.AddressLine2));
            foreach (var address in addresses)
            {
                var row = result.NewRow();
                row[nameof(Address.City)] = address.City;
                row[nameof(Address.CountryRegion)] = address.CountryRegion;
                row[nameof(Address.PostalCode)] = address.PostalCode;
                row[nameof(Address.AddressLine1)] = address.AddressLine1;
                row[nameof(Address.AddressLine2)] = address.AddressLine2;
                result.Rows.Add(row);
            }
            return result;

        }

        private static DbContextOptionsBuilder<AdventureWorks> BuildContext()
        {
            var contextBuilder = new DbContextOptionsBuilder<AdventureWorks>();
            DbConnectionStringBuilder connectionStringBuilder = new System.Data.Common.DbConnectionStringBuilder();
            connectionStringBuilder["Data Source"] = "(local)";
            connectionStringBuilder["integrated Security"] = true;
            connectionStringBuilder["Initial Catalog"] = "AdventureWorksLT2019";
            connectionStringBuilder["Encrypt"] = true;
            connectionStringBuilder["TrustServerCertificate"] = true;
            contextBuilder.UseSqlServer("Server=LAPTOP-51T48HPT\\MSSQLSERVER01;TrustServerCertificate=True;Database=AdventureWorksLT2019;Trusted_Connection=True;MultipleActiveResultSets=true");
            contextBuilder.Options.Freeze();
            return contextBuilder;
        }
    }

}
