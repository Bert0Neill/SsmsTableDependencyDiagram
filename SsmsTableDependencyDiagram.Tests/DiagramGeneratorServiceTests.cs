namespace SsmsTableDependencyDiagram.MsTests
{
    using Moq;
    using SsmsTableDependencyDiagram.Application.Interfaces;
    using SsmsTableDependencyDiagram.Application.Services;
    using SsmsTableDependencyDiagram.Domain.Models;
    using System.Data;
    using Syncfusion.Windows.Forms.Diagram.Controls;
    using System.Windows.Forms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System;
    using Syncfusion.Windows.Forms.Diagram;

    [TestClass]
    public class DiagramGeneratorServiceTests
    {
        private Mock<IErrorService> _mockErrorService;
        private Mock<ISQLService> _mockSQLService;
        private Mock<IXMLService> _mockXMLService;
        private Mock<ToolStripComboBox> _mockCboDatabase;
        private Mock<ToolStripComboBox> _mockCboTable;
        private Mock<Diagram> _mockDiagram;
        private DiagramGeneratorService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockErrorService = new Mock<IErrorService>();
            _mockSQLService = new Mock<ISQLService>();
            _mockXMLService = new Mock<IXMLService>();
            _mockDiagram = new Mock<Syncfusion.Windows.Forms.Diagram.Controls.Diagram>();


            //_mockDiagram.Setup(d => d.Model).Returns(new Mock<Model>().Object);
            //_mockDiagram.Setup(d => d.View).Returns(new Mock<View>().Object);


            _mockCboDatabase = new Mock<ToolStripComboBox>();
            _mockCboTable = new Mock<ToolStripComboBox>();

            _service = new DiagramGeneratorService(
                _mockDiagram.Object,
                _mockErrorService.Object,
                _mockCboDatabase.Object,
                _mockCboTable.Object,
                new SharedData(),
                _mockSQLService.Object,
                _mockXMLService.Object
            );
        }

        [TestMethod]
        public void GenerateDiagram_ValidData_CallsSQLAndXMLServices()
        {
            // Arrange
            var selectedDatabase = "TestDB";
            var selectedTable = new DatabaseMetaData { TABLE_SCHEMA = "dbo", TABLE_NAME = "TestTable" };
            var dependencyTables = new List<string> { "TestTable" };
            var dependencyTablesMetaDataXML = "<xml>test</xml>";
            var updatedXml = "<xml>updated</xml>";
            var relationships = new DataTable();

            // Setup mocks
            _mockCboDatabase.Setup(cbo => cbo.ComboBox.SelectedValue).Returns(selectedDatabase);
            _mockCboTable.Setup(cbo => cbo.ComboBox.SelectedValue).Returns(selectedTable);

            _mockSQLService.Setup(sql => sql.RetrieveDependencyTables(It.IsAny<string>(), selectedDatabase, $"[{selectedTable.TABLE_SCHEMA}].[{selectedTable.TABLE_NAME}]"))
                .Returns(dependencyTables);
            _mockSQLService.Setup(sql => sql.RetrieveDependencyTablesMetaData(It.IsAny<string>(), selectedDatabase, dependencyTables))
                .Returns(dependencyTablesMetaDataXML);
            _mockSQLService.Setup(sql => sql.RetrieveRelationshipData(It.IsAny<string>(), selectedDatabase, dependencyTables))
                .Returns(relationships);

            _mockXMLService.Setup(xml => xml.GenerateXmlDocFromDBData(dependencyTablesMetaDataXML))
                .Returns(updatedXml);

            // Act
            _service.GenerateDiagram(true);

            // Assert
            _mockSQLService.Verify(sql => sql.RetrieveDependencyTables(It.IsAny<string>(), selectedDatabase, It.IsAny<string>()), Times.Once);
            _mockSQLService.Verify(sql => sql.RetrieveDependencyTablesMetaData(It.IsAny<string>(), selectedDatabase, dependencyTables), Times.Once);
            _mockSQLService.Verify(sql => sql.RetrieveRelationshipData(It.IsAny<string>(), selectedDatabase, dependencyTables), Times.Once);
            _mockXMLService.Verify(xml => xml.GenerateXmlDocFromDBData(dependencyTablesMetaDataXML), Times.Once);
        }

        [TestMethod]
        public void GenerateDiagram_ExceptionThrown_LogsError()
        {
            // Arrange: Force an exception in one of the dependencies
            _mockSQLService.Setup(sql => sql.RetrieveDependencyTables(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Test exception"));

            // Act
            _service.GenerateDiagram();

            // Assert: Verify that the error service logs the exception
            _mockErrorService.Verify(es => es.LogAndDisplayErrorMessage(It.IsAny<Exception>()), Times.Once);
        }
    }
}



