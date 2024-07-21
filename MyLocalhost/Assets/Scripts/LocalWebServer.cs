using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class LocalWebServer : MonoBehaviour
{
    private HttpListener _httpListener;
    private const string Url = "http://localhost:55050/";
    private string _webContentPath;
    public TextMeshProUGUI debug;

    private void Start()
    {
        // Definir la ruta de los archivos en StreamingAssets
        _webContentPath = Path.Combine(Application.streamingAssetsPath, "WebContent");
        StartServer();
        StartApacheServer();
        OpenBrowser(Url);
    }

    private void OnApplicationQuit()
    {
        StopServer();
        StopXampp();
    }

    private void StartServer()
    {
        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add(Url);
        _httpListener.Start();
        debug.text = $"Servidor iniciado en {Url}";
        UnityEngine.Debug.Log(debug.text);
        Task.Run(() => Listen());
    }

    private void StopServer()
    {
        if (_httpListener != null)
        {
            _httpListener.Stop();
            _httpListener.Close();
            debug.text = "Servidor detenido.";
            UnityEngine.Debug.Log(debug.text);
        }
    }

    private async Task Listen()
    {
        while (_httpListener.IsListening)
        {
            var context = await _httpListener.GetContextAsync();
            var response = context.Response;
            string requestUrl = context.Request.Url.LocalPath.TrimStart('/');

            string filePath = Path.Combine(_webContentPath, requestUrl);
            if (File.Exists(filePath))
            {
                byte[] buffer = File.ReadAllBytes(filePath);
                response.ContentType = GetContentType(filePath);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                byte[] buffer = Encoding.UTF8.GetBytes("<html><body><h1>Error 404: Not Found</h1></body></html>");
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }

            response.OutputStream.Close();
        }
    }

    private string GetContentType(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        switch (extension)
        {
            case ".html": return "text/html";
            case ".css": return "text/css";
            case ".js": return "application/javascript";
            default: return "application/octet-stream";
        }
    }

    private void OpenBrowser(string url)
    {
        try
        {
            url += "index.html";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"No se pudo abrir el navegador: {e.Message}");
        }
    }

    private void StartApacheServer()
    {
        string xamppPath = GetXamppPath();

        if (!string.IsNullOrEmpty(xamppPath))
        {
            string apacheStartPath = Path.Combine(xamppPath, "apache_start.bat");

            if (File.Exists(apacheStartPath))
            {
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = apacheStartPath,
                    UseShellExecute = true
                };

                try
                {
                    Process.Start(processInfo);
                    UnityEngine.Debug.Log("Apache server started successfully.");
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError("Failed to start Apache server: " + ex.Message);
                }
            }
            else
            {
                UnityEngine.Debug.LogError("apache_start.bat not found at path: " + apacheStartPath);
            }
        }
        else
        {
            UnityEngine.Debug.LogError("XAMPP path not found.");
        }
    }

    // Este método intenta obtener la ruta de XAMPP de una manera que se pueda ajustar a tus necesidades.
    private string GetXamppPath()
    {
        // Por defecto, busca en el directorio de instalación común de XAMPP.
        string[] possiblePaths = new string[]
        {
            @"C:\xampp",
            @"D:\xampp",
            @"E:\xampp",
            @"F:\xampp",
            @"G:\xampp",
            @"H:\xampp"
            // Agrega más rutas si es necesario
        };

        foreach (string path in possiblePaths)
        {
            if (Directory.Exists(path))
            {
                return path;
            }
        }

        return string.Empty;
    }

    private void StopXampp()
    {
        string xamppPath = GetXamppPath();

        if (!string.IsNullOrEmpty(xamppPath))
        {
            string apacheStartPath = Path.Combine(xamppPath, "apache_stop.bat");

            if (File.Exists(apacheStartPath))
            {
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = apacheStartPath,
                    UseShellExecute = true
                };

                try
                {
                    Process.Start(processInfo);
                    UnityEngine.Debug.Log("Apache server stopped successfully.");
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError("Failed to stop Apache server: " + ex.Message);
                }
            }
        }
    }
}
