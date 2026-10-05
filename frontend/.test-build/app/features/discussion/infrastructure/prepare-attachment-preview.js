"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.prepareAttachmentPreview = prepareAttachmentPreview;
const detect_image_mime_1 = require("../../../shared/infrastructure/detect-image-mime");
async function prepareAttachmentPreview(blob, settings) {
    const type = blob.type.split(";")[0].trim().toLowerCase();
    const limit = Object.hasOwn(settings.mime, type) ? settings.mime[type] : undefined;
    if (!limit || blob.size === 0 || blob.size > limit) {
        throw new Error("This attachment has an unsupported type or size.");
    }
    if (type === "text/plain") {
        const text = new TextDecoder("utf-8", { fatal: true }).decode(await blob.arrayBuffer());
        return { imageUrl: null, text, unavailable: false };
    }
    const bytes = new Uint8Array(await blob.slice(0, 12).arrayBuffer());
    if ((0, detect_image_mime_1.detectImageMime)(bytes) !== type)
        throw new Error("This attachment has an invalid image format.");
    // The subscriber owns the object URL; create it only after asynchronous validation finishes.
    return { imageUrl: null, text: null, unavailable: false };
}
