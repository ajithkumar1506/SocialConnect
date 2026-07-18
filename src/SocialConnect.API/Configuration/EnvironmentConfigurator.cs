using DotNetEnv;

namespace SocialConnect.API.Configuration;

public class EnvironmentConfigurator
{
    public void LoadDotEnv()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            var envPath = Path.Combine(dir.FullName, ".env");
            if (File.Exists(envPath))
            {
                Env.Load(envPath);
                break;
            }
            if (
                Directory.GetFiles(dir.FullName, "*.sln").Length > 0
                || Directory.Exists(Path.Combine(dir.FullName, ".git"))
            )
                break;
            dir = dir.Parent;
        }
    }
}
