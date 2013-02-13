namespace Aquila_Software
{
    interface Command
    {
        bool Execute(params string[] args);
    }
}