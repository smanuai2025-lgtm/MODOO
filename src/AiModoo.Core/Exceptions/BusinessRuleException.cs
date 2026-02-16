namespace AiModoo.Core.Exceptions;

public class BusinessRuleException : Exception
{
    public string Code { get; }

    public BusinessRuleException(string message) : base(message)
    {
        Code = "BUSINESS_RULE_VIOLATION";
    }

    public BusinessRuleException(string code, string message) : base(message)
    {
        Code = code;
    }

    public BusinessRuleException(string message, Exception innerException)
        : base(message, innerException)
    {
        Code = "BUSINESS_RULE_VIOLATION";
    }
}
