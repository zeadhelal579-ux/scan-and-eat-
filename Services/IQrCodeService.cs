// FILE: Services/IQrCodeService.cs
// PURPOSE: Contract wrapping the QRCoder library (04-BACKEND-ARCHITECTURE.md
//          section 2), used by TablesController's QR-code endpoint.

namespace ScanEat.Api.Services;

public interface IQrCodeService
{
    // Builds the full menu link for the given table token and renders it as
    // a PNG. Returns the raw image bytes.
    byte[] GenerateTableQrPng(string qrToken);
}
