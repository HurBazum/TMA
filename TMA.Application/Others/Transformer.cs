using TMA.Application.Dtos;
using TMA.Domain;

namespace TMA.Application.Others;

public static class Transformer
{
    public static TaskDto ToDto(TaskEntity entity) => new()
    {
        Id = entity.Id.Value.ToString(),
        Title = entity.Title.Value,
        DeadlineDate = entity.Deadline,
        Priority = entity.Priority,
        CreatedDate = entity.CreatedDate,
        Completed = entity.Status == Shared.TaskStatus.Completed
    };

    public static List<TaskDto> ToDtos(IEnumerable<TaskEntity> entities) => Enumerable.Select(entities, ToDto).ToList();
}