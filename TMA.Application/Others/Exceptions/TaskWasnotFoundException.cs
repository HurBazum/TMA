namespace TMA.Application.Others.Exceptions;

public class TaskWasnotFoundException(string message) : Exception(message)
{
}