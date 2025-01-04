using System;
using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Mtlq.Models;

namespace Mtlq.Commands;

public abstract class BaseCommand<T>(string name, string description, JsonTypeInfo<T> typeInfo)
    : Command(name, description)
{
    protected async Task<int> WrapExecuteAsync(Func<Task<T>> executeAsync)
    {
        try
        {
            var result = await executeAsync();
            WriteResponse(result);
            return 0;
        }
        catch (Exception ex)
        {
            WriteError(ex);
            return 1;
        }
    }

    protected virtual Task<T> ExecuteAsync() =>
        throw new NotImplementedException("No-argument ExecuteAsync not implemented.");

    private void WriteResponse(T value) =>
        Console.Out.WriteLine(JsonSerializer.Serialize(value, typeInfo));

    private static void WriteError(Exception ex) =>
        Console.Error.WriteLine(
            JsonSerializer.Serialize(
                new CommandError { Message = ex.Message, Details = ex.ToString() },
                ErrorJsonContext.Default.CommandError
            )
        );
}
