using System.Net.Http;
using PyStudentIDE.UI.Services;

namespace PyStudentIDE.UI;

public partial class App : System.Windows.Application
{
    public static ApiClient Api { get; } = new(new HttpClient { BaseAddress = new Uri("http://localhost:5000/") });
    public static ScriptManager ScriptMgr { get; } = new();
    public static int CurrentUserId { get; set; }
    public static string CurrentUserName { get; set; } = string.Empty;
    public static string CurrentUserRole { get; set; } = string.Empty;
    public static string CurrentToken { get; set; } = string.Empty;
}
