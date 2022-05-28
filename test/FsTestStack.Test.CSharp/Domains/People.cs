using FluentNHibernate.Mapping;

namespace FsTestStack.Test.CSharp.Domains;

public class People
{
    public People(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
    
    protected People()
    {
    }
    public virtual int Id { get; protected set; }
    public virtual string FirstName { get; } = null!;
    public virtual string LastName { get; } = null!;
}

public class PeopleMapping : ClassMap<People>
{
    public PeopleMapping()
    {
        Id(p => p.Id);
        Map(p => p.FirstName);
        Map(p => p.LastName);
    }
}