using FsTestStack.Test.CSharp.Examples;

namespace FsTestStack.Test.CSharp.Domains;

public class ProdGreetings : IGreetings
{
    public string Greetings()
    {
        return "Greetings from Prod";
    }
}