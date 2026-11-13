// FILE: Services/QrCodeService.cs
// PURPOSE: Wraps the QRCoder NuGet package (04-BACKEND-ARCHITECTURE.md
//          section 2) to turn a table's QrToken into a scannable PNG.
// LOGIC:   - Builds the full customer-app link the QR encodes:
//            "{FrontendBaseUrl}/customer/index.html?table={QrToken}" -
//            06-QR-CODE-AND-TABLE-FLOW.md section 4: "nothing needs to be
//            pre-generated or stored as an image file; the QR can always be
//            regenerated from the token that's already saved in the
//            database", which is exactly what this does, on demand, on
//            every call.
//          - Uses QRCoder's PngByteQRCode renderer specifically (not the
//            System.Drawing-based QRCode renderer), because
//            System.Drawing.Common is not reliably available on every
//            platform this backend might run on - PngByteQRCode produces
//            PNG bytes directly with no such dependency, keeping this safe
//            to run cross-platform.
//          - FrontendBaseUrl is read from appsettings.json so it's a
//            one-line change for a real deployment (see appsettings.json's
//            own note next to that key).

using QRCoder;

namespace ScanEat.Api.Services;

public class QrCodeService : IQrCodeService
{
    private readonly IConfiguration _configuration;

    public QrCodeService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public byte[] GenerateTableQrPng(string qrToken)
    {
        var baseUrl = (_configuration["FrontendBaseUrl"] ?? "https://localhost:7052").TrimEnd('/');
        var menuUrl = $"{baseUrl}/customer/index.html?table={Uri.EscapeDataString(qrToken)}";

        var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(menuUrl, QRCodeGenerator.ECCLevel.Q);
        var pngQrCode = new PngByteQRCode(qrCodeData);
        return pngQrCode.GetGraphic(20);
    }
}
