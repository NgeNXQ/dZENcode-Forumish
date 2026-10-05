"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.parseAppSettings = parseAppSettings;
const is_record_1 = require("./is-record");
function positive(value, maximum = 2147483647) {
    if (typeof value !== "number" || !Number.isSafeInteger(value) || value <= 0 || value > maximum) {
        throw new Error("Application settings contain invalid limits.");
    }
    return value;
}
function captchaHeader(value) {
    // Proof headers must not replace standard request or security headers.
    if (typeof value !== "string" || !/^X-Captcha-[A-Za-z0-9-]+$/i.test(value)) {
        throw new Error("Application settings contain invalid captcha headers.");
    }
    return value;
}
function parseAppSettings(value) {
    if (!(0, is_record_1.isRecord)(value) || !(0, is_record_1.isRecord)(value['captcha']) || !(0, is_record_1.isRecord)(value['comment']) ||
        !(0, is_record_1.isRecord)(value['attachment']) || !(0, is_record_1.isRecord)(value['attachment']['mime'])) {
        throw new Error("Application settings are incomplete.");
    }
    const apiBaseUrl = value['apiBaseUrl'];
    // Keep captcha proofs and fingerprints on the application's origin.
    if (typeof apiBaseUrl !== "string" || !/^\/(?:[A-Za-z0-9_-]+\/?)+$/.test(apiBaseUrl)) {
        throw new Error("The API base URL must be an absolute path on this origin.");
    }
    const captcha = value['captcha'];
    const comment = value['comment'];
    const attachment = value['attachment'];
    const mime = attachment['mime'];
    if (!(0, is_record_1.isRecord)(mime))
        throw new Error("Attachment settings are incomplete.");
    const supportedTypes = ["image/jpeg", "image/png", "image/gif", "text/plain"];
    if (Object.keys(mime).length !== supportedTypes.length) {
        throw new Error("Application settings contain unsupported attachment types.");
    }
    const settings = {
        apiBaseUrl: apiBaseUrl.replace(/\/$/, ""),
        requestTimeoutMs: positive(value['requestTimeoutMs']),
        captcha: {
            width: positive(captcha['width'], 4096),
            height: positive(captcha['height'], 4096),
            codeLength: positive(captcha['codeLength'], 128),
            expirationSeconds: positive(captcha['expirationSeconds'], 2147483),
            idHeader: captchaHeader(captcha['idHeader']),
            codeHeader: captchaHeader(captcha['codeHeader']),
        },
        comment: {
            emailMaximumLength: positive(comment['emailMaximumLength']),
            usernameMinimumLength: positive(comment['usernameMinimumLength']),
            usernameMaximumLength: positive(comment['usernameMaximumLength']),
            homePageMaximumLength: positive(comment['homePageMaximumLength']),
            messageMaximumLength: positive(comment['messageMaximumLength']),
            defaultPageSize: positive(comment['defaultPageSize']),
            maximumPageSize: positive(comment['maximumPageSize']),
        },
        attachment: {
            mime: Object.fromEntries(supportedTypes.map(type => [type, positive(mime[type])])),
            pollIntervalMs: positive(attachment['pollIntervalMs']),
            pollAttempts: positive(attachment['pollAttempts'], 1000),
        },
    };
    if (settings.comment.defaultPageSize > settings.comment.maximumPageSize ||
        settings.comment.usernameMinimumLength > settings.comment.usernameMaximumLength ||
        settings.captcha.idHeader.toLowerCase() === settings.captcha.codeHeader.toLowerCase()) {
        throw new Error("Application settings contain conflicting limits.");
    }
    return settings;
}
