// See https://aka.ms/new-console-template for more information

using OraclePlayground;
using System.Data;

DataTable data = Extract.GetData();

Loader.Load(data);
