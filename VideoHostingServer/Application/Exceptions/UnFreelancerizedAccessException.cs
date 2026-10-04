namespace Application.Exceptions;

public class UnFreelancerizedAccessException : Exception
{
    public UnFreelancerizedAccessException() : base("Доступ заборонено") { }

    public UnFreelancerizedAccessException(string message) : base(message) { }

    public UnFreelancerizedAccessException(string message, Exception innerException) : base(message, innerException) { }
}
