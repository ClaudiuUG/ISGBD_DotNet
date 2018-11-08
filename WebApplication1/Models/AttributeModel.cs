using KeyValueDatabaseApi.Context;

namespace KeyValueDatabaseApi.Models
{
    public class AttributeModel
    {
        public AttributeModel(string attributeName, string attributeType)
        {
            AttributeName = attributeName;
            AttributeType = attributeType;
        }

        public string AttributeName { get; }

        public string AttributeType { get; }
    }

    public class AttributeModelEntryMapper
    {
        public AttributeEntry MapToEntry(AttributeModel attributeModel)
        {
            return new AttributeEntry(attributeModel.AttributeName, attributeModel.AttributeType, -1, false);
        }
    }
}