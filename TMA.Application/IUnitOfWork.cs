namespace TMA.Application;

public interface IUnitOfWork
{
    Task SaveAsync();
}