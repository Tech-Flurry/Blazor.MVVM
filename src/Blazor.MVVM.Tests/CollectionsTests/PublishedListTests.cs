using Blazor.MVVM.Tests.SetupTests;
using TechFlurry.Blazor.MVVM.Utils.Collections;

namespace Blazor.MVVM.Tests.CollectionsTests;

[TestFixture]
public class PublishedListTests
{
    private PublishedList<ViewModelA> _publishedList;
    [SetUp]
    public void Setup()
    {
        _publishedList = new PublishedList<ViewModelA>()
        {
             new ViewModelA(){ A= 1 },
             new ViewModelA(){ A= 2 },
             new ViewModelA(){ A= 3 },
        };
    }


    [Test]
    [TestCase("default")]
    [TestCase("list")]
    [TestCase("capacity")]
    [TestCase("collection")]
    [TestCase("static")]
    public void TestInitialize(string method)
    {
        PublishedList<ViewModelA> publishedList = method switch
        {
            "default" => new PublishedList<ViewModelA>(),
            "list" => new PublishedList<ViewModelA>(new List<ViewModelA>()),
            "capacity" => new PublishedList<ViewModelA>(10),
            "collection" => new PublishedList<ViewModelA>(new ViewModelA[] { new ViewModelA() }),
            "static" => PublishedList<ViewModelA>.GeneratePublishedList(new List<int> { 1, 2, 3 }, x => new ViewModelA() { A = x }),
            _ => null,
        };

        //assert
        Assert.That(publishedList, Is.Not.Null);
    }


    [Test]
    public void TestAddMethod()
    {
        // Arrange
        var publishedList = new PublishedList<ViewModelA>();
        int expectedCount = 1;
        bool collectionChangedInvoked = false;
        bool propertyChangedInvoked = false;
        publishedList.CollectionChanged += (sender, e) => collectionChangedInvoked = true;
        publishedList.PropertyChanged += (sender, e) => propertyChangedInvoked = true;

        // Act
        publishedList.Add(new ViewModelA
        {
            A = 1,
        });

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(publishedList, Has.Count.EqualTo(expectedCount));
            Assert.That(collectionChangedInvoked, Is.True);
            Assert.That(propertyChangedInvoked, Is.True);
        });
    }

    [Test]
    public void TestClearMethod()
    {
        // Arrange
        int expectedCount = 0;
        bool collectionChangedInvoked = false;
        bool propertyChangedInvoked = false;
        _publishedList.CollectionChanged += (sender, e) => collectionChangedInvoked = true;
        _publishedList.PropertyChanged += (sender, e) => propertyChangedInvoked = true;
        // Act
        _publishedList.Clear();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_publishedList, Has.Count.EqualTo(expectedCount));
            Assert.That(collectionChangedInvoked, Is.True);
            Assert.That(propertyChangedInvoked, Is.True);
        });
    }

    [Test]
    public void TestContainsMethod()
    {
        // Arrange
        var itemToCheck = _publishedList[2];
        bool expectedResult = true;

        // Act
        var result = _publishedList.Contains(itemToCheck);

        // Assert
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [Test]
    public void TestRemoveMethod()
    {
        // Arrange
        var itemToRemove = _publishedList[2];
        int expectedCount = 2;
        bool collectionChangedInvoked = false;
        bool propertyChangedInvoked = false;
        _publishedList.CollectionChanged += (sender, e) => collectionChangedInvoked = true;
        _publishedList.PropertyChanged += (sender, e) => propertyChangedInvoked = true;

        // Act
        _publishedList.Remove(itemToRemove);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_publishedList, Has.Count.EqualTo(expectedCount));
            Assert.That(collectionChangedInvoked, Is.True);
            Assert.That(propertyChangedInvoked, Is.True);
        });
    }

    [Test]
    public void TestIndexOfMethod()
    {
        // Arrange
        var itemToFind = _publishedList[2];
        int expectedIndex = 2;

        // Act
        var result = _publishedList.IndexOf(itemToFind);

        // Assert
        Assert.That(result, Is.EqualTo(expectedIndex));
    }

    [Test]
    public void TestInsertMethod()
    {
        // Arrange
        var itemToInsert = new ViewModelA { A = 4 };
        int indexToInsert = 1;
        int expectedCount = 4;
        bool collectionChangedInvoked = false;
        bool propertyChangedInvoked = false;
        _publishedList.CollectionChanged += (sender, e) => collectionChangedInvoked = true;
        _publishedList.PropertyChanged += (sender, e) => propertyChangedInvoked = true;

        // Act
        _publishedList.Insert(indexToInsert, itemToInsert);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_publishedList, Has.Count.EqualTo(expectedCount));
            Assert.That(collectionChangedInvoked, Is.True);
            Assert.That(propertyChangedInvoked, Is.True);
        });
    }

    [Test]
    public void TestRemoveAtMethod()
    {
        // Arrange
        int indexToRemove = 1;
        int expectedCount = 2;
        bool collectionChangedInvoked = false;
        bool propertyChangedInvoked = false;
        _publishedList.CollectionChanged += (sender, e) => collectionChangedInvoked = true;
        _publishedList.PropertyChanged += (sender, e) => propertyChangedInvoked = true;

        // Act
        _publishedList.RemoveAt(indexToRemove);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_publishedList, Has.Count.EqualTo(expectedCount));
            Assert.That(collectionChangedInvoked, Is.True);
            Assert.That(propertyChangedInvoked, Is.True);
        });
    }

    [Test]
    public void PublishedList_Should_CopyTo_Array()
    {
        //arrange
        var array = new ViewModelA[3];
        bool collectionChangedInvoked = false;
        bool propertyChangedInvoked = false;
        _publishedList.CollectionChanged += (sender, e) => collectionChangedInvoked = true;
        _publishedList.PropertyChanged += (sender, e) => propertyChangedInvoked = true;

        //act
        _publishedList.CopyTo(array, 0);


        //assert
        Assert.Multiple(() =>
        {
            for (int i = 0; i < array.Length; i++)
            {
                Assert.That(array[i].A, Is.EqualTo(_publishedList[i].A));
            }
            Assert.That(collectionChangedInvoked, Is.True);
            Assert.That(propertyChangedInvoked, Is.True);
        });
    }

    
}