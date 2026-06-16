using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using PdfiumViewer;

namespace TIManager.Utils
{
    public static class PdfHelper
    {
        public static string GerarMiniatura(string pdfPath)
        {
            try
            {
                string thumbFolder = @"C:\TIManagerData\Thumbnails";
                if (!Directory.Exists(thumbFolder)) Directory.CreateDirectory(thumbFolder);

                string thumbName = Path.GetFileNameWithoutExtension(pdfPath) + "_thumb.jpg";
                string thumbPath = Path.Combine(thumbFolder, thumbName);

                using (var document = PdfDocument.Load(pdfPath))
                {
                    if (document.PageCount > 0)
                    {
                        // Renderiza a primeira página (index 0)
                        // DPI 96 é suficiente para miniatura
                        using (var image = document.Render(0, 300, 400, 96, 96, false))
                        {
                            image.Save(thumbPath, ImageFormat.Jpeg);
                        }
                        return thumbPath;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao gerar miniatura: " + ex.Message);
            }
            return null;
        }
    }
}
