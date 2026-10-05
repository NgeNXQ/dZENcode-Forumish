export function detectImageMime(bytes: Uint8Array): string | null {
    if (bytes.length >= 8 && [137, 80, 78, 71, 13, 10, 26, 10].every((byte, i) => bytes[i] === byte)) {
        return "image/png";
    }
    if (bytes.length >= 3 && bytes[0] === 255 && bytes[1] === 216 && bytes[2] === 255) {
        return "image/jpeg";
    }
    const header = String.fromCharCode(...bytes.subarray(0, 12));
    if (header.startsWith("GIF87a") || header.startsWith("GIF89a")) return "image/gif";
    if (header.startsWith("RIFF") && header.slice(8, 12) === "WEBP") return "image/webp";
    return null;
}
