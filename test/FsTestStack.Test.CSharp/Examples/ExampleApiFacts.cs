using System.Net;
using FsTestStack.Test.CSharp.Domains;
using Microsoft.Extensions.DependencyInjection;

namespace FsTestStack.Test.CSharp.Examples;

public class ExampleApiFacts : ApiFactBase
{
    [Fact]
    public async void should_be_able_to_run_test_for_application_logic()
    {
        DbSave(new People("John", "Doe"));

        using var httpClient = Launch();
        
        var response = await httpClient.GetAsync("/people");

        var body = await response.Content.ReadFromJsonAsync(new
        {
            id = default(int), firstName = default(string), lastName = default(string)
        }.OfArray());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var people = body[0];
        Assert.Equal("John", people.firstName);
        Assert.Equal("Doe", people.lastName);
        Assert.NotEqual(0, people.id);
    }
    
    [Fact]
    public async void should_be_able_to_mock_interface()
    {
        using var httpClient = Launch(b => b.Services.AddTransient<IGreetings, TestGreetings>());
        
        var response = await httpClient.GetAsync("/greetings");
        
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Greetings from Test", body);
    }

}