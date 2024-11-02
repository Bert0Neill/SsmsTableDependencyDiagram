using System.Collections.Generic;
using System.Xml;

namespace TableDiagramExtension.Interfaces
{
    public interface IXMLController
    {        
        string GenerateXmlDocFromDBData(string xmlString);
    }
}