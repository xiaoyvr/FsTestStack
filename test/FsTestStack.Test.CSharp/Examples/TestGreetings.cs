using FsTestStack.Test.CSharp.Domains;

namespace FsTestStack.Test.CSharp.Examples;

public class TestGreetings : IGreetings
{
    public string Greetings()
    {
        return "Greetings from Test";
    }
}