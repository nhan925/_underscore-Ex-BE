namespace student_management_api.Exceptions;

public class EnvironmentVariableNotFoundException : Exception
{
    public EnvironmentVariableNotFoundException()
        : base("The env variable not found") { }

    public EnvironmentVariableNotFoundException(string message)
        : base(message) { }

    public EnvironmentVariableNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}
