using System.Collections.Generic;
using System.Xml;

namespace VSAutoTableDependencyDiagram.Interfaces
{
    internal interface IXMLController
    {        
        string GenerateXmlDocFromDBData(string xmlString);
    }
}