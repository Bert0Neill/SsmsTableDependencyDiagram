using System.Collections.Generic;
using System.Xml;

namespace SsmsTableDependencyDiagram.Application.Interfaces
{
    public interface IXMLService
    {        
        string GenerateXmlDocFromDBData(string xmlString);
    }
}