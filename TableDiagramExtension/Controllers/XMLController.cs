using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SsmsTableDependencyDiagram.Domain.Resources;
using System;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using TableDiagramExtension.Interfaces;

namespace TableDiagramExtension.Controllers
{
    public class XMLController : IXMLController
    {
        private readonly IErrorController _errorService;

        public XMLController() 
        {
            _errorService = ServiceProviderContainer.ServiceProvider.GetService<IErrorController>(); // inject error handling service
        }

        public string GenerateXmlDocFromDBData(string xmlString)
        {
            try
            {
                // Create an XmlDocument
                XmlDocument xmlDocFormatted = new XmlDocument();

                // Create the root element <dataroot>
                XmlElement datarootElement = xmlDocFormatted.CreateElement(TextStrings.dataroot);
                datarootElement.SetAttribute("xmlns:od", TextStrings.urn);
                datarootElement.SetAttribute("xmlns:xsi", TextStrings.schIns);                
                datarootElement.SetAttribute("generated", "2023-01-18T15:03:23");
                
                xmlDocFormatted.AppendChild(datarootElement);

                // craete xmlDoc for parsing current format - to retrieve data
                XDocument xmlDoc = XDocument.Parse(xmlString);

                // Accessing Table elements and attributes
                var tableElements = xmlDoc.Descendants(TextStrings.Table);
                foreach (var tableElement in tableElements)
                {
                    //string schema = tableElement.Attribute("Schema").Value;
                    string tableName = tableElement.Attribute(TextStrings.Name).Value;
                    string tableId = tableElement.Attribute(TextStrings.TableId).Value;

                    // add <Address>
                    XmlElement xmlFormattedTableRoot = xmlDocFormatted.CreateElement(tableName);

                    // add to xmlDocFormatted node <TableID>123</TableID> to <Address> node
                    XmlElement tableElementID = xmlDocFormatted.CreateElement(TextStrings.TableId);
                    tableElementID.InnerText = tableId;
                    xmlFormattedTableRoot.AppendChild(tableElementID);

                    // Accessing Column elements and attributes within each Table
                    var columnElements = tableElement.Elements(TextStrings.Column);

                    foreach (var columnElement in columnElements)
                    {
                        string columnName = columnElement.Attribute(TextStrings.Name).Value;
                        string isPrimaryKey = columnElement.Attribute(TextStrings.IsPrimaryKey).Value;
                        string columnDataType = columnElement.Attribute(TextStrings.ColumnDataType)?.Value ?? TextStrings.NA; ;
                        string isForeignKey = columnElement.Attribute(TextStrings.ForeignKey)?.Value ?? TextStrings.NA;

                        // add column node <EmpID>EmpID</EmpID>
                        XmlElement columnElementName = xmlDocFormatted.CreateElement(columnName);
                        columnElementName.InnerText = columnName;
                        xmlFormattedTableRoot.AppendChild(columnElementName);

                        if (columnDataType != TextStrings.NA)
                        {                            
                            // add attribute, that will later be added to concrete class collection of columns
                            columnElementName.SetAttribute(TextStrings.ColumnDataType, columnDataType);
                        }

                        if (isForeignKey != TextStrings.NA)
                        {
                            // add <ForeignKey> 123 </ForeignKey>
                            XmlElement foreignKeyElementName = xmlDocFormatted.CreateElement(TextStrings.ForeignKey);
                            foreignKeyElementName.InnerText = columnName;
                            xmlFormattedTableRoot.AppendChild(foreignKeyElementName);
                        }

                        if (isPrimaryKey == "1")
                        {
                            // add <PrimaryKey> empID </PrimaryKey>
                            XmlElement primaryKeyElementName = xmlDocFormatted.CreateElement(TextStrings.PrimaryKey);
                            primaryKeyElementName.InnerText = columnName;
                            xmlFormattedTableRoot.AppendChild(primaryKeyElementName);
                        }
                    }

                    // finally add to <dataroot>
                    datarootElement.AppendChild(xmlFormattedTableRoot);
                }

                return xmlDocFormatted.OuterXml;
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
                return string.Empty;
            }
        }
    }
}
