using System.Collections;
using System.ComponentModel;
using System.Data;

namespace DataTableCustomization
{
    /// <summary>
    /// A <see cref="CustomDataTable"/>-row which implements <see cref="INotifyDataErrorInfo"/> to support async-validation
    /// </summary>
    public sealed class CustomDataRow : ICustomTypeDescriptor, INotifyDataErrorInfo
    {
        private Dictionary<DataColumn, object?> _dictionary;

        public object? First() => _dictionary.FirstOrDefault().Value;

        public object? Last() => _dictionary.LastOrDefault().Value;

        public object?[] ItemArray => _dictionary.Values.ToArray();

        public DataColumn[] Columns => _dictionary.Keys.ToArray();

        public CustomDataTable Table { get; }

        private CustomDataRow(CustomDataTable parentTable)
        {
            _dictionary = new Dictionary<DataColumn, object?>();
            Table = parentTable;
        }

        internal CustomDataRow(IEnumerable<DataColumn> dataColumns, CustomDataTable parentTable)
        {
            _dictionary = new Dictionary<DataColumn, object?>();
            foreach (var column in dataColumns)
            {
                _dictionary[column] = null;
            }

            Table = parentTable;
        }

        public object? this[DataColumn column] => _dictionary.Keys.Contains(column) ? _dictionary[column] : null;

        public object? this[string columnName]
        {
            get
            {
                var column = _dictionary.Keys.FirstOrDefault(k => k.ColumnName == columnName);
                if (column is null)
                {
                    throw new ArgumentException("Column does not exist", nameof(columnName));
                }

                return _dictionary[column];
            }
        }

        public object? this[int columnIndex]
        {
            get
            {
                if (columnIndex < 0 || columnIndex >= _dictionary.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(columnIndex));
                }

                int currentIndex = 0;
                foreach (var key in _dictionary.Keys)
                {
                    if (currentIndex == columnIndex)
                    {
                        return _dictionary[key];
                    }

                    currentIndex++;
                }

                // Should never reach here because of the bounds check above
                throw new InvalidOperationException("Index out of bounds.");
            }
        }

        #region ICustomTypeDescriptor
        public bool SetPropertyValue(string propertyName, object value)
        {
            var column = _dictionary.Keys.FirstOrDefault(k => k.ColumnName == propertyName);
            if (column is null)
            {
                return false;
            }

            _dictionary[column] = value;
            return true;
        }

        public void SetPropertyValue(DataColumn dataColumn, object value)
        {
            _dictionary[dataColumn] = value;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(new Attribute[0]);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            ArrayList properties = new ArrayList();
            foreach (var e in _dictionary)
            {
                properties.Add(new MyPropertyDescriptor(_dictionary, e.Key));
            }

            PropertyDescriptor[] props =
                (PropertyDescriptor[])properties.ToArray(typeof(PropertyDescriptor));

            return new PropertyDescriptorCollection(props);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return _dictionary;
        }

        #endregion

        #region INotifyDataErrorInfo

        private Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        private void OnDataErrorsChanged(string propertyName)
        {
            if (ErrorsChanged != null)
            {
                ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        public bool HasErrors => _errors.Count > 0;

        public IEnumerable GetErrors(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || !_errors.ContainsKey(propertyName))
            {
                return null;
            }

            return _errors[propertyName];
        }

        public void AddError(string propertyName, string errorMessage)
        {
            if (!_errors.ContainsKey(propertyName))
            {
                _errors.Add(propertyName, new List<String>());
            }

            if (!_errors[propertyName].Contains(errorMessage))
            {
                _errors[propertyName].Add(errorMessage);
                OnDataErrorsChanged(propertyName);
            }
        }

        public void RemoveError(string propertyName, string errorMessage)
        {
            if (_errors.ContainsKey(propertyName))
            {
                if (_errors[propertyName].Contains(errorMessage))
                {
                    _errors[propertyName].Remove(errorMessage);
                    if (_errors[propertyName].Count == 0)
                    {
                        _errors.Remove(propertyName);
                    }

                    OnDataErrorsChanged(propertyName);
                }
            }
        }

        public void RemoveErrors(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                OnDataErrorsChanged(propertyName);
            }
        }

        #endregion
    }
}
