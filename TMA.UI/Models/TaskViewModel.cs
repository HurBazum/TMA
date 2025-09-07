using TMA.UI.Infrastructure;
using TMA.UI.ViewModels.Base;
using TMA.Shared;

namespace TMA.UI.Models;

public class TaskViewModel : ViewModelBase
{
    public event EventHandler<ModelChangedEventArgs>? ModelChanged;
    public string Id { get; init; } = null!;

    private string _title = null!;
    public string Title 
    { 
        get => _title;
        set => Set(ref _title, value); 
    }

    private DateTime? _deadline;
    public DateTime? Deadline 
    { 
        get => _deadline; 
        set => Set(ref _deadline, value);
    }
    public DateTime CreatedDate { get; init; }

    private bool _completed;
    public bool Completed 
    {
        get => _completed;
        set => Set(ref _completed, value);
    }
       

    private TaskPriority _priority;
    public TaskPriority Priority
    {
        get => _priority;
        set
        {
            Set(ref _priority, value);
            SetPDictionaryTrueValue(value);            
        }
    }

    public Dictionary<TaskPriority, bool> PDictionary 
    {
        get;
        private set;

    } = new()
    {
        { TaskPriority.Normal, false },
        { TaskPriority.Medium, false },
        { TaskPriority.High, false }
    };

    public void SetPDictionaryTrueValue(TaskPriority value)
    {
        foreach(var key in PDictionary.Keys)
        {
            PDictionary[key] = false;            
        }

        PDictionary[value] = true;
    }

    public override string ToString() => $"Id: {Id}\nTitle: {Title}\nCompleted: {Completed}";
}