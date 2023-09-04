using Xunit;

namespace WithoutMediatR.RequestResponseMultipleDispatchWithReduce;

file record SomeRequest(string Message);

file interface IMyValidator
{
    bool Validate(SomeRequest request);
}

file class ValidatorA : IMyValidator
{
    bool IMyValidator.Validate(SomeRequest request)
    {
        // domain logic
        return true;
    }
}

file class ValidatorB : IMyValidator
{
    bool IMyValidator.Validate(SomeRequest request)
    {
        // domain logic
        return false;
    }
}

file class Validators : IMyValidator
{
    private readonly IEnumerable<IMyValidator> _handlers;

    internal Validators(IEnumerable<IMyValidator> handlers)
    {
        _handlers = handlers;
    }
    
    bool IMyValidator.Validate(SomeRequest request) => 
        _handlers
            .Select(h => h.Validate(request))  // map
            .Aggregate((acc, i) => acc && i);  // reduce
}

file class Client
{
    private readonly IMyValidator _validator;

    internal Client(IMyValidator validator)
    {
        _validator = validator;
    }

    internal string DispatchToAll(SomeRequest request) => 
        _validator.Validate(request) 
            ? "all good!" 
            : "sorry, the request is not valid";
}

public class Without
{
    [Fact]
    void dispatch_all_requests()
    {
        var validators = new Validators(
            new IMyValidator[]{ new ValidatorA(), new ValidatorB()});
        
        var client = new Client(validators);
        
        var response = client.DispatchToAll(new SomeRequest("my message"));
        
        Assert.Equal("sorry, the request is not valid", response);
    }
}
