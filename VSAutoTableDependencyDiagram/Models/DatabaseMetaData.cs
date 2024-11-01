using System.Collections.Generic;
using System.Xml.Serialization;

namespace VSAutoTableDependencyDiagram.Models
{
    [XmlRoot(ElementName = "DatabaseMetaData")]
    public class DatabaseMetaData
    {

        [XmlElement(ElementName = "TABLE_CATALOG")]
        public string TABLE_CATALOG { get; set; }

        [XmlElement(ElementName = "TABLE_SCHEMA")]
        public string TABLE_SCHEMA { get; set; }

        [XmlElement(ElementName = "TABLE_NAME")]
        public string TABLE_NAME { get; set; }

        [XmlElement(ElementName = "COLUMN_NAME")]
        public string COLUMN_NAME { get; set; }

        [XmlElement(ElementName = "DATA_TYPE")]
        public string DATA_TYPE { get; set; }
    }

    [XmlRoot(ElementName = "DocumentElement")]
    public class DocumentElement
    {

        [XmlElement(ElementName = "DatabaseMetaData")]
        public List<DatabaseMetaData> DBSchema { get; set; }
    }
}
