using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace VWAP_Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TradeTable TradeTable { get; private set; }
        public ChartData<double> ChartData { get; private set; }

        // Constructor
        public MainWindow()
        {
            InitializeComponent();

            // Set visibilty of various controls
            dataGridTabs.Visibility = Visibility.Collapsed;
            btnSelectSavePath.Visibility = Visibility.Collapsed;
            filterGrid.Visibility = Visibility.Collapsed;
            filterChart.Visibility = Visibility.Collapsed;
        }

        // Event handler for clicking the path selection button
        private void btnSelectPath_Click(object sender, RoutedEventArgs e)
        {
            // Create a new openfiledialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // Set filter so only .csv files can be opened
            openFileDialog.Filter = "Comma-separated values (*.csv)|*.csv";

            // Show the dialog, if there is a valid selection
            if (openFileDialog.ShowDialog() == true)
            {
                // Set the text of the path textbox to the path
                path.Text = openFileDialog.FileName;
            }
        }
        
        // Event handler for clicking the display button
        private void btnDisplay_Click(object sender, RoutedEventArgs e)
        {
            // Create a new TradeTable
            TradeTable = new TradeTable();

            // Attempt to read the csv at the path selected, if it succeeds
            if (TradeTable.ReadCSV(path.Text))
            {
                // Set the data context for the datagrids to the query results
                stockDataGrid.DataContext = TradeTable.GetStockVWAP();
                tradeTypeGrid.DataContext = TradeTable.GetStockPerTradeTypeVWAP();

                // Create a set to contain epics
                HashSet<string> epics = new HashSet<string>();

                // Iterate through the rows of the table
                foreach (DataRow row in TradeTable.Table.Rows)
                {
                    // Add the epic to the set
                    _ = epics.Add(row.ItemArray[0].ToString());
                }

                // Use the epics as the itemsource for the filter list
                filter.ItemsSource = epics;

                // Set visibility for the tabs and the export button
                dataGridTabs.Visibility = Visibility.Visible;
                btnSelectSavePath.Visibility = Visibility.Visible;
            }
        }

        // Event handler for clicking the export button
        private void btnSelectSavePath_Click(object sender, RoutedEventArgs e)
        {
            //Create a new savefiledialog
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // Set filter so only .csv or .xml files can be saved
            saveFileDialog.Filter = "Comma-separated values (*.csv)|*.csv|eXtensible Markup Language (*.xml)|*.xml";
  
            // Add an event handler for when OK is pressed to check the file extension
            saveFileDialog.FileOk += CheckFileExtension;
            
            // Show the dialog, if there is a valid selection
            if (saveFileDialog.ShowDialog() == true)
            {
                // Get the extension used
                string extension = Path.GetExtension(saveFileDialog.FileName);
                
                // Initialise variables for the data
                string data = "";
                string data2 = "";

                // Get data depending on extension used
                switch (extension)
                {
                    // Get csv data
                    case ".csv":
                        data = DataTableToCSV((DataTable)stockDataGrid.DataContext);
                        data2 = DataTableToCSV((DataTable)tradeTypeGrid.DataContext);
                        break;
                    // Get xml data
                    case ".xml":
                        data = DataTableToXML((DataTable)stockDataGrid.DataContext);
                        data2 = DataTableToXML((DataTable)tradeTypeGrid.DataContext);
                        break;
                }

                // Write the files
                File.WriteAllText(saveFileDialog.FileName.Replace(extension, "_per_stock" + extension), data);
                File.WriteAllText(saveFileDialog.FileName.Replace(extension, "_per_stock_trade_type_pair" + extension), data2);
            }
        }

        // Checks if the file is using the correct extension
        private void CheckFileExtension(object sender, CancelEventArgs e)
        {
            // Cast the sending object to a savefiledialog
            SaveFileDialog saveFileDialog = (SaveFileDialog)sender;
            
            // If the extension isn't .csv or .xml
            if (Path.GetExtension(saveFileDialog.FileName).ToLower() != ".csv" && Path.GetExtension(saveFileDialog.FileName).ToLower() != ".xml")
            {
                // Cancel the event
                e.Cancel = true;
                // Show an error messagebox
                _ = MessageBox.Show("Invalid file extension", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        // Takes a datatable as input and outputs a CSV representative string
        private string DataTableToCSV(DataTable dataTable)
        {
            // Create a new stringbuilder
            StringBuilder stringBuilder = new StringBuilder();
            
            // Get the column names from the table
            IEnumerable<string> columNames = dataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName);

            // Append a line to the string of the names separated by commas and encapsulated with ""
            _ = stringBuilder.Append("\"");
            _ = stringBuilder.Append(string.Join("\",\"", columNames));
            _ = stringBuilder.AppendLine("\"");

            // Iterate through each row in the table
            foreach (DataRow row in dataTable.Rows)
            {
                // Iterate through the items in the row
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    // Get the item
                    object temp = row.ItemArray[i];
                    
                    // If the item is a string
                    if (temp.GetType() == typeof(string))
                    {
                        // Encapsulate in ""
                        temp = "\"" + temp.ToString() + "\"";
                    }
                    else
                    {
                        // Get its representation as a string
                        temp = temp.ToString();
                    }

                    // Append the item
                    _ = stringBuilder.Append(temp);

                    // If the item isn't the last in the row
                    if (i < row.ItemArray.Length - 1)
                    {
                        // Append a comma
                        _ = stringBuilder.Append(",");
                    }
                }

                // End the line
                _ = stringBuilder.AppendLine();
            }

            // Output the string
            return stringBuilder.ToString();
        }

        // Takes a datatable as input and outputs an XML representative string
        private string DataTableToXML(DataTable dataTable)
        {
            // Create a new stringbuilder
            StringBuilder stringBuilder = new StringBuilder();

            // Iterate through the rows in the table
            foreach (DataRow row in dataTable.Rows)
            {
                // Append the start tag for the asset
                _ = stringBuilder.AppendLine("<asset>");

                // Iterate through the items in the array
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    // Append a tab space
                    _ = stringBuilder.Append("\t");
                    // Append the start tag with the column name
                    _ = stringBuilder.Append("<" + dataTable.Columns[i].ToString() + ">");
                    // Append the item as a string
                    _ = stringBuilder.Append(row.ItemArray[i].ToString());
                    // Append the closing tag with the column name
                    _ = stringBuilder.AppendLine("</" + dataTable.Columns[i].ToString() + ">");
                }

                // Append the closing tag for the asset
                _ = stringBuilder.AppendLine("</asset>");
            }

            // Output the string
            return stringBuilder.ToString();
        }
        
        // Event handler for when the filter is changed
        private void filter_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get the query for the filter
            DataTable filteredTable = TradeTable.GetFilteredVWAP((string)filter.SelectedValue);

            // If the query failed
            if (filteredTable == null)
            {
                // Hide the grid and chart
                filterGrid.Visibility = Visibility.Hidden;
                filterChart.Visibility = Visibility.Hidden;
            }
            else
            {
                // Create a new chartdata with the query results
                ChartData = new ChartData<double>(filteredTable);
                
                // Set the datacontext for the grid and chart
                filterGrid.DataContext = filteredTable;
                filterChart.DataContext = ChartData;
                
                // Show the grid and chart
                filterGrid.Visibility = Visibility.Visible;
                filterChart.Visibility = Visibility.Visible;
            }
        }
    }
}
