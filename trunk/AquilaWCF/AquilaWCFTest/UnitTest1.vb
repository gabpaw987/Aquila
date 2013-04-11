Imports System.Text

<TestClass()>
Public Class UnitTest1

    Private testContextInstance As TestContext

    '''<summary>
    '''Ruft den Textkontext mit Informationen über
    '''den aktuellen Testlauf sowie Funktionalität für diesen auf oder legt diese fest.
    '''</summary>
    Public Property TestContext() As TestContext
        Get
            Return testContextInstance
        End Get
        Set(ByVal value As TestContext)
            testContextInstance = Value
        End Set
    End Property

#Region "Zusätzliche Testattribute"
    '
    ' Sie können beim Schreiben der Tests folgende zusätzliche Attribute verwenden:
    '
    ' Verwenden Sie ClassInitialize, um vor Ausführung des ersten Tests in der Klasse Code auszuführen.
    ' <ClassInitialize()> Public Shared Sub MyClassInitialize(ByVal testContext As TestContext)
    ' End Sub
    '
    ' Verwenden Sie ClassCleanup, um nach Ausführung aller Tests in einer Klasse Code auszuführen.
    ' <ClassCleanup()> Public Shared Sub MyClassCleanup()
    ' End Sub
    '
    ' Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen.
    ' <TestInitialize()> Public Sub MyTestInitialize()
    ' End Sub
    '
    ' Mit TestCleanup können Sie nach jedem einzelnen Test Code ausführen.
    ' <TestCleanup()> Public Sub MyTestCleanup()
    ' End Sub
    '
#End Region

    <TestMethod()>
    Public Sub TestMethod1()
        ' TODO: Testlogik hier einfügen
    End Sub

End Class
