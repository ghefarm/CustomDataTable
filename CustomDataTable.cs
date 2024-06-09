using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;

namespace DataTableCustomization
{
    /// <summary>
    /// An in-memory table with async-validation support. That is, the table row implements <see cref="INotifyDataErrorInfo"/>.
    /// </summary>
    /// <remarks> Designed as an alternativ to <see cref="DataTable"/> that does not support async validation in the UI (Grid)</remarks>
    public class CustomDataTable : ObservableCollection<CustomDataRow>
    {
        private DataTable _innerTable;

        public IEnumerable<DataColumn> Columns
        {
            get
            {
                var columns = new List<DataColumn>();
                foreach (DataColumn dataColumn in _innerTable.Columns)
                {
                    columns.Add(dataColumn);
                }
                return columns.AsEnumerable();
            }
        }

        public CustomDataTable()
        {
            _innerTable = new DataTable();
        }

        public object? this[int rowIndex, int columnIndex] => Items[rowIndex][columnIndex];

        public void AddColumn(string columnName, bool readOnly = false)
        {
            _innerTable.Columns.Add(new DataColumn(columnName, typeof(string))
            {
                ReadOnly = readOnly
            });
        }

        public void AddColumn(string columnName, Type dataType, bool readOnly = false)
        {
            _innerTable.Columns.Add(new DataColumn(columnName, dataType)
            {
                ReadOnly = readOnly
            });
        }

        /// <summary>
        /// Creates a new <see cref="CustomDataRow"/> with the same schema as the table.
        /// </summary>
        /// <returns></returns>
        public CustomDataRow NewCustomRow()
        {
            var columns = new List<DataColumn>();
            foreach (DataColumn dataColumn in _innerTable.Columns)
            {
                columns.Add(dataColumn);
            }

            return new CustomDataRow(columns.AsEnumerable(), this);
        }

        /// <summary>
        /// Creates a row using specified values and adds it to the end of teh collection of rows.
        /// </summary>
        public void AddRow(params object?[] values)
        {
            var innerRow = _innerTable.Rows.Add(values);

            var customRow = NewCustomRow();
            foreach (DataColumn column in _innerTable.Columns)
            {
                customRow.SetPropertyValue(column, innerRow[column]);
            }

            base.Add(customRow);
        }

        /// <summary>
        /// Creates a new row with the same schema as the table.
        /// </summary>
        /// <remarks> For compatibility with MS DataTable</remarks>
        /// <returns></returns>
        public DataRow NewRow()
        {
            return _innerTable.NewRow();
        }

        /// <summary>
        /// Adds the given row to the table.
        /// </summary>
        /// <param name="dataRow"></param>
        /// <remarks> For compatibility with MS DataTable</remarks>
        /// <returns></returns>
        public void AddRow(DataRow dataRow)
        {
            _innerTable.Rows.Add(dataRow);

            var customRow = NewCustomRow();
            foreach (DataColumn column in _innerTable.Columns)
            {
                customRow.SetPropertyValue(column, dataRow[column]);
            }

            Add(customRow);
        }
    }
}
