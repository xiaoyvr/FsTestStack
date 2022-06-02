using FsTestStack.Test.CSharp.Domains;

namespace FsTestStack.Test.CSharp.Examples;

public class ExampleDbFacts : DbFactBase
{
    [Fact]
    public void should_be_able_to_save_people()
    {
        DbSave(new People("John", "Doe"));

        var peopleService = new PeopleService(Db.CreateSession());

        var people = peopleService.GetAll().First();
        
        Assert.NotEqual(0, people.Id);
        Assert.Equal("John", people.FirstName);
        Assert.Equal("Doe", people.LastName);

    }
}