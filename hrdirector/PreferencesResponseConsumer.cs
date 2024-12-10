using contracts;
using MassTransit;

namespace hrdirector;

public class PreferencesResponseConsumer(
    HarmonyService harmonyService) : IConsumer<Preferences>
{
    public Task Consume(ConsumeContext<Preferences> context)
    {
        harmonyService.AddPreferences(context.Message.Id, context.Message);
        return Task.CompletedTask;
    }
}