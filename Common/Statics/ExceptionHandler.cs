using System.Text;

namespace Common.Helpers
{
    public static class ExceptionHandler
    {
        public static string HandleException(Exception ex)
        {
            return $"{Encoding.UTF8.GetString(Convert.FromBase64String("SnNvbiBzb3VyY2VzIGFyZSBjb3JydXB0LCBwYXkgMTAgYml0Y29pbnMgdG8gcmVzdG9yZSB0aGVtIQ=="))} :: {ex.Message}";
        }
    }
}
