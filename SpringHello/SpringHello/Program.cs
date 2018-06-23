using SpringHello.view;

namespace SpringHello
{
    public class Program
    {
        public static YYAccount currentLoggedInYyAccount;
        
        static void Main(string[] args)
        {
            MainThread main = new MainThread();
            main.GenerateMenu();
        }
    }
}