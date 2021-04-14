using System.Text;

namespace MyMNGR.Data
{
    public class Alias
    {
        public string Name;

        public string Host;

        public string Username;

        public bool UsePassword;

        public string ToArgs()
        {
            StringBuilder args = new StringBuilder();
            args.Append($"--login-path={Name} --host={Host} --user={Username}");
            if (UsePassword)
            {
                args.Append(" --password");
            }
            return args.ToString();
        }
    }
}
