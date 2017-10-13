using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace GrowCreate.PipelineCRM.Models.DetachedPublishedContent
{
    internal class DetachedPublishedProperty : IPublishedProperty
    {
        private readonly PublishedPropertyType _propertyType;
        private object _rawValue;
        private Lazy<object> _sourceValue;
        private Lazy<object> _objectValue;
        private Lazy<object> _xpathValue;
        private readonly bool _isPreview;

        public DetachedPublishedProperty(PublishedPropertyType propertyType, object value)
            : this(propertyType, value, false)
        {
        }

        public DetachedPublishedProperty(PublishedPropertyType propertyType, object value, bool isPreview)
        {
            _propertyType = propertyType;
            _isPreview = isPreview;

            SetValue(value);
        }

        internal void SetValue(object value)
        {
            _rawValue = value;

            _sourceValue = new Lazy<object>(() => _rawValue != null ? _propertyType.ConvertDataToSource(_rawValue, _isPreview) : null);
            _objectValue = new Lazy<object>(() => _rawValue != null ? _propertyType.ConvertSourceToObject(_sourceValue.Value, _isPreview) : null);
            _xpathValue = new Lazy<object>(() => _rawValue != null ? _propertyType.ConvertSourceToXPath(_sourceValue.Value, _isPreview) : null);
        }

        public string PropertyTypeAlias
        {
            get
            {
                return _propertyType.PropertyTypeAlias;
            }
        }

        public bool HasValue
        {
            get { return DataValue != null && DataValue.ToString().Trim().Length > 0; }
        }

        public object DataValue { get { return _rawValue; } internal set {} }

        public object Value { get { return _objectValue.Value; } }

        public object XPathValue { get { return _xpathValue.Value; } }
        public int DataTypeId { get; internal set; }
    }
}