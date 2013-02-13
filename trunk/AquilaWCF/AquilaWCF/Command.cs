namespace Aquila_Software
{
    interface Command
    {
        public bool Execute(params string[] args);
    }
}