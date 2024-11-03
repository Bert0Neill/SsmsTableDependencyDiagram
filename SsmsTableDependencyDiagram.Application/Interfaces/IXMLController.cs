using System.Collections.Generic;
using System.Xml;

namespace SsmsTableDependencyDiagram.Application.Interfaces
{
    public interface IXMLController
    {        
        string GenerateXmlDocFromDBData(string xmlString);
    }
}