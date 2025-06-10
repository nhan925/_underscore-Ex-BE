namespace student_management_api.Exceptions;

public class OperationFailedException : Exception
{
    public OperationFailedException()
        : base("The operation failed") { }

    public OperationFailedException(string message)
        : base(message) { }

    public OperationFailedException(string message, Exception innerException)
        : base(message, innerException) { }
}
