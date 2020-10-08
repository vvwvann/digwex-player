using DigWex.Managers;

namespace DigWex.Commands
{
  public abstract class Command : ICommand
  {
    protected readonly CommandManager CommandService;
    protected readonly Journal Journal;
    protected readonly int CommandId;

    public Command(int id = -1)
    {
      CommandService = CommandManager.Instance;
      Journal = Journal.Instance;
      CommandId = id;
    }

    public abstract void StartAsync();
  }
}
