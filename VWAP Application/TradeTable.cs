using System;
using System.Linq;
using System.Data;
using System.Windows;
using Microsoft.VisualBasic.FileIO;

namespace VWAP_Application
{
    /// <summary>
    /// Class for import and query of .csv file
    /// </summary>
    public class TradeTable
    {
        // DataTable containing imported CSV data
        public DataTable Table { get; private set; }

        public TradeTable()
        {
            Table = new DataTable();
        }

        public TradeTable(string path)
        {
            Table = new DataTable();
            _ = ReadCSV(path);
        }

        // Takes in a path to a csv file and updates the table with the values
        // Returns a boolean to show if it was successful
        public bool ReadCSV(string path)
        {
            TextFieldParser parser;
            
            try
            {
                // Create a new parser using the path and set it up for parsing csv
                parser = new TextFieldParser(path);
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                // Get the titles of the columns
                string[] titles = parser.ReadFields();
                _ = Table.Columns.Add(titles[0], typeof(string));
                _ = Table.Columns.Add(titles[1], typeof(string));
                _ = Table.Columns.Add(titles[2], typeof(string));
                _ = Table.Columns.Add(titles[3], typeof(string));
                _ = Table.Columns.Add(titles[4], typeof(long));
                _ = Table.Columns.Add(titles[5], typeof(double));

                // Current row's index
                int i = 0;
                // Repeat until the end of the file
                while (!parser.EndOfData)
                {
                    // Parse the fields on the current line
                    string[] fields = parser.ReadFields();
                    
                    // Add a row to the table
                    _ = Table.Rows.Add();

                    // Iterate over fields
                    for (int j = 0; j < fields.Length; j++)
                    {
                        // Set the value in the current row and column to the corresponding field
                        Table.Rows[i][j] = fields[j];
                    }

                    // Increment the row index
                    i++;
                }
            }
            catch (Exception e)
            {
                // Show an error mesagebox to show the issue
                _ = MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        // Returns a datatable of VWAPS per stock
        public DataTable GetStockVWAP()
        {
            // Get an enumerable table for LINQ
            EnumerableRowCollection<DataRow> enumTable = Table.AsEnumerable();
            var query = from stock in enumTable
                        // group stocks by epic and isin
                        group stock by (
                            epic: stock.Field<string>("epic"),
                            isin: stock.Field<string>("isin")
                        )
                        into g
                        select (
                            g.Key.epic,
                            g.Key.isin,
                            // sales is the sum of the quantity of sales * price per sale
                            sales: g.Sum(x => x.Field<long>("quantity") * x.Field<double>("price")),
                            // quantity is the sum of the quantities
                            quantity: g.Sum(x => x.Field<long>("quantity"))
                        ) into h
                        select (
                            h.epic,
                            h.isin,
                            VWAP: h.sales / h.quantity
                        );
            
            // Create a new datatable
            DataTable dataTable = new DataTable();

            // Add the columns
            _ = dataTable.Columns.Add("epic", typeof(string));
            _ = dataTable.Columns.Add("isin", typeof(string));
            _ = dataTable.Columns.Add("VWAP", typeof(double));

            // Iterate through the query results
            foreach ((string epic, string isin, double VWAP) in query)
            {
                // Add a row with the corresponding values
                _ = dataTable.Rows.Add(epic, isin, VWAP);
            }

            // Return the results
            return dataTable;
        }

        // Returns a datatable of VWAPS per stock-tradetype pair
        public DataTable GetStockPerTradeTypeVWAP()
        {
            // Get an enumerable table for LINQ
            EnumerableRowCollection<DataRow> enumTable = Table.AsEnumerable();
            var query = from stock in enumTable
                        // group stocks by epic, isin and tradetype
                        group stock by (
                            epic: stock.Field<string>("epic"),
                            isin: stock.Field<string>("isin"),
                            tradeType: stock.Field<string>("trade type")
                        )
                        into g
                        select (
                            g.Key.epic,
                            g.Key.isin,
                            g.Key.tradeType,
                            // sales is the sum of the quantity of sales * price per sale
                            sales: g.Sum(x => x.Field<long>("quantity") * x.Field<double>("price")),
                            // quantity is the sum of the quantities
                            quantity: g.Sum(x => x.Field<long>("quantity"))
                        ) into h
                        select (
                            h.epic,
                            h.isin,
                            h.tradeType,
                            VWAP: h.sales / h.quantity
                        );
            
            // Create a new datatable
            DataTable dataTable = new DataTable();

            // Add the columns
            _ = dataTable.Columns.Add("epic", typeof(string));
            _ = dataTable.Columns.Add("isin", typeof(string));
            _ = dataTable.Columns.Add("trade type", typeof(string));
            _ = dataTable.Columns.Add("VWAP", typeof(double));

            // Iterate through the query results
            foreach ((string epic, string isin, string tradeType, double VWAP) in query)
            {
                // Add a row with the corresponding values
                _ = dataTable.Rows.Add(epic, isin, tradeType, VWAP);
            }

            // Return the results
            return dataTable;
        }

        // Returns a datatable of VWAPs for a stock with the given epic
        public DataTable GetFilteredVWAP(string epic)
        {
            // Get an enumerable table for LINQ
            EnumerableRowCollection<DataRow> enumTable = Table.AsEnumerable();

            if (!enumTable.Any(row => epic == row.Field<string>("epic")))
            {
                return null;
            }

            var query = from stock in enumTable
                        // filter stocks by epic
                        where stock.Field<string>("epic") == epic
                        // group stocks by epic, isin and tradetype
                        group stock by (
                            epic: stock.Field<string>("epic"),
                            isin: stock.Field<string>("isin"),
                            tradeType: stock.Field<string>("trade type")
                        )
                        into g
                        select (
                            g.Key.isin,
                            g.Key.tradeType,
                            // sales is the sum of the quantity of sales * price per sale
                            sales: g.Sum(x => x.Field<long>("quantity") * x.Field<double>("price")),
                            // quantity is the sum of the quantities
                            quantity: g.Sum(x => x.Field<long>("quantity"))
                        ) into h
                        select (
                            h.isin,
                            h.tradeType,
                            VWAP: h.sales / h.quantity
                        );

            var query2 = from stock in enumTable
                        // filter stocks by epic
                         where stock.Field<string>("epic") == epic
                        // group stocks by epic and isin
                         group stock by (
                             epic: stock.Field<string>("epic"),
                             isin: stock.Field<string>("isin")
                         )
                         into g
                         select (
                             g.Key.isin,
                             // sales is the sum of the quantity of sales * price per sale
                             sales: g.Sum(x => x.Field<long>("quantity") * x.Field<double>("price")),
                             // quantity is the sum of the quantities
                             quantity: g.Sum(x => x.Field<long>("quantity"))
                         ) into h
                         select (
                             h.isin,
                             VWAP: h.sales / h.quantity
                         );

            // Create a new datatable
            DataTable dataTable = new DataTable();

            // Add the columns
            _ = dataTable.Columns.Add("epic", typeof(string));
            _ = dataTable.Columns.Add("isin", typeof(string));
            _ = dataTable.Columns.Add("trade type", typeof(string));
            _ = dataTable.Columns.Add("VWAP", typeof(double));

            // Iterate through the query results
            foreach ((string isin, double VWAP) in query2)
            {
                // Add a row with the corresponding values
                _ = dataTable.Rows.Add(epic, isin, "Overall", VWAP);
            }

            // Iterate through the query results
            foreach ((string isin, string tradeType, double VWAP) in query)
            {
                // Add a row with the corresponding values
                dataTable.Rows.Add(epic, isin, tradeType, VWAP);
            }

            // Return the results
            return dataTable;
        }
    }
}
