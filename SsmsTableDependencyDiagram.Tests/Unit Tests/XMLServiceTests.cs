using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SsmsTableDependencyDiagram.Application.Interfaces;
using System.Xml;
using SsmsTableDependencyDiagram.Application.Services;

namespace SsmsTableDependencyDiagram.MsTests
{
    [TestClass]
    public class YourClassTests
    {
        private Mock<IErrorService> _mockErrorService;
        private XMLService _xmlService;

        [TestInitialize]
        public void Setup()
        {
            // Arrange: Create a mock of the IErrorService
            _mockErrorService = new Mock<IErrorService>();
            _xmlService = new XMLService(_mockErrorService.Object);
        }

        [TestMethod]
        public void GenerateXmlDocFromDBData_ValidXml_ReturnsExpectedXml()
        {
            // Arrange: Sample input XML
            string inputXml = @"<root><Table Name='Employee' TableId='123'><Column Name='EmpID' IsPrimaryKey='1' ColumnDataType='int' /></Table></root>";
            string expectedTableName = "Employee";
            string expectedTableId = "123";

            // Act: Call the method under test
            var result = _xmlService.GenerateXmlDocFromDBData(inputXml);

            // Assert: Load result XML and check structure
            XmlDocument resultXml = new XmlDocument();
            resultXml.LoadXml(result);

            XmlNode tableNode = resultXml.SelectSingleNode($"//dataroot/{expectedTableName}");
            Assert.IsNotNull(tableNode, "Table element should exist in the result XML.");

            XmlNode tableIdNode = tableNode.SelectSingleNode("TableId");
            Assert.AreEqual(expectedTableId, tableIdNode?.InnerText, "TableID should match the expected value.");
        }

        [TestMethod]
        public void GenerateXmlDocFromDBData_InvalidXml_LogsErrorAndReturnsEmptyString()
        {
            // Arrange: Invalid XML input
            string inputXml = "<root><Table Name='Employee'></root>";

            // Act: Call the method with invalid XML
            var result = _xmlService.GenerateXmlDocFromDBData(inputXml);

            // Assert: Verify error handling and empty result
            Assert.AreEqual(string.Empty, result, "Result should be empty on invalid XML input.");
            _mockErrorService.Verify(es => es.LogAndDisplayErrorMessage(It.IsAny<Exception>()), Times.Once);
        }
    }
}

