import { AttachmentSettings } from "../../../shared/orchestration/attachment-settings";
import { detectImageMime } from "../../../shared/infrastructure/detect-image-mime";
import { AttachmentPreview } from "../orchestration/attachment-preview";

export async function prepareAttachmentPreview(blob: Blob, settings: AttachmentSettings): Promise<AttachmentPreview> {
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
    if (detectImageMime(bytes) !== type) throw new Error("This attachment has an invalid image format.");
    // The subscriber owns the object URL; create it only after asynchronous validation finishes.
    return { imageUrl: null, text: null, unavailable: false };
}
