using System;

namespace Aquila_Software
{
    public class SettingsHandlerService : SettingsHandler
    {
        public bool SetSetting(params object[] args)
        {
            try
            {
            }
            catch (Exception)
            {
                Console.Error.WriteLine("An exception occured!");
                return false;
            }
            return true;
        }
    }
}