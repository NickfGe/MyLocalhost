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
    private string _htmlFilePath;
    public TextMeshProUGUI debug;

    private void Start()
    {
        // Asegurarse de que la carpeta WebContent esté en la ruta correcta después de la compilación
        _htmlFilePath = Path.Combine(Application.streamingAssetsPath, "WebContent", "index.html");
        StartServer();
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
        Debug.Log(debug.text);
        Application.OpenURL(Url);
        Task.Run(() => Listen());
    }

    private void StopServer()
    {
        if (_httpListener != null)
        {
            _httpListener.Stop();
            _httpListener.Close();
            debug.text = "Servidor detenido.";
            Debug.Log(debug.text);
        }
    }

    private async Task Listen()
    {
        while (_httpListener.IsListening)
        {
            var context = await _httpListener.GetContextAsync();
            var response = context.Response;

            if (File.Exists(_htmlFilePath))
            {
                byte[] buffer = File.ReadAllBytes(_htmlFilePath);
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
}
