﻿namespace student_management_api.Exceptions;

public class ForbiddenException: Exception
{
    public ForbiddenException()
        : base("You do not have permission to access this resource.") { }

    public ForbiddenException(string message)
        : base(message) { }

    public ForbiddenException(string message, Exception innerException)
        : base(message, innerException) { }
}
