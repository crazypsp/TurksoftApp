// iText7
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Geom;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Business.Interface;
using TurkSoft.Entities.Document;

namespace TurkSoft.Business.Managers
{
    public class BankaEkstreManager : IBankaEkstreBusiness
    {
        private readonly string[] basliklar = ["tarih", "açıklama", "aciklama", "tutar", "bakiye"];
        public async Task<List<BankaHareket>> OkuExcelAsync(IFormFile dosya, string klasorYolu)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var stream = new MemoryStream();
            await dosya.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets.FirstOrDefault();
            if (sheet == null) return new();

            var headerRow = FindHeaderRow(sheet);
            if (headerRow == -1) return new();

            var indexes = FindColumnIndexes(sheet, headerRow);

            var hareketler = new List<BankaHareket>();
            for (int i = headerRow + 1; i <= sheet.Dimension.End.Row; i++)
            {
                string tarihStr = sheet.Cells[i, indexes["tarih"]].Text.Trim();
                string aciklama = sheet.Cells[i, indexes["aciklama"]].Text.Trim();
                string tutarStr = sheet.Cells[i, indexes["tutar"]].Text.Trim();
                string bakiyeStr = sheet.Cells[i, indexes["bakiye"]].Text.Trim();

                // Tarih parse (Türkçe format ve '-' düzeltme)
                var turkceCulture = new CultureInfo("tr-TR");
                tarihStr = tarihStr.Replace("-", " "); // 04/07/2025 11:39:10

                if (DateTime.TryParse(tarihStr, turkceCulture, DateTimeStyles.None, out var tarih) &&
                    decimal.TryParse(tutarStr, NumberStyles.Number, turkceCulture, out var tutar) &&
                    decimal.TryParse(bakiyeStr, NumberStyles.Number, turkceCulture, out var bakiye))
                {
                    hareketler.Add(new BankaHareket
                    {
                        Tarih = tarih,
                        Aciklama = aciklama,
                        Tutar = tutar,
                        Bakiye = bakiye,
                        HesapKodu = "",
                        KaynakDosya = dosya.FileName,
                        BankaAdi = System.IO.Path.GetFileNameWithoutExtension(dosya.FileName),
                        KlasorYolu = klasorYolu
                    });
                }
            }
            return hareketler;
        }

        public async Task<List<BankaHareket>> OkuPDFAsync(IFormFile dosya, string klasorYolu)
        {
            var hareketler = new List<BankaHareket>();

            // PDF stream yükle
            using var stream = new MemoryStream();
            await dosya.CopyToAsync(stream);
            stream.Position = 0;

            // PDF format kontrolü
            byte[] header = new byte[5];
            await stream.ReadAsync(header, 0, 5);
            string headerText = Encoding.ASCII.GetString(header);
            if (!headerText.StartsWith("%PDF", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Gönderilen dosya geçerli bir PDF formatında değil.");
            }

            // Tekrar başa al
            stream.Position = 0;

            // Şifreli PDF desteği için ReaderProperties
            var readerProps = new ReaderProperties();
            // readerProps.SetPassword(Encoding.UTF8.GetBytes("PAROLA")); // Gerekirse aktif et

            using var reader = new PdfReader(stream, readerProps);
            using var pdf = new PdfDocument(reader);

            var turkceCulture = new CultureInfo("tr-TR");

            for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
            {
                // Daha iyi satır koruması için LocationTextExtractionStrategy kullan
                var strategy = new LocationTextExtractionStrategy();
                string pageText = PdfTextExtractor.GetTextFromPage(pdf.GetPage(i), strategy);

                // Satırlara ayır ve boşlukları temizle
                var lines = pageText
                    .Replace("\r", "")
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .ToList();

                foreach (var line in lines)
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 4) continue;

                    // İlk alan tarih mi kontrol et
                    if (DateTime.TryParse(parts[0], turkceCulture, DateTimeStyles.None, out var tarih) &&
                        decimal.TryParse(parts[^2], NumberStyles.Number, turkceCulture, out var tutar) &&
                        decimal.TryParse(parts[^1], NumberStyles.Number, turkceCulture, out var bakiye))
                    {
                        string aciklama = string.Join(' ', parts.Skip(1).Take(parts.Length - 3));

                        hareketler.Add(new BankaHareket
                        {
                            Tarih = tarih,
                            Aciklama = aciklama,
                            Tutar = tutar,
                            Bakiye = bakiye,
                            HesapKodu = "",
                            KaynakDosya = dosya.FileName,
                            BankaAdi = System.IO.Path.GetFileNameWithoutExtension(dosya.FileName),
                            KlasorYolu = klasorYolu
                        });
                    }
                }
            }

            return hareketler;
        }
    

        public async Task<List<HesapKodEsleme>> OkuTxtAsync(IFormFile dosya, string KlasorYolu)
        {
            var liste = new List<HesapKodEsleme>();
            using var reader = new StreamReader(dosya.OpenReadStream());
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                var parts = line?.Split('=');
                if (parts?.Length == 2)
                {
                    liste.Add(new HesapKodEsleme
                    {
                        AnahtarKelime = parts[0].Trim(),
                        HesapKodu = parts[1].Trim()
                    });
                }
            }
            return liste;
        }

        public async Task<bool> YazTxtAsync(List<HesapKodEsleme> eslemeler, string KlasorYolu)
        {
            var path = System.IO.Path.Combine("Dosyalar", KlasorYolu.Replace('/', System.IO.Path.DirectorySeparatorChar), "accounting_match.txt");
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            var lines = eslemeler.Select(x => $"{x.AnahtarKelime}={x.HesapKodu}");
            await File.WriteAllLinesAsync(path, lines);
            return true;
        }

        private int FindHeaderRow(ExcelWorksheet sheet)
        {
            for (int row = 1; row < sheet.Dimension.End.Row; row++)
            {
                var headers = Enumerable.Range(1, sheet.Dimension.End.Column)
                    .Select(col => sheet.Cells[row, col].Text.Trim().ToLower()).ToList();
                if (headers.Any(h => basliklar.Any(b => headers.Contains(b))))
                    return row;
            }
            return -1;
        }
        private Dictionary<string, int> FindColumnIndexes(ExcelWorksheet sheet, int headerRow)
        {
            var map=new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int col = 1; col <= sheet.Dimension.End.Column; col++)
            {
                string val = sheet.Cells[headerRow, col].Text.Trim().ToLower();
                if (val.Contains("tarih")) map["tarih"] = col;
                else if (val.Contains("açıklama") || val.Contains("aciklama")) map["aciklama"] = col;
                else if (val.Contains("tutar")) map["tutar"] = col;
                else if (val.Contains("bakiye")) map["bakiye"] = col;
            }
            return map;
        }
    }
}
