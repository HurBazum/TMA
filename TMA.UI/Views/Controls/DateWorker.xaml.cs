using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TMA.UI.Views.Controls;

/// <summary>
/// Логика взаимодействия для DateWorker.xaml
/// </summary>
public partial class DateWorker : UserControl, INotifyPropertyChanged
{
    public static readonly DependencyProperty DateProperty = DependencyProperty.Register(
        "Date", 
        typeof(DateTime), 
        typeof(DateWorker), 
        new FrameworkPropertyMetadata(default(DateTime), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDateChanged));

    public static readonly DependencyProperty CmdProperty = DependencyProperty.Register(
        "Cmd",
        typeof(ICommand),
        typeof(DateWorker),
        new PropertyMetadata(default));

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        "Header",
        typeof(string),
        typeof(DateWorker),
        new PropertyMetadata(default, OnParametersChanged));

    public static readonly DependencyProperty LimitProperty = DependencyProperty.Register(
        "Limit",
        typeof(bool),
        typeof(DateWorker),
        new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    public DateTime Date
    {
        get => (DateTime)GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }

    public ICommand Cmd
    {
        get => (ICommand)GetValue(CmdProperty);
        set => SetValue(CmdProperty, value);
    }

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public bool Limit
    {
        get => (bool)GetValue(LimitProperty);
        set => SetValue(LimitProperty, value);
    }

    private void OnDateChanged()
    {
        OnPropertyChanged(nameof(Day));
        OnPropertyChanged(nameof(Month));
        OnPropertyChanged(nameof(Year));
    }


    // ?
    private void OnParametersChanged(string add)
    {
        for(int i = 0; i < Parameters.Count; i++)
        {
            Parameters[i] = string.Concat(add, Parameters[i]);
        }
        OnPropertyChanged(nameof(Parameters));
    }

    private static void OnParametersChanged(DependencyObject control, DependencyPropertyChangedEventArgs e)
    {
        var obj = (DateWorker)control;
        obj.OnParametersChanged(obj.Header);
    }

    private static void OnDateChanged(DependencyObject control, DependencyPropertyChangedEventArgs e)
    {
        var obj = (DateWorker)control;
        obj.OnDateChanged();
    }

    public int Day => Date.Day;
    public int Month => Date.Month;
    public int Year => Date.Year;
    public List<string> Parameters { get; set; } = [
                $"_Day_Increment",
                $"_Month_Increment",
                $"_Year_Increment",
                $"_Day_Decrement",
                $"_Month_Decrement",
                $"_Year_Decrement" ];
    public DateWorker()
    {
        InitializeComponent();
    }
}