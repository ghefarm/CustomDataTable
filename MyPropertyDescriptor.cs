using System.Collections;
using System.ComponentModel;
using System.Data;

namespace DataTableCustomization
{
    internal class MyPropertyDescriptor : PropertyDescriptor
    {
        IDictionary _dictionary;
        DataColumn _key;

        internal MyPropertyDescriptor(IDictionary d, DataColumn key)
            : base(key.ToString(), null)
        {
            _dictionary = d;
            _key = key;
        }

        public override Type PropertyType
        {
            get
            {
                if (_key is DataColumn column)
                {
                    return column.DataType;
                }

                return _dictionary[_key].GetType();
            }
        }

        public override void SetValue(object component, object value)
        {
            _dictionary[_key] = value;
        }

        public override object GetValue(object component)
        {
            return _dictionary[_key];
        }

        public override bool IsReadOnly
        {
            get
            {
                if (_key is DataColumn column)
                {
                    return column.ReadOnly;
                }
                return false;
            }
        }

        public override Type ComponentType
        {
            get { return null; }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void ResetValue(object component)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
}
