using TMA.UI.Infrastructure.Commands;

namespace TMA.UI.Infrastructure.Commands.SpecialCommands;

public static class CommandCreator
{
    public static LambdaCommand CreateDateCmd(Dictionary<string, PropertyAccessor<DateTime?>> dic) =>
        new LambdaCommand((parameter) =>
        {
            if(parameter is not string value || string.IsNullOrEmpty(value))
            {
                return;
            }

            string[] headers = value.Split('_');

            bool extractAccessor = dic.TryGetValue(headers[0], out PropertyAccessor<DateTime?>? accessor);

            if(!extractAccessor)
            {
                return;
            }

            bool isIncrement = headers.Last() == "Increment";

            int x = isIncrement ? 1 : -1;

            DateTime? date = accessor.Getter.Invoke();

            switch(headers[1])
            {
                case "Day": date = date.Value.AddDays(x); break;
                case "Month": date = date.Value.AddMonths(x); break;
                case "Year": date = date.Value.AddYears(x); break;
            }

            accessor.Setter.Invoke(date);

        }, parameter => true);
}
