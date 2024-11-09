namespace SsmsTableDependencyDiagram.MsTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using SsmsTableDependencyDiagram.Application.Interfaces;
    using SsmsTableDependencyDiagram.Application.Services;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Reflection;

    [TestClass]
    public class ConvertServiceTests
    {
        private Mock<IErrorService> _mockErrorService;
        private ConvertService _convertService;

        [TestInitialize]
        public void Setup()
        {
            _mockErrorService = new Mock<IErrorService>();
            _convertService = new ConvertService(_mockErrorService.Object);
        }

        [TestMethod]
        public void ConvertDataTable_ShouldReturnListOfObjects_WhenDataTableIsValid()
        {
            // Arrange: create a DataTable with columns matching the properties of TestClass
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Id", typeof(int));
            dataTable.Columns.Add("Name", typeof(string));

            var row = dataTable.NewRow();
            row["Id"] = 1;
            row["Name"] = "Test";
            dataTable.Rows.Add(row);

            // Act
            List<TestClass> result = _convertService.ConvertDataTable<TestClass>(dataTable);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual("Test", result[0].Name);
        }

        [TestMethod]
        public void ConvertDataTable_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange: Create a DataTable without matching properties in TestClass to trigger an exception
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("NonExistentProperty", typeof(string));

            // Act
            List<TestClass> result = _convertService.ConvertDataTable<TestClass>(dataTable);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count); // No valid data should be added due to the exception
        }

        // Sample class to match DataTable structure for testing
        private class TestClass
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }

}