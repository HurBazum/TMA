using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMA.Application.Queries;
using TMA.Domain;
using TMA.Domain.VOs;

namespace TMA.Application.QueryHandlers
{
    public class GetByTitleQueryHandler(ITaskRepository taskRepository)
    {
        private readonly ITaskRepository _taskRepository = taskRepository;

        public Task<TaskEntity> Handle(GetByTitleQuery query)
        {
            throw new NotImplementedException();
        }
    }
}
