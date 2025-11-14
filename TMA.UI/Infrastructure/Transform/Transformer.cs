using TMA.UI.Models;
using TMA.Application.Dtos;

namespace TMA.UI.Infrastructure.Transform;

public static class Transformer
{
    public static TaskDto ToDto(TaskViewModel tvm) => new()
    {
        Id = tvm.Id ?? string.Empty,
        Title = tvm.Title,
        Priority = tvm.Priority,
        CreatedDate = tvm.CreatedDate,
        DeadlineDate = tvm.Deadline,
        Completed = tvm.Completed,
        Status = tvm.Status
    };

    public static TaskViewModel ToModel(TaskDto dto) => new()
    {
        Id = dto.Id,
        Title = dto.Title,
        Priority = dto.Priority,
        CreatedDate = dto.CreatedDate,
        Deadline = dto.DeadlineDate,
        Completed = dto.Completed,
        Status = dto.Status
    };

    public static IEnumerable<TaskViewModel> ToModel(IEnumerable<TaskDto> dtos) => Enumerable.Select(dtos, x => ToModel(x));
}