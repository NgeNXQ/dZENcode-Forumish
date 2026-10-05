"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.isWebUrl = isWebUrl;
function isWebUrl(value) {
    try {
        const url = new URL(value);
        return (url.protocol === "http:" || url.protocol === "https:") &&
            !url.username && !url.password;
    }
    catch {
        return false;
    }
}
