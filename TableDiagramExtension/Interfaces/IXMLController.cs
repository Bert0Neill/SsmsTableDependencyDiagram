using System.Collections.Generic;
using System.Xml;

namespace TableDiagramExtension.Interfaces
{
    internal interface IXMLController
    {        
        string GenerateXmlDocFromDBData(string xmlString);
    }
}