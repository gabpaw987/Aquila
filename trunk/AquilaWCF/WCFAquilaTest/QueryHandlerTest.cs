using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquila_Software;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WCFAquilaTest
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für UnitTest1
    /// </summary>
    [TestClass]
    public class QueryHandlerTest
    {
        public QueryHandlerTest()
        {
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Ruft den Textkontext mit Informationen über
        ///den aktuellen Testlauf sowie Funktionalität für diesen auf oder legt diese fest.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Zusätzliche Testattribute

        //
        // Sie können beim Schreiben der Tests folgende zusätzliche Attribute verwenden:
        //
        // Verwenden Sie ClassInitialize, um vor Ausführung des ersten Tests in der Klasse Code auszuführen.
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Verwenden Sie ClassCleanup, um nach Ausführung aller Tests in einer Klasse Code auszuführen.
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen.
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Mit TestCleanup können Sie nach jedem einzelnen Test Code ausführen.
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        #endregion Zusätzliche Testattribute

        [TestMethod]
        public void TestInsertBar()
        {
            Aquila_Software.QueryHandler.insertBar("Test", "Hallo", 10, new Tuple<System.DateTime, decimal, decimal, decimal, decimal>(DateTime.Now, 10m, 10m, 10m, 10m));
        }

        [TestMethod]
        public void TestInsertOrder()
        {
            Aquila_Software.QueryHandler.insertOrder("", DateTime.Now, DateTime.Now, 0m, 0m, 0);
        }

        [TestMethod]
        public void TestInsertSignal()
        {
            Aquila_Software.QueryHandler.insertSignal("", 0, DateTime.Now);
        }
    }
}