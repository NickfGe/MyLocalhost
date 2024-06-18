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
        OpenBrowser(Url);
    }

    private void OnApplicationQuit()
    {
        StopServer();
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
}
