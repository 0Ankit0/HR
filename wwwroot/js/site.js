window.renderQrCode = (elementId, qrCodeUri) => {
    // Remove any previous QR code
    const el = document.getElementById(elementId);
    if (!el) return;
    el.innerHTML = "";
    // Create QR code
    new QRCode(el, {
        text: qrCodeUri,
        width: 200,
        height: 200,
        colorDark: "#000000",
        colorLight: "#ffffff",
        correctLevel: QRCode.CorrectLevel.H
    });
};

window.downloadFileFromBytes = (fileName, contentType, bytes) => {
    // Convert .NET byte[] (Uint8Array) to a Blob and trigger download
    const blob = new Blob([new Uint8Array(bytes)], { type: contentType });
    const url = URL.createObjectURL(blob);

    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = fileName;
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);

    setTimeout(() => URL.revokeObjectURL(url), 1000);
};