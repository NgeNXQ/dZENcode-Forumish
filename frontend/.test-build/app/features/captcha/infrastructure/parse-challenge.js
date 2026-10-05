"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.parseChallenge = parseChallenge;
const is_record_1 = require("../../../shared/orchestration/is-record");
const detect_image_mime_1 = require("../../../shared/infrastructure/detect-image-mime");
function parseChallenge(value) {
    if (!(0, is_record_1.isRecord)(value) || typeof value['id'] !== "string" ||
        !/^[A-Za-z0-9-]{1,128}$/.test(value['id']) || typeof value['payload'] !== "string" ||
        value['payload'].length > 2 * 1024 * 1024 ||
        !/^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$/.test(value['payload'])) {
        throw new Error("Invalid verification image response.");
    }
    const bytes = Uint8Array.from(atob(value['payload'].slice(0, 16)), char => char.charCodeAt(0));
    const mime = (0, detect_image_mime_1.detectImageMime)(bytes);
    if (!mime)
        throw new Error("Invalid verification image format.");
    return { id: value['id'], imageUrl: `data:${mime};base64,${value['payload']}` };
}
