using System.Collections.Generic;
using LiveCharts;
using System.Data;

namespace VWAP_Application
{
    /// <summary>
    /// Class for holding values for columnchart
    /// </summary>
    public class ChartData<T>
    {
        // Name of the epic
        public string Name { get; private set; }
        
        // Values of columns
        public ChartValues<T> Values { get; private set; }
        
        // Labels of columns
        public List<string> Labels { get; private set; }
        
        // Constructor
        public ChartData(DataTable dataTable)
        {
            Values = new ChartValues<T>();
            Labels = new List<string>();

            // Get the name of the epic
            Name = (string)dataTable.Rows[0][0];
            
            // Iterate through the rows of the table
            foreach (DataRow row in dataTable.Rows)
            {
                // Get the value of the type of trade
                Labels.Add((string)row[dataTable.Columns[2]]);
                
                // Get the value of the VWAP
                Values.Add((T)row[dataTable.Columns[3]]);
            }
        }
    }
}
